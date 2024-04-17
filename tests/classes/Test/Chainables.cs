/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : Chainables
 * @created     : Friday Mar 29, 2024 15:40:05 CST
 */

namespace GodotEGP.Test;

using GodotEGP.Logging;
using GodotEGP.Chainables;
using GodotEGP.Chainables.Extensions;
using GodotEGP.Chainables.Exceptions;

public partial class Chainables : TestContext
{
	/**************************
	*  chain creation tests  *
	**************************/
	
    [Fact]
    public void CreateChainableStack()
    {
    	
		var c1 = new Chainable();
		var c2 = new Chainable();
		var c3 = new Chainable();

		List<IChainable> list = new() { c1, c2, c3 };

		var c = c1 | c2 | c3;

		Assert.Equal(c.Stack.Count, list.Count);

		for (int i = 0; i < list.Count; i++)
		{
			Assert.Equal(c.Stack[i].GetHashCode(), list[i].GetHashCode());
		}
    }


    /***********************
	*  chain passthrough   *
	***********************/

	[Theory]
	[InlineData("1", "1")]
    public async void ChainableExecutionRunSinglePassthrough(string input, string expected)
    {
		var c = new Chainable();

		Assert.Equal(await c.Run(input), expected);
    }

	[Theory]
	[InlineData("1", "1")]
    public async void ChainableExecutionRunChainPassthrough(string input, string expected)
    {
		var c = new Chainable() | new Chainable() | new Chainable();

		Assert.Equal(await c.Run(input), expected);
    }


    /***************************
	*  chain execution: Run()  *
	***************************/

	[Theory]
	[InlineData("1", "1x")]
    public async void ChainableExecutionRunSingle(string input, string expected)
    {
		var c = new ChainableNonStreamTest1();

		LoggerManager.LogDebug("Chainable interfaces", "", "interfaces", c.GetType().GetInterfaces());

		Assert.Equal(expected, await c.Run(input));
    }

	[Theory]
	[InlineData("1", "1xxx")]
    public async void ChainableExecutionRunChain(string input, string expected)
    {
		var c = new ChainableNonStreamTest1() | new ChainableNonStreamTest1() | new ChainableNonStreamTest1();

		Assert.Equal(expected, await c.Run(input));
    }

	[Theory]
	[InlineData("1", "1xyxxyx")]
    public async void ChainableExecutionRunMultipleChains(string input, string expected)
    {
		var c = new ChainableNonStreamTest1() | new ChainableNonStreamTest2() | new ChainableNonStreamTest1();

		Assert.Equal(expected, await (c | c).Run(input));
    }

	[Theory]
	[InlineData("1", "<1>")]
    public async void ChainableExecutionRunStreamOnly(string input, string expected)
    {
		var c = new ChainableStreamingTestWrapTags();

		Assert.Equal(expected, await c.Run(input));
    }

    /******************************
	*  chain execution: Batch()  *
	******************************/

	[Theory]
	[InlineData(new string[] { "item1", "item2" }, new string[] { "item1x", "item2x" })]
    public async void ChainableExecutionBatchSingle(string[] input, string[] expected)
    {
		var c = new ChainableNonStreamTest1();

		Assert.Equivalent(expected, await c.Batch(input));
    }

	[Theory]
	[InlineData(new string[] { "item1", "item2" }, new string[] { "item1xyx", "item2xyx" })]
    public async void ChainableExecutionBatchChain(string[] input, string[] expected)
    {
		var c = new ChainableNonStreamTest1() | new ChainableNonStreamTest2() | new ChainableNonStreamTest1();

		Assert.Equivalent(expected, await c.Batch(input));
    }


    /*******************************
	*  chain execution: Stream()  *
	*******************************/

