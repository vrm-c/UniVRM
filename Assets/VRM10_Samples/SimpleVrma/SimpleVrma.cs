using UniGLTF;
using UnityEngine;
using UniVRM10;
using UniVRM10.VRM10Viewer;
using VRMShaders;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SimpleVrma : MonoBehaviour
{
    public Vrm10Instance Vrm;

    public Vrm10AnimationInstance Vrma;

    bool m_boxman = true;

    void OnGUI()
    {
#if UNITY_EDITOR
        if (GUILayout.Button("open vrm"))
        {
            var path = EditorUtility.OpenFilePanel("open vrm", "", "vrm");
            Debug.Log(path);
            LoadVrm(path);
        }

        if (GUILayout.Button("open vrma"))
        {
            var path = EditorUtility.OpenFilePanel("open vrma", "", "vrma");
            Debug.Log(path);
            LoadVrma(path);
        }
#endif

        m_boxman = GUILayout.Toggle(m_boxman, "BoxMan");
        if (Vrma != null)
        {
            Vrma.BoxMan.enabled = m_boxman;
        }
    }

    async void LoadVrm(string path)
    {
        Vrm = await Vrm10.LoadPathAsync(path,
            canLoadVrm0X: true,
            showMeshes: false,
            awaitCaller: new ImmediateCaller());

        var instance = Vrm.GetComponent<RuntimeGltfInstance>();
        instance.ShowMeshes();
    }

    async void LoadVrma(string path)
    {
        // load vrma
        using GltfData data = new AutoGltfFileParser(path).Parse();
        using var loader = new VrmAnimationImporter(data);
        var instance = await loader.LoadAsync(new ImmediateCaller());

        Vrma = instance.GetComponent<Vrm10AnimationInstance>();
        Vrm.Runtime.VrmAnimation = Vrma;
        Debug.Log(Vrma);

        var animation = Vrma.GetComponent<Animation>();
        animation.Play();
    }
}
