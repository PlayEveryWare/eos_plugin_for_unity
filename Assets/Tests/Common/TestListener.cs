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

using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using PlayEveryWare.EpicOnlineServices.Tests;
using UnityEngine.TestRunner;

[assembly: TestRunCallback(typeof(TestListener))]
public class TestListener : ITestRunCallback
{
    private const string HISTORIC_COVERAGE_PATH = "tools/coverage/historic/";

    public void RunStarted(ITest testsToRun)
    {
        // Called when the test run starts.
    }

    /// <summary>
    /// This method is called when the test run finishes.
    /// </summary>
    /// <param name="testResults">
    /// The results of the test run.
    /// </param>
    public void RunFinished(ITestResult testResults)
    {
        CodeCoverageHistoryCleaner.Run();
    }

    public void TestStarted(ITest test)
    {
        // Called when an individual test starts.
    }

    public void TestFinished(ITestResult result)
    {
        // Called when an individual test finishes.
    }
}
