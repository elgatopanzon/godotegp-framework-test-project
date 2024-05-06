/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ECSTests
 * @created     : Tuesday Apr 30, 2024 14:13:22 CST
 */

namespace GodotEGP.Tests.ECSv3;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using GodotEGP.ECSv3;
using GodotEGP.ECSv3.Components;

public partial class ECSTests : TestContext
{
	[Fact]
	public void ECSTests_entity_create()
	{
		ECS ecs = new ECS();

		EntityHandle e1 = ecs.Create();
		EntityHandle e2 = ecs.Create();
		EntityHandle e3 = ecs.Create();

		LoggerManager.LogDebug("Entity 1", "", "e1", e1);
		LoggerManager.LogDebug("Entity 2", "", "e2", e2);
		LoggerManager.LogDebug("Entity 3", "", "e3", e3);

		e1.Destroy();
		e2.Destroy();
		e3.Destroy();

		// idempotent test
		e1.Destroy();
		e2.Destroy();
		e3.Destroy();
	}

	[Fact]
	public void ECSTests_entity_create_with_name()
	{
		ECS ecs = new ECS();

		EntityHandle namedEntity = ecs.Create("named");

		Assert.Equal("named", ecs.GetEntityName(namedEntity));

		// get the entity with the same name again, verify it's the same
		EntityHandle namedEntitySame = ecs.Create("named");

		Assert.Equal("named", ecs.GetEntityName(namedEntitySame));
	}

	[Fact]
	public void ECSTests_entity_recycle_and_alive_check()
	{
		ECS ecs = new ECS();

		EntityHandle e1 = ecs.Create();

		LoggerManager.LogDebug("Entity 1", "", "e1", e1);

		// check e1 is alive
		Assert.True(e1.IsAlive());

		e1.Destroy();

		// after recycled it should have a version
		EntityHandle e2 = ecs.Create();

		LoggerManager.LogDebug("Entity 2", "", "e2", e2);

		// check new ID matches recycled ID
		Assert.Equal(e1.Entity.Id, e2.Entity.Id);

		// check version is incremeneted
		Assert.Equal(1, e2.Entity.Version);

		// check e1 is not alive
		Assert.False(e1.IsAlive());
	}

	[Fact]
	public void ECSTests_entity_archetype_state()
	{
		ECS ecs = new ECS();

		EntityHandle e1 = ecs.Create();

		// set archetype id using a standard id
		Entity testEntity = Entity.CreateFrom(123);
		ecs.SetEntityArchetypeState(e1, testEntity, true);

		// check it has it
		Assert.True(ecs.GetEntityArchetypeState(e1, testEntity));

		// set archetype id using a full id
		Entity testEntityFull = Entity.CreateFrom(123, 456);
		ecs.SetEntityArchetypeState(e1, testEntityFull, true);

		// check it has it
		Assert.True(ecs.GetEntityArchetypeState(e1, testEntityFull));

		// add another one
		Entity testEntityFull2 = Entity.CreateFrom(7, 89);
		ecs.SetEntityArchetypeState(e1, testEntityFull2, true);

		// remove all in different order, then check state
		ecs.SetEntityArchetypeState(e1, testEntity, false);
		Assert.False(ecs.GetEntityArchetypeState(e1, testEntity));
		Assert.True(ecs.GetEntityArchetypeState(e1, testEntityFull));
		Assert.True(ecs.GetEntityArchetypeState(e1, testEntityFull2));

		ecs.SetEntityArchetypeState(e1, testEntityFull2, false);
		Assert.False(ecs.GetEntityArchetypeState(e1, testEntity));
		Assert.True(ecs.GetEntityArchetypeState(e1, testEntityFull));
		Assert.False(ecs.GetEntityArchetypeState(e1, testEntityFull2));

		ecs.SetEntityArchetypeState(e1, testEntityFull, false);
		Assert.False(ecs.GetEntityArchetypeState(e1, testEntity));
		Assert.False(ecs.GetEntityArchetypeState(e1, testEntityFull));
		Assert.False(ecs.GetEntityArchetypeState(e1, testEntityFull2));
	}

	[Fact]
	public void ECSTests_entity_id_creation()
	{
		ECS ecs = new ECS();

		Entity e1 = ecs.Create();
		Entity e2 = ecs.Create();
		Entity e3 = ecs.Create();

		// register components and create their type IDs
		ecs.RegisterComponent<TestTag>();
		ecs.RegisterComponent<TestData>();
		ecs.RegisterComponent<TestData2>();

		// get basic component ID
		Entity idBasicComponent = ecs.Id<TestTag>();

		// get a component pair ID
		Entity idComponentPair = ecs.Id<TestTag, TestData>();

		// get the opposite component pair ID
		Entity idComponentPairFlipped = ecs.Id<TestData, TestTag>();

		// should fail because both cannot be data components
		Entity idComponentPairBothData = ecs.Id<TestData, TestData2>();

		// get component entity pair ID
		Entity idEntityComponentPair = ecs.Id<TestTag>(e2);

		// get entity pair ID
		Entity idEntityPair = ecs.Id(e2, e3);

		LoggerManager.LogDebug("IdBasicComponent", "", "id", idBasicComponent);
		LoggerManager.LogDebug("IdComponentPair", "", "id", idComponentPair);
		LoggerManager.LogDebug("IdComponentPairFlipped", "", "id", idComponentPairFlipped);
		LoggerManager.LogDebug("IdComponentPairBothData", "", "id", idComponentPairBothData);
		LoggerManager.LogDebug("IdEntityComponentPair", "", "id", idEntityComponentPair);
		LoggerManager.LogDebug("IdEntityPair", "", "id", idEntityPair);

		// validate created IDs have correct part values
		Assert.Equal((uint) 5003, idBasicComponent.Id);
		Assert.Equal((uint) 0, idBasicComponent.PairId);

		Assert.Equal((uint) 5003, idComponentPair.Id);
		Assert.Equal((uint) 5004, idComponentPair.PairId);

		Assert.Equal((uint) 5004, idComponentPairFlipped.Id);
		Assert.Equal((uint) 5003, idComponentPairFlipped.PairId);

		Assert.Equal((uint) 5003, idEntityComponentPair.Id);
		Assert.Equal((uint) 5001, idEntityComponentPair.PairId);

		Assert.Equal((uint) 5001, idEntityPair.Id);
		Assert.Equal((uint) 5002, idEntityPair.PairId);
	}

