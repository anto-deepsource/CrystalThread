using System;
using System.Collections.Generic;

class Heap<T> where T : IComparable {
	static int initial_size = 10;
	public T[] underlyingArray = new T[initial_size];
	public int Count { get; set; }
	public bool Sorted { get; set; }
	public bool Ascending { get; set; }
	
	public void Add(T value) {
		GrowArrayMaybe();
		underlyingArray[++Count] = value;
		Swim(Count);
	}
	
	public T PopTop() {
		T result = underlyingArray[1];
		T lastValue = underlyingArray[Count--];
		underlyingArray[1] = lastValue;
		underlyingArray[Count + 1] = default(T);
		
		Sink(1);
		return result;
	}
	
	public T[] Sort(bool trim = true) {
		int startLength = Count;

		while (Count > 0) {
			T topValue = PopTop();
			underlyingArray[Count + 1] = topValue;
		}

		Sorted = true;

		if (!trim)
			return underlyingArray;
		else
			return Sublist(underlyingArray, 1, startLength + 1);
	}
	
	public void BuildHeap(List<T> list ) {
		Count = 0;
		foreach( var v in list ) {
			Add(v);
		}
	}

	public void BuildHeap(T[] list) {
		Count = 0;
		foreach (var v in list) {
			Add(v);
		}
	}

	void GrowArrayMaybe() {
		if ( Count == underlyingArray.Length-1 ) {
			int newSize = underlyingArray.Length*2;
			T[] newArray = new T[newSize];
			for ( int i = 0; i < underlyingArray.Length; i++ )
				newArray[i] = underlyingArray[i];
			underlyingArray = newArray;
		}
	}

	void Sink( int index ) {
		T value = underlyingArray[index];
		int leftIndex = index * 2;
		int rightIndex = index * 2 + 1;
		bool hasLeft = leftIndex <= Count;
		bool hasRight = rightIndex <= Count;
		T left = hasLeft ? underlyingArray[leftIndex] : default(T);
		T right = hasRight ? underlyingArray[rightIndex] : default(T);
		if ( hasLeft && hasRight ) {
			SwapBaseWithChildrenMaybe(index);
		} else if ( hasLeft && Choose(value, left)) {
			SwapValues(underlyingArray, index, leftIndex);
			Sink(leftIndex);
		} else if ( hasRight && Choose(value, right)) {
			SwapValues(underlyingArray, index, rightIndex);
			Sink(rightIndex);
		}
		
	}

	void SwapBaseWithChildrenMaybe(int index) {
		int leftIndex = index * 2;
		int rightIndex = index * 2 + 1;

		// Ascending: pick the lowest value
		T value = underlyingArray[index];
		T left = underlyingArray[leftIndex];
		T right = underlyingArray[rightIndex];
		bool valueOverLeft = Choose(value, left);
		bool valueOverRight = Choose(value, right);
		bool leftOverRight = Choose(left, right);

		// CompareTo will be negative if left < right, positive if left > right
		//if (value.CompareTo(left) < 0 && value.CompareTo(right) < 0) {
		if (!valueOverLeft && !valueOverRight) {
			// then the base is already the smallest value
		//} else if (left.CompareTo(value) < 0 && left.CompareTo(right) < 0) {
		} else if (valueOverLeft && !leftOverRight) {
			// then the left child is the lowest value
			SwapValues(underlyingArray, index, leftIndex);
			Sink(leftIndex);
		//} else if (right.CompareTo(value) < 0 && right.CompareTo(left) < 0) {
		} else if (valueOverRight && leftOverRight) {
			// then the right child is the lowest value
			SwapValues(underlyingArray, index, rightIndex);
			Sink(rightIndex);
		} else if (valueOverLeft && valueOverRight) {
			// then the two children are equal but still less than the parent
			SwapValues(underlyingArray, index, leftIndex);
			Sink(leftIndex);
		}
	}

	/// <summary>
	/// Chooses either the left element or the right element based on the comparison between them and whether the heap is set to ascending or descending
	/// 
	/// </summary>
	/// <param name="left"></param>
	/// <param name="right"></param>
	/// <returns> true for left, false for right </returns>
	bool Choose( T left, T right ) {
		int c = left.CompareTo(right);
		// c will be negative if right > left, positive if right < left
		// c will be negative if left < right, positive if left > right
		if (Ascending) {
			return c > 0.0f;
		} else
			return c < 0.0f;
	}

	void Swim( int index ) {
		T value = underlyingArray[index];
		int parentIndex = (int)(index / 2);
		if (parentIndex <= 0)
			return;
		T parent = underlyingArray[parentIndex];
		if (Choose( parent, value ) ) {
			SwapValues(underlyingArray, index, parentIndex);
			Swim(parentIndex);
		}
	}

	public T this[int index] { get { return underlyingArray[index + 1]; } }

	private void SwapValues(T[] numbers, int index1, int index2) {
		if (index1 == index2)
			return;
		if (index1 < 0 || index1 >= numbers.Length || index2 < 0 || index2 >= numbers.Length)
			throw new System.IndexOutOfRangeException();
		T temp = numbers[index1];
		numbers[index1] = numbers[index2];
		numbers[index2] = temp;
	}

	/// <summary>
	/// Returns a new array that is a partial copy of the original, using only the values of 'start' (inclusive) to 'end' (exclusive)
	/// </summary>
	/// <param name="numbers">The original array</param>
	/// <param name="start">The index to start copying at (inclusive)</param>
	/// <param name="end">The index to copying to (exclusive)</param>
	/// <returns>A new array of size end minus start</returns>
	public T[] Sublist(T[] numbers, int start, int end) {
		T[] result = new T[end - start];
		int resultIndex = 0;
		for (int i = start; i < end; i++) {
			result[resultIndex++] = numbers[i];
		}

		return result;
	}
}