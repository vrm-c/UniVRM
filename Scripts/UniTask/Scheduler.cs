namespace UniTask
{
    public interface IScheduler
    {
        void Enqueue(TaskChain item);
    }
}
