using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Veldrid.Utilities
{
    public delegate int RayCastFilter<T>(Ray ray, T item, List<RayCastHit<T>> hits);

    /// <summary>
    /// Maintains a reference to the current root node of a dynamic octree.
    /// The root node may change as items are added and removed from contained nodes.
    /// </summary>
    /// <typeparam name="T">The type stored in the octree.</typeparam>
    public class Octree<T>
    {
        private OctreeNode<T> _currentRoot;

        private List<OctreeItem<T>> _pendingMoveStage = new List<OctreeItem<T>>();

        public Octree(BoundingBox boundingBox, int maxChildren)
        {
            _currentRoot = new OctreeNode<T>(boundingBox, maxChildren);
        }

        /// <summary>
        /// The current root node of the octree. This may change when items are added and removed.
        /// </summary>
        public OctreeNode<T> CurrentRoot => _currentRoot;

        public OctreeItem<T> AddItem(BoundingBox itemBounds, T item)
        {
            OctreeItem<T> ret;
            _currentRoot = _currentRoot.AddItem(ref itemBounds, item, out ret);
            return ret;
        }

        public void GetContainedObjects(BoundingFrustum frustum, List<T> results)
        {
            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }

            _currentRoot.GetContainedObjects(ref frustum, results);
        }

        public void GetContainedObjects(BoundingFrustum frustum, List<T> results, Func<T, bool> filter)
        {
            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }

            _currentRoot.GetContainedObjects(ref frustum, results, filter);
        }

        public int RayCast(Ray ray, List<RayCastHit<T>> hits, RayCastFilter<T> filter)
        {
            if (hits == null)
            {
                throw new ArgumentNullException(nameof(hits));
            }

            return _currentRoot.RayCast(ray, hits, filter);
        }

        public void GetAllContainedObjects(List<T> results)
        {
            _currentRoot.GetAllContainedObjects(results);
        }

        public void GetAllContainedObjects(List<T> results, Func<T, bool> filter)
        {
            _currentRoot.GetAllContainedObjects(results, filter);
        }

        public bool RemoveItem(T item)
        {
            OctreeItem<T> octreeItem;
            if (!_currentRoot.TryGetContainedOctreeItem(item, out octreeItem))
            {
                return false;
            }
            else
            {
                RemoveItem(octreeItem);
                return true;
            }
        }

        public void RemoveItem(OctreeItem<T> octreeItem)
        {
            octreeItem.Container.RemoveItem(octreeItem);
            _currentRoot = _currentRoot.TryTrimChildren();
        }

        public void MoveItem(T item, BoundingBox newBounds)
        {
            OctreeItem<T> octreeItem;
            if (!_currentRoot.TryGetContainedOctreeItem(item, out octreeItem))
            {
                throw new VeldridException(item + " is not contained in the octree. It cannot be moved.");
            }
            else
            {
                MoveItem(octreeItem, newBounds);
            }
        }

        public void MoveItem(OctreeItem<T> octreeItem, BoundingBox newBounds)
        {
            if (newBounds.ContainsNaN())
            {
                throw new VeldridException("Invalid bounds: " + newBounds);
            }
            OctreeNode<T> newRoot = octreeItem.Container.MoveContainedItem(octreeItem, newBounds);
            if (newRoot != null)
            {
                _currentRoot = newRoot;
            }

            _currentRoot = _currentRoot.TryTrimChildren();
        }

        public void Clear() => _currentRoot.Clear();

        /// <summary>
        /// Apply pending moves. This may change the current root node.
        /// </summary>
        public void ApplyPendingMoves()
        {
            _pendingMoveStage.Clear();
            _currentRoot.CollectPendingMoves(_pendingMoveStage);
            foreach (OctreeItem<T> item in _pendingMoveStage)
            {
                MoveItem(item, item.Bounds);
                item.HasPendingMove = false;
            }
        }
    }

    [DebuggerDisplay("{DebuggerDisplayString,nq}")]
    public class OctreeNode<T>
    {
        private readonly List<OctreeItem<T>> _items = new List<OctreeItem<T>>();
        private readonly OctreeNodeCache _nodeCache;
        private OctreeNode<T>[] _children = Array.Empty<OctreeNode<T>>();
        private BoundingBox _bounds;

        public BoundingBox Bounds { get { return _bounds; } set { _bounds = value; } }
        public int MaxChildren { get; private set; }
        public OctreeNode<T>[] Children { get => _children; private set => _children = value; }
        public OctreeNode<T> Parent { get; private set; }

        private const int NumChildNodes = 8;

        public static OctreeNode<T> CreateNewTree(ref BoundingBox bounds, int maxChildren)
        {
            return new OctreeNode<T>(ref bounds, maxChildren, new OctreeNodeCache(maxChildren), null);
        }

        public OctreeNode<T> AddItem(BoundingBox itemBounds, T item)
        {
            OctreeItem<T> ignored;
            return AddItem(itemBounds, item, out ignored);
        }

        public OctreeNode<T> AddItem(ref BoundingBox itemBounds, T item)
        {
            OctreeItem<T> ignored;
            return AddItem(ref itemBounds, item, out ignored);
        }

        public OctreeNode<T> AddItem(BoundingBox itemBounds, T item, out OctreeItem<T> itemContainer)
        {
            return AddItem(ref itemBounds, item, out itemContainer);
        }

        public OctreeNode<T> AddItem(ref BoundingBox itemBounds, T item, out OctreeItem<T> octreeItem)
        {
            if (Parent != null)
            {
                throw new VeldridException("Can only add items to the root Octree node.");
            }

            octreeItem = _nodeCache.GetOctreeItem(ref itemBounds, item);
            return CoreAddRootItem(octreeItem);
        }

        private OctreeNode<T> CoreAddRootItem(OctreeItem<T> octreeItem)
        {
            OctreeNode<T> root = this;
            bool result = CoreAddItem(octreeItem);
            if (!result)
            {
                root = ResizeAndAdd(octreeItem);
            }

            return root;
        }

        /// <summary>
        /// Move a contained OctreeItem. If the root OctreeNode needs to be resized, the new root node is returned.
        /// </summary>
        public OctreeNode<T> MoveContainedItem(OctreeItem<T> item, BoundingBox newBounds)
        {
            OctreeNode<T> newRoot = null;

            OctreeNode<T> container = item.Container;
            if (!container._items.Contains(item))
            {
                throw new VeldridException("Can't move item " + item + ", its container does not contain it.");
            }

            item.Bounds = newBounds;
            if (container.Bounds.Contains(ref item.Bounds) == ContainmentType.Contains)
            {
                // Item did not leave the node.
                newRoot = null;

                // It may have moved into the bounds of a child node.
                for (int i = 0; i < _children.Length; i++)
                {
                    if (_children[i].CoreAddItem(item))
                    {
                        _items.Remove(item);
                        break;
                    }
                }
            }
            else
            {
                container._items.Remove(item);
                item.Container = null;

                OctreeNode<T> node = container;
                while (node.Parent != null && !node.CoreAddItem(item))
                {
                    node = node.Parent;
                }

                if (item.Container == null)
                {
                    // This should only occur if the item has moved beyond the root node's bounds.
                    // We need to resize the root tree.
                    Debug.Assert(node == GetRootNode());
                    newRoot = node.CoreAddRootItem(item);
                }

                container.Parent.ConsiderConsolidation();
            }

            return newRoot;
        }

        /// <summary>
        /// Mark an item as having moved, but do not alter the octree structure. Call <see cref="Octree{T}.ApplyPendingMoves"/> to update the octree structure.
        /// </summary>
        public void MarkItemAsMoved(OctreeItem<T> octreeItem, BoundingBox newBounds)
        {
            if (!_items.Contains(octreeItem))
            {
                throw new VeldridException("Cannot mark item as moved which doesn't belong to this OctreeNode.");
            }
            if (newBounds.ContainsNaN())
            {
                throw new VeldridException("Invalid bounds: " + newBounds);
            }

            octreeItem.HasPendingMove = true;
            octreeItem.Bounds = newBounds;
        }

        private OctreeNode<T> GetRootNode()
        {
            if (Parent == null)
            {
                return this;
            }

            OctreeNode<T> root = Parent;
            while (root.Parent != null)
            {
                root = root.Parent;
            }

            return root;
        }

        public void RemoveItem(OctreeItem<T> octreeItem)
        {
            OctreeNode<T> container = octreeItem.Container;
            if (!container._items.Remove(octreeItem))
            {
                throw new VeldridException("Item isn't contained in its container.");
            }

            if (container.Parent != null)
            {
                container.Parent.ConsiderConsolidation();
            }

            _nodeCache.AddOctreeItem(octreeItem);
        }

        private void ConsiderConsolidation()
        {
            if (_children.Length > 0 && GetItemCount() < MaxChildren)
            {
                ConsolidateChildren();
                Parent?.ConsiderConsolidation();
            }
        }

        private void ConsolidateChildren()
        {
            for (int i = 0; i < _children.Length; i++)
            {
                OctreeNode<T> child = _children[i];
                child.ConsolidateChildren();

                foreach (OctreeItem<T> childItem in child._items)
                {
                    _items.Add(childItem);
                    childItem.Container = this;
                }
            }

            RecycleChildren();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetContainedObjects(BoundingFrustum frustum, List<T> results)
        {
            Debug.Assert(results != null);
            CoreGetContainedObjects(ref frustum, results, null);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetContainedObjects(ref BoundingFrustum frustum, List<T> results)
        {
            Debug.Assert(results != null);
            CoreGetContainedObjects(ref frustum, results, null);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetContainedObjects(ref BoundingFrustum frustum, List<T> results, Func<T, bool> filter)
        {
            Debug.Assert(results != null);
            CoreGetContainedObjects(ref frustum, results, filter);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetAllContainedObjects(List<T> results) => GetAllContainedObjects(results, null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetAllContainedObjects(List<T> results, Func<T, bool> filter)
        {
            Debug.Assert(results != null);
            CoreGetAllContainedObjects(results, filter);
        }

        public int RayCast(Ray ray, List<RayCastHit<T>> hits, RayCastFilter<T> filter)
        {
            if (!ray.Intersects(Bounds))
            {
                return 0;
            }
            else
            {
                int numHits = 0;
                foreach (OctreeItem<T> item in _items)
                {
                    if (ray.Intersects(item.Bounds))
                    {
                        numHits += filter(ray, item.Item, hits);
                    }
                }
                for (int i = 0; i < _children.Length; i++)
                {
                    numHits += _children[i].RayCast(ray, hits, filter);
                }

                return numHits;
            }
        }

        public BoundingBox GetPreciseBounds()
        {
            Vector3 min = new Vector3(float.MaxValue);
            Vector3 max = new Vector3(float.MinValue);
            return CoreGetPreciseBounds(ref min, ref max);
        }

        private BoundingBox CoreGetPreciseBounds(ref Vector3 min, ref Vector3 max)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                OctreeItem<T> item = _items[i];
                min = Vector3.Min(min, item.Bounds.Min);
                max = Vector3.Max(max, item.Bounds.Max);
            }

            for (int i = 0; i < _children.Length; i++)
            {
                _children[i].CoreGetPreciseBounds(ref min, ref max);
            }

            return new BoundingBox(min, max);
        }

        public int GetItemCount()
        {
            int count = _items.Count;
            for (int i = 0; i < _children.Length; i++)
            {
                count += _children[i].GetItemCount();
            }

            return count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CoreGetContainedObjects(ref BoundingFrustum frustum, List<T> results, Func<T, bool> filter)
        {
            ContainmentType ct = frustum.Contains(ref _bounds);
            if (ct == ContainmentType.Contains)
            {
                CoreGetAllContainedObjects(results, filter);
            }
            else if (ct == ContainmentType.Intersects)
            {
                for (int i = 0; i < _items.Count; i++)
                {
                    OctreeItem<T> octreeItem = _items[i];
                    if (frustum.Contains(ref octreeItem.Bounds) != ContainmentType.Disjoint)
                    {
                        if (filter == null || filter(octreeItem.Item))
                        {
                            results.Add(octreeItem.Item);
                        }
                    }
                }
                for (int i = 0; i < _children.Length; i++)
                {
                    _children[i].CoreGetContainedObjects(ref frustum, results, filter);
                }
            }
        }

        public IEnumerable<OctreeItem<T>> GetAllOctreeItems()
        {
            foreach (OctreeItem<T> item in _items)
            {
                yield return item;
            }
            foreach (OctreeNode<T> child in Children)
            {
                foreach (OctreeItem<T> childItem in child.GetAllOctreeItems())
                {
                    yield return childItem;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CoreGetAllContainedObjects(List<T> results, Func<T, bool> filter)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                OctreeItem<T> octreeItem = _items[i];
                if (filter == null || filter(octreeItem.Item))
                {
                    results.Add(octreeItem.Item);
                }
            }
            for (int i = 0; i < _children.Length; i++)
            {
                _children[i].CoreGetAllContainedObjects(results, filter);
            }
        }

        public void Clear()
        {
            if (Parent != null)
            {
                throw new VeldridException("Can only clear the root OctreeNode.");
            }

            RecycleNode();
        }

        private void RecycleNode(bool recycleChildren = true)
        {
            if (recycleChildren)
            {
                RecycleChildren();
            }

            _items.Clear();
            _nodeCache.AddNode(this);
        }

        private void RecycleChildren()
        {
            if (_children.Length != 0)
            {
                for (int i = 0; i < _children.Length; i++)
                {
                    _children[i].RecycleNode();
                }

                _nodeCache.AddAndClearChildrenArray(_children);
                _children = Array.Empty<OctreeNode<T>>();
            }
        }

        private bool CoreAddItem(OctreeItem<T> item)
        {
            if (Bounds.Contains(ref item.Bounds) != ContainmentType.Contains)
            {
                return false;
            }

            if (_items.Count >= MaxChildren && _children.Length == 0)
            {
                OctreeNode<T> newNode = SplitChildren(ref item.Bounds, null);
                if (newNode != null)
                {
                    bool succeeded = newNode.CoreAddItem(item);
                    Debug.Assert(succeeded, "Octree node returned from SplitChildren must fit the item given to it.");
                    return true;
                }
            }
            else if (_children.Length > 0)
            {
                for (int i = 0; i < _children.Length; i++)
                {
                    if (_children[i].CoreAddItem(item))
                    {
                        return true;
                    }
                }
            }

            // Couldn't fit in any children.
#if DEBUG
            foreach (OctreeNode<T> child in _children)
            {
                Debug.Assert(child.Bounds.Contains(ref item.Bounds) != ContainmentType.Contains);
            }
#endif

            _items.Add(item);
            item.Container = this;

            return true;
        }

        // Splits the node into 8 children
        private OctreeNode<T> SplitChildren(ref BoundingBox itemBounds, OctreeNode<T> existingChild)
        {
            Debug.Assert(_children.Length == 0, "Children must be empty before SplitChildren is called.");

            OctreeNode<T> childBigEnough = null;
            _children = _nodeCache.GetChildrenArray();
            Vector3 center = Bounds.GetCenter();
            Vector3 dimensions = Bounds.GetDimensions();

            Vector3 quaterDimensions = dimensions * 0.25f;

            int i = 0;
            for (float x = -1f; x <= 1f; x += 2f)
            {
                for (float y = -1f; y <= 1f; y += 2f)
                {
                    for (float z = -1f; z <= 1f; z += 2f)
                    {
                        Vector3 childCenter = center + (quaterDimensions * new Vector3(x, y, z));
                        Vector3 min = childCenter - quaterDimensions;
                        Vector3 max = childCenter + quaterDimensions;
                        BoundingBox childBounds = new BoundingBox(min, max);
                        OctreeNode<T> newChild;

                        if (existingChild != null && existingChild.Bounds == childBounds)
                        {
                            newChild = existingChild;
                        }
                        else
                        {
                            newChild = _nodeCache.GetNode(ref childBounds);
                        }

                        if (childBounds.Contains(ref itemBounds) == ContainmentType.Contains)
                        {
                            Debug.Assert(childBigEnough == null);
                            childBigEnough = newChild;
                        }

                        newChild.Parent = this;
                        _children[i] = newChild;
                        i++;
                    }
                }
            }

            PushItemsToChildren();
#if DEBUG
            for (int g = 0; g < _children.Length; g++)
            {
                Debug.Assert(_children[g] != null);
            }
#endif
            return childBigEnough;
        }

        private void PushItemsToChildren()
        {
            for (int i = 0; i < _items.Count; i++)
            {
                OctreeItem<T> item = _items[i];
                for (int c = 0; c < _children.Length; c++)
                {
                    if (_children[c].CoreAddItem(item))
                    {
                        _items.Remove(item);
                        i--;
                        break;
                    }
                }
            }

#if DEBUG
            for (int i = 0; i < _items.Count; i++)
            {
                for (int c = 0; c < _children.Length; c++)
                {
                    Debug.Assert(_children[c].Bounds.Contains(ref _items[i].Bounds) != ContainmentType.Contains);
                }
            }
#endif
        }

        private OctreeNode<T> ResizeAndAdd(OctreeItem<T> octreeItem)
        {
            OctreeNode<T> oldRoot = this;
            Vector3 oldRootCenter = Bounds.GetCenter();
            Vector3 oldRootHalfExtents = Bounds.GetDimensions() * 0.5f;

            Vector3 expandDirection = Vector3.Normalize(octreeItem.Bounds.GetCenter() - oldRootCenter);
            Vector3 newCenter = oldRootCenter;
            if (expandDirection.X >= 0) // oldRoot = Left
            {
                newCenter.X += oldRootHalfExtents.X;
            }
            else
            {
                newCenter.X -= oldRootHalfExtents.X;
            }

            if (expandDirection.Y >= 0) // oldRoot = Bottom
            {
                newCenter.Y += oldRootHalfExtents.Y;
            }
            else
            {
                newCenter.Y -= oldRootHalfExtents.Y;
            }

            if (expandDirection.Z >= 0) // oldRoot = Far
            {
                newCenter.Z += oldRootHalfExtents.Z;
            }
            else
            {
                newCenter.Z -= oldRootHalfExtents.Z;
            }

            BoundingBox newRootBounds = new BoundingBox(newCenter - oldRootHalfExtents * 2f, newCenter + oldRootHalfExtents * 2f);
            OctreeNode<T> newRoot = _nodeCache.GetNode(ref newRootBounds);
            OctreeNode<T> fittingNode = newRoot.SplitChildren(ref octreeItem.Bounds, oldRoot);
            if (fittingNode != null)
            {
                bool succeeded = fittingNode.CoreAddItem(octreeItem);
                Debug.Assert(succeeded, "Octree node returned from SplitChildren must fit the item given to it.");
                return newRoot;
            }
            else
            {
                return newRoot.CoreAddRootItem(octreeItem);
            }
        }

        public OctreeNode(BoundingBox box, int maxChildren)
            : this(ref box, maxChildren, new OctreeNodeCache(maxChildren), null)
        {
        }

        private OctreeNode(ref BoundingBox bounds, int maxChildren, OctreeNodeCache nodeCache, OctreeNode<T> parent)
        {
            Bounds = bounds;
            MaxChildren = maxChildren;
            _nodeCache = nodeCache;
            Parent = parent;
        }

        private void Reset(ref BoundingBox newBounds)
        {
            Bounds = newBounds;

            _items.Clear();
            Parent = null;

            if (_children.Length != 0)
            {
                _nodeCache.AddAndClearChildrenArray(Children);
                _children = Array.Empty<OctreeNode<T>>();
            }
        }

        /// <summary>
        /// Attempts to find an OctreeNode for the given item, in this OctreeNode and its children.
        /// </summary>
        /// <param name="item">The item to find.</param>
        /// <param name="octreeItem">The contained OctreeItem.</param>
        /// <returns>true if the item was contained in the Octree; false otherwise.</returns>
        internal bool TryGetContainedOctreeItem(T item, out OctreeItem<T> octreeItem)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                OctreeItem<T> containedItem = _items[i];
                if (containedItem.Item.Equals(item))
                {
                    octreeItem = containedItem;
                    return true;
                }
            }

            for (int i = 0; i < _children.Length; i++)
            {
                OctreeNode<T> child = _children[i];
                Debug.Assert(child != null, "node child cannot be null.");
                if (child.TryGetContainedOctreeItem(item, out octreeItem))
                {
                    return true;
                }
            }

            octreeItem = null;
            return false;
        }

        /// <summary>
        /// Determines if there is only one child node in use. If so, recycles all other nodes and returns that one.
        /// If this is not true, the node is returned unchanged.
        /// </summary>
        internal OctreeNode<T> TryTrimChildren()
        {
            if (_items.Count == 0)
            {
                OctreeNode<T> loneChild = null;
                for (int i = 0; i < _children.Length; i++)
                {
                    OctreeNode<T> child = _children[i];
                    if (child.GetItemCount() != 0)
                    {
                        if (loneChild != null)
                        {
                            return this;
                        }
                        else
                        {
                            loneChild = child;
                        }
                    }
                }

                if (loneChild != null)
                {
                    // Recycle excess
                    for (int i = 0; i < _children.Length; i++)
                    {
                        OctreeNode<T> child = _children[i];
                        if (child != loneChild)
                        {
                            child.RecycleNode();
                        }
                    }

                    RecycleNode(recycleChildren: false);

                    // Return lone child in use
                    loneChild.Parent = null;
                    return loneChild;
                }
            }

            return this;
        }

        internal void CollectPendingMoves(List<OctreeItem<T>> pendingMoves)
        {
            for (int i = 0; i < _children.Length; i++)
            {
                _children[i].CollectPendingMoves(pendingMoves);
            }

            for (int i = 0; i < _items.Count; i++)
            {
                OctreeItem<T> item = _items[i];
                if (item.HasPendingMove)
                {
                    pendingMoves.Add(item);
                }
            }
        }

        private string DebuggerDisplayString
        {
            get
            {
                return string.Format("{0} - {1}, Items:{2}", Bounds.Min, Bounds.Max, _items.Count);
            }
        }

        private class OctreeNodeCache
        {
            private readonly Stack<OctreeNode<T>> _cachedNodes = new Stack<OctreeNode<T>>();
            private readonly Stack<OctreeNode<T>[]> _cachedChildren = new Stack<OctreeNode<T>[]>();
            private readonly Stack<OctreeItem<T>> _cachedItems = new Stack<OctreeItem<T>>();

            public int MaxChildren { get; private set; }

            public int MaxCachedItemCount { get; set; } = 100;

            public OctreeNodeCache(int maxChildren)
            {
                MaxChildren = maxChildren;
            }

            public void AddNode(OctreeNode<T> child)
            {
                Debug.Assert(!_cachedNodes.Contains(child));
                if (_cachedNodes.Count < MaxCachedItemCount)
                {
                    for (int i = 0; i < child._items.Count; i++)
                    {
                        OctreeItem<T> item = child._items[i];
                        item.Item = default(T);
                        item.Container = null;
                    }
                    child.Parent = null;
                    child._children = Array.Empty<OctreeNode<T>>();

                    _cachedNodes.Push(child);
                }
            }

            public void AddOctreeItem(OctreeItem<T> octreeItem)
            {
                if (_cachedItems.Count < MaxCachedItemCount)
                {
                    octreeItem.Item = default(T);
                    octreeItem.Container = null;
                    _cachedItems.Push(octreeItem);
                }
            }

            public OctreeNode<T> GetNode(ref BoundingBox bounds)
            {
                if (_cachedNodes.Count > 0)
                {
                    OctreeNode<T> node = _cachedNodes.Pop();
                    node.Reset(ref bounds);
                    return node;
                }
                else
                {
                    return CreateNewNode(ref bounds);
                }
            }

            public void AddAndClearChildrenArray(OctreeNode<T>[] children)
            {
                if (_cachedChildren.Count < MaxCachedItemCount)
                {
                    for (int i = 0; i < children.Length; i++)
                    {
                        children[i] = null;
                    }

                    _cachedChildren.Push(children);
                }
            }

            public OctreeNode<T>[] GetChildrenArray()
            {
                if (_cachedChildren.Count > 0)
                {
                    OctreeNode<T>[] children = _cachedChildren.Pop();
#if DEBUG
                    Debug.Assert(children.Length == 8);
                    for (int i = 0; i < children.Length; i++)
                    {
                        Debug.Assert(children[i] == null);
                    }
#endif

                    return children;
                }
                else
                {
                    return new OctreeNode<T>[NumChildNodes];
                }
            }

            public OctreeItem<T> GetOctreeItem(ref BoundingBox bounds, T item)
            {
                OctreeItem<T> octreeItem;
                if (_cachedItems.Count > 0)
                {
                    octreeItem = _cachedItems.Pop();
                    octreeItem.Bounds = bounds;
                    octreeItem.Item = item;
                    octreeItem.Container = null;
                }
                else
                {
                    octreeItem = CreateNewItem(ref bounds, item);
                }

                return octreeItem;
            }

            private OctreeItem<T> CreateNewItem(ref BoundingBox bounds, T item) => new OctreeItem<T>(ref bounds, item);

            private OctreeNode<T> CreateNewNode(ref BoundingBox bounds)
            {
                OctreeNode<T> node = new OctreeNode<T>(ref bounds, MaxChildren, this, null);
                return node;
            }
        }
    }

    public class OctreeItem<T>
    {
        /// <summary>The node this item directly resides in. /// </summary>
        public OctreeNode<T> Container;
        public BoundingBox Bounds;
        public T Item;

        public bool HasPendingMove { get; set; }

        public OctreeItem(ref BoundingBox bounds, T item)
        {
            Bounds = bounds;
            Item = item;
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}", Bounds, Item);
        }
    }
}