/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ComponentArrayTests
 * @created     : Sunday Apr 21, 2024 13:07:17 CST
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

public partial class ComponentArrayTests : TestContext
{
	[Fact]
	public void ComponentArrayTests_test_multiple_insert()
	{
		var arr = new ComponentArray<TestStruct>(32);

		arr.InsertComponent(0, new TestStruct());

		Assert.Throws<ComponentExistsException>(() => arr.InsertComponent(0, new TestStruct()));
	}

	[Fact]
	public void ComponentArrayTests_test_nonexistant_remove()
	{
		var arr = new ComponentArray<TestStruct>(32);

		Assert.Throws<ComponentNotFoundException>(() => arr.RemoveComponent(0));
	}

	[Fact]
	public void ComponentArrayTests_test_nonexistant_get()
	{
		var arr = new ComponentArray<TestStruct>(32);

		Assert.Throws<ComponentNotFoundException>(() => arr.GetComponent(0));
	}
}

