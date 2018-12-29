using System;
using System.Collections.Generic;


namespace UniJSON
{
    public class RpcDispatcher<T> 
        where T : IListTreeItem, IValue<T>
    {
        delegate void Callback(int id, ListTreeNode<T> args, IRpc f);
        Dictionary<string, Callback> m_map = new Dictionary<string, Callback>();

        #region Action
        public void Register<A0>(string method, Action<A0> action)
        {
            m_map.Add(method, (id, args, f) =>
            {
                var it = args.ArrayItems().GetEnumerator();

                var a0 = default(A0);
                it.MoveNext();
                it.Current.Deserialize(ref a0);

                try
                {
                    action(a0);
                    f.ResponseSuccess(id);
                }
                catch(Exception ex)
                {
                    f.ResponseError(id, ex);
                }
            });
        }

        public void Register<A0, A1>(string method, Action<A0, A1> action)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Func
        public void Register<A0, A1, R>(string method, Func<A0, A1, R> action)
        {
            m_map.Add(method, (id, args, f) =>
            {
                var it = args.ArrayItems().GetEnumerator();

                var a0 = default(A0);
                it.MoveNext();
                it.Current.Deserialize(ref a0);

                var a1 = default(A1);
                it.MoveNext();
                it.Current.Deserialize(ref a1);

                try
                {
                    var r = action(a0, a1);
                    f.ResponseSuccess(id, r);
                }
                catch(Exception ex)
                {
                    f.ResponseError(id, ex);
                }
            });
        }
        #endregion

        public void Call(IRpc f, int id, string method, ListTreeNode<T> args)
        {
            Callback callback;
            if (!m_map.TryGetValue(method, out callback))
            {
                throw new KeyNotFoundException();
            }
            callback(id, args, f);
        }
    }
}
