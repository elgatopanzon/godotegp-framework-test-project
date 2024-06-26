/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : PackedArrayTests
 * @created     : Saturday Apr 20, 2024 20:10:52 CST
 */

namespace GodotEGP.Benchmarks.Collections;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;
using GodotEGP.Collections;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

using GodotEGP.ECS;

public partial class PackedArrayBenchmarks
{
#if (!DEBUG)
	[Fact]
	public void PackedArrayBenchmarks_Access()
	{
		BenchmarkRunner.Run<PackedArrayBenchmark_Access>();
		BenchmarkRunner.Run<PackedArrayBenchmark_Big_Access>();
	}
	[Fact]
	public void PackedArrayBenchmarks_RemoveAt()
	{
		BenchmarkRunner.Run<PackedArrayBenchmark_RemoveAt>();
		BenchmarkRunner.Run<PackedArrayBenchmark_Big_RemoveAt>();
	}
	[Fact]
	public void PackedArrayBenchmarks_Insert()
	{
		BenchmarkRunner.Run<PackedArrayBenchmark_Insert>();
		BenchmarkRunner.Run<PackedArrayBenchmark_Big_Insert>();
	}
	[Fact]
	public void PackedArrayBenchmarks_Iteration()
	{
		BenchmarkRunner.Run<PackedArrayBenchmark_Iteration>();
		BenchmarkRunner.Run<PackedArrayBenchmark_Big_Iteration>();
	}
#endif
}

public struct TestStruct
{
	public int TestInt;
	public double TestDouble;
	public string TestString;
}

public partial class PackedArrayBenchmarkBase
{
	protected int _maxItems = 32;
	protected int _maxItemsBigMultiplier = 3200;
	protected int _insertTestAmount = 32;
	protected TestStruct[] _arr;
	protected PackedArray<TestStruct> _parr;
	protected PackedArrayDictBacked<TestStruct> _parrDict;
	protected PackedArrayDictionary<TestStruct> _parrDictionary;
	protected List<TestStruct> _list;
	protected TestStruct[] _arrBig;
	protected PackedArray<TestStruct> _parrBig;
	protected PackedArrayDictBacked<TestStruct> _parrDictBig;
	protected PackedArrayDictionary<TestStruct> _parrDictionaryBig;
	protected List<TestStruct> _listBig;


	protected TestStruct[] _arrEmpty;
	protected PackedArray<TestStruct> _parrEmpty;
	protected PackedArrayDictBacked<TestStruct> _parrDictEmpty;
	protected PackedArrayDictionary<TestStruct> _parrDictionaryEmpty;
	protected List<TestStruct> _listEmpty;
	protected TestStruct[] _arrBigEmpty;
	protected PackedArray<TestStruct> _parrBigEmpty;
	protected PackedArrayDictBacked<TestStruct> _parrDictBigEmpty;
	protected PackedArrayDictionary<TestStruct> _parrDictionaryBigEmpty;
	protected List<TestStruct> _listBigEmpty;

	protected PackedDictionary<int, TestStruct> _pdict;
	protected PackedDictionary<int, TestStruct> _pdictBig;
	protected PackedDictionary<int, TestStruct> _pdictEmpty;
	protected PackedDictionary<int, TestStruct> _pdictBigEmpty;

	protected Dictionary<int, TestStruct> _dict;
	protected Dictionary<int, TestStruct> _dictBig;
	protected Dictionary<int, TestStruct> _dictEmpty;
	protected Dictionary<int, TestStruct> _dictBigEmpty;

	[IterationSetup]
	public void Setup()
	{
		Array_Setup();
		PArray_Setup();
		PDict_Setup();
		Dict_Setup();
		List_Setup();
	}

	public void Array_Setup()
	{
		_arr = new TestStruct[_maxItems];
		_arrBig = new TestStruct[_maxItems * _maxItemsBigMultiplier];
		_arrEmpty = new TestStruct[_maxItems];
		_arrBigEmpty = new TestStruct[_maxItems * _maxItemsBigMultiplier];
	}

