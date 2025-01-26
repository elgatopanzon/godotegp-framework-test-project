/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : SystemSchedulerTests
 * @created     : Wednesday May 08, 2024 18:36:12 CST
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

public partial class SystemSchedulerTests : TestContext
{
	[Fact]
	public void ECSv4SystemSchedulerTests_test_running_system()
	{
		ECS ecs = new ECS();

		// create some entities which have TestData component
		EntityHandle e1 = ecs.Create("e1");
		EntityHandle e2 = ecs.Create("e2");
		EntityHandle e3 = ecs.Create("e3");

		e1.Set<TestData>(new TestData() {
			Long1 = 123,
			});
		e2.Set<TestData>(new TestData() {
			Long1 = 456,
			});

		// register the systems with a different phase
		EntityHandle s2 = ecs.RegisterSystem<TestDataMutator, FinalPhase>("TestDataMutilator", ecs.CreateQuery().Has<TestData>().Build());
		EntityHandle s1 = ecs.RegisterSystem<TestSystem2, OnStartupPhase>("s1", Entity.CreateFrom(0)); // no query id

		LoggerManager.LogDebug("System 1 entity", "", "entity", s1);
		LoggerManager.LogDebug("System 2 entity", "", "entity", s2);

		// trigger updating systems
		ecs.Update(0); // fake delta time

		// verify the TestData has been updated for the entities
		Assert.Equal((ulong) 124, e1.Get<TestData>().Long1);
		Assert.Equal((ulong) 457, e2.Get<TestData>().Long1);
	}
}

public struct TestDataMutator : IEcsSystem
{
	public static void Update(double deltaTimeSys, double deltaTime, ECS core, Query query)
	{
		foreach (var entity in query.Entities.Entities)
		{
			ref TestData td = ref core.Get<TestData>(entity);

			td.Long1 += 1;
		}
	}
}
