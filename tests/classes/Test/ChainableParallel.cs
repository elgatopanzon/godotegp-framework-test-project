/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ChainableParallel
 * @created     : Wednesday Apr 03, 2024 19:41:45 CST
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

public partial class ChainableParallelTests : TestContext
{
	[Fact]
    public async void ChainableParallelRun()
    {
    	var parallelChain = new ChainableParallel(new() {
			{ "simplerun1", new ChainableNonStreamTest2() },
			{ "simplerun2", new ChainableStreamingTestWrapTags() },
    		});

    	var res = await parallelChain.Run("kittens");

    	LoggerManager.LogDebug("ChainableParallel result", "", "res", res);

    	Assert.Equivalent(new Dictionary<string, object>() {
			{ "simplerun1", "kittensy" },
			{ "simplerun2", "<kittens>" },
    		}, res);
    }

	[Fact]
    public async void ChainableParallelStream()
    {
    	var parallelChain = new ChainableParallel(new() {
			{ "streamrun1", new ChainableNonStreamTest2() },
			{ "streamrun2", new ChainableStreamingTestWrapTags() },
    		});

		// because ChainableParallel doesn't support streaming of the 2 inputs
		// before they are all complete, we only get a single output for this
		// one
		await foreach (var output in parallelChain.Stream("cats"))
		{
    		LoggerManager.LogDebug("ChainableParallel stream output", "", "output", output);

    		Assert.Equivalent(new Dictionary<string, object>() {
				{ "streamrun1", "catsy" },
				{ "streamrun2", "<cats>" },
    			}, output);
		}
    }

	[Fact]
    public async void ChainableParallelRunMaxConcurrency()
    {
    	var parallelChain = new ChainableParallel(new() {
			{ "x", new ChainableNonStreamTest1() },
			{ "y", new ChainableNonStreamTest2() },
			{ "tags", new ChainableStreamingTestWrapTags() },
    		});

    	parallelChain = parallelChain.Config(maxConcurrency:2);

    	var res = await parallelChain.Run("kittens");

    	LoggerManager.LogDebug("ChainableParallel result", "", "res", res);

    	Assert.Equivalent(new Dictionary<string, object>() {
			{ "x", "kittensx" },
			{ "y", "kittensy" },
			{ "tags", "<kittens>" },
    		}, res);
    }

	[Fact]
    public async void ChainableParallelStreamMaxConcurrency()
    {
    	var parallelChain = new ChainableParallel(new() {
			{ "streamrun1", new ChainableNonStreamTest2() },
			{ "streamrun2", new ChainableStreamingTestWrapTags() },
			{ "tags", new ChainableStreamingTestWrapTags() },
    		});

    	parallelChain = parallelChain.Config(maxConcurrency:2);

		// because ChainableParallel doesn't support streaming of the 2 inputs
		// before they are all complete, we only get a single output for this
		// one
		await foreach (var output in parallelChain.Stream("cats"))
		{
    		LoggerManager.LogDebug("ChainableParallel stream output", "", "output", output);

    		Assert.Equivalent(new Dictionary<string, object>() {
				{ "streamrun1", "catsy" },
				{ "streamrun2", "<cats>" },
				{ "tags", "<cats>" },
    			}, output);
		}
    }
}
