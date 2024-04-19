/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : RandomNumberGeneratorTests
 * @created     : Thursday Apr 18, 2024 21:45:47 CST
 */

namespace GodotEGP.Tests.Random;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using GodotEGP.Random;

public partial class RandomNumberGeneratorTests : TestContext
{
	[Fact]
	public void RandomNumberGeneratorTests_random_seeding()
	{
		var rng = new NumberGenerator(seed:123456, state:1);

		int randi = rng.Randi();

		var rng2 = new NumberGenerator(seed:123456, state:1);

		int randi2 = rng2.Randi();

		Assert.Equal(randi, randi2);
	}
}

