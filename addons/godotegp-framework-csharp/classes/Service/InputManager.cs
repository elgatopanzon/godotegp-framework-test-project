/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : InputManager
 * @created     : Wednesday Nov 22, 2023 15:42:37 CST
 */

namespace GodotEGP.Service;

using System;
using System.Collections.Generic;
using System.Linq;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

public partial class InputManager : Service
{
	private InputManagerConfig _config { get; set; }
	private InputMappingConfig _mappingConfig { get; set; }

	public InputManager()
	{
		
	}

	public void SetMappingConfig(InputMappingConfig mappingConfig)
	{
		_mappingConfig = mappingConfig;
	}

	public void SetConfig(InputManagerConfig config)
	{
		LoggerManager.LogDebug("Setting config");

		_config = config;

		if (!GetReady())
		{
			_SetServiceReady(true);
		}

		ResetInputActions();
		SetupInputActions();
	}


	/*******************
	*  Godot methods  *
	*******************/

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		_On_InputEvent(@event);
	}

	/*********************
	*  Service methods  *
	*********************/
	
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


	/*************************************
	*  Input action management methods  *
	*************************************/
	
	public void ResetInputActions(bool eraseActions = true)
	{
		foreach (var action in InputMap.GetActions())
		{
			if (ActionExists(action))
			{
				ResetInputAction(action);

				if (eraseActions)
				{
					LoggerManager.LogDebug("Erasing input action", "", "action", action.ToString());
					InputMap.EraseAction(action);
				}
			}

		}
	}

	public void ResetInputAction(StringName action)
	{
		LoggerManager.LogDebug("Resetting input action", "", "action", action.ToString());
		
		foreach (var e in InputMap.ActionGetEvents(action))
		{
			LoggerManager.LogDebug("Erasing action event", "", action.ToString(), e);
			InputMap.ActionEraseEvent(action, e);
		}
	}

	public bool ActionExists(StringName action)
	{
		return _mappingConfig.Mappings.ContainsKey(action.ToString());
	}

	public void SetupInputActions()
	{
		// add actions from config known actions
		foreach (var action in _config.Actions)
		{
			LoggerManager.LogDebug("Adding input action", "", action.Key, action.Value);
			InputMap.AddAction(action.Key, (float) action.Value.Deadzone);

			ConfigureActionMapping(action.Key);
		}
	}

	public void ConfigureActionMapping(StringName action)
	{
		if (_mappingConfig.Mappings.TryGetValue(action, out var ev))
		{
			foreach (var actionMapping in ev.Events)
			{
				var e = (dynamic) actionMapping.ToInputEvent();

				LoggerManager.LogDebug("Action mapping found", "", "mapping", actionMapping);

				InputMap.ActionAddEvent(action, e);
			}
		}
	}

	/********************
	*  Event handlers  *
	********************/
	
	public void _On_InputEvent(InputEvent @e)
	{
		LoggerManager.LogDebug(@e.AsText());
	}
}

