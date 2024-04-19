/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : EventFilterTests
 * @created     : Thursday Apr 18, 2024 16:47:59 CST
 */

namespace GodotEGP.Tests.Event;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Event.Filters;
using GodotEGP.Config;

public partial class EventFilterTests : TestContext
{
	[Fact]
	public void EventFilterTests_filter_OwnerObjectFilter()
	{
		var obj = new List<int>();
		var filter = new OwnerObjectFilter(obj);

		var eventObj = new Event();
		eventObj.SetOwner(obj);

		Assert.True(filter.Match(eventObj));
	}

	[Fact]
	public void EventFilterTests_filter_SignalTypeFilter()
	{
		var obj = new List<int>();
		var filter = new SignalTypeFilter("timeout");

		var eventObj = new GodotSignal();
		eventObj.SetOwner(obj);
		eventObj.SetSignalName("timeout");

		Assert.True(filter.Match(eventObj));
	}

	[Fact]
	public void EventFilterTests_filter_ObjectTypeFilter()
	{
		var obj = new List<int>();
		var filter = new ObjectTypeFilter(typeof(Event));

		var eventObj = new Event();
		eventObj.SetOwner(obj);

		Assert.True(filter.Match(eventObj));
	}

	[Fact]
	public void EventFilterTests_filter_OwnerObjectTypeFilter()
	{
		var obj = new List<int>();
		var filter = new OwnerObjectTypeFilter(typeof(List<int>));

		var eventObj = new Event();
		eventObj.SetOwner(obj);

		Assert.True(filter.Match(eventObj));
	}

	[Fact]
	public void EventFilterTests_filter_InputStateActionFilter()
	{
		var sn = new StringName("action");
		var filter = new InputStateActionFilter(sn, InputStateActionFilter.State.Pressed);

		var eventObj = new InputStateChanged();
		eventObj.ActionStates = new();
		eventObj.ActionStates[sn] = new() {
			Pressed = true,
		};

		Assert.True(filter.Match(eventObj));
	}
}

