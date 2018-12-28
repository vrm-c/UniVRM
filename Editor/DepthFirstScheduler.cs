using NUnit.Framework;
using System.Linq;


namespace DepthFirstScheduler
{
    public class DepthFirstScheduler
    {
        [Test]
        public void ScheduleTreeTest()
        {
            var s = Schedulable.Create();

            var tasks = s.GetRoot().Traverse().ToArray();
            Assert.AreEqual(2, tasks.Length);

            var task_int = s.AddTask(Scheduler.CurrentThread, () => 0);
            task_int = task_int.ContinueWith(Scheduler.CurrentThread, _ => 1);

            var status = s.Execute();
            Assert.AreEqual(ExecutionStatus.Done, status);
        }
    }
}
