using UnityEditor;
using UnityEngine;

namespace UniVRM10
{
    [CustomPropertyDrawer(typeof(VRM10SpringBoneCollider))]
    public class VRM10SpringBoneColliderDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            const string ButtonLabel = "Reselect";
            var ButtonSize = GUI.skin.button.CalcSize(new GUIContent(ButtonLabel));
            try
            {
                // オブジェクトフィールドを表示
                var collider = property.objectReferenceValue as VRM10SpringBoneCollider;
                EditorGUI.ObjectField(rect, property, new GUIContent(collider.GetIdentificationName()));

                // 選択中のトランスフォームに複数のコライダが含まれる場合はリセレクトボタンを表示
                var colliders = collider.transform.GetComponents<VRM10SpringBoneCollider>();
                if (colliders.Length > 1)
                {
                    if (GUI.Button(new Rect(rect.x + EditorGUIUtility.labelWidth - ButtonSize.x, rect.y, ButtonSize.x, rect.height), ButtonLabel))
                    {
                        // リセレクト用ウィンドウの呼び出し
                        VRM10SpringBoneColliderEditorWindow.Show(property);
                    }
                }
            }
            catch
            {
                // オブジェクトフィールドを表示
                EditorGUI.ObjectField(rect, property, new GUIContent());
            }
        }
    }

    public class VRM10SpringBoneColliderEditorWindow : EditorWindow
    {
        private static SerializedProperty targetProperty;
        private Vector2 scrollPosition;

        public static void Show(SerializedProperty property)
        {
            // プロパティを保存
            targetProperty = property;

            // タイトルを設定して補助ウィンドウとして開く
            var window = CreateInstance<VRM10SpringBoneColliderEditorWindow>();
            window.titleContent = new GUIContent("Reselect Collider from \"" + (property.objectReferenceValue as VRM10SpringBoneCollider).transform.name + "\"");
            window.ShowAuxWindow();
        }

        private void OnGUI()
        {
            // プロパティが無い場合はウィンドウを閉じる
            if (targetProperty is null)
            {
                Close();
                return;
            }

            // コライダが無い場合はウィンドウを閉じる
            var transform = (targetProperty.objectReferenceValue as VRM10SpringBoneCollider).transform;
            var colliders = transform.GetComponents<VRM10SpringBoneCollider>();
            if (colliders.Length == 0)
            {
                Close();
                return;
            }

            // スクロール可能なウィンドウにコライダの数だけボタンを表示
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            foreach (var collider in colliders)
            {
                if (GUILayout.Button(collider.GetIdentificationName()))
                {
                    // プロパティを選択されたコライダに置き換える
                    targetProperty.objectReferenceValue = collider;
                    targetProperty.serializedObject.ApplyModifiedProperties();
                    Close();
                }
            }
            GUILayout.EndScrollView();
        }
    }
}