	[Fact]
	public void ECSTests_component_add_and_remove()
	{
		ECS ecs = new ECS();

		EntityHandle e1 = ecs.Create();
		EntityHandle e2 = ecs.Create();
		EntityHandle e3 = ecs.Create();

		// register components
		ecs.RegisterComponent<TestTag>();
		ecs.RegisterComponent<TestTag2>();
		ecs.RegisterComponent<TestData>();
		ecs.RegisterComponent<TestData2>();

		// idempotent test
		ecs.RegisterComponent<TestTag>();
		ecs.RegisterComponent<TestTag2>();
		ecs.RegisterComponent<TestData>();
		ecs.RegisterComponent<TestData2>();
		
		// add tag component to entity
		e1.Add<TestTag>();
		e1.Add<TestTag, TestTag2>();

		// idempotent test
		e1.Add<TestTag>();
		e1.Add<TestTag, TestTag2>();

		// add data component to entity
		e1.Set<TestData>(new TestData() {
			Long1 = 1,
			});

		// add component pair to entity
		e1.Set<TestTag, TestData>(new TestData() {
			Long1 = 2,
			});

		// add entity component pair to entity
		e1.Set<TestData>(e2, new TestData() {
			Long1 = 3,
			});

		// add entity pair to entity
		e1.Add(e2, e3);

		// check the entity has the components
		Assert.True(e1.Has<TestTag>());
		Assert.True(e1.Has<TestTag, TestTag2>());
		Assert.True(e1.Has<TestData>());
		Assert.True(e1.Has<TestTag, TestData>());
		Assert.True(e1.Has<TestData>(e2));
		Assert.True(e1.Has(e2, e3));

		LoggerManager.LogDebug("TestData", "", "component", e1.Get<TestData>());
		LoggerManager.LogDebug("TestTag, TestData", "", "component", e1.Get<TestTag, TestData>());
		LoggerManager.LogDebug("TestData e2", "", "component", e1.Get<TestData>(e2));

		// verify the right components are obtained
		Assert.Equal((ulong) 1, e1.Get<TestData>().Long1);
		Assert.Equal((ulong) 2, e1.Get<TestTag, TestData>().Long1);
		Assert.Equal((ulong) 3, e1.Get<TestData>(e2).Long1);

		// remove all the components
		e1.Remove<TestTag>();
		e1.Remove<TestTag, TestTag2>();
		e1.Remove<TestData>();
		e1.Remove<TestTag, TestData>();
		e1.Remove<TestData>(e2);
		e1.Remove(e2, e3);

		// idempotent test
		e1.Remove<TestTag>();
		e1.Remove<TestTag, TestTag2>();
		e1.Remove<TestData>();
		e1.Remove<TestTag, TestData>();
		e1.Remove<TestData>(e2);
		e1.Remove(e2, e3);

		// verify none of the components exist
		Assert.False(e1.Has<TestTag>());
		Assert.False(e1.Has<TestTag, TestTag2>());
		Assert.False(e1.Has<TestData>());
		Assert.False(e1.Has<TestTag, TestData>());
		Assert.False(e1.Has<TestData>(e2));
		Assert.False(e1.Has(e2, e3));
	}

	[Fact]
	public void ECSTests_test_component_config()
	{
		ECS ecs = new ECS();

		EntityHandle e1 = ecs.Create();

		// register components
		Entity ec1 = ecs.RegisterComponent<TestData>();

		// get the componnt config from the component
		ref EcsComponentConfig config = ref ecs.GetComponentConfig(ec1);

		LoggerManager.LogDebug("Component config", "", "config", config);
	}
}

public struct TestTag : ITag {}
public struct TestTag2 : ITag {}
public struct TestTag3 : ITag {}
public struct TestTag4 : ITag {}
public struct TestData : IComponentData 
{
	public ulong Long1;
	public ulong Long2;
	public ulong Long3;
	public ulong Long4;
}

public struct TestData2 : IComponentData 
{
	public double Double1;
	public double Double2;
	public double Double3;
	public double Double4;
}

public struct TestData3 : IComponentData 
{
	public int Int1;
	public int Int2;
	public int Int3;
	public int Int4;
}
