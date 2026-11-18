using SubsectableSortedDictionary.Exceptions;

namespace Emperor.SubsectableSortedDictionary
{
    using System.Collections.Generic;
    using System.Collections;

    public enum RBBSTColor
    {
        Red,
        Black
    }

    public enum Direction
    {
        Left,
        Right
    }

    public class SSBSTNode<TKey, TValue>
        where TKey : IComparable<TKey>, IEquatable<TKey>
    {

        public TKey Key;
        public TValue Value;
        public RBBSTColor Color;

        public SSBSTNode<TKey, TValue>? Left = null;
        public SSBSTNode<TKey, TValue>? Right = null;
        public SSBSTNode<TKey, TValue>? Parent = null;

        public static void SwapNodeValues(SSBSTNode<TKey, TValue> n1, SSBSTNode<TKey, TValue> n2)
        {
            var n1k = n1.Key;
            var n1v = n1.Value;
            n1.Key = n2.Key;
            n1.Value = n2.Value;
            n2.Key = n1k;
            n2.Value = n1v;
        }

        public SSBSTNode(TKey key, TValue value, RBBSTColor color)
        {
            this.Key = key;
            this.Value = value;
            Color = color;
        }

        public SSBSTNode(TKey key, TValue value, SSBSTNode<TKey, TValue>? parent)
        {
            this.Key = key;
            this.Value = value;
            this.Parent = parent;

            Color = RBBSTColor.Red;
        }

        public SSBSTNode<TKey, TValue>? GetNodeByOppositeDirection(Direction direction)
        {
            return direction == Direction.Left ? Right : Left;
        }

        public SSBSTNode<TKey, TValue>? GetNodeByDirection(Direction direction)
        {
            return direction == Direction.Left ? Left : Right;
        }

        public void SetNodeByDirection(Direction direction, SSBSTNode<TKey, TValue>? node)
        {
            if (direction == Direction.Left)
            {
                Left = node;
            }
            else
            {
                Right = node;
            }
        }

        public void SetNodeByOppositeDirection(Direction direction, SSBSTNode<TKey, TValue>? node)
        {
            if (direction == Direction.Right)
            {
                Left = node;
            }
            else
            {
                Right = node;
            }
        }

        public Direction GetChildDirection(SSBSTNode<TKey, TValue> node)
        {
            if (node == Left) return Direction.Left;
            if (node == Right) return Direction.Right;
            throw new ArgumentException($"Root is not a child of node with key {Key}.");
        }

        public void ReplaceChildNode(SSBSTNode<TKey, TValue> node, SSBSTNode<TKey, TValue>? replacementNode)
        {
            if (node == Left)
            {
                Left = replacementNode;
            }
            else
            {
                Right = replacementNode;
            }

            replacementNode?.Parent = this;
        }

        /// <summary>
        /// Get the number of children this node has.
        /// </summary>
        /// <param name="direction">
        /// Returns the direction of the node if return value is 1.
        /// Otherwise, a garbage value (Direction.Right for 2 nodes, -1 for 0 nodes)
        /// </param>
        /// <returns>Number of children this node contains.</returns>
        public int GetChildCount(out Direction direction)
        {
            direction = (Direction)(-1);
            int sum = 0;
            if (Left is not null)
            {
                sum++;
                direction = Direction.Left;
            }

            if (Right is not null)
            {
                sum++;
                direction = Direction.Right;
            }

            return sum;
        }
    }

    /// <summary>
    /// Implementation of a Red-Black Binary Search Tree that is subsectable.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class SubsectableBinarySearchTree<TKey, TValue> :
        IDictionary<TKey, TValue>
        where TKey : IComparable<TKey>, IEquatable<TKey>
    {
        private SSBSTNode<TKey, TValue>? root;
        public bool IsReadOnly { get; } = false;
        
        #region Tree Rotation

        private void RotateOnNode(SSBSTNode<TKey, TValue> node, Direction direction)
        {
            var overallParent = node.Parent;

            var newParent = node.GetNodeByOppositeDirection(direction) ??
                            throw new InvalidOperationException("Tried to rotate on a null node.");
            var newChild = newParent.GetNodeByDirection(direction);

            node.SetNodeByOppositeDirection(direction, newChild);
            newParent.SetNodeByDirection(direction, node);

            newChild?.Parent = node;

            newParent.Parent = overallParent;
            node.Parent = newParent;

            if (overallParent is null)
            {
                root = newParent;
                return;
            }

            overallParent.ReplaceChildNode(node, newParent);
        }

        #endregion

        #region Add/Insert

        public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);
        public void Add(TKey key, TValue value) => InsertNode(key, value, false);

