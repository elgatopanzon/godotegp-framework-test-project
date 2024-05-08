/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : PackedArrayTests
 * @created     : Saturday Apr 20, 2024 20:10:52 CST
 */

namespace GodotEGP.Tests.Collections;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;
using GodotEGP.Collections;

using GodotEGP.ECS;

public partial class PackedArrayTests : TestContext
{
	[Fact]
	public void PackedArrayTests_test_adding_data()
	{
		PackedArray<int> arr = new(10);

		arr.Add(123);

		Assert.Equal(1, arr.Count);
		Assert.Equal(123, arr[0]);
	}

	[Fact]
	public void PackedArrayTests_test_removing_data()
	{
		PackedArray<int> arr = new(10);

		List<int> values = new() { 11, 22, 33, 44, 55, 66, 77, 88, 99, 1010 };

		// add values to the array
		foreach (var val in values)
		{
			arr.Add(val);
		}

		// remove an index from both
		arr.RemoveAt(3);
		values.RemoveAt(3);

		// remove the next index from the arr
		arr.RemoveAt(4);

		// remove the same index, because list shifted
		values.RemoveAt(3);

		// verify the order
		int counter = 0;
		foreach (var val in arr.OrderedArray)
		{
			LoggerManager.LogDebug("Array original value", "", counter.ToString(), values[counter]);
			LoggerManager.LogDebug("Array actual value", "", counter.ToString(), val);

			Assert.Equal(values[counter], val);

			counter++;
		}
	}

	[Fact]
	public void PackedArrayTests_test_removing_value()
	{
		PackedArray<int> arr = new(10);

		List<int> values = new() { 11, 22, 33, 44, 55, 66, 77, 88, 99, 1010 };

		// add values to the array
		foreach (var val in values)
		{
			arr.Add(val);
		}

		// remove some of the items from both
		arr.Remove(66);
		values.Remove(66);

		arr.Remove(99);
		values.Remove(99);

		arr.Remove(11);
		values.Remove(11);

		arr.Remove(88);
		values.Remove(88);

		arr.Remove(1010);
		values.Remove(1010);

		// verify the order
		int counter = 0;
		foreach (var val in arr.OrderedArray)
		{
			LoggerManager.LogDebug("Array original value", "", counter.ToString(), values[counter]);
			LoggerManager.LogDebug("Array actual value", "", counter.ToString(), val);

			Assert.Equal(values[counter], val);

			counter++;
		}
	}

	[Fact]
	public void PackedArrayTests_stresstest_vs_list()
	{
		int maxItems = 32;

		var watch = System.Diagnostics.Stopwatch.StartNew();
		var parr = new PackedArray<TestStruct>(maxItems);
		for (int i = 0; i < maxItems; i++)
		{
			parr.Add(new TestStruct() {
				TestInt = i,
			});
		}
		watch.Stop();

		LoggerManager.LogDebug("PackedArray init", "", "time", watch.Elapsed.TotalMilliseconds);

		watch = System.Diagnostics.Stopwatch.StartNew();
		var list = new List<TestStruct>(maxItems);
		for (int i = 0; i < maxItems; i++)
		{
			list.Add(new TestStruct() {
				TestInt = i,
			});
		}
		watch.Stop();

		LoggerManager.LogDebug("List init", "", "time", watch.Elapsed.TotalMilliseconds);




		watch = System.Diagnostics.Stopwatch.StartNew();
		parr.RemoveAt(2);
		watch.Stop();

		LoggerManager.LogDebug("PackedArray RemoveAt", "", "time", watch.Elapsed.TotalMilliseconds);


		watch = System.Diagnostics.Stopwatch.StartNew();
		list.RemoveAt(2);
		watch.Stop();

		LoggerManager.LogDebug("List RemoveAt", "", "time", watch.Elapsed.TotalMilliseconds);




		watch = System.Diagnostics.Stopwatch.StartNew();
		parr.Insert(3, new TestStruct());
		watch.Stop();

		LoggerManager.LogDebug("PackedArray Insert", "", "time", watch.Elapsed.TotalMilliseconds);

		watch = System.Diagnostics.Stopwatch.StartNew();
		list.Insert(3, new TestStruct());
		watch.Stop();

		LoggerManager.LogDebug("List Insert", "", "time", watch.Elapsed.TotalMilliseconds);


		watch = System.Diagnostics.Stopwatch.StartNew();
		int counter = 0;
		foreach (var entry in parr.ArraySegment)
		{
			counter++;
		}
		watch.Stop();

		LoggerManager.LogDebug("PackedArray unordered iteration", "", "time", watch.Elapsed.TotalMilliseconds);


		watch = System.Diagnostics.Stopwatch.StartNew();
		int counter1 = 0;
		foreach (var entry in parr.OrderedArray)
		{
			counter1++;
		}
		watch.Stop();

		LoggerManager.LogDebug("PackedArray ordered iteration", "", "time", watch.Elapsed.TotalMilliseconds);

		watch = System.Diagnostics.Stopwatch.StartNew();
		var counter2 = 0;
		foreach (var entry in list)
		{
			counter2++;
		}
		watch.Stop();

		LoggerManager.LogDebug("List ordered iteration", "", "time", watch.Elapsed.TotalMilliseconds);
	}
}

public struct TestStruct
{
	public int TestInt;
	public double TestDouble;
	public string TestString;
}
