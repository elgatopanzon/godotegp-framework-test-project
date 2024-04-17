/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ChainableLambda
 * @created     : Friday Apr 05, 2024 17:01:38 CST
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


public partial class ChainableLambdaTests : TestContext
{
	[Fact]
    public async void ChainableLambdaRunSimple()
    {
    	var c = new ChainableLambda(async (x) => {
    		LoggerManager.LogDebug("x", "", "x", x);
    		return x;
    	});

    	Assert.Equal("test", await c.Run("test"));
    }

	[Fact]
    public async void ChainableLambdaRunTyped()
    {
    	var c = new ChainableLambda<string, string>(async (x) => {
    		return x+"s";
    	});

    	Assert.Equal("tests", await c.Run("test"));
    }

	[Fact]
    public async void ChainableLambdaRunFromMethod()
    {
    	var c = ChainableLambda<string, string>.FromMethod(TestMethod);

    	Assert.Equal("testing", await c.Run("test"));
    }

	[Fact]
    public async void ChainableLambdaRunFromMethodAsync()
    {
    	var c = ChainableLambda<string, string>.FromMethodAsync(TestMethodAsync);

    	Assert.Equal("testing", await c.Run("test"));
    }

	[Theory]
	[InlineData(new int[] {2,3,4,5}, 10, new int[] {21,31,41,51})]
    public async void ChainableLambdaStream(int[] genNumbers, int input, int[] expectedChunks)
    {
		var c = new ChainableStreamingTestGenerateNumbers() { GenNumbers = genNumbers } 
			| new ChainableLambda<int, int>(async (x) => {
				return x + 1;
				});

		List<int?> outputChunks = new();
		await foreach (var number in c.Stream(input))
		{
			LoggerManager.LogDebug("Output number", "", "output", number);

			outputChunks.Add((number as int?));

			Assert.Equal(expectedChunks[outputChunks.Count - 1], outputChunks.Last());
		}
    }

	[Fact]
    public async void ChainableLambdaRunFromMethodWithParams()
    {
    	var c = ChainableLambda.FromMethod(TestMethodParams);
    	c = c.Param("character", "ers");

    	Assert.Equal("testers", await c.Run("test"));
    }


	[Fact]
    public async void ChainableLambdaRunFromMethodWithInvalidParams()
    {
    	var c = ChainableLambda.FromMethod(TestMethodParams);
    	c = c.Param("character_invalid", "ers");

		await Assert.ThrowsAsync<ChainableLambdaDynamicMethodParameterMissingException>(async() => await c.Run("test"));
    }

    public string TestMethod(string input)
    {
    	return input+"ing";
    }

    public async Task<string> TestMethodAsync(string input)
    {
    	return input+"ing";
    }

    public string TestMethodParams(string input, string character)
    {
    	return input + character;
    }
}

