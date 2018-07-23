using System;
using System.Collections.Generic;

class BinarySearchTree<T> where T : IComparable<T> {

	TreeNode<T> root;
	
	IComparer<T> comparer;

	public int Count { get; set; }
	
	public BinarySearchTree(IComparer<T> comparer = null) {
		this.comparer = comparer;
		if ( this.comparer==null ) {
			this.comparer = Comparer<T>.Default;
		}
	}

	public void Add(T value) {
		Count++;
		if (root == null)
			root = new TreeNode<T>(value);
		else
			NodeAdd(root, value);
	}

	public void AddAll(IList<T> items) {
		foreach ( T item in items ) {
			Add(item);
		}
	}

	void NodeAdd(TreeNode<T> node, T newValue) {
		int c = node.value.CompareTo(newValue);
		if (comparer != null)
			c = comparer.Compare(node.value, newValue);
		if (c > 0) {
			if (node.Left == null)
				node.Left = new TreeNode<T>(newValue);
			else
				NodeAdd(node.Left, newValue);
		}
		else {
			if (node.Right == null)
				node.Right = new TreeNode<T>(newValue);
			else
				NodeAdd(node.Right, newValue);
		}
	}

	public bool Remove(T data) {
		// first make sure there exist some items in this tree
		if (root == null)
			return false;       // no items to remove

		// Now, try to find data in the tree
		TreeNode<T> current = root, parent = null;

		if ( !TryGetNodeWith(data, out current, out parent ) ) {
			return false;
		}
		// At this point, we've found the node to remove
		Count--;

		int result;
		// We now need to "rethread" the tree
		// CASE 1: If current has no right child, then current's left child becomes
		//         the node pointed to by the parent
		if (current.Right == null) {
			if (parent == null)
				root = current.Left;
			else {
				result = comparer.Compare(parent.value, current.value);
				if (result > 0)
					// parent.Value > current.Value, so make current's left child a left child of parent
					parent.Left = current.Left;
				else if (result < 0)
					// parent.Value < current.Value, so make current's left child a right child of parent
					parent.Right = current.Left;
			}
		}
		// CASE 2: If current's right child has no left child, then current's right child
		//         replaces current in the tree
		else if (current.Right.Left == null) {
			current.Right.Left = current.Left;

			if (parent == null)
				root = current.Right;
			else {
				result = comparer.Compare(parent.value, current.value);
				if (result > 0)
					// parent.Value > current.Value, so make current's right child a left child of parent
					parent.Left = current.Right;
				else if (result < 0)
					// parent.Value < current.Value, so make current's right child a right child of parent
					parent.Right = current.Right;
			}
		}
		// CASE 3: If current's right child has a left child, replace current with current's
		//          right child's left-most descendent
		else {
			// We first need to find the right node's left-most child
			TreeNode<T> leftmost = current.Right.Left, lmParent = current.Right;
			while (leftmost.Left != null) {
				lmParent = leftmost;
				leftmost = leftmost.Left;
			}

			// the parent's left subtree becomes the leftmost's right subtree
			lmParent.Left = leftmost.Right;

			// assign leftmost's left and right to current's left and right children
			leftmost.Left = current.Left;
			leftmost.Right = current.Right;

			if (parent == null)
				root = leftmost;
			else {
				result = comparer.Compare(parent.value, current.value);
				if (result > 0)
					// parent.Value > current.Value, so make leftmost a left child of parent
					parent.Left = leftmost;
				else if (result < 0)
					// parent.Value < current.Value, so make leftmost a right child of parent
					parent.Right = leftmost;
			}
		}

		return true;
	}

	 bool TryGetNodeWith( T item, out TreeNode<T> node, out TreeNode<T> parent ) {
		node = root;
		parent = null;
		int result = comparer.Compare(node.value, item);
		while (result != 0) {
			if (result > 0) {
				// current.Value > data, if data exists it's in the left subtree
				parent = node;
				node = node.Left;
			} else if (result < 0) {
				// current.Value < data, if data exists it's in the right subtree
				parent = node;
				node = node.Right;
			}

			// If current == null, then we didn't find the item to remove
			if (node == null)
				return false;
			else
				result = comparer.Compare(node.value, item);
		}
		return true;
	}

	public bool Contains( T item ) {
		return NodeContains(root, item);
	}

	bool NodeContains( TreeNode<T> node, T item ) {
		if (node == null)
			return false;
		if ( node.value.Equals( item ) )
			return true;
		if (NodeContains(node.Left, item))
			return true;
		return NodeContains(node.Right, item);
	}
	
	public void Clear() {
		root = null;
		Count = 0;
	}

	public T Min {
		get {
			if (root == null) {
				return default(T);
			}
			return NodeMin(root);
		}
	}

	private T NodeMin( TreeNode<T> node ) {
		if (node.Left == null) {
			return node.value;
		}
		return NodeMin(node.Left);
	}

	public T Max {
		get {
			if (root == null) {
				return default(T);
			}
			return NodeMax(root);
		}
	}

	private T NodeMax(TreeNode<T> node) {
		if (node.Right == null) {
			return node.value;
		}
		return NodeMax(node.Right);
	}
}

class TreeNode<T> where T : IComparable<T> {
	public T value;
	public TreeNode<T> Left { get; set; }
	public TreeNode<T> Right { get; set; }

	public TreeNode(T value) {
		this.value = value;
	}

}