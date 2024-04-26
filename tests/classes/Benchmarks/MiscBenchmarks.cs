/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : MiscBenchmarks
 * @created     : Thursday Apr 25, 2024 19:33:14 CST
 */

namespace GodotEGP.Tests.Benchmarks;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public partial class MiscBenchmarks : TestContext
{
#if (!DEBUG)
	[Fact]
	public void MiscBenchmarks_bitwise_long_to_2_ints()
	{
		BenchmarkRunner.Run<MiscBenchmarks_BitwiseLong2Int>();
	}

	[Fact]
	public void MiscBenchmarks_struct_layout_long_to_2_ints()
	{
		BenchmarkRunner.Run<MiscBenchmarks_StructLong2Int>();
	}
#endif
}

public partial class MiscBenchmarks_BitwiseLong2Int
{
	private ulong _testLong = 12345678910111213;
	private uint _testInt1 = 1576189421;
	private uint _testInt2 = 2874452;

	[Fact]
	[Benchmark]
	public void LongTo2Ints()
	{
		uint int1 = (uint)(_testLong);	
		uint int2 = (uint)(_testLong >> 32);	

		LoggerManager.LogDebug("long to 2 ints", "", "long", _testLong);
		LoggerManager.LogDebug("long to 2 ints", "", "int1", int1);
		LoggerManager.LogDebug("long to 2 ints", "", "int2", int2);
	}

	[Fact]
	[Benchmark]
	public void TwoIntsToLong()
	{
		ulong longCombined = (_testInt2 << 32) | _testInt1;

		LoggerManager.LogDebug("2 ints to long", "", "int1", _testInt1);
		LoggerManager.LogDebug("2 ints to long", "", "int2", _testInt2);
		LoggerManager.LogDebug("2 ints to long", "", "long", _testLong);
	}
}

public partial class MiscBenchmarks_StructLong2Int
{
	private ExplicitLayoutLong2Ints _struct = new() {
		Long = 12345678910111213,
	};

	[Fact]
	[Benchmark]
	public void LongTo2Ints()
	{
		uint int1 = _struct.Int1;
		uint int2 = _struct.Int2;

		LoggerManager.LogDebug("long struct to 2 ints", "", "long", _struct.Long);
		LoggerManager.LogDebug("int1 struct to 2 ints", "", "int1", int1);
		LoggerManager.LogDebug("int2 struct to 2 ints", "", "int2", int2);
	}
}

[StructLayout(LayoutKind.Explicit)]
public struct ExplicitLayoutLong2Ints
{
	[FieldOffset(0)]
	public ulong Long;

	[FieldOffset(0)]
	public uint Int1;

	[FieldOffset(32)]
	public uint Int2;
}
