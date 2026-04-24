using System.Collections;
using System.Diagnostics.CodeAnalysis;
using TreeDataStructures.Interfaces;

namespace TreeDataStructures.Core;

public abstract class BinarySearchTreeBase<TKey, TValue, TNode>(IComparer<TKey>? comparer = null) 
    : ITree<TKey, TValue>
    where TNode : Node<TKey, TValue, TNode>
{
    protected TNode? Root;
    public IComparer<TKey> Comparer { get; protected set; } = comparer ?? Comparer<TKey>.Default; // use it to compare Keys

    public int Count { get; protected set; }
    
    public bool IsReadOnly => false;

    public ICollection<TKey> Keys
    {
        get
        {
            List<TKey> keys = new List<TKey>(Count);
            foreach (var entry in InOrder())
            {
                keys.Add(entry.Key);
            }
            return keys;
        }
    }

    public ICollection<TValue> Values
    {
        get
        {
            List<TValue> values = new List<TValue>(Count);
            foreach (var entry in InOrder())
            {
                values.Add(entry.Value);
            }
            return values;
        }
    }
    
    
    public virtual void Add(TKey key, TValue value)
    {
        TNode? y = null;
        TNode? x = Root;
        int cmp = 0;
        while (x != null)
        {
            y = x;
            cmp = Comparer.Compare(key, x.Key);
            if (cmp == 0)
            {
                x.Value = value;
                return;
            }
            x = cmp < 0 ? x.Left : x.Right;
        }

        TNode newNode = CreateNode(key, value);
        newNode.Parent = y;
        if (y == null)
        {
            Root = newNode;
        }
        else if (cmp < 0)
        {
            y.Left = newNode;
        }
        else
        {
            y.Right = newNode;
        }

        Count++;
        OnNodeAdded(newNode);
    }

    public virtual bool Remove(TKey key)
    {
        TNode? node = FindNode(key);
        if (node == null) { return false; }

        RemoveNode(node);
        this.Count--;
        return true;
    }

    protected virtual void RemoveNode(TNode z)
    {
        TNode? x;
        TNode? parent;
        if (z.Left == null)
        {
            parent = z.Parent;
            x = z.Right;
            Transplant(z, z.Right);
        }
        else if (z.Right == null)
        {
            parent = z.Parent;
            x = z.Left;
            Transplant(z, z.Left);
        }
        else
        {
            TNode y = Minimum(z.Right);
            parent = y.Parent;
            x = y.Right;
            if (y.Parent != z)
            {
                Transplant(y, y.Right);
                y.Right = z.Right;
                y.Right.Parent = y;
            }
            else
            {
                parent = y;
            }
            Transplant(z, y);
            y.Left = z.Left;
            y.Left.Parent = y;
        }
        OnNodeRemoved(parent, x);
    }

    protected TNode Minimum(TNode node)
    {
        while (node.Left != null)
        {
            node = node.Left;
        }
        return node;
    }

    public virtual bool ContainsKey(TKey key) => FindNode(key) != null;
    
    public virtual bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        TNode? node = FindNode(key);
        if (node != null)
        {
            value = node.Value;
            return true;
        }
        value = default;
        return false;
    }

    public TValue this[TKey key]
    {
        get => TryGetValue(key, out TValue? val) ? val : throw new KeyNotFoundException();
        set => Add(key, value);
    }

    
    #region Hooks
    
    /// <summary>
    /// Вызывается после успешной вставки
    /// </summary>
    /// <param name="newNode">Узел, который встал на место</param>
    protected virtual void OnNodeAdded(TNode newNode) { }
    
    /// <summary>
    /// Вызывается после удаления. 
    /// </summary>
    /// <param name="parent">Узел, чей ребенок изменился</param>
    /// <param name="child">Узел, который встал на место удаленного</param>
    protected virtual void OnNodeRemoved(TNode? parent, TNode? child) { }
    
    #endregion
    
    
    #region Helpers
    protected abstract TNode CreateNode(TKey key, TValue value);
    
    
    protected TNode? FindNode(TKey key)
    {
        TNode? current = Root;
        while (current != null)
        {
            int cmp = Comparer.Compare(key, current.Key);
            if (cmp == 0) { return current; }
            current = cmp < 0 ? current.Left : current.Right;
        }
        return null;
    }

    protected void RotateLeft(TNode x)
    {
        TNode y = x.Right ?? throw new InvalidOperationException("Right child is null");
        x.Right = y.Left;
        if (y.Left != null)
        {
            y.Left.Parent = x;
        }
        y.Parent = x.Parent;
        if (x.Parent == null)
        {
            Root = y;
        }
        else if (x.IsLeftChild)
        {
            x.Parent.Left = y;
        }
        else
        {
            x.Parent.Right = y;
        }
        y.Left = x;
        x.Parent = y;
    }

    protected void RotateRight(TNode y)
    {
        TNode x = y.Left ?? throw new InvalidOperationException("Left child is null");
        y.Left = x.Right;
        if (x.Right != null)
        {
            x.Right.Parent = y;
        }
        x.Parent = y.Parent;
        if (y.Parent == null)
        {
            Root = x;
        }
        else if (y.IsLeftChild)
        {
            y.Parent.Left = x;
        }
        else
        {
            y.Parent.Right = x;
        }
        x.Right = y;
        y.Parent = x;
    }
    
    protected void RotateBigLeft(TNode x)
    {
        RotateLeft(x);
    }
    
    protected void RotateBigRight(TNode y)
    {
        RotateRight(y);
    }
    
    protected void RotateDoubleLeft(TNode x)
    {
        TNode? right = x.Right;
        if (right != null)
        {
            RotateRight(right);
        }
        RotateLeft(x);
    }
    
    protected void RotateDoubleRight(TNode y)
    {
        TNode? left = y.Left;
        if (left != null)
        {
            RotateLeft(left);
        }
        RotateRight(y);
    }
    
    protected void Transplant(TNode u, TNode? v)
    {
        if (u.Parent == null)
        {
            Root = v;
        }
        else if (u.IsLeftChild)
        {
            u.Parent.Left = v;
        }
        else
        {
            u.Parent.Right = v;
        }
        v?.Parent = u.Parent;
    }
    #endregion
    
    public TreeIterator InOrder() => new TreeIterator(Root, TraversalStrategy.InOrder);
    public TreeIterator PreOrder() => new TreeIterator(Root, TraversalStrategy.PreOrder);
    public TreeIterator PostOrder() => new TreeIterator(Root, TraversalStrategy.PostOrder);
    public TreeIterator InOrderReverse() => new TreeIterator(Root, TraversalStrategy.InOrderReverse);
    public TreeIterator PreOrderReverse() => new TreeIterator(Root, TraversalStrategy.PreOrderReverse);
    public TreeIterator PostOrderReverse() => new TreeIterator(Root, TraversalStrategy.PostOrderReverse);

    IEnumerable<TreeEntry<TKey, TValue>> ITree<TKey, TValue>.InOrder() => InOrder();
    IEnumerable<TreeEntry<TKey, TValue>> ITree<TKey, TValue>.PreOrder() => PreOrder();
    IEnumerable<TreeEntry<TKey, TValue>> ITree<TKey, TValue>.PostOrder() => PostOrder();
    IEnumerable<TreeEntry<TKey, TValue>> ITree<TKey, TValue>.InOrderReverse() => InOrderReverse();
    IEnumerable<TreeEntry<TKey, TValue>> ITree<TKey, TValue>.PreOrderReverse() => PreOrderReverse();
    IEnumerable<TreeEntry<TKey, TValue>> ITree<TKey, TValue>.PostOrderReverse() => PostOrderReverse();
    
    /// <summary>
    /// Внутренний класс-итератор для KeyValuePair.
    /// Реализует паттерн Iterator вручную, без yield return (ban).
    /// </summary>
    private struct TreePairIterator : IEnumerator<KeyValuePair<TKey, TValue>>
    {
        private TreeIterator _inner;

        public TreePairIterator(TNode? root)
        {
            _inner = new TreeIterator(root, TraversalStrategy.InOrder);
        }

        public KeyValuePair<TKey, TValue> Current => new(_inner.Current.Key, _inner.Current.Value);
        object IEnumerator.Current => Current;

        public bool MoveNext() => _inner.MoveNext();
        public void Reset() => _inner.Reset();
        public void Dispose() => _inner.Dispose();
    }

    /// <summary>
    /// Внутренний класс-итератор. 
    /// Реализует паттерн Iterator вручную, без yield return (ban).
    /// </summary>
    public struct TreeIterator :
        IEnumerable<TreeEntry<TKey, TValue>>,
        IEnumerator<TreeEntry<TKey, TValue>>
    {
        private readonly TNode? _root;
        private readonly TraversalStrategy _strategy;
        private Stack<(TNode node, int state, int depth)>? _stack;
        private TreeEntry<TKey, TValue> _current;
        private bool _started;

        public TreeIterator(TNode? root, TraversalStrategy strategy)
        {
            _root = root;
            _strategy = strategy;
            _stack = null;
            _current = default;
            _started = false;
        }
        
        public IEnumerator<TreeEntry<TKey, TValue>> GetEnumerator() => this;
        IEnumerator IEnumerable.GetEnumerator() => this;
        
        public TreeEntry<TKey, TValue> Current => _current;
        object IEnumerator.Current => Current;
        
        public bool MoveNext()
        {
            if (!_started)
            {
                _stack = new Stack<(TNode node, int state, int depth)>();
                if (_root != null)
                {
                    _stack.Push((_root, 0, 0));
                }
                _started = true;
            }

            if (_stack == null || _stack.Count == 0) return false;

            while (_stack.Count > 0)
            {
                var (node, state, depth) = _stack.Pop();

                switch (_strategy)
                {
                    case TraversalStrategy.InOrder:
                        if (state == 0)
                        {
                            _stack.Push((node, 1, depth));
                            if (node.Left != null) _stack.Push((node.Left, 0, depth + 1));
                        }
                        else if (state == 1)
                        {
                            _current = new TreeEntry<TKey, TValue>(node.Key, node.Value, depth);
                            if (node.Right != null) _stack.Push((node.Right, 0, depth + 1));
                            return true;
                        }
                        break;

                    case TraversalStrategy.PreOrder:
                        if (state == 0)
                        {
                            _current = new TreeEntry<TKey, TValue>(node.Key, node.Value, depth);
                            if (node.Right != null) _stack.Push((node.Right, 0, depth + 1));
                            if (node.Left != null) _stack.Push((node.Left, 0, depth + 1));
                            return true;
                        }
                        break;

                    case TraversalStrategy.PostOrder:
                        if (state == 0)
                        {
                            _stack.Push((node, 1, depth));
                            if (node.Right != null) _stack.Push((node.Right, 0, depth + 1));
                            if (node.Left != null) _stack.Push((node.Left, 0, depth + 1));
                        }
                        else if (state == 1)
                        {
                            _current = new TreeEntry<TKey, TValue>(node.Key, node.Value, depth);
                            return true;
                        }
                        break;

                    case TraversalStrategy.InOrderReverse:
                        if (state == 0)
                        {
                            _stack.Push((node, 1, depth));
                            if (node.Right != null) _stack.Push((node.Right, 0, depth + 1));
                        }
                        else if (state == 1)
                        {
                            _current = new TreeEntry<TKey, TValue>(node.Key, node.Value, depth);
                            if (node.Left != null) _stack.Push((node.Left, 0, depth + 1));
                            return true;
                        }
                        break;

                    case TraversalStrategy.PreOrderReverse:
                        if (state == 0)
                        {
                            _current = new TreeEntry<TKey, TValue>(node.Key, node.Value, depth);
                            if (node.Left != null) _stack.Push((node.Left, 0, depth + 1));
                            if (node.Right != null) _stack.Push((node.Right, 0, depth + 1));
                            return true;
                        }
                        break;

                    case TraversalStrategy.PostOrderReverse:
                        if (state == 0)
                        {
                            _stack.Push((node, 1, depth));
                            if (node.Left != null) _stack.Push((node.Left, 0, depth + 1));
                            if (node.Right != null) _stack.Push((node.Right, 0, depth + 1));
                        }
                        else if (state == 1)
                        {
                            _current = new TreeEntry<TKey, TValue>(node.Key, node.Value, depth);
                            return true;
                        }
                        break;
                }
            }

            return false;
        }
        
        public void Reset()
        {
            _stack = null;
            _started = false;
            _current = default;
        }

        public void Dispose()
        {
            _stack = null;
        }
    }
    
    
    public enum TraversalStrategy { InOrder, PreOrder, PostOrder, InOrderReverse, PreOrderReverse, PostOrderReverse }
    
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => new TreePairIterator(Root);
    
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


    public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);
    public void Clear() { Root = null; Count = 0; }
    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        TNode? node = FindNode(item.Key);
        if (node == null) return false;
        return EqualityComparer<TValue>.Default.Equals(node.Value, item.Value);
    }
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => throw new NotImplementedException();
    public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);
}