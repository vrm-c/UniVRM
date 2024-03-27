
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
            public void RunStarted(ITestAdaptor testsToRun)
            {
            }

            public void RunFinished(ITestResultAdaptor result)
            {
                Debug.Log($"Failed Test Count: {result.FailCount}");
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
            }
        }
    }
}