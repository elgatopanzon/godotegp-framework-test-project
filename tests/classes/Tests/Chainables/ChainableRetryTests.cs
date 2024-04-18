/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ChainableRetry
 * @created     : Saturday Apr 06, 2024 23:16:03 CST
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

public partial class ChainableRetryTests : TestContext
{
	[Fact]
	public async void ChainableRetryTestWaitTime()
	{
		var c = new ChainableRetry() {
			Chainable = new ChainableWillFail(),
			WaitTimeSec = 0, // disable the wait time
			MaxRetries = 5,
		};

		await Assert.ThrowsAsync(typeof(System.NotSupportedException), async () => {
			await c.Run("gg");	
			});

		Assert.Equal(0, c.RetriesRemaining);
	}
}

