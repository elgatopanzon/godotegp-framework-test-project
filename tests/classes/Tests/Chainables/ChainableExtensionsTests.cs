/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ChainableExtensions
 * @created     : Tuesday Apr 02, 2024 15:13:33 CST
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

public partial class ChainableExtensionsTests : TestContext
{
	[Theory]
	[InlineData("doctor ", "who?", "doctor who?")]
    public async void ChainableExtensionBind(string input, string character, string expected)
    {
    	// this should create a chainable, then return a clone of it with the
    	// bound parameter
		var c = new ChainableNonStreamTest2();
		c.CustomProperty1 = 10;

		var cb = c.Bind("character", character);

		LoggerManager.LogDebug("Previous object config", "", "config", c.Config);
		LoggerManager.LogDebug("New object config", "", "config", cb.Config);

		// verify that the previous chainable's config doesn't contain the newly
		// binded parameter value
		// NOTE: this is more of a check of the Bind() extension method than an
		// actual chainable test and should be moved to a ChainableExtensions.cs
		// test class
		Assert.DoesNotContain(new KeyValuePair<string, object>("character", character), c.Config.Params);
		Assert.Contains(new KeyValuePair<string, object>("character", character), cb.Config.Params);

		Assert.Equal(10, cb.CustomProperty1);

		Assert.Equal(expected, await cb.Run(input));
    }

	[Theory]
	[InlineData("doctor ", "who?", "doctor who?")]
    public async void ChainableExtensionParam(string input, string character, string expected)
    {
    	// this should create a chainable, then return a clone of it with the
    	// bound parameter
		var c = new ChainableNonStreamTest2();
		c.CustomProperty1 = 10;

		var cb = c.Param("character", character);

		LoggerManager.LogDebug("Previous object config", "", "config", c.Config);
		LoggerManager.LogDebug("New object config", "", "config", cb.Config);

		// verify that the previous chainable's config doesn't contain the newly
		// binded parameter value
		// NOTE: this is more of a check of the Bind() extension method than an
		// actual chainable test and should be moved to a ChainableExtensions.cs
		// test class
		Assert.DoesNotContain(new KeyValuePair<string, object>("character", character), c.Config.Params);
		Assert.Contains(new KeyValuePair<string, object>("character", character), cb.Config.Params);

		Assert.Equal(10, cb.CustomProperty1);

		Assert.Equal(expected, await cb.Run(input));
    }

	[Theory]
	[InlineData("doctor ", "who!", "doctor who!")]
    public async void ChainableExtensionConfigMerge(string input, string character, string expected)
    {
		var c = new ChainableNonStreamTest2();

		var cc = c.Bind("character", character);

		LoggerManager.LogDebug("New object config after bind", "", "config", cc.Config);

		cc = cc.Config(tags:new() { expected }, mergeCollections:true);

		cc = cc.Config(mergeCollections:true, runParams:new() { { "character2", character } });

		LoggerManager.LogDebug("Previous object config", "", "config", c.Config);
		LoggerManager.LogDebug("New object config after merge", "", "config", cc.Config);

		// make sure the tag exists
		Assert.Contains(expected, cc.Config.Tags);

		Assert.DoesNotContain(new KeyValuePair<string, object>("character", character), c.Config.Params);
		Assert.Contains(new KeyValuePair<string, object>("character", character), cc.Config.Params);

		// check if character2 exists in the new chain, and not in the previous
		Assert.DoesNotContain(new KeyValuePair<string, object>("character2", character), c.Config.Params);
		Assert.Contains(new KeyValuePair<string, object>("character2", character), cc.Config.Params);

		Assert.Equal(expected, await cc.Run(input));
    }

	[Theory]
	[InlineData("doctor ", "who?", "doctor who?")]
    public async void ChainableExtensionBindClassProperties(string input, string character, string expected)
    {
    	// this should create a chainable, then return a clone of it with the
    	// bound parameter
		var c = new ChainableNonStreamTest2();

		var cb = c.Bind("character", character);
		cb = cb.Bind("CustomProperty2", expected);

		LoggerManager.LogDebug("Previous object config", "", "config", c.Config);
		LoggerManager.LogDebug("New object config", "", "config", cb.Config);

		Assert.Equal(expected, await cb.Run(input));

		// check that after the run, the config object's class property has been
		// set correctly by the bind
		Assert.Equal(expected, cb.CustomProperty2);
    }

	[Theory]
	[InlineData("the ", "doctor", "the doctor")]
    public async void ChainableExtensionConfigurableParams(string input, string character, string expected)
    {
    	// this should create a chainable, then return a clone of it with the
    	// bound parameter
		var c = new ChainableNonStreamTest2();
		var ccf = c.ConfigurableParams("character", new ChainableConfigurableParam() {
			Id = "transform_character",
			Name = "Transform Character",
			Description = "The character used in the transformation process",
			});

		ccf = ccf.Config(runParams:new() { { "transform_character", character } });

		LoggerManager.LogDebug("Previous object config", "", "config", c.Config);
		LoggerManager.LogDebug("New object config", "", "config", ccf.Config);

		Assert.Equal(expected, await ccf.Run(input));
    }

	[Theory]
	[InlineData("doctor ", "who!", "doctor who!x")]
    public async void ChainableExtensionConfigOnChain(string input, string character, string expected)
    {
		var c = new ChainableNonStreamTest2() | new ChainableNonStreamTest1();

		var cc = c.Bind("character", character);

		LoggerManager.LogDebug("Previous object config", "", "config", c.Config);
		LoggerManager.LogDebug("New object config after merge", "", "config", cc.Config);

		Assert.Equal(expected, await cc.Run(input));
    }

	[Theory]
	[InlineData("doctor ", "who!", "doctor xwho!")]
    public async void ChainableExtensionConfigOnChainLast(string input, string character, string expected)
    {
		var c = new ChainableNonStreamTest1() | new ChainableNonStreamTest2();

		var cc = c.Bind("character", character);

		LoggerManager.LogDebug("Previous object config", "", "config", c.Config);
		LoggerManager.LogDebug("New object config after merge", "", "config", cc.Config);

		Assert.Equal(expected, await cc.Run(input));
    }
}