	[Theory]
	[InlineData(new int[] {2,3,4,5}, 10, new int[] {20,30,40,50})]
    public async void ChainableExecutionStreamSingle(int[] genNumbers, int input, int[] expectedChunks)
    {
		var c = new ChainableStreamingTestGenerateNumbers() { GenNumbers = genNumbers };

		List<int?> outputChunks = new();
		await foreach (var number in c.Stream(input))
		{
			outputChunks.Add((number as int?));

			Assert.Equal(expectedChunks[outputChunks.Count - 1], outputChunks.Last());
		}
    }
    
	[Theory]
	[InlineData("hello", "{hello}")]
    public async void ChainableExecutionStreamSingleNonStreaming(string input, string expected)
    {
		var c = new ChainableNonStreamingTestWrapBraces();

		// since this is a non-streaming chainable, we expect the foreach loop
		// to simply return the buffered output as a single chunk
		await foreach (var output in c.Stream(input))
		{
			Assert.Equal(expected, output);
		}
    }

	[Theory]
	[InlineData(new int[] {2,3,4,5}, 10, new string[] {"40","60","80","100"})]
    public async void ChainableExecutionStreamMultipleChains(int[] genNumbers, int input, string[] expectedChunks)
    {
		var c = new ChainableStreamingTestGenerateNumbers() { GenNumbers = genNumbers } | new ChainableStreamingTestMultiplyNumbers2();

		List<int?> outputChunks = new();
		await foreach (var number in c.Stream(input))
		{
			outputChunks.Add((number as int?));

			Assert.Equal(expectedChunks[outputChunks.Count - 1], outputChunks.Last().ToString());
		}
    }

	[Theory]
	[InlineData(2, "{4}")]
    public async void ChainableExecutionStreamMixedEnd(int input, string expected)
    {
		var c = new ChainableStreamingTestMultiplyNumbers2() | new ChainableNonStreamingTestWrapBraces();

		// since this is a non-streaming chainable, we expect the foreach loop
		// to simply return the buffered output as a single chunk
		await foreach (var output in c.Stream(input))
		{
			Assert.Equal(expected, output);
		}
    }

	[Theory]
	[InlineData(new int[] {2,3}, 10, "<[100]>")]
    public async void ChainableExecutionStreamMixedMiddle(int[] genNumbers, int input, string expectedChunk)
    {
		var c = new ChainableStreamingTestGenerateNumbers() { GenNumbers = genNumbers } | new ChainableStreamingTestMultiplyNumbers2() | new ChainableNonStreamingTestWrapBrackets() | new ChainableStreamingTestWrapTags();

		await foreach (var output in c.Stream(input))
		{
			Assert.Equal(expectedChunk, output);
		}
    }

	[Theory]
	[InlineData(new int[] {8,4}, 5, "<[240]>")]
    public async void ChainableExecutionStreamMixedNonNativeStart(int[] genNumbers, int input, string expectedChunk)
    {
		var c = new ChainableNonStreanTestAdd5() | new ChainableStreamingTestGenerateNumbers() { GenNumbers = genNumbers } | new ChainableStreamingTestMultiplyNumbers2() | new ChainableNonStreamingTestWrapBrackets() | new ChainableStreamingTestWrapTags();

		// the starting object is a non-streaming chainable, so it will pass
		// it's final output to the next one which is a streaming chainable
		// however, since the rest of the chain contains a non-streaming
		// chainable the individual numbers will be sent as the final output,
		// yielding just 1 chunk containing the final output
		await foreach (var output in c.Stream(input))
		{
			Assert.Equal(expectedChunk, output);
		}
    }

	[Theory]
	[InlineData(2, "{4}")]
    public async void ChainableExecutionStreamSchemaValidationTest(int input, string expected)
    {
    	// this is a deliberately failing chain because the multiply numbers
    	// takes an int, not a string, tripping the chain schema mismatch
    	// exception during the chain creation stage
		await Assert.ThrowsAsync(typeof(ChainableChainSchemaMismatchException), async () => {
			var c = new ChainableStreamingTestMultiplyNumbers2() | new ChainableNonStreamingTestWrapBraces() | new ChainableStreamingTestMultiplyNumbers2();

			await foreach (var output in c.Stream(input))
			{
				Assert.Equal(expected, output);
			}
		});
    }

