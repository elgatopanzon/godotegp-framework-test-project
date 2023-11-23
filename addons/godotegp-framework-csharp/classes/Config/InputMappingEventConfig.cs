/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : InputMappingConfig
 * @created     : Thursday Nov 23, 2023 12:53:45 CST
 */

namespace GodotEGP.Config;

using System;
using System.Collections.Generic;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Objects.Validated;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

public partial class InputMappingEventConfig : VConfig
{
	internal readonly VValue<List<InputMappingEvent>> _mappingEvents;

	public List<InputMappingEvent> Events
	{
		get { return _mappingEvents.Value; }
		set { _mappingEvents.Value = value; }
	}

	public InputMappingEventConfig()
	{
		_mappingEvents = AddValidatedValue<List<InputMappingEvent>>(this)
		    .Default(new List<InputMappingEvent>())
		    .ChangeEventsEnabled();

		var e1 = new InputMappingEvent();
		e1.Keycode = Key.A;

		var e2 = new InputMappingEvent();
		e2.JoypadButton = JoyButton.A;

		Events.Add(e1);
		Events.Add(e2);
	}
}

public partial class InputMappingEvent
{
	private int _deviceId = 0;
	public int DeviceId
	{
		get { return _deviceId; }
		set { _deviceId = value; }
	}

	private JoyButton _joypadButton = JoyButton.Invalid;
	public JoyButton JoypadButton
	{
		get { return _joypadButton; }
		set { _joypadButton = value; }
	}

	private JoyAxis _joyAxis = JoyAxis.Invalid;
	public JoyAxis JoypadAxis
	{
		get { return _joyAxis; }
		set { _joyAxis = value; }
	}

	private bool _joyAxisDirection;
	public bool AxisDirection
	{
		get { return _joyAxisDirection; }
		set { _joyAxisDirection = value; }
	}

	private Key _keycode = Key.None;
	public Key Keycode
	{
		get { return _keycode; }
		set { _keycode = value; }
	}

	internal bool _allowEcho = false;
	public bool AllowEcho
	{
		get { return _allowEcho = false; }
		set { _allowEcho = value; }
	}

	private bool _altPressed;
	public bool AltPressed
	{
		get { return _altPressed; }
		set { _altPressed = value; }
	}

	private bool _ctrlPressed;
	public bool CtrlPressed
	{
		get { return _ctrlPressed; }
		set { _ctrlPressed = value; }
	}

	private bool _metaPressed;
	public bool MetaPressed
	{
		get { return _metaPressed; }
		set { _metaPressed = value; }
	}

	private bool _shiftPressed;
	public bool ShiftPressed
	{
		get { return _shiftPressed; }
		set { _shiftPressed = value; }
	}

	private MouseButton _mouseButton;
	public MouseButton MouseButton
	{
		get { return _mouseButton; }
		set { _mouseButton = value; }
	}

	public InputMappingEvent()
	{

	}
}
