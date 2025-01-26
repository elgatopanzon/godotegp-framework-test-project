/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ECSTests
 * @created     : Tuesday Apr 30, 2024 14:13:22 CST
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

public partial class ECSTests : TestContext
{
	[Fact]
	public void ECSv4ECSTests_entity_create()
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
	public void ECSv4ECSTests_entity_create_with_name()
	{
		ECS ecs = new ECS();

		EntityHandle namedEntity = ecs.Create("named");

		Assert.Equal("named", ecs.GetEntityName(namedEntity));

		// get the entity with the same name again, verify it's the same
		EntityHandle namedEntitySame = ecs.Create("named");

		Assert.Equal("named", ecs.GetEntityName(namedEntitySame));
	}

	[Fact]
	public void ECSv4ECSTests_entity_recycle_and_alive_check()
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

		// check e1 is not alive
		// NOTE: since switching to int IDs it's no longer possible to have the
		// version in the entity, so we can't easily check if the old one is
		// alive or not
		// Assert.False(e1.IsAlive());
	}

	[Fact]
	public void ECSv4ECSTests_entity_archetype_state()
	{
		ECS ecs = new ECS();

		EntityHandle e1 = ecs.Create();

		// set archetype id using a standard id
		Entity testEntity = Entity.CreateFrom(123);
		ecs.SetEntityArchetypeState(e1, testEntity, true);

		// check it has it
		Assert.True(ecs.GetEntityArchetypeState(e1, testEntity));
	}

	[Fact]
	public void ECSv4ECSTests_entity_id_creation()
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

		LoggerManager.LogDebug("IdBasicComponent", "", "id", idBasicComponent);
	}

	[Fact]
	public void ECSv4ECSTests_component_add_and_remove()
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

		// idempotent test
		e1.Add<TestTag>();

		// add data component to entity
		e1.Set<TestData>(new TestData() {
			Long1 = 1,
			});


		// check the entity has the components
		Assert.True(e1.Has<TestTag>());
		Assert.True(e1.Has<TestData>());

		LoggerManager.LogDebug("TestData", "", "component", e1.Get<TestData>());

		// verify the right components are obtained
		Assert.Equal((ulong) 1, e1.Get<TestData>().Long1);

		// remove all the components
		e1.Remove<TestTag>();
		e1.Remove<TestData>();

		// idempotent test
		e1.Remove<TestTag>();
		e1.Remove<TestData>();

		// verify none of the components exist
		Assert.False(e1.Has<TestTag>());
		Assert.False(e1.Has<TestData>());
	}

	[Fact]
	public void ECSv4ECSTests_test_component_config()
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

public struct TestTag : ITagComponent {public static int Id { get; set; }}
public struct TestTag2 : ITagComponent {public static int Id { get; set; }}
public struct TestTag3 : ITagComponent {public static int Id { get; set; }}
public struct TestTag4 : ITagComponent {public static int Id { get; set; }}
public struct TestData : IDataComponent 
{
	public static int Id { get; set; }
	public ulong Long1;
	public ulong Long2;
	public ulong Long3;
	public ulong Long4;
}

public struct TestData2 : IDataComponent 
{
	public static int Id { get; set; }
	public double Double1;
	public double Double2;
	public double Double3;
	public double Double4;
}

public struct TestData3 : IDataComponent 
{
	public static int Id { get; set; }
	public int Int1;
	public int Int2;
	public int Int3;
	public int Int4;
}