        private void InsertNode(TKey key, TValue value, bool allowOverwrite = true)
        {
            if (root is null)
            {
                root = new SSBSTNode<TKey, TValue>(key, value, RBBSTColor.Black);
                Count++;
                return;
            }

            SSBSTNode<TKey, TValue> insertingNode;
            var currentParentNode = root;
            while (true)
            {
                var comparison = currentParentNode.Key.CompareTo(key);
                if (comparison == 0)
                {
                    if (allowOverwrite)
                    {
                        currentParentNode.Value = value;
                        return;
                    }

                    throw new ArgumentException(
                        $"Key {key} already present in dictionary. " +
                        $"To add w/ overwriting existing keys, consider using insertion of the form <dictionary>[<key>] = <value>."
                        );
                }

                var targetLeaf = comparison > 0 ? currentParentNode.Left : currentParentNode.Right;
                if (targetLeaf is null)
                {
                    insertingNode = new SSBSTNode<TKey, TValue>(key, value, currentParentNode);
                    if (comparison > 0)
                    {
                        currentParentNode.Left = insertingNode;
                    }
                    else
                    {
                        currentParentNode.Right = insertingNode;
                    }

                    Count++;

                    break;
                }

                currentParentNode = targetLeaf;
            }

            do
            {
                if (currentParentNode.Color == RBBSTColor.Black) return;

                var grandparent = currentParentNode.Parent;

                if (grandparent is null)
                {
                    currentParentNode.Color = RBBSTColor.Black;
                    return;
                }

                var direction = grandparent.GetChildDirection(currentParentNode);
                var uncle = grandparent.GetNodeByOppositeDirection(direction);

                if (uncle is null || uncle.Color == RBBSTColor.Black)
                {
                    if (insertingNode == currentParentNode.GetNodeByOppositeDirection(direction))
                    {
                        RotateOnNode(currentParentNode, direction);
                        currentParentNode = grandparent.GetNodeByDirection(direction);
                    }

                    RotateOnNode(grandparent, direction == Direction.Left ? Direction.Right : Direction.Left);
                    currentParentNode.Color = RBBSTColor.Black;
                    grandparent.Color = RBBSTColor.Red;
                    return;
                }

                currentParentNode.Color = RBBSTColor.Black;
                uncle.Color = RBBSTColor.Black;
                grandparent.Color = RBBSTColor.Red;
                insertingNode = grandparent;

            } while ((currentParentNode = insertingNode.Parent) is not null);

            root.Color = RBBSTColor.Black;
        }

        #endregion

        #region Remove

