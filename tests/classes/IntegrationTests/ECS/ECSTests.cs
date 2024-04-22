/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ECS
 * @created     : Sunday Apr 21, 2024 18:53:39 CST
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

public partial class ECSTests : TestContext
{
	[Fact]
	public void ECS_test_create_destroy_entity()
	{
		// create ECS instance
		var ecs = new ECS(1000, 32, 32);

		// add some entities
		var entity1 = ecs.CreateEntity();
		var entity2 = ecs.CreateEntity();
		var entity3 = ecs.CreateEntity();

		ecs.DestroyEntity(entity2);
	}

	[Fact]
	public void ECS_test_register_add_remove_components()
	{
		// create ECS instance
		var ecs = new ECS(1000, 32, 32);

		ecs.RegisterComponent<TestComponent1>();
		ecs.RegisterComponent<TestComponent2>();
	}

	[Fact]
	public void ECS_test_register_add_remove_systems()
	{
		// create ECS instance
		var ecs = new ECS(1000, 32, 32);

		ecs.RegisterSystem<TestSystem1>();
		ecs.RegisterSystem<TestSystem2>();
	}

	[Fact]
	public void ECS_test_system_entity_archetypes()
	{
		// create ECS instance
		var ecs = new ECS(1000, 32, 32);

		// register the test system
		SystemBase system = ecs.RegisterSystem<TestProcessingSystem1>();

		// register the test component
		ecs.RegisterComponent<TestProcessingComponent1>();

		// assign the TestProcessingComponent1 component to the test system
		ecs.SetSystemComponentArchetypeState<TestProcessingSystem1, TestProcessingComponent1>(true);

		// create some entities
		int entity1 = ecs.CreateEntity();
		int entity2 = ecs.CreateEntity();
		int entity3 = ecs.CreateEntity();

		// give some of them the test component
		ecs.AddComponent<TestProcessingComponent1>(entity1, new TestProcessingComponent1());
		ecs.AddComponent<TestProcessingComponent1>(entity3, new TestProcessingComponent1());

		LoggerManager.LogDebug("System entities", "", "system", system.Entities);

		// issue a fake process call to trigger the changes on the entity's
		// components
		system._Process(0.0);

		// verify that the components have been processed for each entity
		// correctly
		Assert.True(ecs.GetComponent<TestProcessingComponent1>(entity1).Processed);
		Assert.True(ecs.GetComponent<TestProcessingComponent1>(entity3).Processed);

		// verify the non-matching components have not been put into the
		// system's entities
		Assert.DoesNotContain(entity2, system.Entities);
	}
}

public struct TestProcessingComponent1
{
	public bool Processed;
}

public partial class TestProcessingSystem1 : SystemBase
{
	public override void _Process(double deltaTime)
	{
		foreach (int entityId in _entities)
		{
			ref TestProcessingComponent1 testComponent = ref _ecs.GetComponent<TestProcessingComponent1>(entityId);

			LoggerManager.LogDebug("Processing entity", "", "entity", entityId);

			testComponent.Processed = true;
		}
	}
}
