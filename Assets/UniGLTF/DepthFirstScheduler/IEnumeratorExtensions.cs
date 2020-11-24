using System;
using System.Collections;
using System.Collections.Generic;


namespace
    DepthFirstScheduler
{
    public static class IEnumeratorExtensions
    {
        [Obsolete("Use CoroutineToEnd")]
        public static void CoroutinetoEnd(this IEnumerator coroutine)
        {
            CoroutineToEnd(coroutine);
        }

        public static void CoroutineToEnd(this IEnumerator coroutine)
        {
            var stack = new Stack<IEnumerator>();
            stack.Push(coroutine);
            while (stack.Count > 0)
            {
                if (stack.Peek().MoveNext())
                {
                    var nested = stack.Peek().Current as IEnumerator;
                    if (nested != null)
                    {
                        stack.Push(nested);
                    }
                }
                else
                {
                    stack.Pop();
                }
            }
        }
    }
}