        public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);

        public bool Remove(TKey key)
        {
            var removingNode = GetNodeByKey(key, throwable: false);
            if (removingNode is null) return false;

            Count--;

            Direction currentDirection;
            while (true)
            {
                var childCount = removingNode.GetChildCount(out currentDirection);

                switch (childCount)
                {
                    case 0:
                        if (removingNode.Parent is null)
                        {
                            root = null;
                            return true;
                        }

                        currentDirection = removingNode.Parent.GetChildDirection(removingNode);
                        removingNode.Parent.ReplaceChildNode(removingNode, null);

                        // "When the deleted node has no children (both NULL), and is red, simply remove the leaf node."
                        if (removingNode.Color == RBBSTColor.Red)
                        {
                            return true;
                        }

                        goto redBlackCorrections;
                    case 1:
                        var theNode = removingNode.GetNodeByDirection(currentDirection) ??
                                      throw new SSSDInternalNodeException(
                                          "Accessed node was null in a case where one child was found."
                                          );
                        theNode.Color = RBBSTColor.Black;
                        if (removingNode.Parent is null)
                        {
                            root = theNode;
                            theNode.Parent = null;
                            return true;
                        }

                        removingNode.Parent.ReplaceChildNode(removingNode, theNode);
                        return true;
                    case 2:
                        // removingNode.Left is safe to access since both children are filled.
                        var swappingNode = GetLargestNodeInSubtree(removingNode.Left);
                        SSBSTNode<TKey, TValue>.SwapNodeValues(removingNode, swappingNode);

                        removingNode = swappingNode;
                        continue;
                    default:
                        throw new SSSDInternalNodeException($"Child count return value is configured incorrectly. " +
                                                            $"Got {childCount} children, expected value in range [0, 1, 2].");
                }
            }

            redBlackCorrections:

            // RemoveNode changes the parent of the removing node so that if the removing node was the root, 
            // then now its parent is valid. 
            var parent = removingNode.Parent ??
                         throw new SSSDInternalNodeException(
                             $"Root node was swapped incorrectly; node with key {removingNode.Key} " +
                             $"attempted removal with null parent.");

            bool firstPass = true;

            SSBSTNode<TKey, TValue>? sibling, distantNephew, closeNephew;

            do
            {
                if (!firstPass)
                {
                    currentDirection = removingNode.Parent.GetChildDirection(removingNode);
                }
                else firstPass = false;

                sibling = parent.GetNodeByOppositeDirection(currentDirection) ??
                          throw new SSSDInternalNodeException("Sibling node cannot be null when attempting tree rebalancing.");
                distantNephew = sibling.GetNodeByOppositeDirection(currentDirection);
                closeNephew = sibling.GetNodeByDirection(currentDirection);

                if (sibling.Color == RBBSTColor.Red)
                {
                    RotateOnNode(parent, currentDirection);
                    parent.Color = RBBSTColor.Red;
                    sibling.Color = RBBSTColor.Black;
                    sibling = closeNephew;

                    distantNephew = sibling.GetNodeByOppositeDirection(currentDirection);
                    if (distantNephew is not null && distantNephew.Color == RBBSTColor.Red)
                    {
                        goto case6;
                    }

                    closeNephew = sibling.GetNodeByDirection(currentDirection);
                    if (closeNephew is not null && closeNephew.Color == RBBSTColor.Red)
                    {
                        goto case5;
                    }

                    sibling.Color = RBBSTColor.Red;
                    parent.Color = RBBSTColor.Black;
                    return true;
                }

                if (distantNephew is not null && distantNephew.Color == RBBSTColor.Red)
                {
                    goto case6;
                }

                if (closeNephew is not null && closeNephew.Color == RBBSTColor.Red)
                {
                    goto case5;
                }

                if (parent.Color == RBBSTColor.Red)
                {
                    sibling.Color = RBBSTColor.Red;
                    parent.Color = RBBSTColor.Black;
                    return true;
                }

                sibling.Color = RBBSTColor.Red;
                removingNode = parent;
            } while ((parent = removingNode.Parent) is not null);

            return true;

            // These being headers/goto locations is actually a lot more efficient then wrapping them in functions.
            // Tried and tested! Also ripped straight from wikipedia. Sorry, don't judge me. I'm not trying to reinvent the
            // wheel here.
            case5:

            RotateOnNode(sibling, currentDirection == Direction.Left ? Direction.Right : Direction.Left);
            sibling.Color = RBBSTColor.Red;
            closeNephew.Color = RBBSTColor.Black;
            distantNephew = sibling;
            sibling = closeNephew;

            // Intentional fallthrough (at least if wikipedia is to be believed)
            case6:

            RotateOnNode(parent, currentDirection);
            sibling.Color = parent.Color;
            parent.Color = RBBSTColor.Black;
            distantNephew.Color = RBBSTColor.Black;
            return true;
        }

        #endregion

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        public void Clear()
        {
            Count = 0;
            root = null;
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            var node = GetNodeByKey(item.Key, false);

            return node is not null && EqualityComparer<TValue>.Default.Equals(node.Value, item.Value);
        }

        public bool ContainsKey(TKey key) => Contains(key);

        public bool Contains(TKey key)
        {
            var node = GetNodeByKey(key, false);
            return node is not null;
        }

        private SSBSTNode<TKey, TValue>? GetNodeByKey(TKey key, bool throwable)
        {
            if (root is null) return null;

            var localRootNode = root;
            while (true)
            {
                var comparison = localRootNode.Key.CompareTo(key);
                if (comparison == 0)
                {
                    return localRootNode;
                }

                var targetLeaf = comparison > 0 ? localRootNode.Left : localRootNode.Right;
                if (targetLeaf is null)
                {
                    return throwable ? throw new ArgumentException("Node does not exist in tree.") : null;
                }

                localRootNode = targetLeaf;
            }
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public SSBSTNode<TKey, TValue> GetLargestNodeInSubtree(SSBSTNode<TKey, TValue> node)
        {
            while (node.Right != null)
            {
                node = node.Right;
            }

            return node;
        }

        public int Count { get; private set; }

        public bool TryGetValue(TKey key, out TValue value)
        {
            throw new NotImplementedException();
        }

        public TValue this[TKey key]
        {
            get => throw new NotImplementedException();
            set => InsertNode(key, value, true);
        }

        public ICollection<TKey> Keys { get; }
        public ICollection<TValue> Values { get; }
    }
}