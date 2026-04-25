using System.Diagnostics.CodeAnalysis;
using TreeDataStructures.Implementations.BST;

namespace TreeDataStructures.Implementations.Splay;

public class SplayTree<TKey, TValue> : BinarySearchTree<TKey, TValue>
{
    protected override BstNode<TKey, TValue> CreateNode(TKey key, TValue value)
        => new(key, value);
    
    protected override void OnNodeAdded(BstNode<TKey, TValue> newNode)
    {
        Splay(newNode);
    }
    
    protected override void OnNodeRemoved(BstNode<TKey, TValue>? parent, BstNode<TKey, TValue>? child)
    {
        if (parent != null)
        {
            Splay(parent);
        }
    }
    
    public override bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        BstNode<TKey, TValue>? node = FindNode(key);
        if (node != null)
        {
            Splay(node);
            value = node.Value;
            return true;
        }
        value = default;
        return false;
    }

    public override bool ContainsKey(TKey key)
    {
        BstNode<TKey, TValue>? node = FindNode(key);
        if (node != null)
        {
            Splay(node);
            return true;
        }
        return false;
    }

    private void Splay(BstNode<TKey, TValue> x)
    {
        while (x.Parent != null)
        {
            if (x.Parent.Parent == null)
            {
                if (x.IsLeftChild)
                {
                    RotateRight(x.Parent);
                }
                else
                {
                    RotateLeft(x.Parent);
                }
            }
            else if (x.IsLeftChild && x.Parent.IsLeftChild)
            {
                RotateRight(x.Parent.Parent);
                RotateRight(x.Parent);
            }
            else if (x.IsRightChild && x.Parent.IsRightChild)
            {
                RotateLeft(x.Parent.Parent);
                RotateLeft(x.Parent);
            }
            else if (x.IsLeftChild && x.Parent.IsRightChild)
            {
                RotateRight(x.Parent);
                RotateLeft(x.Parent);
            }
            else
            {
                RotateLeft(x.Parent);
                RotateRight(x.Parent);
            }
        }
        Root = x;
    }
    
}
