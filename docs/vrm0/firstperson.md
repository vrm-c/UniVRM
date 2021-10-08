# VRMFirstPersonの使い方

{doc}`FirstPerson と Renderer の可視制御 </implementation/first_person>`

実行時に**VRMFirstPerson.Setup**を呼び出すことで、FirstPerson設定に応じた Renderer の可視制御を行うことができます。
明示的に外部から呼び出してください。

## アプリケーションに追加の描画レイヤーを指定する

定数で以下のレイヤーを定義しています。

```csharp
public class VRMFirstPerson : MonoBehaviour
{
    public const int FIRSTPERSON_ONLY_LAYER = 9;
    public const int THIRDPERSON_ONLY_LAYER = 10;

    // 省略
}
```

|{{< img src="images/vrm/layer_setting.png" >}}|
|-----|
|9番と１０番にLayerを設定|

## 実行時にSetupを呼び出して、カメラにLayerMaskを設定する

* VRMFirstPerson.Setupの呼び出し
* 一人称カメラとその他のカメラに対してLayerMask

```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRM;

public class SetupExample : MonoBehaviour
{
    [SerializeField]
    Camera m_firstPersonCamera; // HMDのカメラ

    [SerializeField]
    LayerMask m_firstPersonMask; // HMDのカメラにセットするマスク default | VRMFirstPersonOnly など

    [SerializeField]
    LayerMask m_otherMask; // HMDのカメラにセットするマスク default | VRMThirdPersonOnly など

    [SerializeField]
    VRMFirstPerson m_firstPerson;

    void Reset()
    {
        m_firstPerson = GameObject.FindObjectOfType<VRMFirstPerson>();
    }

    void Start()
    {
        foreach (var camera in GameObject.FindObjectsOfType<Camera>())
        {
            camera.cullingMask = (camera == m_firstPersonCamera)
                ? m_firstPersonMask
                : m_otherMask
                ;
        }

        // VRMFirstPersonの初期化
        if (m_firstPerson != null)
        {
            m_firstPerson.Setup();
        }
    }
}
```
