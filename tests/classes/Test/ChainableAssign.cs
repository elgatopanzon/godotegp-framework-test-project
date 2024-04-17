/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ChainableAssign
 * @created     : Tuesday Apr 02, 2024 23:02:34 CST
 */

namespace GodotEGP.Test;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Objects.Validated;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using GodotEGP.Chainables;
using GodotEGP.Chainables.Extensions;

public partial class ChainableAssignTests : TestContext
{
	[Theory]
	[InlineData("tardis")]
    public async void ChainableAssignExecutionRunPassthrough(string value)
    {
    	// the ChainableAssign accepts a dictionary as input and on it's own
    	// will produce the same dictionary as the output
    	Dictionary<string, object> input = new() {
			{ "test1", value },
    	};

    	var c = new ChainableAssign();

    	Assert.Equivalent(input, await c.Run(input));
    }

	[Theory]
	[InlineData("tardis")]
    public async void ChainableAssignExecutionRunAssign(string value)
    {
    	// the ChainableAssign accepts a dictionary as input and on it's own
    	// will produce the same dictionary as the output
    	Dictionary<string, object> input = new() {
			{ "test1", value },
    	};

    	var c = new ChainableAssign();

    	c = c.Assign("test2", 123);

    	var res = await c.Run(input);

    	LoggerManager.LogDebug("Chain result", "", "res", res);

		Assert.Equal(value, res["test1"]);
		Assert.Equal(123, res["test2"]);
    }

	[Theory]
	[InlineData("tardis")]
    public async void ChainableAssignExecutionRunAssignChain(string value)
    {
    	// the ChainableAssign accepts a dictionary as input and on it's own
    	// will produce the same dictionary as the output
    	Dictionary<string, object> input = new() {
			{ "test1", value },
    	};

    	var c = new ChainableAssign();

		// the first chainable is a shortcut to create a ChainableGetValue with
		// the added param "key" to pick out and passthrough the value for the
		// provided key, which is then forwarded to the rest of the chain
		//
		// in this case, we choose to pick out the result of the key of test1
    	c = c.Assign("test2", c.ValueGetter("test1") | new ChainableNonStreamTest1() | new ChainableNonStreamTest2());

    	var res = await c.Run(input);

    	LoggerManager.LogDebug("Chain result", "", "res", res);

		Assert.Equal(value, res["test1"]);
		Assert.Equal(value+"xy", res["test2"]);
    }

	[Theory]
	[InlineData("tardis ftw")]
    public async void ChainableAssignExecutionRunAssignDictionaryChain(string value)
    {
    	var c = new ChainableNonStreamTest1() | new Dictionary<string, object>() {
			{ "assigned", new ChainableNonStreamTest2() },
    	};

		// the first chainable is a shortcut to create a ChainableGetValue with
		// the added param "key" to pick out and passthrough the value for the
		// provided key, which is then forwarded to the rest of the chain
		//
		// in this case, we choose to pick out the result of the key of test1
    	// c = c.Assign("test2", c.GetValue("test1") | new ChainableNonStreamTest1() | new ChainableNonStreamTest2());

    	var res = (Dictionary<string, object>) await c.Run(value);

    	LoggerManager.LogDebug("Chain result", "", "res", res);

		Assert.Equal(value+"xy", res["assigned"]);
    }
}

