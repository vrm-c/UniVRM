using System;

namespace VrmLib
{
    [Serializable]
    public struct ExportArgs
    {
        /// <summary>
        /// 頂点バッファにsparse機能を使うか
        ///
        /// struct で初期値をdefault以外にするために
        /// nullableなpropertyを使っている
        /// </summary>
        bool? m_sparse;

        public bool sparse
        {
            get
            {
                if (!m_sparse.HasValue)
                {
                    m_sparse = true;
                }
                return m_sparse.Value;
            }
            set
            {
                m_sparse = value;
            }
        }

        /// <summary>
        /// エクスポート時にmorphTargetから法線を削除するか
        ///
        /// struct で初期値をdefault以外にするために
        /// nullableなpropertyを使っている
        /// </summary>
        bool? m_remove_morph_normal;

        public bool removeMorphNormal
        {
            get
            {
                if (!m_remove_morph_normal.HasValue)
                {
                    // TODO: Importerの修正が取り込まれたらtrueにする
                    m_remove_morph_normal = false;
                }
                return m_remove_morph_normal.Value;
            }
            set
            {
                m_remove_morph_normal = value;
            }
        }

        /// <summary>
        /// エクスポート時にtangentを削除するか
        ///
        /// struct で初期値をdefault以外にするために
        /// nullableなpropertyを使っている
        /// </summary>
        bool? m_remove_tangent;

        public bool removeTangent
        {
            get
            {
                if (!m_remove_tangent.HasValue)
                {
                    m_remove_tangent = true;
                }
                return m_remove_tangent.Value;
            }
            set
            {
                m_remove_tangent = value;
            }
        }
    }
}
