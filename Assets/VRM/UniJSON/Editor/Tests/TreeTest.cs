using NUnit.Framework;
using System;
using System.Collections.Generic;


namespace GenericTree
{
    /// <summary>
    /// Generic tree interface
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    interface ITreeNode<T, U>
        where T : ITreeNode<T, U>
    {
        bool IsValid { get; }
        int ValueIndex { get; }
        U Value { get; }

        bool HasParent { get; }
        T Parent { get; }

        IEnumerable<T> Children { get; }
    }

    /// <summary>
    /// Item has parent reference by list index
    /// </summary>
    public interface ITreeItem
    {
        int ParentIndex { get; }
    }

    /// <summary>
    /// Generic tree implementation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    struct TreeNode<T> : ITreeNode<TreeNode<T>, T>
        where T : ITreeItem
    {
        /// <summary>
        /// Whole tree ndoes
        /// </summary>
        public readonly List<T> Values;

        public bool IsValid
        {
            get
            {
                return Values != null;
            }
        }

        /// <summary>
        /// This node index
        /// </summary>
        public int ValueIndex
        {
            get;
            private set;
        }

        public T Value
        {
            get
            {
                if (Values == null)
                {
                    return default(T);
                }
                return Values[ValueIndex];
            }
        }

        public IEnumerable<TreeNode<T>> Children
        {
            get
            {
                for (int i = 0; i < Values.Count; ++i)
                {
                    if (Values[i].ParentIndex == ValueIndex)
                    {
                        yield return new TreeNode<T>(Values, i);
                    }
                }
            }
        }

        public bool HasParent
        {
            get
            {
                return Value.ParentIndex >= 0 && Value.ParentIndex < Values.Count;
            }
        }

        public TreeNode<T> Parent
        {
            get
            {
                if (Value.ParentIndex < 0)
                {
                    throw new Exception("this may root node");
                }
                if (Value.ParentIndex >= Values.Count)
                {
                    throw new IndexOutOfRangeException();
                }
                return new TreeNode<T>(Values, Value.ParentIndex);
            }
        }

        public TreeNode(List<T> values, int index) : this()
        {
            Values = values;
            ValueIndex = index;
        }
    }


    class TreeTests
    {
        [Test]
        public void TreeTest()
        {

        }
    }
}
