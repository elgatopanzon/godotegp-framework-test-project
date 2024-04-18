/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ChainableStreamBuffer
 * @created     : Friday Mar 29, 2024 23:56:09 CST
 */

namespace GodotEGP.Tests.Chainables;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Objects.Validated;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using GodotEGP.Chainables;
using GodotEGP.Chainables.Exceptions;

public partial class ChainableStreamBuffer : TestContext
{
    [Fact]
    public void MergingObjectsString()
    {
    	var buffer = new GodotEGP.Chainables.ChainableStreamBuffer();

    	buffer.Add("1");
    	buffer.Add("2");
    	buffer.Add("3");

    	var merged = buffer.Merge();

    	Assert.Equal("123", merged);
    }

    [Fact]
    public void MergingObjectsInt()
    {
    	var buffer = new GodotEGP.Chainables.ChainableStreamBuffer();

    	buffer.Add(1);
    	buffer.Add(2);
    	buffer.Add(3);

    	var merged = buffer.Merge();

    	Assert.Equal(6, merged);
    }

    [Fact]
    public void MergingObjectsLong()
    {
    	var buffer = new GodotEGP.Chainables.ChainableStreamBuffer();

    	buffer.Add((long) 1);
    	buffer.Add((long) 2);
    	buffer.Add((long) 3);

    	var merged = buffer.Merge();

    	Assert.Equal((long) 6, merged);
    }

    [Fact]
    public void MergingObjectsDouble()
    {
    	var buffer = new GodotEGP.Chainables.ChainableStreamBuffer();

    	buffer.Add(1.1);
    	buffer.Add(2.2);
    	buffer.Add(3.3);

    	var merged = buffer.Merge();

    	Assert.Equal(6.6, merged);
    }

    [Fact]
    public void MergingObjectsByte()
    {
    	var buffer = new GodotEGP.Chainables.ChainableStreamBuffer();

    	buffer.Add((byte) 4);
    	buffer.Add((byte) 8);
    	buffer.Add((byte) 16);

    	var merged = buffer.Merge();

    	Assert.Equal((int) 28, merged);
    }

    [Fact]
    public void MergingObjectsMismatchedNumericTypes()
    {
    	var buffer = new GodotEGP.Chainables.ChainableStreamBuffer();

    	buffer.Add((long) 5);
    	buffer.Add(2);
    	buffer.Add(5.6);

    	Assert.Throws(typeof(ChainableStreamBufferMismatchingTypesException), () => buffer.Merge());
    }

    [Fact]
    public void MergingObjectsMismatchedTypes()
    {
    	var buffer = new GodotEGP.Chainables.ChainableStreamBuffer();

    	buffer.Add((long) 5);
    	buffer.Add("2");
    	buffer.Add(5.6);

    	Assert.Throws(typeof(ChainableStreamBufferMismatchingTypesException), () => buffer.Merge());
    }

    [Fact]
    public void MergingObjectsArrayInt()
    {
    	var buffer = new GodotEGP.Chainables.ChainableStreamBuffer();

    	var list1 = new int[] { 1, 2 };
    	var list2 = new int[] { 3, 4 };

    	buffer.Add(list1);
    	buffer.Add(list2);

    	var merged = buffer.Merge();

    	Assert.Equal(new int[] { 1,2,3,4 }, merged);
    }

    [Fact]
    public void MergingObjectsListObjects()
    {
    	var buffer = new GodotEGP.Chainables.ChainableStreamBuffer();

    	var list1 = new List<object>() { 1, "2" };
    	var list2 = new List<object>() { 3, "4" };

    	buffer.Add(list1);
    	buffer.Add(list2);

    	var merged = buffer.Merge();

    	Assert.Equal(new List<object>() { 1,"2",3,"4" }, merged);
    }

    [Fact]
    public void MergingObjectsListInt()
    {
    	var buffer = new GodotEGP.Chainables.ChainableStreamBuffer();

    	var list1 = new List<int>() { 1, 2 };
    	var list2 = new List<int>() { 3, 4 };

    	buffer.Add(list1);
    	buffer.Add(list2);

    	var merged = buffer.Merge();

    	Assert.Equal(new List<int>() { 1,2,3,4 }, merged);
    }

    [Fact]
    public void MergingObjectsDictionaryObjects()
    {
    	var buffer = new GodotEGP.Chainables.ChainableStreamBuffer();

    	var dict1 = new Dictionary<string, object>() { { "1", 1 } };
    	var dict2 = new Dictionary<string, object>() { { "2", 2 } };
    	var dict3 = new Dictionary<string, object>() { { "2", 3 } };

    	buffer.Add(dict1);
    	buffer.Add(dict2);
    	buffer.Add(dict3);

    	var merged = buffer.Merge();

    	Assert.Equal(new Dictionary<string, object>() { { "1", 1 }, { "2", 3 } }, merged);
    }
}
