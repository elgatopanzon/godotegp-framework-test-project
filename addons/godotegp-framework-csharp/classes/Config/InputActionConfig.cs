/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : InputActionConfig
 * @created     : Thursday Nov 23, 2023 11:33:13 CST
 */

namespace GodotEGP.Config;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Objects.Validated;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

public enum ActionType
{
	Button = 0,
	Trigger = 1,
	Joystick = 2
}

public partial class InputActionConfig : VConfig
{
	internal readonly VValue<ActionType> _controlType;

	public ActionType ControlType
	{
		get { return _controlType.Value; }
		set { _controlType.Value = value; }
	}

	internal readonly VValue<double> _controlDeadzone;

	public double Deadzone
	{
		get { return _controlDeadzone.Value; }
		set { _controlDeadzone.Value = value; }
	}

	public InputActionConfig()
	{
		_controlType = AddValidatedValue<ActionType>(this)
		    .Default(ActionType.Button)
		    .ChangeEventsEnabled();

		_controlDeadzone = AddValidatedValue<double>(this)
		    .Default(0.5)
		    .ChangeEventsEnabled();
	}
}

