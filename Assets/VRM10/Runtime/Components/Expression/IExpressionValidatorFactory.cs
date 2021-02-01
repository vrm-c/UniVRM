namespace UniVRM10
{
    public interface IExpressionValidatorFactory
    {
        IExpressionValidator Create(VRM10ExpressionAvatar expressionAvatar);
    }
}