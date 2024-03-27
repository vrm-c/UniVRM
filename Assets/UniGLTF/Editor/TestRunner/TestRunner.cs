
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

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
            private StackTraceLogType _tmpStackTraceLogType;

            public void RunStarted(ITestAdaptor testsToRun)
            {
                _tmpStackTraceLogType = Application.GetStackTraceLogType(LogType.Log);
                Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);

                Debug.Log($"{LogPrefix}Edit Mode Tests Started.");
            }

            public void RunFinished(ITestResultAdaptor result)
            {
                Debug.Log($"{LogPrefix}Passed: {result.PassCount}, Skipped: {result.SkipCount}, Failed: {result.FailCount}");
                Debug.Log($"{ResultLogPrefix}{result.FailCount}");

                Application.SetStackTraceLogType(LogType.Log, _tmpStackTraceLogType);

                if (Application.isBatchMode)
                {
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
        }
    }
}