	public void PArray_Setup()
	{
		_parr = new PackedArray<TestStruct>();
		for (int i = 0; i < _maxItems; i++)
		{
			_parr.Add(new TestStruct() {
				TestInt = i,
			});
		}

		_parrBig = new PackedArray<TestStruct>();
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

		_parrDictionary = new PackedArrayDictionary<TestStruct>(_maxItems);
		for (int i = 0; i < _maxItems; i++)
		{
			_parrDictionary.Add(new TestStruct() {
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

		_parrDictionaryBig = new PackedArrayDictionary<TestStruct>(_maxItems * _maxItemsBigMultiplier);
		for (int i = 0; i < _maxItems * _maxItemsBigMultiplier; i++)
		{
			_parrDictionaryBig.Add(new TestStruct() {
				TestInt = i,
			});
		}

		_parrEmpty = new PackedArray<TestStruct>(_maxItems);
		// for (int i = 0; i < _maxItems - _insertTestAmount; i++)
		// {
		// 	_parrEmpty.Add(new TestStruct() {
		// 		TestInt = i,
		// 	});
		// }

		_parrBigEmpty = new PackedArray<TestStruct>(_maxItems * _maxItemsBigMultiplier);
		// for (int i = 0; i < (_maxItems * _maxItemsBigMultiplier) - _insertTestAmount; i++)
		// {
		// 	_parrBigEmpty.Add(new TestStruct() {
		// 		TestInt = i,
		// 	});
		// }

		_parrDictEmpty = new PackedArrayDictBacked<TestStruct>(_maxItems);
		// for (int i = 0; i < _maxItems - _insertTestAmount; i++)
		// {
		// 	_parrDictEmpty.Add(new TestStruct() {
		// 		TestInt = i,
		// 	});
		// }

		_parrDictionaryEmpty = new PackedArrayDictionary<TestStruct>(_maxItems);
		// for (int i = 0; i < _maxItems - _insertTestAmount; i++)
		// {
		// 	_parrDictionaryEmpty.Add(new TestStruct() {
		// 		TestInt = i,
		// 	});
		// }

		_parrDictBigEmpty = new PackedArrayDictBacked<TestStruct>(_maxItems * _maxItemsBigMultiplier);
		// for (int i = 0; i < (_maxItems * _maxItemsBigMultiplier) - _insertTestAmount; i++)
		// {
		// 	_parrDictBigEmpty.Add(new TestStruct() {
		// 		TestInt = i,
		// 	});
		// }

		_parrDictionaryBigEmpty = new PackedArrayDictionary<TestStruct>(_maxItems * _maxItemsBigMultiplier);
		// for (int i = 0; i < (_maxItems * _maxItemsBigMultiplier) - _insertTestAmount; i++)
		// {
		// 	_parrDictionaryBigEmpty.Add(new TestStruct() {
		// 		TestInt = i,
		// 	});
		// }
	}

	public void PDict_Setup()
	{
		_pdict = new PackedDictionary<int, TestStruct>();
		for (int i = 0; i < _maxItems; i++)
		{
			_pdict.Add(i, new TestStruct() {
				TestInt = i,
			});
		}

		_pdictBig = new PackedDictionary<int, TestStruct>();
		for (int i = 0; i < _maxItems * _maxItemsBigMultiplier; i++)
		{
			_pdictBig.Add(i, new TestStruct() {
				TestInt = i,
			});
		}

		_pdictEmpty = new PackedDictionary<int, TestStruct>();
		_pdictBigEmpty = new PackedDictionary<int, TestStruct>();
	}

	public void Dict_Setup()
	{
		_dict = new Dictionary<int, TestStruct>();
		for (int i = 0; i < _maxItems; i++)
		{
			_dict.Add(i, new TestStruct() {
				TestInt = i,
			});
		}

		_dictBig = new Dictionary<int, TestStruct>();
		for (int i = 0; i < _maxItems * _maxItemsBigMultiplier; i++)
		{
			_dictBig.Add(i, new TestStruct() {
				TestInt = i,
			});
		}

		_dictEmpty = new Dictionary<int, TestStruct>();
		_dictBigEmpty = new Dictionary<int, TestStruct>();
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

		_listEmpty = new List<TestStruct>(_maxItems);
		_listBigEmpty = new List<TestStruct>(_maxItems * _maxItemsBigMultiplier);
	}
}

public partial class PackedArrayBenchmark_Access : PackedArrayBenchmarkBase
{
	[Benchmark(Baseline = true)]
	public void Array_Small_Access()
	{
		for (int i = 0; i < _maxItems; i++)
		{
			_arrEmpty[i] = _arr[i];
		}
	}

	[Benchmark]
	public void PArray_Small_Access()
	{
		for (int i = 0; i < _maxItems; i++)
		{
			_parrEmpty[i] = _parr[i];
		}
	}

	[Benchmark]
	public void PArrayDictBackedIndex_Small_Access()
	{
		for (int i = 0; i < _maxItems; i++)
		{
			_parrDictEmpty[i] = _parrDict[i];
		}
	}

	[Benchmark]
	public void PArrayDictBackedData_Small_Access()
	{
		for (int i = 0; i < _maxItems; i++)
		{
			_parrDictionaryEmpty[i] = _parrDictionary[i];
		}
	}

	[Benchmark]
	public void List_Small_Access()
	{
		for (int i = 0; i < _maxItems; i++)
		{
			_listEmpty.Add(_list[i]);
		}
	}

	[Benchmark]
	public void PDict_Small_Access()
	{
		for (int i = 0; i < _maxItems; i++)
		{
			_pdictEmpty.Insert(i, _pdict[i]);
		}
	}

	[Benchmark]
	public void Dict_Small_Access()
	{
		for (int i = 0; i < _maxItems; i++)
		{
			_dictEmpty[i] = _dict[i];
		}
	}
}

public partial class PackedArrayBenchmark_RemoveAt : PackedArrayBenchmarkBase
{
	[Benchmark(Baseline = true)]
	public void PArray_Small_RemoveAt()
	{
		_parr.RemoveAt(2);
	}

	[Benchmark]
	public void PArrayDictBackedIndex_Small_RemoveAt()
	{
		_parrDict.RemoveAt(2);
	}

	[Benchmark]
	public void PArrayDictBackedData_Small_RemoveAt()
	{
		_parrDictionary.RemoveAt(2);
	}

	[Benchmark]
	public void List_Small_RemoveAt()
	{
		_list.RemoveAt(2);
	}

	[Benchmark]
	public void PDict_Small_RemoveAt()
	{
		_pdict.Remove(_maxItems - 10);
	}

	[Benchmark]
	public void Dict_Small_RemoveAt()
	{
		_dict.Remove(_maxItems - 10);
	}
}

public partial class PackedArrayBenchmark_Insert : PackedArrayBenchmarkBase
{
	[Benchmark(Baseline = true)]
	public void Array_Small_Insert()
	{
		for (int i = 0; i < _insertTestAmount; i++)
		{
			_arr[i] = new TestStruct();
		}
	}

	[Benchmark]
	public void PArray_Small_Insert()
	{
		for (int i = 0; i < _insertTestAmount; i++)
		{
			_parrEmpty.Insert(i, new TestStruct());
		}
	}

	[Benchmark]
	public void PArrayDictBackedIndex_Small_Insert()
	{
		for (int i = 0; i < _insertTestAmount; i++)
		{
			_parrDictEmpty.Insert(i, new TestStruct());
		}
	}

	[Benchmark]
	public void PArrayDictBackedData_Small_Insert()
	{
		for (int i = 0; i < _insertTestAmount; i++)
		{
			_parrDictionaryEmpty.Insert(i, new TestStruct());
		}
	}

	[Benchmark]
	public void List_Small_Insert()
	{
		for (int i = 0; i < _insertTestAmount; i++)
		{
			_listEmpty.Insert(i, new TestStruct());
		}
	}

	[Benchmark]
	public void PDict_Small_Insert()
	{
		for (int i = 0; i < _insertTestAmount; i++)
		{
			_pdictEmpty.Add(i, new TestStruct());
		}
	}

	[Benchmark]
	public void Dict_Small_Insert()
	{
		for (int i = 0; i < _insertTestAmount; i++)
		{
			_dictEmpty.Add(i, new TestStruct());
		}
	}
}

public partial class PackedArrayBenchmark_Iteration : PackedArrayBenchmarkBase
{
	[Benchmark]
	public void Array_Small_Iteration_Unordered()
	{
		int counter = 0;
		foreach (var entry in _arr)
		{
			counter++;
		}
	}

	[Benchmark]
	public void PArray_Small_Iteration_Unordered()
	{
		int counter = 0;
		foreach (var entry in _parr.Span)
		{
			counter++;
		}
	}

	[Benchmark]
	public void PArray_Small_Iteration_TryNext()
	{
		int counter = 0;
		while (_parr.TryNext(out TestStruct val))
		{
			counter++;
		}
		_parr.Reset();
	}

	[Benchmark]
	public void PArrayDictBackedIndex_Small_Iteration_Unordered()
	{
		int counter = 0;
		foreach (var entry in _parrDict.Array)
		{
			counter++;
		}
	}

	[Benchmark]
	public void PArrayDictBackedData_Small_Iteration_Unordered()
	{
		int counter = 0;
		foreach (var entry in _parrDictionary.Array)
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
	public void PArrayDictBackedIndex_Small_Iteration_Ordered()
	{
		int counter = 0;
		foreach (var entry in _parrDict.OrderedArray)
		{
			counter++;
		}
	}

	[Benchmark]
	public void PArrayDictBackedData_Small_Iteration_Ordered()
	{
		int counter = 0;
		foreach (var entry in _parrDictionary.OrderedArray)
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
	public void PDict_Small_Iteration_Unordered_Keys()
	{
		int counter = 0;
		foreach (var entry in _pdict.Keys)
		{
			counter++;
		}
	}

	[Benchmark]
	public void PDict_Small_Iteration_Unordered_Values()
	{
		int counter = 0;
		foreach (var entry in _pdict.Values)
		{
			counter++;
		}
	}

	[Benchmark]
	public void Dict_Small_Iteration_Unordered_Keys()
	{
		int counter = 0;
		foreach (var entry in _dict.Keys)
		{
			counter++;
		}
	}

	[Benchmark]
	public void Dict_Small_Iteration_Unordered_Values()
	{
		int counter = 0;
		foreach (var entry in _dict.Values)
		{
			counter++;
		}
	}
}

public partial class PackedArrayBenchmark_Big_Access : PackedArrayBenchmarkBase
{
	[Benchmark(Baseline = true)]
	public void Array_Big_Access()
	{
		for (int i = 0; i < _maxItems * _maxItemsBigMultiplier; i++)
		{
			_arrBigEmpty[i] = _arrBig[i];
		}
	}

	[Benchmark]
	public void PArray_Big_Access()
	{
		for (int i = 0; i < _maxItems * _maxItemsBigMultiplier; i++)
		{
			_parrBigEmpty[i] = _parrBig[i];
		}
	}

	[Benchmark]
	public void PArrayDictBackedIndex_Big_Access()
	{
		for (int i = 0; i < _maxItems * _maxItemsBigMultiplier; i++)
		{
			_parrDictBigEmpty[i] = _parrDictBig[i];
		}
	}

	[Benchmark]
	public void PArrayDictBackedData_Big_Access()
	{
		for (int i = 0; i < _maxItems * _maxItemsBigMultiplier; i++)
		{
			_parrDictionaryBigEmpty[i] = _parrDictionaryBig[i];
		}
	}

	[Benchmark]
	public void List_Big_Access()
	{
		for (int i = 0; i < _maxItems * _maxItemsBigMultiplier; i++)
		{
			_listBigEmpty.Add(_listBig[i]);
		}
	}

	[Benchmark]
	public void PDict_Big_Access()
	{
		for (int i = 0; i < _maxItems * _maxItemsBigMultiplier; i++)
		{
			_pdictBigEmpty.Add(i, _pdictBig[i]);
		}
	}

	[Benchmark]
	public void Dict_Big_Access()
	{
		for (int i = 0; i < _maxItems * _maxItemsBigMultiplier; i++)
		{
			_dictBigEmpty[i] = _dictBig[i];
		}
	}
}

public partial class PackedArrayBenchmark_Big_RemoveAt : PackedArrayBenchmarkBase
{
	[Benchmark(Baseline = true)]
	public void PArray_Big_RemoveAt()
	{
		_parrBig.RemoveAt(2);
	}

	[Benchmark]
	public void PArrayDictBackedIndex_Big_RemoveAt()
	{
		_parrDictBig.RemoveAt(2);
	}

	[Benchmark]
	public void PArrayDictBackedData_Big_RemoveAt()
	{
		_parrDictionaryBig.RemoveAt(2);
	}

	[Benchmark]
	public void List_Big_RemoveAt()
	{
		_listBig.RemoveAt(2);
	}

	[Benchmark]
	public void PDict_Big_RemoveAt()
	{
		_pdictBig.Remove((_maxItems * _maxItemsBigMultiplier) - 10);
	}

	[Benchmark]
	public void Dict_Big_RemoveAt()
	{
		_dictBig.Remove((_maxItems * _maxItemsBigMultiplier) - 10);
	}
}

public partial class PackedArrayBenchmark_Big_Insert : PackedArrayBenchmarkBase
{
	[Benchmark(Baseline = true)]
	public void Array_Big_Insert()
	{
		for (int i = 0; i < _insertTestAmount; i++)
		{
			_arrBig[i] = new TestStruct();
		}
	}

	[Benchmark]
	public void PArray_Big_Insert()
	{
		for (int i = 0; i < (_insertTestAmount * _maxItemsBigMultiplier); i++)
		{
			_parrBigEmpty.Insert(i, new TestStruct());
		}
	}

	[Benchmark]
	public void PArrayDictBackedIndex_Big_Insert()
	{
		for (int i = 0; i < (_insertTestAmount * _maxItemsBigMultiplier); i++)
		{
			_parrDictBigEmpty.Insert(i, new TestStruct());
		}
	}

	[Benchmark]
	public void PArrayDictBackedData_Big_Insert()
	{
		for (int i = 0; i < (_insertTestAmount * _maxItemsBigMultiplier); i++)
		{
			_parrDictionaryBigEmpty.Insert(i, new TestStruct());
		}
	}

	[Benchmark]
	public void List_Big_Insert()
	{
		for (int i = 0; i < (_insertTestAmount * _maxItemsBigMultiplier); i++)
		{
			_listBigEmpty.Insert(i, new TestStruct());
		}
	}

	[Benchmark]
	public void PDict_Big_Insert()
	{
		for (int i = 0; i < _insertTestAmount * _maxItemsBigMultiplier; i++)
		{
			_pdictBigEmpty.Insert(i, new TestStruct());
		}
	}

	[Benchmark]
	public void Dict_Big_Insert()
	{
		for (int i = 0; i < _insertTestAmount * _maxItemsBigMultiplier; i++)
		{
			_dictBigEmpty.Add(i, new TestStruct());
		}
	}
}

public partial class PackedArrayBenchmark_Big_Iteration : PackedArrayBenchmarkBase
{
	[Benchmark(Baseline = true)]
	public void Array_Big_Iteration_Unordered()
	{
		int counter = 0;
		foreach (var entry in _arrBig)
		{
			counter++;
		}
	}

	[Benchmark]
	public void PArray_Big_Iteration_Unordered()
	{
		int counter = 0;
		foreach (var entry in _parrBig.Span)
		{
			counter++;
		}
	}

	[Benchmark]
	public void PArray_Big_Iteration_TryNext()
	{
		int counter = 0;
		while (_parrBig.TryNext(out TestStruct val))
		{
			counter++;
		}
		_parr.Reset();
	}

	[Benchmark]
	public void PArrayDictBackedIndex_Big_Iteration_Unordered()
	{
		int counter = 0;
		foreach (var entry in _parrDictBig.Array)
		{
			counter++;
		}
	}

	[Benchmark]
	public void PArrayDictBackedData_Big_Iteration_Unordered()
	{
		int counter = 0;
		foreach (var entry in _parrDictionaryBig.Array)
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
	public void PArrayDictBackedIndex_Big_Iteration_Ordered()
	{
		int counter = 0;
		foreach (var entry in _parrDictBig.OrderedArray)
		{
			counter++;
		}
	}

	[Benchmark]
	public void PArrayDictBackedData_Big_Iteration_Ordered()
	{
		int counter = 0;
		foreach (var entry in _parrDictionaryBig.OrderedArray)
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

	[Benchmark]
	public void PDict_Big_Iteration_Unordered_Keys()
	{
		int counter = 0;
		foreach (var entry in _pdictBig.Keys)
		{
			counter++;
		}
	}

	[Benchmark]
	public void PDict_Big_Iteration_Unordered_Values()
	{
		int counter = 0;
		foreach (var entry in _pdictBig.Values)
		{
			counter++;
		}
	}

	[Benchmark]
	public void Dict_Big_Iteration_Unordered_Keys()
	{
		int counter = 0;
		foreach (var entry in _dictBig.Keys)
		{
			counter++;
		}
	}

	[Benchmark]
	public void Dict_Big_Iteration_Unordered_Values()
	{
		int counter = 0;
		foreach (var entry in _dictBig.Values)
		{
			counter++;
		}
	}
}
