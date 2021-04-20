using System;
using UnityEditor;

namespace UniGLTF
{
    public static class ValidationExtensions
    {
        public static void DrawGUI(this Validation self)
        {
            if (string.IsNullOrEmpty(self.Message))
            {
                return;
            }

            switch (self.ErrorLevel)
            {
                case ErrorLevels.Info:
                    EditorGUILayout.HelpBox(self.Message, MessageType.Info);
                    break;
                case ErrorLevels.Warning:
                    EditorGUILayout.HelpBox(self.Message, MessageType.Warning);
                    break;
                case ErrorLevels.Critical:
                case ErrorLevels.Error:
                    EditorGUILayout.HelpBox(self.Message, MessageType.Error);
                    break;

                default:
                    throw new NotImplementedException();
            }

            if (self.Extended != null)
            {
                self.Extended();
            }
        }
    }
}