    /******************************
	*  chainable error handling  *
	******************************/
    
	[Theory]
	[InlineData("1")]
    public async void ChainableExecutionErrorHandlingSingle(string input)
    {
		var c = new ChainableWillFail();

		// since it's a single chainable, it should throw an exception here
		await Assert.ThrowsAnyAsync<Exception>(async () => await c.Run(input));
    }

	[Theory]
	[InlineData("1")]
    public async void ChainableExecutionErrorHandlingChain(string input)
    {
		var c = new ChainableNonStreamingTestWrapBrackets() | new ChainableWillFail() | new ChainableNonStreamingTestWrapBrackets();

		// if the chain throws an error because one of the chainables failed, then
		// it should gracefully be handled by the chain manager and it should end.
		// the expected output should therefore be null
		Assert.Equal(null, await c.Run(input));
    }

	[Theory]
	[InlineData(new string[] { "item1", "item2" }, new string[] { "item1x", "item2x" })]
    public async void ChainableExecutionErrorHandlingBatchSingle(string[] input, string[] expected)
    {
		var c = new ChainableWillFail();

		// since it's a single chainable, it should throw an exception here
		await Assert.ThrowsAnyAsync<Exception>(async () => await c.Batch(input));
    }

	[Theory]
	[InlineData(new int[] {2,3}, 10, "<[4060]>")]
    public async void ChainableExecutionErrorHandlingStream(int[] genNumbers, int input, string expectedChunk)
    {
		var c = new ChainableWillFailStream() | new ChainableStreamingTestMultiplyNumbers2() | new ChainableNonStreamingTestWrapBrackets() | new ChainableStreamingTestWrapTags();

		// in non-streaming mode this would gracefully fail during the
		// exception, but since it's a stream the exception should be thrown.
		// why? a half-streamed process would be harder to retry
		await Assert.ThrowsAnyAsync<Exception>(async () => {
			await foreach (var output in c.Stream(input))
			{
				Assert.Equal(expectedChunk, output);
			}
		});
    }

	/**********************************
	*  executing with custom params  *
	**********************************/
	[Theory]
	[InlineData("doctor ", "who?", "doctor who?")]
    public async void ChainableExecutionRunCustomParams(string input, string character, string expected)
    {
		var c = new ChainableNonStreamTest2();

		c.Config.Params = new() {
			{ "character", character },
		};

		// the RunWithCustomParams method reads the config object on the
		// chainable and invokes a matching method signature
		Assert.Equal(expected, await c.Run(input));
    }

	[Theory]
	[InlineData("doctor ", "who?", "doctor who?")]
    public async void ChainableExecutionRunCustomParamsInvalidType(string input, string character, string expected)
    {
		var c = new ChainableNonStreamTest2();

		c.Config.Params = new() {
			{ "character", 1 },
		};

		// the RunWithCustomParams method reads the config object on the
		// chainable and invokes a matching method signature
		await Assert.ThrowsAsync(typeof(System.ArgumentException), async () => {
			await c.Run(input);
		});
    }

	[Theory]
	[InlineData("asd", "d", "asdd")]
    public async void ChainableExecutionStreamCustomParams(string input, string character, string expected)
    {
    	// this is a deliberately failing chain because the multiply numbers
    	// takes an int, not a string, tripping the chain schema mismatch
    	// exception during the chain creation stage
		var c = new ChainableNonStreamTest2();

		c.Config.Params = new() {
			{ "character", character },
		};

		await foreach (var output in c.Stream(input))
		{
			Assert.Equal(expected, output);
		}
    }

	[Theory]
	[InlineData(new string[] { "item1", "item2" }, new string[] { "item1joke", "item2joke" })]
    public async void ChainableExecutionBatchCustomParams(string[] input, string[] expected)
    {
		var c = new ChainableNonStreamTest2();

		c.Config.Params = new() {
			{ "character", "joke" },
		};

		Assert.Equivalent(expected, await c.Batch(input));
    }


