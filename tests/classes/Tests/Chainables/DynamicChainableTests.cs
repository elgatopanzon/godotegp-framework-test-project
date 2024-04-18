/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : DynamicChainable
 * @created     : Friday Apr 05, 2024 16:10:48 CST
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
using GodotEGP.Chainables.Extensions;

public partial class DynamicChainableTests : TestContext
{
	[Fact]
    public async void DynamicChainableExecution()
    {
    	var c = new DynamicChainable();

    	var realChain = new ChainableNonStreamingTestWrapBraces();

    	c.Target = realChain;

    	Assert.Equal("{test}", await c.Run("test"));
    }

	[Fact]
    public async void DynamicChainableExecution2()
    {
    	var c = new DynamicChainable();

    	var parallelChain = new ChainableParallel(new() {
			{ "simplerun1", new ChainableNonStreamTest2() },
			{ "simplerun2", new ChainableStreamingTestWrapTags() },
    		});

    	c.Target = parallelChain;

    	var res = await c.Run("kittens");

    	LoggerManager.LogDebug("DynamicChainable result", "", "res", res);

    	Assert.Equivalent(new Dictionary<string, object>() {
			{ "simplerun1", "kittensy" },
			{ "simplerun2", "<kittens>" },
    		}, res);
    }
}
