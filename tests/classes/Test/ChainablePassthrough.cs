/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ChainablePassthrough
 * @created     : Friday Apr 05, 2024 15:21:36 CST
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

public partial class ChainablePassthroughTests : TestContext
{
	[Fact]
    public async void ChainableParallelPassthrough()
    {
    	var parallelPassthroughChain = new ChainableParallel(new() {
			{ "original", new ChainablePassthrough() }, // pass through the input
			{ "additional", new ChainablePassthrough().Assign("edited", new ChainableLambda<Dictionary<string, object>, int>(async (x) => {
				return (int) x["number"] * 3;
					})) }, // add a key to the output
			{ "additional2", new ChainablePassthrough().Assign("edited2", new ChainableLambda<Dictionary<string, object>, int>(async (x) => {
				return (int) x["number"] * 6;
					})) }, // add a key to the output
			{ "modified", new ChainableLambda<Dictionary<string, object>, int>(async (x) => {
				return (int) x["number"] + 1;
					}) }, // different return type
    		});

    	var res = await parallelPassthroughChain.Run(new Dictionary<string, object> { { "number", 1 } });

    	LoggerManager.LogDebug("ChainableParallel result", "", "res", res);

    	Assert.Equivalent(new Dictionary<string, object>() {
    			{ "original", new Dictionary<string, object> { { "number", 1 } } },
    			{ "additional", new Dictionary<string, object> { { "number", 1 }, { "edited", 3 } } },
    			{ "modified", 2 },
    		}, res);
    }
}