    /*******************************
	*  typed chainable execution  *
	*******************************/
    
	[Fact]
    public async void ChainableExecutionCoerceCasting()
    {
    	var c = new ChainableStreamingTestMultiplyNumbers2();
		int input = 10;
		int output = await c.Run<int, int>(input);

		LoggerManager.LogDebug("Coerced output", "", output.GetType().Name, output);

		Assert.Equal(input * 2, output);
    }

	[Fact]
    public async void ChainableExecutionCoerceCastingIntToDouble()
    {
    	var c = new ChainableStreamingTestMultiplyNumbers2();
		int input = 10;
		double output = await c.Run<int, double>(input);

		LoggerManager.LogDebug("Coerced output", "", output.GetType().Name, output);

		Assert.Equal(input * 2, output);
    }

	[Fact]
    public async void ChainableExecutionCoerceCastingDoubleToDouble()
    {
    	// this chainable only accepts an int
    	var c = new ChainableStreamingTestMultiplyNumbers2();

    	// this will technically produce loss of precision
		double input = 10;
		double output = await c.Run<int, double>(input);

		LoggerManager.LogDebug("Coerced output", "", output.GetType().Name, output);

		Assert.Equal(input * 2, output);
    }

	[Fact]
    public async void ChainableExecutionCoerceCastingIntToString()
    {
    	// this chainable only accepts an int
    	var c = new ChainableStreamingTestMultiplyNumbers2();

		int input = 10;
		string output = await c.Run<int, string>(input);

		LoggerManager.LogDebug("Coerced output", "", output.GetType().Name, output);

		Assert.Equal((input * 2).ToString(), output);
    }

	[Fact]
    public async void ChainableExecutionCoerceCastingStringToInt()
    {
    	// this chainable only accepts an int
    	var c = new ChainableStreamingTestMultiplyNumbers2();

		// starting input happens to be a string
		string input = "10";

		// <int, int> coerce input string to int, coerce output to int (but it's
		// already an int anyway)
		int output = await c.Run<int, int>(input);

		LoggerManager.LogDebug("Coerced output", "", output.GetType().Name, output);

		Assert.Equal((Int32.Parse(input) * 2), output);
    }

	[Fact]
    public async void ChainableExecutionCoerceBatchListInt()
    {
    	var c = new ChainableStreamingTestMultiplyNumbers2();

		// starting input happens to be an array of strings
		List<string> inputs = new() {"15", "25", "100"};

		// get back an array of ints!
		List<int> outputs = await c.Batch<int, int>(inputs.Coerce<List<object>>());

		LoggerManager.LogDebug("Coerced output", "", outputs.GetType().Name, outputs);

		for (int i = 0; i < inputs.Count(); i++)
		{
			Assert.Equal((Int32.Parse(inputs[i]) * 2), outputs[i]);
		}
    }

	[Fact]
    public async void ChainableExecutionCoerceBatchArrayStringToInt()
    {
    	var c = new ChainablePassthrough();

		// starting input happens to be an array of strings
		string[] inputs = new string[] {"15", "25", "100"};

		// get back an array of ints!
		List<int> outputs = await c.Batch<string, int, int>(inputs.ToList());

		LoggerManager.LogDebug("Coerced output", "", outputs.GetType().Name, outputs);

		for (int i = 0; i < inputs.Count(); i++)
		{
			Assert.Equal((Int32.Parse(inputs[i])), outputs[i]);
		}
    }

