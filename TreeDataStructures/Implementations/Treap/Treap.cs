using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.Treap;

public class Treap<TKey, TValue> : BinarySearchTreeBase<TKey, TValue, TreapNode<TKey, TValue>>
{
    /// <summary>
    /// Разрезает дерево с корнем <paramref name="root"/> на два поддерева:
    /// Left: все ключи <= <paramref name="key"/>
    /// Right: все ключи > <paramref name="key"/>
    /// </summary>
    protected virtual (TreapNode<TKey, TValue>? Left, TreapNode<TKey, TValue>? Right) Split(TreapNode<TKey, TValue>? root, TKey key)
    {
        if (root == null) return (null, null);

        if (Comparer.Compare(root.Key, key) <= 0)
        {
            var (left, right) = Split(root.Right, key);
            root.Right = left;
            if (left != null) left.Parent = root;
            if (right != null) right.Parent = null;
            return (root, right);
        }
        else
        {
            var (left, right) = Split(root.Left, key);
            root.Left = right;
            if (right != null) right.Parent = root;
            if (left != null) left.Parent = null;
            return (left, root);
        }
    }

    protected virtual TreapNode<TKey, TValue>? Merge(TreapNode<TKey, TValue>? left, TreapNode<TKey, TValue>? right)
    {
        if (left == null) return right;
        if (right == null) return left;

        if (left.Priority > right.Priority)
        {
            left.Right = Merge(left.Right, right);
            if (left.Right != null) left.Right.Parent = left;
            return left;
        }
        else
        {
            right.Left = Merge(left, right.Left);
            if (right.Left != null) right.Left.Parent = right;
            return right;
        }
    }

    public override void Add(TKey key, TValue value)
    {
        TreapNode<TKey, TValue>? existing = FindNode(key);
        if (existing != null)
        {
            existing.Value = value;
            return;
        }

        var (left, right) = Split(Root, key);
        TreapNode<TKey, TValue> newNode = CreateNode(key, value);
        Root = Merge(Merge(left, newNode), right);
        if (Root != null) Root.Parent = null;
        Count++;
        OnNodeAdded(newNode);
    }

    public override bool Remove(TKey key)
    {
        TreapNode<TKey, TValue>? node = FindNode(key);
        if (node == null) return false;

        TreapNode<TKey, TValue>? merged = Merge(node.Left, node.Right);
        Transplant(node, merged);
        if (merged != null) merged.Parent = node.Parent;

        Count--;
        OnNodeRemoved(node.Parent, merged);
        return true;
    }

    protected override TreapNode<TKey, TValue> CreateNode(TKey key, TValue value)
    {
        return new TreapNode<TKey, TValue>(key, value);
    }
    protected override void OnNodeAdded(TreapNode<TKey, TValue> newNode)
    {
    }
    
    protected override void OnNodeRemoved(TreapNode<TKey, TValue>? parent, TreapNode<TKey, TValue>? child)
    {
    }
    
}