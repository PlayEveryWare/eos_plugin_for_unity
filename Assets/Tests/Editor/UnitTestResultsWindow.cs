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
    using EpicOnlineServices.Editor.Windows;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using UnityEditor;
    using UnityEngine.UIElements;

    public class UnitTestResultsWindow : EOSEditorWindow
    {
        private const string UXMLPath = "Assets/Tests/Editor/";
        private const int DefaultListHeight = 16;
        private readonly char[] _unicodeResultText = { 'O', '\u2713', 'X' };

        private readonly string[] _resultStyleColor = { "itemCheck--skip", "itemCheck--pass", "itemCheck--fail" };

        public UnitTestResultsWindow() : base("EOS Unit Test Results") { }

        private enum TestResult
        {
            Skipped = 0,
            Passed = 1,
            Failed = 2,
        }

        private class TestResultData
        {
            public TestResult result;
            public string className;
            public string testName;
            public string failureMessage;
            public string stackTrace;
            public string output;
            public float duration;
        }

        private int _testsPassed, _testFailed, _testSkipped;
        private float _totalDuration;
        private List<TestResultData> _results = new();

        private ListView _listView;
        private Label _successValueLabel;
        private Label _failValueLabel;
        private Label _skipValueLabel;
        private Label _totalDurationLabel;
        private TextField _loggingText;

        [MenuItem("Tools/EOS Plugin/Tests/Result Parser", priority = 11)]
        public static void OpenUnitTestResultsWindow()
        {
            var window = GetWindow<UnitTestResultsWindow>();
            window.SetIsEmbedded(false);
        }

        protected override void Setup()
        {
            SetAutoResize(false);
            base.Setup();

            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{UXMLPath}EOSResultParserEditor.uxml");
            visualTree.CloneTree(root);

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>($"{UXMLPath}EOSResultParserEditor.uss");
            root.styleSheets.Add(styleSheet);

            // Cache the stats labels and message log
            _successValueLabel = root.Q<Label>("PassValue");
            _failValueLabel = root.Q<Label>("FailedValue");
            _skipValueLabel = root.Q<Label>("SkippedValue");
            _loggingText = root.Q<TextField>("TestLogging");
            _totalDurationLabel = root.Q<Label>("TotalDurationValue");

            _loggingText.isReadOnly = true;

            Button loadButton = rootVisualElement.Q<Button>("LoadResults");
            loadButton.clickable.clicked += () =>
            {
                var filename =
                    EditorUtility.OpenFilePanelWithFilters("Results file", "", new string[] { "XML", "xml" });
                if (!string.IsNullOrWhiteSpace(filename))
                {
                    rootVisualElement.Q<Label>("Filename").text = filename;
                    ParseTestResultsFile(filename);

                    _successValueLabel.text = _testsPassed.ToString();
                    _failValueLabel.text = _testFailed.ToString();
                    _skipValueLabel.text = _testSkipped.ToString();
                    _totalDurationLabel.text = $"{_totalDuration}s";
                    _listView.Rebuild();
                }
            };

            // Setup the list view so it displays the results properly and allows selection
            var itemTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{UXMLPath}EOSResultParserEditorItem.uxml");

            _listView = new ListView(_results, DefaultListHeight, MakeItem, BindItem)
            {
                name = "ListView",
                selectionType = SelectionType.Single
            };
            _listView.onSelectionChange += obj => DisplayTestLog((TestResultData)obj.First());
            _listView.style.flexGrow = 1.0f;

            var topPanel = rootVisualElement.Q<VisualElement>("TopPanel");
            topPanel.Add(_listView);
            return;

            void BindItem(VisualElement e, int i)
            {
                var check = e.Q<Label>("ItemCheck");

                for (int j = 0; j < _resultStyleColor.Length; j++)
                {
                    check.EnableInClassList(_resultStyleColor[j], (int)_results[i].result == j);
                }

                check.text = _unicodeResultText[(int)_results[i].result].ToString();

                var label = e.Q<Label>("ItemLabel");
                label.text = $"{_results[i].className}:{_results[i].testName}";
            }

            VisualElement MakeItem() => itemTree.CloneTree();
        }

        protected override void RenderWindow()
        {
            // Set the container height to the window
            var container = rootVisualElement.Q<VisualElement>("Container");
            if (container != null)
            {
                container.style.height = new StyleLength(position.height);
            }
        }

        /// <summary>
        /// Opens the specified XML result file and parses it into an easier to read format.
        /// </summary>
        /// <param name="filename"></param>
        private void ParseTestResultsFile(string filename)
        {
            _results.Clear();
            _testsPassed = 0;
            _testFailed = 0;
            _testSkipped = 0;

            var xml = XDocument.Load(filename);
            string s = xml.Element("test-suite")?.Attribute("duration")?.Value;
            if (s == null)
            {
                return;
            }

            _totalDuration = float.Parse(s);

            var query = from item in xml.Descendants("test-case") select item;
            foreach (XElement element in query)
            {
                var resultData = new TestResultData();

                // Get the name of the test by the class and the test name
                var splitClassName = element.Attribute("classname")?.Value.Split('.');
                resultData.testName = element.Attribute("name")?.Value;
                resultData.className = (splitClassName ?? Array.Empty<string>()).Last();

                string value = element.Attribute("duration")?.Value;
                if (value != null)
                {
                    resultData.duration = float.Parse(value);

                    var resultValue = element.Attribute("result")?.Value;
                    resultData.result = TestResult.Skipped;
                    switch (resultValue)
                    {
                        case "Passed":
                            _testsPassed++;
                            resultData.result = TestResult.Passed;
                            break;
                        case "Failed":
                            _testFailed++;
                            resultData.result = TestResult.Failed;
                            break;
                        default:
                            _testSkipped++;
                            break;
                    }
                }

                var failureElement = element.Element("failure");
                if (failureElement != null)
                {
                    resultData.failureMessage = GetElementString(failureElement, "message");
                    resultData.stackTrace = GetElementString(failureElement, "stack-trace");
                }

                resultData.output = GetElementString(element, "output");

                _results.Add(resultData);
            }
        }

        /// <summary>
        /// Get the string value of the specified element name that's a child of the provided root element.
        /// </summary>
        /// <param name="root">Root <see cref="XElement"/> where the child element name is located.</param>
        /// <param name="elementName">Element name to get under the root.</param>
        /// <returns>The string value the element contains.</returns>
        private static string GetElementString(XElement root, string elementName)
        {
            string result = string.Empty;

            var element = root.Elements().FirstOrDefault(x => x.Name.LocalName == elementName);
            if (element != null)
            {
                result = element.Value;
            }

            return result;
        }

        /// <summary>
        /// Display the logging information into the text field at the bottom.
        /// </summary>
        /// <param name="data"><see cref="TestResultData"/> containing logging information.</param>
        private void DisplayTestLog(TestResultData data)
        {
            string log = string.Empty;

            log += $"{data.testName} ({data.duration}s)\n---\n";

            if (data.result == TestResult.Failed)
            {
                log += data.failureMessage.Trim() + "\n---\n";
            }

            log += data.output;
            _loggingText.SetValueWithoutNotify(log);
        }
    }
}