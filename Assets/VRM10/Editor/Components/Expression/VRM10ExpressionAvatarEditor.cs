using UnityEditor;

namespace UniVRM10
{
    [CustomEditor(typeof(VRM10ExpressionAvatar))]
    public class ExpressionAvatarEditor : ExpressionEditorBase
    {
        /// <summary>
        /// ExpressionAvatar から 編集対象の Expression を選択する
        /// </summary>
        ExpressionClipSelector m_selector;
        ExpressionClipSelector Selector
        {
            get
            {
                if (m_selector == null)
                {
                    m_selector = new ExpressionClipSelector((VRM10ExpressionAvatar)target, serializedObject);
                    m_selector.Selected += OnSelected;
                    OnSelected(m_selector.GetSelected());
                }
                return m_selector;
            }
        }

        /// <summary>
        /// 選択中の Expression のエディタ
        /// </summary>
        SerializedExpressionEditor m_serializedEditor;

        protected override VRM10Expression CurrentExpression()
        {
            return Selector.GetSelected();
        }

        void OnSelected(VRM10Expression clip)
        {
            if (PreviewSceneManager == null)
            {
                m_serializedEditor = null;
                return;
            }

            if (clip != null)
            {
                // select clip
                var status = SerializedExpressionEditor.EditorStatus.Default;
                if (m_serializedEditor != null)
                {
                    status = m_serializedEditor.Status;
                }
                m_serializedEditor = new SerializedExpressionEditor(clip, PreviewSceneManager, status);
            }
            else
            {
                // clear selection
                m_serializedEditor = null;
                PreviewSceneManager.Bake(default, 1.0f);
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            // selector
            Selector.DrawGUI();

            // editor
            if (m_serializedEditor != null)
            {
                Separator();
                m_serializedEditor.Draw(out VRM10Expression bakeValue);
                PreviewSceneManager.Bake(bakeValue, 1.0f);
            }
        }
    }
}
