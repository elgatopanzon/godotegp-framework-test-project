/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : SystemManagerTests
 * @created     : Wednesday May 08, 2024 16:04:20 CST
 */

namespace GodotEGP.Tests.ECSv4;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using GodotEGP.ECSv4;
using GodotEGP.ECSv4.Components;
using GodotEGP.ECSv4.Queries;
using GodotEGP.ECSv4.Systems;

public partial class SystemManagerTests : TestContext
{
	[Fact]
	public void ECSv4SystemManagerTests_test_system_registration()
	{
		ECS ecs = new ECS();

		// register the systems
		EntityHandle s1 = ecs.RegisterSystem<TestSystem1, OnUpdatePhase>();
		EntityHandle s2 = ecs.RegisterSystem<TestSystem2, OnUpdatePhase>();

		LoggerManager.LogDebug("System 1 entity", "", "entity", s1);
		LoggerManager.LogDebug("System 2 entity", "", "entity", s2);

		// assert default name is set
		Assert.Contains(nameof(TestSystem1), s1.Name);
		Assert.Contains(nameof(TestSystem2), s2.Name);
	}

	[Fact]
	public void ECSv4SystemManagerTests_test_system_registration_with_query()
	{
		ECS ecs = new ECS();

		// register the systems
		EntityHandle s1 = ecs.RegisterSystem<TestSystem1, OnUpdatePhase>("s1", ecs.CreateQuery().Build());
		EntityHandle s2 = ecs.RegisterSystem<TestSystem2, OnUpdatePhase>("s2", Entity.CreateFrom(123)); // fake query id

		LoggerManager.LogDebug("System 1 entity", "", "entity", s1);
		LoggerManager.LogDebug("System 2 entity", "", "entity", s2);

		// assert custom name is set
		Assert.Contains("s1", s1.Name);
		Assert.Contains("s2", s2.Name);
	}
}

public struct TestSystem1 : IEcsSystem
{
	public static void Update(double deltaTimeSys, double deltaTime, ECS core, Query query)
	{
		LoggerManager.LogDebug("Updating", "TestSystem1");
	}
}
public struct TestSystem2 : IEcsSystem
{
	public static void Update(double deltaTimeSys, double deltaTime, ECS core, Query query)
	{
		LoggerManager.LogDebug("Updating", "TestSystem2");
	}
}
