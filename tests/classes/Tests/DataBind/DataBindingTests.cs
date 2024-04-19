/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : DataBindingTests
 * @created     : Thursday Apr 18, 2024 15:56:19 CST
 */

namespace GodotEGP.Tests.DataBind;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using GodotEGP.DataBind;

public partial class DataBindingTests : TestContext
{
	[Fact]
	public void DataBindingTests_binding_event()
	{
		var testObj = new DataBindingTestObject();

		var bind = new DataBinding<string, Event>(testObj, () => {
				return testObj.GetString();
			}, 
		(v) => {
			LoggerManager.LogDebug("Setter", "", "object", v);
		});

		testObj.Run();
	}
}

public partial class DataBindingTestObject
{
	public string GetString()
	{
		return "string";
	}

	public void Run()
	{
		this.Emit<Event>();
	}
}
