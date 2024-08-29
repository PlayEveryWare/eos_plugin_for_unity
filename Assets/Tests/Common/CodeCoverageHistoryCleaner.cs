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
        private const string CODE_COVERAGE_CONSOLIDATED_FILE = "Consolidated_CoverageHistory.xml";

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
                string outputFilePath = Path.Combine(CODE_COVERAGE_HISTORIC_DATA_DIRECTORY, CODE_COVERAGE_CONSOLIDATED_FILE);

                // Get all CoverageHistory.xml files in the directory
                string[] coverageFiles = Directory.GetFiles(CODE_COVERAGE_HISTORIC_DATA_DIRECTORY, "*CoverageHistory.xml", SearchOption.AllDirectories);

                Dictionary<DateTime, XElement> lastEntryPerDay = new();

                // For each file
                foreach (string file in coverageFiles)
                {
                    XDocument doc = XDocument.Load(file);

                    var entries = doc.Descendants("coverage")
                        .Select(element => new
                        {
                            DateTime.ParseExact(element.Attribute("date")?.Value, CODE_COVERAGE_DATETIME_FORMAT, null).Date,
                            Element = element
                        });

                    // For each entry in the file
                    foreach (var entry in entries)
                    {
                        // If there already is an entry for the indicated date.
                        if (lastEntryPerDay.ContainsKey(entry.Date))
                        {
                            // Replace the entry if the current one happened
                            // later in the day.
                            var existingEntryTime = DateTime.ParseExact(lastEntryPerDay[entry.Date].Attribute("date")?.Value, CODE_COVERAGE_DATETIME_FORMAT, null);
                            var newEntryTime = DateTime.ParseExact(entry.Element.Attribute("date")?.Value, CODE_COVERAGE_DATETIME_FORMAT, null);

                            if (newEntryTime > existingEntryTime)
                            {
                                lastEntryPerDay[entry.Date] = entry.Element;
                            }
                        }
                        else
                        {
                            // If there isn't an entry for the indicated date,
                            // then add the current entry for the date.
                            lastEntryPerDay[entry.Date] = entry.Element;
                        }
                    }
                }

                // Create a new XML document for the consolidated history
                XElement root = new("CoverageHistory", lastEntryPerDay.Values);

                // Save the contents of the consolidated history. Omit any 
                // duplicate namespaces and disable formatting, since this file
                // should only ever be read by the code coverage utility itself.
                root.Save(outputFilePath, SaveOptions.DisableFormatting | SaveOptions.OmitDuplicateNamespaces);

                Debug.Log($"Consolidated CoverageHistory saved to {outputFilePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"CoverageHistoryCleaner failed: {ex.Message}");
            }
        }

        public static void Run()
        {
            ExtractLineCoverage();
            ConsolidateCoverageHistories();
        }
    }
}