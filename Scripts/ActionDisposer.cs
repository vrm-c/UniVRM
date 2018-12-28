using System;


namespace UniJSON
{
    public struct ActionDisposer : IDisposable
    {
        Action m_action;

        public ActionDisposer(Action action)
        {
            m_action = action;
        }

        public void Dispose()
        {
            m_action();
        }
    }
}
