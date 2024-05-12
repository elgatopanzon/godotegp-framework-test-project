/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : IndexMapTests
 * @created     : Saturday May 11, 2024 19:14:18 CST
 */

namespace GodotEGP.Tests.Collections;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using GodotEGP.Collections;

public partial class IndexMapTests : TestContext
{
	[Fact]
	public void IndexMapTests_test_set_get_data_low_index()
	{
		IndexMap<ulong> map = new();

		ulong val = 43834908374908;

		map.Set(0, val);
		map.Set(1, val);
		map.Set(2, val);
		map.Set(123, val);

		Assert.Equal(val, map[0]);
		Assert.Equal(val, map[1]);
		Assert.Equal(val, map[2]);
		Assert.Equal(val, map[123]);
	}

	[Fact]
	public void IndexMapTests_test_get_invalid()
	{
		IndexMap<ulong> map = new();

		ulong val = 43834908374908;

		map.Set(123, val);

		Assert.Equal(val, map[123]);
		Assert.Throws<IndexOutOfRangeException>(() => map[120]);
	}

	[Fact]
	public void IndexMapTests_test_set_get_data_high_index()
	{
		IndexMap<ulong> map = new();

		ulong val = 43834908374908;

		map.Set(38787687, val);

		Assert.Equal(val, map[38787687]);
	}

	[Fact]
	public void IndexMapTests_test_multiple_set()
	{
		IndexMap<ulong> map = new();

		ulong val1 = 43834908374908;
		ulong val2 = 349283240983;
		ulong val3 = 304930493409097;

		map.Set(123, val1);
		map.Set(456, val2);
		map.Set(789, val3);

		Assert.Equal(val1, map[123]);
		Assert.Equal(val2, map[456]);
		Assert.Equal(val3, map[789]);
	}

	[Fact]
	public void IndexMapTests_test_unset()
	{
		IndexMap<ulong> map = new();

		ulong val1 = 43834908374908;
		ulong val2 = 349283240983;
		ulong val3 = 304930493409097;

		map.Set(123, val1);
		map.Set(456, val2);
		map.Set(789, val3);

		bool res = map.Unset(456);
		Assert.True(res);

		res = map.Unset(456);
		Assert.False(res);

		res = map.Unset(1234);
		Assert.False(res);

		Assert.Equal(val1, map[123]);
		Assert.Equal(val3, map[789]);
	}

	[Fact]
	public void IndexMapTests_test_multi_set_unset()
	{
		IndexMap<ulong> map = new();

		ulong val1 = 43834908374908;
		ulong val2 = 349283240983;
		ulong val3 = 304930493409097;
		ulong val4 = 23829839389;
		ulong val5 = 98978382373;
		ulong val6 = 5212126726;

		map.Set(123, val1);
		map.Set(456, val2);
		map.Set(789, val3);
		map.Set(1010, val6);

		map.Unset(456);

		map.Set(456, val4);
		map.Set(457, val5);

		map.Unset(123);
		map.Set(123, val1);
		map.Unset(123);
		map.Set(123, val1);
		map.Unset(123);
		map.Set(123, val1);

		Assert.Equal(val1, map[123]);
		Assert.Equal(val4, map[456]);
		Assert.Equal(val5, map[457]);
		Assert.Equal(val6, map[1010]);
	}
}

