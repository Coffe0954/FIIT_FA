using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.AVL;

public class AvlTree<TKey, TValue> : BinarySearchTreeBase<TKey, TValue, AvlNode<TKey, TValue>>
    where TKey : IComparable<TKey>
{
    protected override AvlNode<TKey, TValue> CreateNode(TKey key, TValue value)
        => new(key, value);
    
    protected override void OnNodeAdded(AvlNode<TKey, TValue> newNode)
    {
        Balance(newNode);
    }

    protected override void OnNodeRemoved(AvlNode<TKey, TValue>? parent, AvlNode<TKey, TValue>? child)
    {
        if (parent != null)
        {
            Balance(parent);
        }
    }

    private void Balance(AvlNode<TKey, TValue> node)
    {
        while (node != null)
        {
            UpdateHeight(node);
            int balance = GetBalance(node);

            if (balance > 1)
            {
                if (GetBalance(node.Left) < 0)
                {
                    RotateLeft(node.Left!);
                    UpdateHeight(node.Left.Left!);
                    UpdateHeight(node.Left!);
                }
                RotateRight(node);
                UpdateHeight(node.Parent!.Right!);
                UpdateHeight(node.Parent!);
                node = node.Parent;
            }
            else if (balance < -1)
            {
                if (GetBalance(node.Right) > 0)
                {
                    RotateRight(node.Right!);
                    UpdateHeight(node.Right.Right!);
                    UpdateHeight(node.Right!);
                }
                RotateLeft(node);
                UpdateHeight(node.Parent!.Left!);
                UpdateHeight(node.Parent!);
                node = node.Parent;
            }

            if (node.Parent == null) break;
            node = node.Parent;
        }
    }

    private int GetHeight(AvlNode<TKey, TValue>? node) => node?.Height ?? 0;

    private int GetBalance(AvlNode<TKey, TValue>? node)
        => node == null ? 0 : GetHeight(node.Left) - GetHeight(node.Right);

    private void UpdateHeight(AvlNode<TKey, TValue> node)
    {
        node.Height = 1 + Math.Max(GetHeight(node.Left), GetHeight(node.Right));
    }

    
}