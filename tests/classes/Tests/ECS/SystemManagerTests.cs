/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : SystemManagerTests
 * @created     : Sunday Apr 21, 2024 16:55:07 CST
 */

namespace GodotEGP.Tests.ECS;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using GodotEGP.ECS;
using GodotEGP.ECS.Exceptions;

using System.Collections;

public partial class SystemManagerTests : TestContext
{
	[Fact]
	public void SystemManagerTests_registering_system()
	{
		var systemManager = new SystemManager();

		systemManager.RegisterSystem<TestSystem1>(null);
		systemManager.RegisterSystem<TestSystem2>(null);
	}

	[Fact]
	public void SystemManagerTests_test_system_entity_archetypes()
	{
		var systemManager = new SystemManager();

		var system1 = systemManager.RegisterSystem<TestSystem1>(null);
		var system2 = systemManager.RegisterSystem<TestSystem2>(null);

		systemManager.SetSystemArchetype<TestSystem1>(new BitArray(new bool[] { true, false, false, false }));
		systemManager.SetSystemArchetype<TestSystem2>(new BitArray(new bool[] { false, true, false, false }));

		systemManager.UpdateEntityArchetype(0, new BitArray(new bool[] { true, false, false, true }));
		systemManager.UpdateEntityArchetype(1, new BitArray(new bool[] { false, true, false, true }));

		LoggerManager.LogDebug("System 1 entities", "", "entities", system1.Entities);
		LoggerManager.LogDebug("System 2 entities", "", "entities", system2.Entities);

		Assert.Contains(0, system1.Entities);
		Assert.Contains(1, system2.Entities);
	}
}

public partial class TestSystem1 : SystemBase
{

}

public partial class TestSystem2 : SystemBase
{

}
