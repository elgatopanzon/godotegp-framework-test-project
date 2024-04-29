/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ECSTests
 * @created     : Sunday Apr 28, 2024 20:15:21 CST
 */

namespace GodotEGP.Tests.ECSv2;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using GodotEGP.ECSv2;

public partial class ECSTests : TestContext
{
	[Fact]
	public void ECSTests_test_component_archetype_single()
	{
		ECS ecs = new();		

		// create an entity
		Entity entity1 = ecs.CreateEntity();
		ecs.AddComponent<TestComponent1>(entity1, new TestComponent1() {
			TestInt = 111,
			});

		// get the archetype
		var archetype = ecs.GetArchetype(entity1);

		LoggerManager.LogDebug("Entity1 archetype", "", "archetype", archetype);

		// verify the archetype exists for the added component
		Assert.Equal(Entity.CreateFrom(2).Id, archetype[0].Id);

		// remove the component
		ecs.RemoveComponent<TestComponent1>(entity1);

		LoggerManager.LogDebug("Entity1 archetype after remove", "", "archetype", archetype);

		Assert.Empty(archetype);
	}

	[Fact]
	public void ECSTests_test_component_archetype_multiple()
	{
		ECS ecs = new();		

		// create some entities
		Entity entity1 = ecs.CreateEntity(); // 1
		Entity entity2 = ecs.CreateEntity(); // 2
		Entity entity3 = ecs.CreateEntity(); // 3

		// add some components
		// TestComponent1 = 4
		// TestComponent2 = 5
		// TestComponent3 = 6
		ecs.AddComponent<TestComponent1>(entity1, new TestComponent1() {
			TestInt = 111,
			});
		ecs.AddComponent<TestComponent2>(entity1, new TestComponent2() {
			TestLong = 111111,
			});

		ecs.AddComponent<TestComponent1>(entity2, new TestComponent1() {
			TestInt = 222,
			});
		ecs.AddComponent<TestComponent3>(entity2, new TestComponent3() {
			TestDouble = 1.1,
			});

		ecs.AddComponent<TestComponent2>(entity3, new TestComponent2() {
			TestLong = 222222,
			});
		ecs.AddComponent<TestComponent3>(entity3, new TestComponent3() {
			TestDouble = 2.2,
			});

		// get the archetypes for entities
		var archetype1 = ecs.GetArchetype(entity1);
		var archetype2 = ecs.GetArchetype(entity2);
		var archetype3 = ecs.GetArchetype(entity3);

		LoggerManager.LogDebug("Entity1 archetype", "", "archetype", archetype1);
		LoggerManager.LogDebug("Entity2 archetype", "", "archetype", archetype2);
		LoggerManager.LogDebug("Entity3 archetype", "", "archetype", archetype3);

		// check archetypes are right
		Assert.True(ecs.HasComponent<TestComponent1>(entity1));
		Assert.True(ecs.HasComponent<TestComponent2>(entity1));

		Assert.True(ecs.HasComponent<TestComponent1>(entity2));
		Assert.True(ecs.HasComponent<TestComponent3>(entity2));

		Assert.True(ecs.HasComponent<TestComponent2>(entity3));
		Assert.True(ecs.HasComponent<TestComponent3>(entity3));

		// check some false ones too
		Assert.False(ecs.HasComponent<TestComponent1>(entity3));
		Assert.False(ecs.HasComponent<TestComponent2>(entity2));
	}

	[Fact]
	public void ECSTests_test_component_pairs()
	{
		ECS ecs = new();		

		// create an entity
		Entity entity1 = ecs.CreateEntity();

		// add the pair of components
		ecs.AddComponent<TestComponentPairLeft, TestComponentPairRight>(entity1, new TestComponentPairLeft(), new TestComponentPairRight());

		// get the archetype
		var archetype = ecs.GetArchetype(entity1);

		LoggerManager.LogDebug("Entity1 archetype", "", "archetype", archetype);

		// verify the archetype matches for the added pair
		Assert.Equal(Entity.CreateFrom(2).Id, archetype[0].Id);
		Assert.Equal(Entity.CreateFrom(3).Id, archetype[0].PairId);

		Assert.True(ecs.HasComponent<TestComponentPairLeft, TestComponentPairRight>(entity1));

		// remove the component pair
		ecs.RemoveComponent<TestComponentPairLeft, TestComponentPairRight>(entity1);

		LoggerManager.LogDebug("Entity1 archetype after remove", "", "archetype", archetype);

		Assert.Empty(archetype);
	}

	[Fact]
	public void ECSTests_test_component_pairs_with_entities()
	{
		ECS ecs = new();		

		// create some entities
		Entity entity1 = ecs.CreateEntity();
		Entity entity2 = Entity.CreateFrom(1212);

		LoggerManager.LogDebug("Creating new entity", "", "entity", entity2);

		// add the entity with a relation component
		ecs.AddComponent<TestComponent1>(entity1, new TestComponent1(), entity2);

		// get the archetype
		var archetype = ecs.GetArchetype(entity1);

		LoggerManager.LogDebug("Entity1 archetype", "", "archetype", archetype);

		// verify the archetype matches for the added pair
		Assert.Equal(Entity.CreateFrom(2).Id, archetype[0].Id);
		Assert.Equal(Entity.CreateFrom(1212).Id, archetype[0].PairId);

		Assert.True(ecs.HasComponent<TestComponent1>(entity1, entity2));

		// remove the component pair
		ecs.RemoveComponent<TestComponent1>(entity1, entity2);

		LoggerManager.LogDebug("Entity1 archetype after remove", "", "archetype", archetype);

		Assert.Empty(archetype);
	}
}

public struct TestComponent1 : IComponent
{
	public int TestInt;
}

public struct TestComponent2 : IComponent
{
	public long TestLong;
}

public struct TestComponent3 : IComponent
{
	public double TestDouble;
}

public struct TestComponentPairLeft : IComponent
{
}
public struct TestComponentPairRight : IComponent
{
}
