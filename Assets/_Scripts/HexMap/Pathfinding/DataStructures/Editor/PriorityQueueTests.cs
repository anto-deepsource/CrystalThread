using HexMap.Pathfinding;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PriorityQueueTests {

	[Test]
	public void Add1() {
		PriorityQueue<int, int> queue = new PriorityQueue<int, int>();

		queue.Add(1, 1);

		Assert.AreEqual(1, queue.Count);
	}

	[Test]
	public void Add2() {
		PriorityQueue<int, int> queue = new PriorityQueue<int, int>();

		int count = 0;

		for( int i = 0;i <= 200; i ++ ) {
			queue.Add( i*10, i);
			count++;
		}
		
		Assert.AreEqual(count, queue.Count);
	}

	[Test]
	public void PopTop1() {
		PriorityQueue<int, int> queue = new PriorityQueue<int, int>();

		int count = 0;

		for (int i = 0; i <= 200; i++) {
			queue.Add(i * 10, i);
			count++;
		}
		
		Assert.AreEqual(count, queue.Count);

		for( int i = 200; i > 0; i -- ) {
			Assert.AreEqual(i, queue.PopTop() );
		}
	}

	[Test]
	public void PopTop2() {
		PriorityQueue<int, int> queue = new PriorityQueue<int, int>();

		int count = 0;

		int[] values = new int[] { 9, 0, -15, 200, 90, 2, 12};

		for (int i = 0; i < values.Length; i++) {
			queue.Add(i, values[i]);
			count++;
		}

		Assert.AreEqual(count, queue.Count);
		Assert.AreEqual(values.Length, queue.Count);

		List<int> sortedValues = new List<int>(values);
		sortedValues.Sort();

		for (int i = 0; i < sortedValues.Count-1; i++) {
			Assert.LessOrEqual(sortedValues[i], sortedValues[i+1]);
		}
		for (int i = sortedValues.Count - 1; i >= 0 ; i--) {
			Assert.AreEqual(sortedValues[i], queue.PopTop());
		}
	}

	[Test]
	public void PopTop3() {
		PriorityQueue<int, int> queue = new PriorityQueue<int, int>();

		int count = 0;
		List<int> values = new List<int>();

		//string logString = "";

		for (int i = 0; i < 900; i++) {
			int newValue = Mathf.FloorToInt(UnityEngine.Random.value * 1000f - 500f);
			values.Add(newValue);
			//logString = logString + ", " + newValue;
		}

		//Debug.Log(logString);

		for (int i = 0; i < values.Count; i++) {
			queue.Add(i, values[i]);
			count++;
		}

		Assert.AreEqual(count, queue.Count);
		Assert.AreEqual(values.Count, queue.Count);

		List<int> sortedValues = new List<int>(values);
		sortedValues.Sort();

		for (int i = 0; i < sortedValues.Count - 1; i++) {
			Assert.LessOrEqual(sortedValues[i], sortedValues[i + 1]);
		}
		for (int i = sortedValues.Count - 1; i >= 0; i--) {
			Assert.AreEqual(sortedValues[i], queue.PopTop());
		}
	}

	/// <summary>
	/// Ensures that popping a value means that trying to get 
	/// that pair by key afterwards returns false.
	/// </summary>
	[Test]
	public void PopTop4() {
		PriorityQueue<int, int> queue = new PriorityQueue<int, int>();

		int count = 0;

		int[] values = new int[] { 9, 0, -15, 200, 90, 2, 12 };

		for (int i = 0; i < values.Length; i++) {
			queue.Add(i, values[i]);
			count++;
		}

		Assert.AreEqual(count, queue.Count);
		Assert.AreEqual(values.Length, queue.Count);

		List<int> sortedValues = new List<int>(values);
		sortedValues.Sort();

		for (int i = 0; i < sortedValues.Count - 1; i++) {
			Assert.LessOrEqual(sortedValues[i], sortedValues[i + 1]);
		}
		for (int i = sortedValues.Count - 1; i >= 0; i--) {
			Assert.AreEqual(sortedValues[i], queue.PopTop());
		}
		for (int i = 0; i < values.Length; i++) {
			int resultValue;
			Assert.IsFalse(queue.TryGet(i, out resultValue));
		}
	}

	[Test]
	public void Contains1() {
		PriorityQueue<int, int> queue = new PriorityQueue<int, int>();

		int count = 0;

		for (int i = 0; i <= 200; i++) {
			queue.Add(i * 10, i);
			count++;
		}

		Assert.AreEqual(count, queue.Count);

		for (int i = 0; i <= 200; i++) {
			Assert.IsTrue(queue.Contains(i * 10));
			Assert.IsFalse(queue.Contains(i * 10+1));
		}

		Assert.IsFalse(queue.Contains(-1));
		Assert.IsFalse(queue.Contains(99));
	}

	[Test]
	public void TryGet1() {
		PriorityQueue<int, int> queue = new PriorityQueue<int, int>();

		int times = 300;

		int count = 0;

		for (int i = 0; i < times; i++) {
			queue.Add(i * 10, i);
			count++;
		}

		Assert.AreEqual(count, queue.Count);

		for (int i = 0; i < times; i++) {
			int resultValue;
			bool result = queue.TryGet(i * 10, out resultValue);
			Assert.IsTrue(result);
			Assert.AreEqual(i, resultValue);
			Assert.IsFalse(queue.TryGet(i * 10 + 1, out resultValue));
		}
	}

	[Test]
	public void TryGet2() {
		PriorityQueue<int, int> queue = new PriorityQueue<int, int>();

		int count = 0;
		List<int> values = new List<int>();
		
		for (int i = 0; i < 900; i++) {
			int newValue = Mathf.FloorToInt(UnityEngine.Random.value * 1000f - 500f);
			values.Add(newValue);
		}
		
		for (int i = 0; i < values.Count; i++) {
			queue.Add(i, values[i]);
			count++;
		}

		Assert.AreEqual(count, queue.Count);
		Assert.AreEqual(values.Count, queue.Count);

		for (int i = 0; i < values.Count; i++) {
			int resultValue;
			bool result = queue.TryGet(i, out resultValue);
			Assert.IsTrue(result);
			Assert.AreEqual(values[i], resultValue);
		}
	}

	[Test]
	public void Remove1() {
		PriorityQueue<int, int> queue = new PriorityQueue<int, int>();

		int count = 0;

		int[] values = new int[] { 9, 0, -15, 200, 90, 2, 12 };

		for (int i = 0; i < values.Length; i++) {
			queue.Add(i, values[i]);
			count++;
		}

		Assert.AreEqual(count, queue.Count);
		Assert.AreEqual(values.Length, queue.Count);

		queue.Remove(2);

		int resultValue;

		for (int i = 0; i < values.Length; i++) {
			if ( i==2 ) {
				Assert.IsFalse(queue.TryGet(i, out resultValue));
			} else {
				bool result = queue.TryGet(i, out resultValue);
				Assert.IsTrue(result);
				Assert.AreEqual(values[i], resultValue);
			}
		}
	}

	[Test]
	public void Remove2() {
		PriorityQueue<int, int> queue = new PriorityQueue<int, int>();

		int count = 0;

		int[] values = new int[] { 9, 0, -15, 200, 90, 2, 12 };

		for (int i = 0; i < values.Length; i++) {
			queue.Add(i, values[i]);
			count++;
		}

		Assert.AreEqual(count, queue.Count);
		Assert.AreEqual(values.Length, queue.Count);

		int resultValue;

		for (int i = 0; i < values.Length; i++) {
			bool result = queue.TryGet(i, out resultValue);
			Assert.IsTrue(result);
			Assert.AreEqual(values[i], resultValue);

			queue.Remove(i);
			count--;

			Assert.IsFalse(queue.TryGet(i, out resultValue));
			Assert.AreEqual(count, queue.Count);
		}

		Assert.AreEqual(0, queue.Count);
	}

	[Test]
	public void Remove3() {
		PriorityQueue<int, int> queue = new PriorityQueue<int, int>();
		
		Dictionary<int,int> keyValues = new Dictionary<int, int>();

		AddSome(queue, keyValues, 100);

		for( int i = 0; i < 200; i ++ ) {
			RemoveSome(queue, keyValues, UnityEngine.Random.Range(1,10) );
			AddSome(queue, keyValues, UnityEngine.Random.Range(1, 10));
		}

		int resultValue;

		foreach ( var pair in keyValues ) {
			bool result = queue.TryGet(pair.Key, out resultValue);
			Assert.IsTrue(result);
			Assert.AreEqual(pair.Value, resultValue);
		}
	}

	private void AddSome(PriorityQueue<int, int> queue, Dictionary<int, int> keyValues, int count = 10) {
		Assert.AreEqual(keyValues.Count, queue.Count);
		
		for( int i = 0; i < count; i ++ ) {
			int newKey = UnityEngine.Random.Range(-10000, 10000);
			while (keyValues.ContainsKey(newKey)) {
				newKey = UnityEngine.Random.Range(-10000, 10000);
			}
			int newValue = UnityEngine.Random.Range(-10000, 10000);
			keyValues.Add(newKey, newValue);
			queue.Add(newKey, newValue);
		}

		Assert.AreEqual(keyValues.Count, queue.Count);
	}

	private void RemoveSome(PriorityQueue<int, int> queue, Dictionary<int, int> keyValues, int count = 10) {
		Assert.AreEqual(keyValues.Count, queue.Count);

		List<int> keys = new List<int>(keyValues.Keys);

		for (int i = 0; i < count && keyValues.Count > 0; i++) {
			int index = Mathf.FloorToInt(UnityEngine.Random.value * keyValues.Count);
			int key = keys[index];
			if (keyValues.ContainsKey(key)) {
				queue.Remove(key);
				keyValues.Remove(key);
			}
		}

		Assert.AreEqual(keyValues.Count, queue.Count);
	}

	/// <summary>
	/// Ensure that clearing the queue does just that, and that querying it for
	/// values that were there before no longer return anything and that using
	/// the queue thereafter acts the way it should
	/// </summary>
	[Test]
	public void Clear1() {
		PriorityQueue<int, int> queue = new PriorityQueue<int, int>();

		Dictionary<int, int> keyValues = new Dictionary<int, int>();

		AddSome(queue, keyValues, 100);

		for (int i = 0; i < 200; i++) {
			RemoveSome(queue, keyValues, UnityEngine.Random.Range(1, 10));
			AddSome(queue, keyValues, UnityEngine.Random.Range(1, 10));
		}

		int resultValue;

		foreach (var pair in keyValues) {
			bool result = queue.TryGet(pair.Key, out resultValue);
			Assert.IsTrue(result);
			Assert.AreEqual(pair.Value, resultValue);
		}

		queue.Clear();

		Assert.AreEqual(0, queue.Count);

		foreach (var pair in keyValues) {
			Assert.IsFalse(queue.TryGet(pair.Key, out resultValue));
		}

		keyValues.Clear();

		AddSome(queue, keyValues, 100);

		for (int i = 0; i < 200; i++) {
			RemoveSome(queue, keyValues, UnityEngine.Random.Range(1, 10));
			AddSome(queue, keyValues, UnityEngine.Random.Range(1, 10));
		}

		foreach (var pair in keyValues) {
			bool result = queue.TryGet(pair.Key, out resultValue);
			Assert.IsTrue(result);
			Assert.AreEqual(pair.Value, resultValue);
		}
	}
}