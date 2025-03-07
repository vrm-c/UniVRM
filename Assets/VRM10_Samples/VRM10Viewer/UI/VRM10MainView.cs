using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UniVRM10.VRM10Viewer
{
    public class VRM10MainView : MonoBehaviour
    {
        [Header("Material")]
        [SerializeField]
        Material m_pbrOpaqueMaterial = default;
        [SerializeField]
        Material m_pbrAlphaBlendMaterial = default;
        [SerializeField]
        Material m_mtoonMaterialOpaque = default;
        [SerializeField]
        Material m_mtoonMaterialAlphaBlend = default;

        VRM10ViewerController m_controller;

        void OnEnable()
        {
            m_controller = new VRM10ViewerController(
                (m_mtoonMaterialOpaque != null && m_mtoonMaterialAlphaBlend != null) ? new TinyMToonrMaterialImporter(m_mtoonMaterialOpaque, m_mtoonMaterialAlphaBlend) : null,
                (m_pbrOpaqueMaterial != null && m_pbrAlphaBlendMaterial != null) ? new TinyPbrMaterialImporter(m_pbrOpaqueMaterial, m_pbrAlphaBlendMaterial) : null
            );
        }

        void OnDisable()
        {
            m_controller.Dispose();
        }
    }
}