/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ComponentManagerTests
 * @created     : Sunday Apr 28, 2024 19:44:43 CST
 */

namespace GodotEGP.Tests.ECSv2;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using GodotEGP.ECSv2;

public partial class ComponentManagerTests : TestContext
{
	[Fact]
	public void ComponentManagerTests_test_add_remove_component()
	{
		EntityManager entityManager = new();
		ComponentManager componentManager = new(entityManager);

		// create entity
		Entity entity = entityManager.Create();

		LoggerManager.LogDebug("Entity id", "", "id", entity);

		// add component to entity
		componentManager.Add<TestComponent>(entity, new TestComponent() {
			TestInt = 123,
		});

		ulong componentTypeId = componentManager.GetComponentType<TestComponent>();

		LoggerManager.LogDebug("Component id", "", "TestComponent", componentTypeId);

		TestComponent testComponent = componentManager.GetComponent<TestComponent>(entity);

		LoggerManager.LogDebug("TestComponent from manager", "", "testComponent", testComponent);

		// verify obtained component matches test data
		Assert.Equal(123, testComponent.TestInt);

		// replace component
		componentManager.Add<TestComponent>(entity, new TestComponent() {
			TestInt = 456,
		});

		testComponent = componentManager.GetComponent<TestComponent>(entity);

		LoggerManager.LogDebug("TestComponent after setting again", "", "testComponent", testComponent);

		// verify obtained component matches replaced data
		Assert.Equal(456, testComponent.TestInt);

		// destroy all components
		componentManager.DestroyEntityComponents(entity);

		// try to get a component that doesn't exist
		Assert.Throws<IndexOutOfRangeException>(() => componentManager.GetComponent<TestComponent>(entity));
	}
}

public struct TestComponent : IComponent
{
	public int TestInt;
}
