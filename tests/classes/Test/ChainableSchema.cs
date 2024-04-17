/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ChainableSchema
 * @created     : Sunday Mar 31, 2024 13:48:06 CST
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

public partial class ChainableSchemaTests : TestContext
{
	[Fact]
	public void ChainableSchemaTestDefaultTypes()
	{
		var schema = ChainableSchema.BuildFromObject(new Chainable());

		LoggerManager.LogDebug("Schema object", "", "schema", schema);

		// the default is no types, so nothing to validate in the schema
		Assert.Empty(schema.Input.Types);
		Assert.Empty(schema.Output.Types);
	}

	[Fact]
	public void ChainableSchemaTestInputTypesBasic()
	{
		var schema = ChainableSchema.BuildFromObject(new ChainableNonStreamTest2());

		LoggerManager.LogDebug("Schema object", "", "schema", schema);

		Assert.Contains(typeof(string), schema.Input.Types);
		Assert.Contains(typeof(string), schema.Output.Types);
	}
}

