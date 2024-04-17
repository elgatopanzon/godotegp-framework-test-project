/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ChainableBranch
 * @created     : Saturday Apr 06, 2024 00:02:58 CST
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

public partial class ChainableBranchTests : TestContext
{
	[Fact]
	public async void ChainableBranchRunLambdaConditions()
	{
		var branchDefault = new ChainableNonStreamingTestWrapBrackets();

		var branch = new ChainableBranch(branches: new() {
				((x) => {
				 LoggerManager.LogDebug(x.GetType().Name);
				 return (x is string);
				}, new ChainableNonStreamTest1()),
				((x) => {
				 LoggerManager.LogDebug(x.GetType().Name);
				 return (x is int);
				}, new ChainableNonStreamingTestWrapBraces() | new ChainableNonStreamTest2()),
			},
			defaultBranch:branchDefault);

		var res = await branch.Run(50);

		LoggerManager.LogDebug("Branch result", "", "res", res);

		Assert.Equal("{50}y", res);
	}
}
