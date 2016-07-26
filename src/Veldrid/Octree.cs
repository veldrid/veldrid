using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace Veldrid
{
    /// <summary>
    /// Maintains a reference to the current root node of a dynamic octree.
    /// The root node may change as items are added and removed from contained nodes.
    /// </summary>
    /// <typeparam name="T">The type stored in the octree.</typeparam>
    public class Octree<T>
    {
        private OctreeNode<T> _currentRoot;

        public Octree(BoundingBox boundingBox, int maxChildre)
        {
            _currentRoot = new OctreeNode<T>(boundingBox, maxChildre);
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
            _currentRoot.GetContainedObjects(ref frustum, results);
        }

        public void GetContainedObjects(BoundingFrustum frustum, List<T> results, Func<T, bool> filter)
        {
            _currentRoot.GetContainedObjects(ref frustum, results, filter);
        }

        public int RayCast(Ray ray, List<T> hits)
        {
            return _currentRoot.RayCast(ray, hits);
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
                octreeItem.Container.RemoveItem(octreeItem);
                return true;
            }
        }

        public void MoveItem(T item, BoundingBox newBounds)
        {
            OctreeItem<T> octreeItem;
            if (!_currentRoot.TryGetContainedOctreeItem(item, out octreeItem))
            {
                throw new InvalidOperationException(item + " is not contained in the octree. It cannot be moved.");
            }
            else
            {
                MoveItem(octreeItem, newBounds);
            }
        }

        public void MoveItem(OctreeItem<T> octreeItem, BoundingBox newBounds)
        {
            var newRoot = octreeItem.Container.ItemMoved(octreeItem, newBounds);
            if (newRoot != null)
            {
                _currentRoot = newRoot;
            }
        }

        public void Clear() => _currentRoot.Clear();
    }

    [DebuggerDisplay("{DebuggerDisplayString,nq}")]
    public class OctreeNode<T>
    {
        private readonly List<OctreeItem<T>> _items = new List<OctreeItem<T>>();
        private readonly OctreeNodeCache _nodeCache;

        public BoundingBox Bounds { get; private set; }
        public int MaxChildren { get; private set; }
        public OctreeNode<T>[] Children { get; private set; } = Array.Empty<OctreeNode<T>>();
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
                throw new InvalidOperationException("Can only add items to the root Octree node.");
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
        public OctreeNode<T> ItemMoved(OctreeItem<T> item, BoundingBox newBounds)
        {
            OctreeNode<T> newRoot = null;

            var container = item.Container;
            if (!container._items.Contains(item))
            {
                throw new InvalidOperationException("Can't move item " + item + ", its container does not contain it.");
            }

            item.Bounds = newBounds;
            if (container.Bounds.Contains(ref item.Bounds) == ContainmentType.Contains)
            {
                // Item did not leave the node.
                newRoot = null;
            }
            else
            {
                container._items.Remove(item);
                item.Container = null;

                var node = container;
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
            var container = octreeItem.Container;
            if (!container._items.Remove(octreeItem))
            {
                throw new InvalidOperationException("Item isn't contained in its container.");
            }

            if (container.Parent != null)
            {
                container.Parent.ConsiderConsolidation();
            }

            _nodeCache.AddOctreeItem(octreeItem);
        }

        private void ConsiderConsolidation()
        {
            if (Children.Length > 0 && GetItemCount() < MaxChildren)
            {
                ConsolidateChildren();
            }
        }

        private void ConsolidateChildren()
        {
            foreach (var child in Children)
            {
                child.ConsolidateChildren();

                foreach (var childItem in child._items)
                {
                    _items.Add(childItem);
                    childItem.Container = this;
                }
            }

            RecycleChildren();
        }

        public void GetContainedObjects(BoundingFrustum frustum, List<T> results)
        {
            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }

            frustum = CoreGetContainedObjects(ref frustum, results, null);
        }

        public void GetContainedObjects(ref BoundingFrustum frustum, List<T> results)
        {
            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }

            frustum = CoreGetContainedObjects(ref frustum, results, null);
        }

        public void GetContainedObjects(ref BoundingFrustum frustum, List<T> results, Func<T, bool> filter)
        {
            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }

            frustum = CoreGetContainedObjects(ref frustum, results, filter);
        }

        public void GetAllContainedObjects(List<T> results) => GetAllContainedObjects(results);
        public void GetAllContainedObjects(List<T> results, Func<T, bool> filter)
        {
            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }

            CoreGetAllContainedObjects(results, filter);
        }

        public int RayCast(Ray ray, List<T> hits)
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
                        numHits++;
                        hits.Add(item.Item);
                    }
                }
                foreach (var child in Children)
                {
                    numHits += child.RayCast(ray, hits);
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
            foreach (var item in _items)
            {
                min = Vector3.Min(min, item.Bounds.Min);
                max = Vector3.Max(max, item.Bounds.Max);
            }

            foreach (var child in Children)
            {
                child.CoreGetPreciseBounds(ref min, ref max);
            }

            return new BoundingBox(min, max);
        }

        public int GetItemCount()
        {
            return _items.Count + Children.Sum(on => on.GetItemCount());
        }

        private BoundingFrustum CoreGetContainedObjects(ref BoundingFrustum frustum, List<T> results, Func<T, bool> filter)
        {
            ContainmentType ct = frustum.Contains(Bounds);
            if (ct == ContainmentType.Contains)
            {
                CoreGetAllContainedObjects(results, filter);
            }
            else if (ct == ContainmentType.Intersects)
            {
                foreach (var octreeItem in _items)
                {
                    if (frustum.Contains(octreeItem.Bounds) != ContainmentType.Disjoint)
                    {
                        if (filter == null || filter(octreeItem.Item))
                        {
                            results.Add(octreeItem.Item);
                        }
                    }
                }
                foreach (var child in Children)
                {
                    child.CoreGetContainedObjects(ref frustum, results, filter);
                }
            }

            return frustum;
        }

        private bool AnyChild(Func<OctreeItem<T>, bool> filter)
        {
            if (_items.Any(filter))
            {
                return true;
            }

            return Children.Any(node => node.AnyChild(filter));
        }

        // TODO: REMOVE
        public IEnumerable<OctreeItem<T>> GetAllOctreeItems()
        {
            foreach (var item in _items)
            {
                yield return item;
            }
            foreach (var child in Children)
            {
                foreach (var childItem in child.GetAllOctreeItems())
                {
                    yield return childItem;
                }
            }
        }

        private void CoreGetAllContainedObjects(List<T> results, Func<T, bool> filter)
        {
            foreach (var octreeItem in _items)
            {
                if (filter == null || filter(octreeItem.Item))
                {
                    results.Add(octreeItem.Item);
                }
            }
            foreach (var child in Children)
            {
                child.CoreGetAllContainedObjects(results, filter);
            }
        }

        public void Clear()
        {
            if (Parent != null)
            {
                throw new InvalidOperationException("Can only clear the root OctreeNode.");
            }

            RecycleNode();
        }

        private void RecycleNode()
        {
            RecycleChildren();
            _items.Clear();
        }

        private void RecycleChildren()
        {
            if (Children.Length != 0)
            {
                foreach (var child in Children)
                {
                    child.RecycleNode();
                    _nodeCache.AddNode(child);
                }

                _nodeCache.AddAndClearChildrenArray(Children);
                Children = Array.Empty<OctreeNode<T>>();
            }
        }

        private bool CoreAddItem(OctreeItem<T> item)
        {
            if (Bounds.Contains(ref item.Bounds) != ContainmentType.Contains)
            {
                return false;
            }

            if (_items.Count >= MaxChildren && Children.Length == 0)
            {
                OctreeNode<T> newNode = SplitChildren(ref item.Bounds, null);
                if (newNode != null)
                {
                    bool succeeded = newNode.CoreAddItem(item);
                    Debug.Assert(succeeded, "Octree node returned from SplitChildren must fit the item given to it.");
                    return true;
                }
            }
            else if (Children.Length > 0)
            {
                foreach (var child in Children)
                {
                    if (child.CoreAddItem(item))
                    {
                        return true;
                    }
                }
            }

            // Couldn't fit in any children.
            _items.Add(item);
            item.Container = this;
            Console.WriteLine("Added to node. New count is " + _items.Count);
            return true;
        }

        // Splits the node into 8 children
        private OctreeNode<T> SplitChildren(ref BoundingBox itemBounds, OctreeNode<T> existingChild)
        {
            Debug.Assert(Children.Length == 0, "Children must be empty before SplitChildren is called.");

            OctreeNode<T> childBigEnough = null;
            Children = _nodeCache.GetChildrenArray();
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
                            childBigEnough = newChild;
                        }

                        newChild.Parent = this;
                        Children[i] = newChild;
                        i++;
                    }
                }
            }

            PushItemsToChildren();
            return childBigEnough;
        }

        private void PushItemsToChildren()
        {
            for (int i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                foreach (var child in Children)
                {
                    if (child.CoreAddItem(item))
                    {
                        _items.Remove(item);
                        i--;
                        continue;
                    }
                }
            }
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

            if (Children.Length != 0)
            {
                _nodeCache.AddAndClearChildrenArray(Children);
                Children = Array.Empty<OctreeNode<T>>();
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
            foreach (var containedItem in _items)
            {
                if (containedItem.Item.Equals(item))
                {
                    octreeItem = containedItem;
                    return true;
                }
            }

            foreach (var child in Children)
            {
                if (child.TryGetContainedOctreeItem(item, out octreeItem))
                {
                    return true;
                }
            }

            octreeItem = null;
            return false;
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
            private readonly Stack<OctreeNode<T>> _nodes = new Stack<OctreeNode<T>>();
            private readonly Stack<OctreeNode<T>[]> _cachedChildren = new Stack<OctreeNode<T>[]>();
            private readonly Stack<OctreeItem<T>> _cachedItems= new Stack<OctreeItem<T>>();

            public int MaxChildren { get; private set; }

            public OctreeNodeCache(int maxChildren)
            {
                MaxChildren = maxChildren;
            }

            public void AddNode(OctreeNode<T> child)
            {
                _nodes.Push(child);
            }

            public void AddOctreeItem(OctreeItem<T> octreeItem)
            {
                _cachedItems.Push(octreeItem);
            }

            public OctreeNode<T> GetNode(ref BoundingBox bounds)
            {
                if (_nodes.Count > 0)
                {
                    var node = _nodes.Pop();
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
                for (int i = 0; i < children.Length; i++)
                {
                    children[i] = null;
                }

                _cachedChildren.Push(children);
            }

            public OctreeNode<T>[] GetChildrenArray()
            {
                if (_cachedChildren.Count > 0)
                {
                    var children = _cachedChildren.Pop();
#if DEBUG
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