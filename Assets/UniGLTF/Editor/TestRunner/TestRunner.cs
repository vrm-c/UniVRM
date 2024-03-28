
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using NUnit.Framework.Interfaces;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
using TestStatus = UnityEditor.TestTools.TestRunner.Api.TestStatus;

namespace UniGLTF
{
    public static class TestRunner
    {
        public static void RunEditModeTests()
        {
            var testRunnerApi = ScriptableObject.CreateInstance<TestRunnerApi>();
            testRunnerApi.RegisterCallbacks(new TestCallback());
            testRunnerApi.Execute(new ExecutionSettings(new Filter
            {
                testMode = TestMode.EditMode,
            }));
        }

        private class TestCallback : ICallbacks
        {
            private static readonly string LogPrefix = $"[[TestRunnerLog]] ";
            private static readonly string ResultLogPrefix = $"[[TestRunnerResult]] ";

            private readonly string _xmlFilePath;
            private StackTraceLogType _tmpStackTraceLogType;

            public TestCallback()
            {
                if (!Application.isBatchMode) return;

                var arguments = System.Environment.GetCommandLineArgs();
                for (var idx = 0; idx < arguments.Length; idx++)
                {
                    if (arguments[idx] == "-testRunnerNUnitXmlFile" && idx + 1 < arguments.Length)
                    {
                        if (!arguments[idx + 1].StartsWith("-"))
                        {
                            _xmlFilePath = arguments[idx + 1];
                            break;
                        }
                    }
                }
            }

            public void RunStarted(ITestAdaptor testsToRun)
            {
                _tmpStackTraceLogType = Application.GetStackTraceLogType(LogType.Log);
                Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);

                Debug.Log($"{LogPrefix}Edit Mode Tests Started.");
            }

            public void RunFinished(ITestResultAdaptor result)
            {
                Debug.Log($"{LogPrefix}Edit Mode Tests Finished.");
                Debug.Log($"{LogPrefix}Passed: {result.PassCount}, Skipped: {result.SkipCount}, Failed: {result.FailCount}");
                Debug.Log($"{ResultLogPrefix}{result.FailCount}");

                Application.SetStackTraceLogType(LogType.Log, _tmpStackTraceLogType);

                if (Application.isBatchMode)
                {
                    if (!string.IsNullOrEmpty(_xmlFilePath))
                    {
                        Debug.Log($"{LogPrefix}Write NUnit XML to {_xmlFilePath}");
                        var xmlNode = CreateNUnitXmlTree(result);

                        using var xmlWriter = new XmlTextWriter(_xmlFilePath, Encoding.UTF8);
                        xmlWriter.Formatting = Formatting.Indented;
                        xmlWriter.WriteStartDocument();
                        xmlNode.WriteTo(xmlWriter);
                        xmlWriter.WriteEndDocument();
                    }
                    EditorApplication.Exit(result.FailCount > 0 ? 1 : 0);
                }
            }

            public void TestStarted(ITestAdaptor test)
            {
            }

            public void TestFinished(ITestResultAdaptor result)
            {
                if (result.HasChildren) return;

                if (result.TestStatus != TestStatus.Passed)
                {
                    Debug.Log($"{LogPrefix}{result.Message}");
                    Debug.Log($"{LogPrefix}{result.StackTrace}");
                }
            }

            /// <summary>
            /// https://forum.unity.com/threads/generating-nunit-compatible-xml-output.769757/
            /// </summary>
            private static TNode CreateNUnitXmlTree(ITestResultAdaptor result)
            {
                var testRunNode = new TNode("test-run");
                testRunNode.AddAttribute("id", "2");
                testRunNode.AddAttribute("testcasecount", (result.PassCount + result.FailCount + result.SkipCount + result.InconclusiveCount).ToString());
                testRunNode.AddAttribute("result", result.ResultState);
                testRunNode.AddAttribute("total", (result.PassCount + result.FailCount + result.SkipCount + result.InconclusiveCount).ToString());
                testRunNode.AddAttribute("passed", result.PassCount.ToString());
                testRunNode.AddAttribute("failed", result.FailCount.ToString());
                testRunNode.AddAttribute("inconclusive", result.InconclusiveCount.ToString());
                testRunNode.AddAttribute("skipped", result.SkipCount.ToString());
                testRunNode.AddAttribute("asserts", result.AssertCount.ToString());
                testRunNode.AddAttribute("engine-version", "3.5.0.0");
                testRunNode.AddAttribute("clr-version", System.Environment.Version.ToString());
                testRunNode.AddAttribute("start-time", result.StartTime.ToString("u"));
                testRunNode.AddAttribute("end-time", result.EndTime.ToString("u"));
                testRunNode.AddAttribute("duration", result.Duration.ToString(CultureInfo.InvariantCulture));

                testRunNode.ChildNodes.Add(result.ToXml());

                return testRunNode;
            }
        }
    }
}