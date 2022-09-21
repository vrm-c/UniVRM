namespace UniVRM10
{
    public enum ControlRigGenerationOption
    {
        /// <summary>
        /// コントロールリグを生成しません。
        /// </summary>
        None,

        /// <summary>
        /// 推奨されるオプションです。
        /// コントロールリグのボーン Transform を生成し、Root の Animator はコントロールリグのボーンを制御するようになります。
        /// </summary>
        Generate,
    }
}