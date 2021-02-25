using System;

namespace UniGLTF.AltTask
{
    public static class NextFrameAwaitable
    {
        // TODO
        // loop スレッド使わないようにしたい
        public static Awaitable Create()
        {
            return Awaitable.Run(() =>
            {
                System.Threading.Thread.Sleep(10);
            });
        }
    }
}
