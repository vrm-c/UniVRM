using System.Collections.Generic;


namespace UniJSON
{
    public interface ITreeNode<T, U> 
        where T : ITreeNode<T, U>
    {
        bool IsValid { get; }

        bool HasParent { get; }
        T Parent { get; }
        IEnumerable<T> Children { get; }

        int ValueIndex { get; }
        U Value { get; }
        void SetValue(U value);
    }

    public interface IListTreeItem
    {
        int ParentIndex { get; }
        int ChildCount { get; }
        void SetChildCount(int count);
    }
}
