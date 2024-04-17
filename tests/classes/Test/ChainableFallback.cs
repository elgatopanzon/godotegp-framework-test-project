/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ChainableFallback
 * @created     : Saturday Apr 06, 2024 16:29:20 CST
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

public partial class ChainableFallbackTests : TestContext
{
	[Fact]
	public async void ChainableFallbackRun()
	{
		var c = new ChainableFallback() {
			Chainable = new ChainableWillFail(),
			Fallbacks = new() {
				new ChainableWillFail(),
				new ChainableNonStreamTest2(),
				new ChainableNonStreamTest1(),
				new ChainableNonStreamingTestWrapBraces(),
				new ChainableNonStreamingTestWrapBrackets(),
			},
		};

		var res = await c.Run("badwolf");
		LoggerManager.LogDebug("Fallback output", "", "output", res);

		Assert.Equal("badwolfy", res);
	}

	[Fact]
	public async void ChainableFallbackStream()
	{
		var c = new ChainableFallback() {
			Chainable = new ChainableWillFail(),
			Fallbacks = new() {
				new ChainableWillFail(),
				new ChainableNonStreamTest1(),
				new ChainableNonStreamTest2(),
				new ChainableNonStreamingTestWrapBraces(),
				new ChainableNonStreamingTestWrapBrackets(),
			},
		};

		await foreach (var output in c.Stream("rose"))
		{
			LoggerManager.LogDebug("Stream fallback output", "", "output", output);

			Assert.Equal("rosex", output);
		}
	}

	[Fact]
	public async void ChainableFallbackRunExceptionHandling()
	{
		var c = new ChainableFallback() {
			Chainable = new ChainableWillFail(),
			Fallbacks = new() {
				new ChainableNonStreamingTestWrapBraces(),
			},
			FallbackExceptions = new(), // don't handle any exceptions
		};

		await Assert.ThrowsAsync(typeof(System.NotSupportedException), async () => {
			await c.Run("asd");	
			});

		c.FallbackExceptions = new() { typeof(NotSupportedException) };

		var res = await c.Run("badwolf");
		LoggerManager.LogDebug("Fallback output", "", "output", res);

		Assert.Equal("{badwolf}", res);
	}

	[Fact]
	public async void ChainableFallbackRunExceptionKey()
	{
		var c = new ChainableFallback() {
			Chainable = new ChainableWillFail(),
			Fallbacks = new() {
				new ChainableWillFail(),
				new ChainablePassthrough(),
			},
			ExceptionKey = "Exception",
		};

		var inputDict = new Dictionary<string, object>() {{ "input", "toast"}};

		var res = await c.Run(inputDict);
		LoggerManager.LogDebug("Fallback exception key output", "", "output", res);

		Assert.Equivalent(inputDict, res);
	}

	[Fact]
	public async void ChainableFallbackStreamCoerce()
	{
		var c = new ChainableFallback() {
			Chainable = new ChainableWillFail(),
			Fallbacks = new() {
				new ChainableWillFail(),
				new ChainablePassthrough(),
				new ChainableNonStreamTest2(),
				new ChainableNonStreamingTestWrapBraces(),
				new ChainableNonStreamingTestWrapBrackets(),
			},
		};

		await foreach (int output in c.Stream<string, int>("55"))
		{
			LoggerManager.LogDebug("Stream fallback output", "", "output", output);

			Assert.Equal(55, output);
		}
	}
}
