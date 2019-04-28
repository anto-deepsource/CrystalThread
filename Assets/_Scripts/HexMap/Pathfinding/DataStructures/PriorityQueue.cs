using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexMap.Pathfinding {

	/// <summary>
	/// Acts as a Max Heap, keeping the highest value at the top.
	/// Also 'keys' each value, allowing efficient querys as to 
	/// whether a key/value pair exists in the heap or not as well as
	/// being able to remove a pair based on its key.
	/// </summary>
	/// <typeparam name="K">Key Type</typeparam>
	/// <typeparam name="T">Value Type</typeparam>
	public class PriorityQueue<K,T> where T: IComparable {

		#region Apriori Settings

		private const int initial_size = 10;

		#endregion

		#region Private Members

		/// <summary>
		/// The dynamic array of values.
		/// </summary>
		private T[] valueArray = new T[initial_size];

		/// <summary>
		/// Mirror of the value array of their corresponding keys.
		/// This is used if we need to go from value to key.
		/// </summary>
		private K[] keyArray = new K[initial_size];

		// NOTE: these dynamic arrays may at some point after some usage contain garbage values
		//			in unused indexes, which shouldn't matter.

		/// <summary>
		/// Maintains a map from any/all keys in the queue to their index in the heap/array.
		/// This is used if we need to go from key to value.
		/// </summary>
		private Dictionary<K, int> keyIndexMap = new Dictionary<K, int>();

		#endregion

		#region Public Properties

		public int Count { get; private set; }

		#endregion

		#region Public Methods

		public void Add(K key, T value) {
			GrowArrayMaybe();
			keyIndexMap[key] = ++Count;
			keyArray[Count] = key;
			valueArray[Count] = value;
			Swim(Count);
		}

		public void Remove(K key) {
			int index;
			if (keyIndexMap.TryGetValue(key, out index)) {
				RemoveAt(index);
			}
		}

		public T PopTop() {
			T result = valueArray[1];
			RemoveAt(1);
			return result;
		}

		public T PeekBest() {
			return valueArray[1];
		}

		public bool Contains(K key) {
			return keyIndexMap.ContainsKey(key);
		}

		public bool TryGet(K key, out T resultValue) {
			int index;
			if (keyIndexMap.TryGetValue(key, out index)) {
				resultValue = valueArray[index];
				return true;
			} else {
				resultValue = default(T);
				return false;
			}
		}

		public void Clear() {
			keyIndexMap.Clear();
			Count = 0;
		}

		#endregion

		#region Private Members

		private void GrowArrayMaybe() {
			if (Count == valueArray.Length - 1) {
				int newSize = valueArray.Length * 2;
				// grow the value array
				{
					T[] newArray = new T[newSize];
					for (int i = 0; i < valueArray.Length; i++)
						newArray[i] = valueArray[i];
					valueArray = newArray;
				}
				// grow the key array
				{
					K[] newArray = new K[newSize];
					for (int i = 0; i < keyArray.Length; i++)
						newArray[i] = keyArray[i];
					keyArray = newArray;
				}
			}
		}
		
		/// <summary>
		/// Heap algorithm: attempts to correct (if needed) the value at the given
		/// index according to the max heap policy in the upwards direction by checking
		/// it against its parent.
		/// </summary>
		/// <param name="index"></param>
		private void Swim(int index) {
			T value = valueArray[index];
			int parentIndex = (int)(index / 2);
			if (parentIndex <= 0)
				return;
			T parent = valueArray[parentIndex];
			if (Choose(parent, value)) {
				SwapValues( index, parentIndex);
				Swim(parentIndex);
			}
		}

		private void Sink(int index) {
			T value = valueArray[index];
			int leftIndex = index * 2;
			int rightIndex = index * 2 + 1;
			bool hasLeft = leftIndex <= Count;
			bool hasRight = rightIndex <= Count;
			T left = hasLeft ? valueArray[leftIndex] : default(T);
			T right = hasRight ? valueArray[rightIndex] : default(T);
			if (hasLeft && hasRight) {
				SwapBaseWithChildrenMaybe(index);
			}
			else if (hasLeft && Choose(value, left)) {
				SwapValues( index, leftIndex);
				Sink(leftIndex);
			}
			else if (hasRight && Choose(value, right)) {
				SwapValues( index, rightIndex);
				Sink(rightIndex);
			}

		}

		private void SwapValues( int index1, int index2) {
			if (index1 == index2)
				return;
			if (index1 < 0 || index1 >= valueArray.Length || 
					index2 < 0 || index2 >= valueArray.Length)
				throw new System.IndexOutOfRangeException();
			// Swap the values in the value array
			T tempValue = valueArray[index1];
			valueArray[index1] = valueArray[index2];
			valueArray[index2] = tempValue;

			// Swap the keys in the key array
			K tempKey = keyArray[index1];
			keyArray[index1] = keyArray[index2];
			keyArray[index2] = tempKey;

			// Update the key index map
			keyIndexMap[keyArray[index1]] = index1;
			keyIndexMap[keyArray[index2]] = index2;
		}

		private void SwapBaseWithChildrenMaybe(int index) {
			int leftIndex = index * 2;
			int rightIndex = index * 2 + 1;

			// Ascending: pick the lowest value
			T value = valueArray[index];
			T left = valueArray[leftIndex];
			T right = valueArray[rightIndex];
			bool valueOverLeft = Choose(value, left);
			bool valueOverRight = Choose(value, right);
			bool leftOverRight = Choose(left, right);

			// CompareTo will be negative if left < right, positive if left > right
			//if (value.CompareTo(left) < 0 && value.CompareTo(right) < 0) {
			if (!valueOverLeft && !valueOverRight) {
				// then the base is already the smallest value
				//} else if (left.CompareTo(value) < 0 && left.CompareTo(right) < 0) {
			}
			else if (valueOverLeft && !leftOverRight) {
				// then the left child is the lowest value
				SwapValues( index, leftIndex);
				Sink(leftIndex);
				//} else if (right.CompareTo(value) < 0 && right.CompareTo(left) < 0) {
			}
			else if (valueOverRight && leftOverRight) {
				// then the right child is the lowest value
				SwapValues( index, rightIndex);
				Sink(rightIndex);
			}
			else if (valueOverLeft && valueOverRight) {
				// then the two children are equal but still less than the parent
				SwapValues( index, leftIndex);
				Sink(leftIndex);
			}
		}

		private bool Choose(T left, T right) {
			int c = left.CompareTo(right);
			// c will be negative if right > left, positive if right < left
			// c will be negative if left < right, positive if left > right
			//if (Ascending) {
			return c > 0.0f;
			//}
			//else
			//return c < 0.0f;
		}

		private void RemoveAt(int index) {
			T result = valueArray[index];
			K key = keyArray[index];
			//valueArray[index] = default(T);
			//keyArray[index] = default(K);
			SwapValues(index, Count--);
			keyIndexMap.Remove(key);
			Sink(index);
		}

		#endregion
	}
}