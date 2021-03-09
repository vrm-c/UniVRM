namespace UniGLTF
{
    /// <summary>
    /// 所有権を移動する関数。
    /// 
    /// * 所有権が移動する。return true => ImporterContext.Dispose の対象から外れる
    /// * 所有権が移動しない。return false => Importer.Context.Dispose でDestroyされる
    /// 
    /// </summary>
    /// <param name="o">対象のオブジェクト</param>
    /// <returns>所有権が移動したらtrue</returns>
    public delegate bool TakeOwnershipFunc(UnityEngine.Object o);
}
