using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitTests {

	[Test]
	public void PassTest() {

	}

	[Test]
	public void FailTest() {
		Assert.Fail();
	}

	[Test]
	public void ThrowExceptionTest() {
		Assert.That(() => { throw new ArgumentNullException(); }, Throws.ArgumentNullException);
	}

	/// <summary>
	/// Using a system of pow2 numbers/types and a flag that can be any combination of those types, returns true if the given value/flag is of the given type.
	/// </summary>
	/// <param name="value"></param>
	/// <param name="compare"></param>
	/// <returns></returns>
	public bool IsA( int value, int type ) {
		return (value & type) > 0;
	}

	[Test]
	public void BitwiseAnd() {
		int type1 = 2;
		int type2 = 4;
		int type3 = 8;

		Assert.IsTrue(!IsA(type1, type2));
		Assert.IsTrue(!IsA(type2, type3));
		Assert.IsTrue(!IsA(type1, type3));

		int testType1 = type1 + type2;
		Assert.IsTrue(IsA(testType1, type1));
		Assert.IsTrue(IsA(testType1, type2));
		Assert.IsTrue(!IsA(testType1, type3));

		int testType2 = type2;
		Assert.IsTrue(!IsA(testType2, type1));
		Assert.IsTrue(IsA(testType2, type2));
		Assert.IsTrue(!IsA(testType2, type3));
		
		int testType3 = type1 + type2 + type3;
		Assert.IsTrue(IsA(testType3, type1));
		Assert.IsTrue(IsA(testType3, type2));
		Assert.IsTrue(IsA(testType3, type3));
	}

	[Test]
	public void ObjectIntValue() {
		Assert.IsTrue(BlackboardEventType.Staggered.GetHashCode() == 2);
		Assert.IsTrue(BlackboardEventType.StaggeredEnd.GetHashCode() == 4);
	}
}
