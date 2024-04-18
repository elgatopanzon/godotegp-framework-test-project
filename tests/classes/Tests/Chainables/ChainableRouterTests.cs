/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ChainableRouter
 * @created     : Saturday Apr 06, 2024 00:46:59 CST
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

public partial class ChainableRouterTests : TestContext
{
	[Fact]
	public async void ChainableRouterRun()
	{
		var router = new ChainableRouter();
		router.Route("route1", new ChainableNonStreamingTestWrapBraces());
		router.Route("route2", new ChainableNonStreamingTestWrapBrackets());

		var res = await router.Run(new Dictionary<string, object>() { { "key", "route2" }, { "input", 50 } });

		LoggerManager.LogDebug("Route result", "", "res", res);

		Assert.Equal("[50]", res);
	}
}

