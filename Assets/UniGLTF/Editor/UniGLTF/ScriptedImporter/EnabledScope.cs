using System;
using UnityEngine;

namespace UniGLTF
{
    public class EnabledScope : IDisposable
    {
        bool m_backup;
        public EnabledScope()
        {
            m_backup = GUI.enabled;
            GUI.enabled = true;
        }

        public void Dispose()
        {
            GUI.enabled = m_backup;
        }
    }
}
