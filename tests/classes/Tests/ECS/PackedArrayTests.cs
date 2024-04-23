/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : PackedArrayTests
 * @created     : Saturday Apr 20, 2024 20:10:52 CST
 */

namespace GodotEGP.Tests.ECS;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

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
		foreach (var entry in parr.Array)
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

	[Fact]
	public void PackedArrayTests_stresstest_benchmark()
	{
		BenchmarkRunner.Run<PackedArrayTestsBenchmark>();
	}
}

public struct TestStruct
{
	public int TestInt;
	public double TestDouble;
	public string TestString;
}

public partial class PackedArrayTestsBenchmark
{
	private int _maxItems = 32;
	private int _maxItemsBigMultiplier = 100;
	private PackedArray<TestStruct> _parr;
	private PackedArrayDictBacked<TestStruct> _parrDict;
	private List<TestStruct> _list;
	private PackedArray<TestStruct> _parrBig;
	private PackedArrayDictBacked<TestStruct> _parrDictBig;
	private List<TestStruct> _listBig;

	[IterationSetup]
	public void Setup()
	{
		PArray_Setup();
		List_Setup();
	}

	public void PArray_Setup()
	{
		_parr = new PackedArray<TestStruct>(_maxItems);
		for (int i = 0; i < _maxItems; i++)
		{
			_parr.Add(new TestStruct() {
				TestInt = i,
			});
		}

		_parrBig = new PackedArray<TestStruct>(_maxItems * _maxItemsBigMultiplier);
		for (int i = 0; i < _maxItems * _maxItemsBigMultiplier; i++)
		{
			_parrBig.Add(new TestStruct() {
				TestInt = i,
			});
		}

		_parrDict = new PackedArrayDictBacked<TestStruct>(_maxItems);
		for (int i = 0; i < _maxItems; i++)
		{
			_parrDict.Add(new TestStruct() {
				TestInt = i,
			});
		}

		_parrDictBig = new PackedArrayDictBacked<TestStruct>(_maxItems * _maxItemsBigMultiplier);
		for (int i = 0; i < _maxItems * _maxItemsBigMultiplier; i++)
		{
			_parrDictBig.Add(new TestStruct() {
				TestInt = i,
			});
		}
	}

	public void List_Setup()
	{
		_list = new List<TestStruct>(_maxItems);
		for (int i = 0; i < _maxItems; i++)
		{
			_list.Add(new TestStruct() {
				TestInt = i,
			});
		}

		_listBig = new List<TestStruct>(_maxItems * _maxItemsBigMultiplier);
		for (int i = 0; i < _maxItems * _maxItemsBigMultiplier; i++)
		{
			_listBig.Add(new TestStruct() {
				TestInt = i,
			});
		}
	}

	[Benchmark]
	public void PArray_Small_RemoveAt()
	{
		_parr.RemoveAt(2);
	}

	[Benchmark]
	public void PArrayDictBacked_Small_RemoveAt()
	{
		_parrDict.RemoveAt(2);
	}

	[Benchmark]
	public void List_Small_RemoveAt()
	{
		_list.RemoveAt(2);
	}

	[Benchmark]
	public void PArray_Small_Insert()
	{
		_parr.RemoveAt(2);
		_parr.Insert(3, new TestStruct());
	}

	[Benchmark]
	public void PArrayDictBacked_Small_Insert()
	{
		_parrDict.RemoveAt(2);
		_parrDict.Insert(3, new TestStruct());
	}

	[Benchmark]
	public void List_Small_Insert()
	{
		_list.RemoveAt(2);
		_list.Insert(3, new TestStruct());
	}

	[Benchmark]
	public void PArray_Small_Iteration_Unordered()
	{
		int counter = 0;
		foreach (var entry in _parr.Array)
		{
			counter++;
		}
	}

	[Benchmark]
	public void PArrayDictBacked_Small_Iteration_Unordered()
	{
		int counter = 0;
		foreach (var entry in _parrDict.Array)
		{
			counter++;
		}
	}

	[Benchmark]
	public void PArray_Small_Iteration_Ordered()
	{
		int counter = 0;
		foreach (var entry in _parr.OrderedArray)
		{
			counter++;
		}
	}

	[Benchmark]
	public void PArrayDictBacked_Small_Iteration_Ordered()
	{
		int counter = 0;
		foreach (var entry in _parrDict.OrderedArray)
		{
			counter++;
		}
	}

	[Benchmark]
	public void List_Small_Iteration_Ordered()
	{
		int counter = 0;
		foreach (var entry in _list)
		{
			counter++;
		}
	}

	[Benchmark]
	public void PArray_Big_RemoveAt()
	{
		_parrBig.RemoveAt(2);
	}

	[Benchmark]
	public void PArrayDictBacked_Big_RemoveAt()
	{
		_parrDictBig.RemoveAt(2);
	}

	[Benchmark]
	public void List_Big_RemoveAt()
	{
		_listBig.RemoveAt(2);
	}

	[Benchmark]
	public void PArray_Big_Insert()
	{
		_parrBig.RemoveAt(2);
		_parrBig.Insert(3, new TestStruct());
	}

	[Benchmark]
	public void PArrayDictBacked_Big_Insert()
	{
		_parrBig.RemoveAt(2);
		_parrBig.Insert(3, new TestStruct());
	}

	[Benchmark]
	public void List_Big_Insert()
	{
		_listBig.RemoveAt(2);
		_listBig.Insert(3, new TestStruct());
	}

	[Benchmark]
	public void PArray_Big_Iteration_Unordered()
	{
		int counter = 0;
		foreach (var entry in _parrBig.Array)
		{
			counter++;
		}
	}

	[Benchmark]
	public void PArrayDictBacked_Big_Iteration_Unordered()
	{
		int counter = 0;
		foreach (var entry in _parrDictBig.Array)
		{
			counter++;
		}
	}

	[Benchmark]
	public void PArray_Big_Iteration_Ordered()
	{
		int counter = 0;
		foreach (var entry in _parrBig.OrderedArray)
		{
			counter++;
		}
	}

	[Benchmark]
	public void PArrayDictBacked_Big_Iteration_Ordered()
	{
		int counter = 0;
		foreach (var entry in _parrDictBig.OrderedArray)
		{
			counter++;
		}
	}

	[Benchmark]
	public void List_Big_Iteration_Ordered()
	{
		int counter = 0;
		foreach (var entry in _listBig)
		{
			counter++;
		}
	}
}