	// [Fact]
    // public async void ChainableExecutionCoerceBatchArrayOfArray()
    // {
    // 	var c = new ChainablePassthrough();
    //
	// 	// starting input happens to be an array of strings
	// 	string[][] inputs = new string[][] {new string[] {"15"}, new string[] {"25"}, new string[] {"100"}};
    //
	// 	// get back an array of ints!
	// 	int[][] outputs = await c.Batch<int[], int[]>(inputs);
    //
	// 	LoggerManager.LogDebug("Coerced output", "", outputs.GetType().Name, outputs);
    //
	// 	for (int i = 0; i < inputs.Count(); i++)
	// 	{
	// 		Assert.Equal((Int32.Parse(inputs[i][0]) * 2), outputs[i][0]);
	// 	}
    // }

	[Fact]
    public async void ChainableExecutionCoerceBatchListOfLists()
    {
    	var c = new ChainablePassthrough();

		// starting input happens to be an array of strings
		List<List<string>> inputs = new() {new() {"15"}, new() {"25"}, new() {"100"}};

		// get back an array of ints!
		// List<List<int>> outputs = await c.Batch<List<string>, List<int>>(inputs.Coerce<List<List<string>>, List<object>>());
		List<List<int>> outputs = await c.Batch<List<string>, List<string>, List<int>>(inputs);

		LoggerManager.LogDebug("Coerced output", "", outputs.GetType().Name, outputs);

		for (int i = 0; i < inputs.Count(); i++)
		{
			Assert.Equal((Int32.Parse(inputs[i][0])), outputs[i][0]);
		}
    }

	[Fact]
    public async void ChainableExecutionCoerceBatchListOfDictionary()
    {
    	var c = new ChainablePassthrough();

		List<Dictionary<string, string>> inputs = new() {new() { {"first", "15"} }, new() { {"second", "25"} }, new() { {"third", "100"} }};
		List<Dictionary<string, int>> expected = new() {new() { {"first", 15} }, new() { {"second", 25} }, new() { {"third", 100} }};

		// accept string, string, coerce internally to string, int, then cast
		// output to string, int
		List<Dictionary<string, int>> outputs = await c.Batch<Dictionary<string, string>, Dictionary<string, int>, Dictionary<string, int>>(inputs);

		LoggerManager.LogDebug("Coerced output", "", outputs.GetType().Name, outputs);

		for (int i = 0; i < expected.Count(); i++)
		{
			Assert.Equivalent(expected[i], outputs[i]);
		}
    }

	[Theory]
	[InlineData(new int[] {2,3,4,5}, 10, new string[] {"20","30","40","50"})]
    public async void ChainableExecutionCoerceStreamIntToString(int[] genNumbers, int input, string[] expectedChunks)
    {
		var c = new ChainableStreamingTestGenerateNumbers() { GenNumbers = genNumbers };

		List<string?> outputChunks = new();
		await foreach (string number in c.Stream<int, string>(input))
		{
			outputChunks.Add(number);

			Assert.Equal(expectedChunks[outputChunks.Count - 1], outputChunks.Last());
		}
    }

	[Fact]
    public async void ChainableExecutionCoerceStreamDictionary()
    {
		var c = new ChainablePassthrough();

		Dictionary<string, string> input = new() { { "value", "12" } };

		List<Dictionary<string, int>> outputChunks = new();
		await foreach (var dict in c.Stream<Dictionary<string, string>, Dictionary<string, int>>(input))
		{
			outputChunks.Add(dict);

			Assert.Equal(input["value"], outputChunks.Last()["value"].ToString());
		}
    }

	[Fact]
    public async void ChainableExecutionCoerceStreamArray()
    {
		var c = new ChainablePassthrough();

		int[] input = new int[] { 123 };

		List<string[]> outputChunks = new();
		await foreach (string[] array in c.Stream<int[], string[]>(input))
		{
			outputChunks.Add(array);

			Assert.Equal(input[0].ToString(), outputChunks.Last()[0].ToString());
		}
    }
}

/****************************
*  test chainable classes  *
****************************/
public partial class ChainableNonStreamTest1 : Chainable, IChainableInput<string>, IChainableOutput<string>
{
	public async override Task<object> _Process()
	{
		LoggerManager.LogDebug(GetType().Name, GetType().Name, "input", Input);

		// append x to the value of test parameter
		Output = (Input as string)+"x";

		return Output;
	}
}

