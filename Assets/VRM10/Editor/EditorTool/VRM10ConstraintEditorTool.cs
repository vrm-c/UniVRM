using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;


[EditorTool("vrm-1.0/Constraint", typeof(UniVRM10.VRM10Controller))]
class VRM10ConstraintEditorTool : EditorTool
{
    static GUIContent s_cachedIcon;
    // NOTE: as were caching this, unity will serialize it between compiles! so if we want to test out new looks,
    // just return the new GUIContent and bypass the cache until were happy with the icon...
    public override GUIContent toolbarIcon
    {
        get
        {
            if (s_cachedIcon == null)
            {
                s_cachedIcon = EditorGUIUtility.IconContent("RelativeJoint2D Icon", "|vrm-1.0 Constraint");
            }
            return s_cachedIcon;
        }
    }

    // This is called for each window that your tool is active in. Put the functionality of your tool here.
    public override void OnToolGUI(EditorWindow window)
    {
        EditorGUI.BeginChangeCheck();

        Vector3 position = Tools.handlePosition;

        using (new Handles.DrawingScope(Color.green))
        {
            position = Handles.Slider(position, Vector3.right);
        }

        if (EditorGUI.EndChangeCheck())
        {
            Vector3 delta = position - Tools.handlePosition;

            Undo.RecordObjects(Selection.transforms, "Move Platform");

            foreach (var transform in Selection.transforms)
                transform.position += delta;
        }
    }
}