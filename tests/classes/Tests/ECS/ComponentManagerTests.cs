/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ComponentManagerTests
 * @created     : Sunday Apr 21, 2024 15:42:04 CST
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

public partial class ComponentManagerTests : TestContext
{
	[Fact]
	public void ComponentManagerTests_registering_component_types()
	{
		var componentManager = new ComponentManager();

		int id1 = componentManager.RegisterComponent<TestComponent1>();
		int id2 = componentManager.RegisterComponent<TestComponent2>();

		Assert.Equal(0, id1);
		Assert.Equal(1, id2);
	}

	[Fact]
	public void ComponentManagerTests_registering_component_type_multiple_times()
	{
		var componentManager = new ComponentManager();

		int id1 = componentManager.RegisterComponent<TestComponent1>();
		Assert.Throws<ComponentAlreadyRegisteredException>(() => componentManager.RegisterComponent<TestComponent1>());
	}

	[Fact]
	public void ComponentManagerTests_getting_array_instance()
	{
		var componentManager = new ComponentManager();

		componentManager.RegisterComponent<TestComponent1>();

		var arr = componentManager.GetComponentArray<TestComponent1>();
	}

	[Fact]
	public void ComponentManagerTests_adding_component()
	{
		var componentManager = new ComponentManager(componentArraySize:128);

		componentManager.RegisterComponent<TestComponent1>();

		var component = new TestComponent1() {
			TestInt1 = 123,
		};

		componentManager.AddComponent(100, component);

		// get the array and get the component directly
		var fetchedComponent = componentManager.GetComponentArray<TestComponent1>().Array[100];

		Assert.Equal(123, fetchedComponent.TestInt1);
	}
}

public partial struct TestComponent1
{
	public int TestInt1;
}
public partial struct TestComponent2
{
	public int TestInt2;
}
