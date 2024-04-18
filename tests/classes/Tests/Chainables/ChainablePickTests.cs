/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ChainablePick
 * @created     : Friday Apr 05, 2024 22:34:18 CST
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

public partial class ChainablePickTests : TestContext
{
	[Fact]
	public async void ChainablePickRunSingleKey()
	{
		var c = new ChainableParallel(new() { 
			{ "test", new ChainableNonStreamingTestWrapBrackets()  }, // convert the input to a dictionary
			}) 
			| new ChainablePick("test"); // pick out the test key

		Assert.Equal(new Dictionary<string, object>() { { "test", "[test]" } }, await c.Run("test"));
	}

	[Fact]
	public async void ChainablePickRunMultipleKeys()
	{
		var c = new ChainableParallel(new() { 
			{ "test", new ChainableNonStreamingTestWrapBrackets()  }, // convert the input to a dictionary
			{ "test2", new ChainableNonStreamingTestWrapBraces()  }, // convert the input to a dictionary
			}) 
			| new ChainablePick(new string[] { "test", "test2" }); // pick out the test key

		Assert.Equal(new Dictionary<string, object>() { { "test", "[test]" }, { "test2", "{test}" } }, await c.Run("test"));
	}

	[Theory]
	[InlineData(new int[] {2,3,4,5}, 10, new string[] {"40","60","80","100"})]
    public async void ChainablePickStream(int[] genNumbers, int input, string[] expectedChunks)
    {
		var c = new ChainableStreamingTestGenerateNumbers() { GenNumbers = genNumbers } | new ChainableStreamingTestMultiplyNumbers2() | new ChainableLambda<int, Dictionary<string, object>>(async (x) => {
				Dictionary<string, object> dict = new() {
					{ "number", x },
					{ "number_2x", x * 2 },
				};

				return dict;
			})
			| new ChainablePick("number").Pick("number")
		;

		List<int?> outputChunks = new();
		await foreach (var number in c.Stream(input))
		{
			outputChunks.Add((number as int?));

			LoggerManager.LogDebug("Output stream", "", "output", number);

			Assert.Equal(expectedChunks[outputChunks.Count - 1], outputChunks.Last().ToString());
		}
    }
}

