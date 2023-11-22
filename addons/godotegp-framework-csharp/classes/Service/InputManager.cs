/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : InputManager
 * @created     : Wednesday Nov 22, 2023 15:42:37 CST
 */

namespace GodotEGP.Service;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

public partial class InputManager : Service
{
	public InputManager()
	{
		
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_SetServiceReady(true);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	// Called when service is registered in manager
	public override void _OnServiceRegistered()
	{
	}

	// Called when service is deregistered from manager
	public override void _OnServiceDeregistered()
	{
		// LoggerManager.LogDebug($"Service deregistered!", "", "service", this.GetType().Name);
	}

	// Called when service is considered ready
	public override void _OnServiceReady()
	{
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		LoggerManager.LogDebug(@event.AsText());
	}
}

