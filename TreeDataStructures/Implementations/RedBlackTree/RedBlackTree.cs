using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.RedBlackTree;

public class RedBlackTree<TKey, TValue> : BinarySearchTreeBase<TKey, TValue, RbNode<TKey, TValue>>
{
    protected override RbNode<TKey, TValue> CreateNode(TKey key, TValue value)
    {
        return new RbNode<TKey, TValue>(key, value);
    }
    
    protected override void OnNodeAdded(RbNode<TKey, TValue> z)
    {
        while (z.Parent is { Color: RbColor.Red })
        {
            if (z.Parent.IsLeftChild)
            {
                RbNode<TKey, TValue>? y = z.Parent.Parent?.Right;
                if (y is { Color: RbColor.Red })
                {
                    z.Parent.Color = RbColor.Black;
                    y.Color = RbColor.Black;
                    z.Parent.Parent!.Color = RbColor.Red;
                    z = z.Parent.Parent;
                }
                else
                {
                    if (z.IsRightChild)
                    {
                        z = z.Parent;
                        RotateLeft(z);
                    }
                    z.Parent!.Color = RbColor.Black;
                    z.Parent.Parent!.Color = RbColor.Red;
                    RotateRight(z.Parent.Parent);
                }
            }
            else
            {
                RbNode<TKey, TValue>? y = z.Parent.Parent?.Left;
                if (y is { Color: RbColor.Red })
                {
                    z.Parent.Color = RbColor.Black;
                    y.Color = RbColor.Black;
                    z.Parent.Parent!.Color = RbColor.Red;
                    z = z.Parent.Parent;
                }
                else
                {
                    if (z.IsLeftChild)
                    {
                        z = z.Parent;
                        RotateRight(z);
                    }
                    z.Parent!.Color = RbColor.Black;
                    z.Parent.Parent!.Color = RbColor.Red;
                    RotateLeft(z.Parent.Parent);
                }
            }
        }
        Root!.Color = RbColor.Black;
    }

    protected override void RemoveNode(RbNode<TKey, TValue> z)
    {
        RbNode<TKey, TValue> y = z;
        RbColor yOriginalColor = y.Color;
        RbNode<TKey, TValue>? x;
        RbNode<TKey, TValue>? parent;

        if (z.Left == null)
        {
            x = z.Right;
            parent = z.Parent;
            Transplant(z, z.Right);
        }
        else if (z.Right == null)
        {
            x = z.Left;
            parent = z.Parent;
            Transplant(z, z.Left);
        }
        else
        {
            y = Minimum(z.Right);
            yOriginalColor = y.Color;
            x = y.Right;
            if (y.Parent == z)
            {
                parent = y;
            }
            else
            {
                parent = y.Parent;
                Transplant(y, y.Right);
                y.Right = z.Right;
                y.Right.Parent = y;
            }
            Transplant(z, y);
            y.Left = z.Left;
            y.Left.Parent = y;
            y.Color = z.Color;
        }

        if (yOriginalColor == RbColor.Black)
        {
            if (x != null && x.Color == RbColor.Red)
            {
                x.Color = RbColor.Black;
            }
            else
            {
                RbDeleteFixup(parent, x);
            }
        }
    }

    private void RbDeleteFixup(RbNode<TKey, TValue>? parent, RbNode<TKey, TValue>? x)
    {
        while (x != Root && (x == null || x.Color == RbColor.Black))
        {
            if (parent == null) break;

            if (x == parent.Left)
            {
                RbNode<TKey, TValue>? w = parent.Right;
                if (w is { Color: RbColor.Red })
                {
                    w.Color = RbColor.Black;
                    parent.Color = RbColor.Red;
                    RotateLeft(parent);
                    w = parent.Right;
                }

                if ((w?.Left == null || w.Left.Color == RbColor.Black) &&
                    (w?.Right == null || w.Right.Color == RbColor.Black))
                {
                    if (w != null) w.Color = RbColor.Red;
                    x = parent;
                    parent = x.Parent;
                }
                else
                {
                    if (w?.Right == null || w.Right.Color == RbColor.Black)
                    {
                        if (w?.Left != null) w.Left.Color = RbColor.Black;
                        if (w != null) w.Color = RbColor.Red;
                        if (w != null) RotateRight(w);
                        w = parent.Right;
                    }

                    if (w != null)
                    {
                        w.Color = parent.Color;
                        parent.Color = RbColor.Black;
                        if (w.Right != null) w.Right.Color = RbColor.Black;
                        RotateLeft(parent);
                    }
                    x = Root;
                }
            }
            else
            {
                RbNode<TKey, TValue>? w = parent.Left;
                if (w is { Color: RbColor.Red })
                {
                    w.Color = RbColor.Black;
                    parent.Color = RbColor.Red;
                    RotateRight(parent);
                    w = parent.Left;
                }

                if ((w?.Right == null || w.Right.Color == RbColor.Black) &&
                    (w?.Left == null || w.Left.Color == RbColor.Black))
                {
                    if (w != null) w.Color = RbColor.Red;
                    x = parent;
                    parent = x.Parent;
                }
                else
                {
                    if (w?.Left == null || w.Left.Color == RbColor.Black)
                    {
                        if (w?.Right != null) w.Right.Color = RbColor.Black;
                        if (w != null) w.Color = RbColor.Red;
                        if (w != null) RotateLeft(w);
                        w = parent.Left;
                    }

                    if (w != null)
                    {
                        w.Color = parent.Color;
                        parent.Color = RbColor.Black;
                        if (w.Left != null) w.Left.Color = RbColor.Black;
                        RotateRight(parent);
                    }
                    x = Root;
                }
            }
        }
        if (x != null) x.Color = RbColor.Black;
    }
}