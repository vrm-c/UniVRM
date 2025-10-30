namespace VRM
{
    /// <summary>
    /// 
    /// * OnImported(Serializableなメンバの初期化)とStart(ランタイムのメンバ初期化)で
    /// セットアップが終わるように注意する。
    /// 
    /// * Reset, Awake, OnEnableには注意する(Editor時はAddComponentで呼ばれ、Runtimeでは開始時に呼ばれる)
    /// 
    /// </summary>
    public interface IVRMComponent
    {
        /// <summary>
        /// Serializableな値を初期化する
        /// </summary>
        /// <param name="imported"></param>
        void OnImported(VRMImporterContext context);
    }
}