public partial class ChainableNonStreamTest2 : Chainable, IChainableInput<string>, IChainableOutput<string>
{
	public int CustomProperty1 { get; set; }
	public string CustomProperty2 { get; set; }

	public string Character { get; set; } = "y";

	public async Task<object> Run(object input = null, string character = "a")
	{
		Character = character;

		return await base.Run(input);
	}

	public async Task<object[]> Batch(object[] input = null, string character = "a")
	{
		Character = character;

		return await base.Batch(input);
	}

	public async IAsyncEnumerable<object> Stream(object input = null, string character = "a")
	{
		Character = character;

		await foreach (var output in base.Stream(input))
		{
			yield return output;		
		}
	}

	public async override Task<object> _Process()
	{
		LoggerManager.LogDebug(GetType().Name, GetType().Name, "input", Input);

		// append x to the value of test parameter
		Output = (Input as string)+Character;

		return Output;
	}
}

public class ChainableStreamingTestGenerateNumbers : Chainable, IChainableInput<int>, IChainableOutput<int>
{
	public int[] GenNumbers { get; set; }

	public async override IAsyncEnumerable<object> _ProcessStream()
	{
		LoggerManager.LogDebug("Processing streaming output", GetType().Name);

		foreach (var value in GenNumbers)
		{
			int? val = value * (Input as int?);

			LoggerManager.LogDebug("Streaming number generated", GetType().Name, "value", val);

			yield return val;
		}
	}
}

public class ChainableStreamingTestWrapTags : Chainable, IChainableOutput<string>
{
	public async override IAsyncEnumerable<object> StreamTransform(IAsyncEnumerable<object> input = null)
	{
		await foreach (var stream in input)
		{
			LoggerManager.LogDebug("Wrap tags received input", GetType().Name, "input", stream);

			yield return $"<{stream}>";
		}
	}
}

public class ChainableStreamingTestMultiplyNumbers2 : Chainable, IChainableInput<int>, IChainableOutput<int>
{
	public async override IAsyncEnumerable<object> StreamTransform(IAsyncEnumerable<object> input = null)
	{
		await foreach (var stream in input)
		{
			LoggerManager.LogDebug("Multiply numbers received input", GetType().Name, "input", stream);

			var res = (stream as int?) * 2;

			LoggerManager.LogDebug("Multiply numbers output", GetType().Name, "output", res);

			yield return res;
		}
	}
}

public class ChainableNonStreamingTestWrapBraces : Chainable, IChainableOutput<string>
{
	public async override Task<object> _Process()
	{
		LoggerManager.LogDebug("Wrap braces received input", GetType().Name, "input", Input);

		return $"{{{Input}}}";
	}
}

public class ChainableNonStreamingTestWrapBrackets : Chainable, IChainableOutput<string>
{
	public async override Task<object> _Process()
	{
		LoggerManager.LogDebug("Wrap brackets received input", GetType().Name, "input", Input);

		return $"[{Input}]";
	}
}

public class ChainableNonStreanTestAdd5 : Chainable, IChainableInput<int>, IChainableOutput<int>
{
	public async override Task<object> _Process()
	{
		LoggerManager.LogDebug("Add 5 received input", GetType().Name, "input", Input);

		return (Input as int?) + 5;
	}
}

public class ChainableWillFail : Chainable
{
	public async override Task<object> _Process()
	{
		LoggerManager.LogDebug("Failing chainable received input", GetType().Name, "input", Input);

		throw new NotSupportedException("This chainable has failed!");
	}
}

public class ChainableWillFailStream : Chainable
{
	public async override IAsyncEnumerable<object> _ProcessStream()
	{
		LoggerManager.LogDebug("Failing streaming chainable received input", GetType().Name, "input", Input);

		throw new NotSupportedException("This chainable has failed!");

		yield return null;
	}
}
