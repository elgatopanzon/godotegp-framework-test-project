using Godot;
using System;

using GodotEGP;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Objects.Extensions;
using GodotEGP.Event.Events;

public partial class Tests : Node2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ServiceRegistry.Get<NodeManager>().SubscribeSignal("UITests.Save.Save", "pressed", false, _On_SaveTest_Save_pressed, isHighPriority: true);
		ServiceRegistry.Get<NodeManager>().SubscribeSignal("UITests.Save.Load", "pressed", false, _On_SaveTest_Load_pressed, isHighPriority: true);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void _On_SaveTest_Save_pressed(IEvent e)
	{
		LoggerManager.LogDebug("Save pressed", "", "event", e);
	}
	public void _On_SaveTest_Load_pressed(IEvent e)
	{
		LoggerManager.LogDebug("Load pressed", "", "event", e);
	}
}
