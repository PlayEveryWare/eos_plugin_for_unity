/*
 * Copyright (c) 2024 PlayEveryWare
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */


namespace PlayEveryWare.EpicOnlineServices.Tests
{
    using EpicOnlineServices.Editor.Utility;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml.Linq;

    using System;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    using Utility;
    using JsonUtility = Utility.JsonUtility;
    using System.Globalization;
    using System.IO.Compression;
    using CompressionLevel = System.IO.Compression.CompressionLevel;

    /// <summary>
    /// This class consolidates any code coverage history data output from the
    /// code coverage utility by reading all code coverage history files, and
    /// keeping only the last code coverage report for each date.
    /// </summary>
    internal class CodeCoverageHistoryCleaner
    {
        /// <summary>
        /// The directory (relative to the root of the repository) at which the
        /// code coverage tool looks for historic coverage data.
        /// </summary>
        private const string CODE_COVERAGE_HISTORIC_DATA_DIRECTORY = "tools/coverage/historic";

        /// <summary>
        /// The format used by Unity's Code Coverage tool for recording the
        /// date and time of a code coverage report.
        /// </summary>
        private const string CODE_COVERAGE_DATETIME_FORMAT = "yyyy-MM-dd_HH-mm-ss";

        /// <summary>
        /// The file name to use when consolidating the code coverage history.
        /// </summary>
        private const string CODE_COVERAGE_CONSOLIDATED_FILENAME = "CoverageHistory.zip";

        /// <summary>
        /// The file in the code coverage report from which to determine the
        /// current percentage of lines of code covered by the unit tests. 
        /// </summary>
        private const string CODE_COVERAGE_REPORT_SUMMARY_FILE = "tools/coverage/results/Report/Summary.xml";

        /// <summary>
        /// The file in which to store the percentage of lines covered, to
        /// subsequently be used to generate a code coverage badge for GitHub.
        /// </summary>
        private const string CODE_COVERAGE_PERCENTAGE_FILE = "tools/coverage/results/current_coverage.txt";

        public static void ExtractLineCoverage()
        {
            XDocument summaryDoc = XDocument.Load(CODE_COVERAGE_REPORT_SUMMARY_FILE);

            var summaryElement = summaryDoc.Descendants("Summary").First();

            ulong coveredLines = ulong.Parse(summaryElement.Descendants("Coveredlines").First()?.Value);
            ulong coverableLines = ulong.Parse(summaryElement.Descendants("Coverablelines").First()?.Value);

            if (coverableLines == 0)
            {
                Debug.LogWarning($"For some reason the coverable lines as measured by the CodeCoverage utility is zero. Please check your configuration. Disregard if you are not trying to generate Code Coverage reports.");
                return;
            }

            double percentage = (double)coveredLines / coverableLines * 100;

            // Round to the nearest whole number
            int roundedPercentage = (int)Math.Round(percentage);

            // Convert to string and output
            string percentageString = roundedPercentage.ToString() + "%";

            // Write the percentage to file
            File.WriteAllText(CODE_COVERAGE_PERCENTAGE_FILE, percentageString);
        }

        /// <summary>
        /// Consolidates any code coverage history data output from the code
        /// coverage utility by reading all code coverage history files, and
        /// keeping only the last code coverage report for each date.
        /// </summary>
        private static void ConsolidateCoverageHistories()
        {
            try
            {
                // Get all CoverageHistory.xml files in the directory
                string[] coverageFiles =
                    Directory.GetFiles(CODE_COVERAGE_HISTORIC_DATA_DIRECTORY, "*_CoverageHistory.xml");

                // Parse files and group by date
                var groupedFiles = coverageFiles
                    .Select(filePath => new
                    {
                        FilePath = filePath,
                        Date = ParseDateFromFileName(filePath)
                    })
                    .Where(x => x.Date != null) // Exclude files that couldn't be parsed
                    .GroupBy(x => x.Date.Value.Date) // Group by the date part only
                    .Select(group => group.OrderByDescending(x => x.Date).First()) // Keep only the most recent file for each date
                    .ToList();

                CompressCoverageHistories(groupedFiles.Select(arg => arg.FilePath).ToArray());
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private static void CompressCoverageHistories(IEnumerable<string> historyFiles)
        {
            string compressedHistoriesFile = Path.Combine(
                CODE_COVERAGE_HISTORIC_DATA_DIRECTORY,
                CODE_COVERAGE_CONSOLIDATED_FILENAME
            );

            // If the file does not exist, then create it and add all the 
            // history files.
            if (!File.Exists(compressedHistoriesFile))
            {
                using ZipArchive archive = ZipFile.Open(compressedHistoriesFile, ZipArchiveMode.Create);
                foreach (string file in historyFiles)
                {
                    archive.CreateEntryFromFile(file, Path.GetFileName(file), CompressionLevel.Optimal);
                }

                // Stop here
                return;
            }

            // If the file does exist, then open it to determine which files (if
            // any) might need to be added to it.
            Dictionary<string, string> entriesToAdd = historyFiles.ToDictionary(Path.GetFileName);
            using (ZipArchive archive = ZipFile.OpenRead(compressedHistoriesFile))
            {
                foreach (var entry in archive.Entries)
                {
                    if (entriesToAdd.ContainsKey(entry.Name))
                        entriesToAdd.Remove(entry.Name);
                }

                if (entriesToAdd.Count == 0)
                {
                    Debug.Log("There were no coverage history files that needed to be added to the archive.");
                }
            }

            // Update the archive with any coverage history files that need to 
            // be added to the archive.
            using (ZipArchive archive = ZipFile.Open(compressedHistoriesFile, ZipArchiveMode.Update))
            {
                foreach ((string entryName, string filePath) in entriesToAdd)
                {
                    archive.CreateEntryFromFile(filePath, entryName, CompressionLevel.Optimal);
                }
            }
        }

        private static void DecompressCoverageHistories()
        {
            string pathToConsolidated = Path.Combine(
                CODE_COVERAGE_HISTORIC_DATA_DIRECTORY,
                CODE_COVERAGE_HISTORIC_DATA_DIRECTORY
            );

            // Skip if the compressed file does not exist
            if (!File.Exists(pathToConsolidated))
                return;

            try
            {
                // Extract files, but do not overrwrite files.
                ZipFile.ExtractToDirectory(
                    pathToConsolidated,
                    CODE_COVERAGE_HISTORIC_DATA_DIRECTORY, 
                    false);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
        }


        // Helper method to parse the date from the filename
        private static DateTime? ParseDateFromFileName(string filePath)
        {
            string fileName = Path.GetFileName(filePath);
            string dateString = fileName.Split('_')[0]; // Get the date part from the filename

            // Try to parse the date string
            if (DateTime.TryParseExact(dateString, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
            {
                string timeString = fileName.Split('_')[1]; // Get the time part from the filename
                string fullDateTimeString = dateString + " " + timeString.Replace('-', ':'); // Combine date and time strings

                if (DateTime.TryParseExact(fullDateTimeString, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fullDateTime))
                {
                    return fullDateTime;
                }
            }

            // Return null if parsing fails
            return null;
        }

        public static void Run()
        {
            ExtractLineCoverage();
            ConsolidateCoverageHistories();
            DecompressCoverageHistories();
        }
    }
}