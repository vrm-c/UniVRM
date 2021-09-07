---
title: "How to Use VRMFirstPerson"
linkTitle: "How to use first-person mode "
date: 2018-05-29T10:00:00+09:00
aliases: ["/en/dev/univrm-0.xx/programming/univrm_use_firstperson/"]
weight: 5
tags: ["api"]
---

# VRMFirstPerson Settings
[VRMFirstPerson]({{< relref "univrm_firstperson.md" >}}) has the following settings for Renderer:

|FirstPersonFlag               |Layer               |Note                                       |
|------------------------------|----------------------|--------------------------------------------|
|Both                          |default               |Specify parts that are not necessarily separated between first-person view and third-person view.|
|ThirdPersonOnly               |VRMThirdPersonOnly|Specify parts that are not rendered in first-person view.|
|FirstPersonOnly               |VRMFirstPersonOnly|Specify parts that are not rendered in third-person view. The auto-created headless model is used.|
|Auto                          |VRMThirdPersonOnly|Automatically create the model in first-person view at runtime and set it to FIRSTPERSON_ONLY_LAYER.|

By calling **VRMFirstPerson.Setup** at runtime, the layer settings described above can be performed. Please call the function explicitly from outside.

# Specify the additional render layers for the application

The following layers are defined as constant:

{{< highlight cs >}}
public class VRMFirstPerson : MonoBehaviour
{
    public const int FIRSTPERSON_ONLY_LAYER = 9;
    public const int THIRDPERSON_ONLY_LAYER = 10;

    // The following parts are omitted
}
{{< / highlight >}}

|{{< img src="images/vrm/layer_setting.png" >}}|
|-----|
|Set Layer in #9 and #10|

# Call Setup function at runtime and set LayerMask in Camera

* Call VRMFirstPerson.Setup
* Set LayerMask for first-person camera view and other camera views

{{< highlight cs >}}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRM;

public class SetupExample : MonoBehaviour
{
    [SerializeField]
    Camera m_firstPersonCamera; // HMD camera
    
    [SerializeField]
    LayerMask m_firstPersonMask; // Set a first-person mask (default | VRMFirstPersonOnly, etc.) in HMD camera

    [SerializeField]
    LayerMask m_otherMask; // Set other masks (default | VRMThirdPersonOnly, etc.) in HMD camera

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

        // VRMFirstPerson initialization
        if (m_firstPerson != null)
        {
            m_firstPerson.Setup();
        }
    }
}
{{< / highlight >}}
