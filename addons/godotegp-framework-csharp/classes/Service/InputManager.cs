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

using GodotEGP.State;

public partial class InputManager : Service
{
	// states
	class InputState : HStateMachine {}
	class InputConfigurationState : HStateMachine {}
	class InputListeningState : HStateMachine {}
	class InputProcessingState : HStateMachine {}

	private InputState _inputState  { get; set; }
	private InputConfigurationState _inputConfigurationState  { get; set; }
	private InputListeningState _inputListeningState  { get; set; }
	private InputProcessingState _inputProcessingState  { get; set; }

	private const int CONFIGURATION_STATE = 0;
	private const int LISTENING_STATE = 1;
	private const int PROCESSING_STATE = 2;

	// config
	private InputManagerConfig _config { get; set; }
	private InputMappingConfig _mappingConfig { get; set; }

	private InputEvent _previousInputEvent;

	// input state
	private Dictionary<StringName, ActionInputState> _actionStates = new();
	private MouseState _mouseState = new();

	public InputManager()
	{
		// init default configs
		_config = new();
		_mappingConfig = new();

		// setup states
		_inputState = new();
		_inputConfigurationState = new();
		_inputListeningState = new();
		_inputProcessingState = new();

		_inputState.AddState(_inputConfigurationState);
		_inputState.AddState(_inputListeningState);
		_inputState.AddState(_inputProcessingState);

		_inputConfigurationState.OnEnter = _State_Configuration_OnEnter;
		_inputConfigurationState.OnUpdate = _State_Configuration_OnUpdate;
		_inputListeningState.OnEnter = _State_Listening_OnEnter;
		_inputListeningState.OnUpdate = _State_Listening_OnUpdate;
		_inputProcessingState.OnEnter = _State_Processing_OnEnter;
		_inputProcessingState.OnUpdate = _State_Processing_OnUpdate;

		_inputState.AddTransition(_inputConfigurationState, _inputListeningState, LISTENING_STATE);
		_inputState.AddTransition(_inputProcessingState, _inputListeningState, LISTENING_STATE);

		_inputState.AddTransition(_inputListeningState, _inputProcessingState, PROCESSING_STATE);

		_inputState.AddTransition(_inputListeningState, _inputConfigurationState, CONFIGURATION_STATE);
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

		_inputState.Transition(CONFIGURATION_STATE);
	}


	/*******************
	*  Godot methods  *
	*******************/

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_inputState.Enter();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		_inputState.Update();
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

	/***************************
	*  State related methods  *
	***************************/

	public void _State_Configuration_OnEnter()
	{
		LoggerManager.LogDebug("Entering configuration state");

		ResetInputActions();
		SetupInputActions();

		_inputState.Transition(LISTENING_STATE);
	}
	
	public void _State_Configuration_OnUpdate()
	{
	}

	public void _State_Listening_OnEnter()
	{
		LoggerManager.LogDebug("Entering listening state");

		// trigger initial input state update
		UpdateInputState();
	}
	
	public void _State_Listening_OnUpdate()
	{
		// we don't want to emit an event here, we just want to update the state
		if (Input.IsAnythingPressed())
		{
			UpdateInputState();
		}
	}

	public void _State_Processing_OnEnter()
	{
		LoggerManager.LogDebug("Entering processing state");
	}

	public void _State_Processing_OnUpdate()
	{
		LoggerManager.LogDebug("Updating processing state");

		UpdateInputState();

		LoggerManager.LogDebug("Action states", "", "state", _actionStates);
		LoggerManager.LogDebug("Mouse state", "", "state", _mouseState);

		// TODO: emit event here containing the current action states object
		
		// return to listening state
		_inputState.Transition(LISTENING_STATE);
	}

	/*************************************
	*  Input action management methods  *
	*************************************/

	public void UpdateInputState()
	{
		UpdateActionInputStates();
		UpdateMouseState();
		UpdateJoypadState();
	}
	
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

					_actionStates.Remove(action);
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

			// add current action to input states
			if (!_actionStates.ContainsKey(action.Key))
			{
				_actionStates.Add(action.Key, new ActionInputState());
				_actionStates[action.Key].Config = action.Value;
			}
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

	public void UpdateActionInputStates()
	{
		// update internal state for all known actions
		foreach (var action in _config.Actions)
		{
			var actionName = action.Key;
			var actionConfig = action.Value;

			_actionStates[actionName].Pressed = Input.IsActionPressed(actionName);
			_actionStates[actionName].JustPressed = Input.IsActionJustPressed(actionName);
			_actionStates[actionName].JustReleased = Input.IsActionJustReleased(actionName);
		}
	}

	public void UpdateMouseState()
	{
		var mousePosition = GetViewport().GetMousePosition();

		_mouseState.X = mousePosition.X;
		_mouseState.Y = mousePosition.Y;

		var mouseVelocity = Input.GetLastMouseVelocity();

		_mouseState.VelocityX = mouseVelocity.X;
		_mouseState.VelocityY = mouseVelocity.Y;

		_mouseState.LeftButtonPressed = Input.IsMouseButtonPressed(MouseButton.Left);
		_mouseState.RightButtonPressed = Input.IsMouseButtonPressed(MouseButton.Right);
		_mouseState.MiddleButtonPressed = Input.IsMouseButtonPressed(MouseButton.Middle);

		_mouseState.WheelUp = Input.IsMouseButtonPressed(MouseButton.WheelUp);
		_mouseState.WheelDown = Input.IsMouseButtonPressed(MouseButton.WheelDown);
		_mouseState.WheelLeft = Input.IsMouseButtonPressed(MouseButton.WheelLeft);
		_mouseState.WheelRight = Input.IsMouseButtonPressed(MouseButton.WheelRight);
	}

	public void UpdateJoypadState()
	{
		var joypadIds = Input.GetConnectedJoypads();

		LoggerManager.LogDebug("Connected joypad count", "", "joypadIds", joypadIds);

		foreach (int joyId in joypadIds)
		{
			LoggerManager.LogDebug("joy", "", "name", Input.GetJoyName(joyId));
			LoggerManager.LogDebug("joy", "", "guid", Input.GetJoyGuid(joyId));
		}
	}

	/***********************************
	*  Input action state management  *
	***********************************/
	
	public void _On_Input(InputEvent @e = null)
	{
		foreach (var action in InputMap.GetActions())
		{
			if (Input.IsActionJustPressed(action))
			{
				LoggerManager.LogDebug("Action just pressed", "", "action", action);
			}
			else if (Input.IsActionPressed(action))
			{
				LoggerManager.LogDebug("Action pressed", "", "action", action);
			}
			else if (Input.IsActionJustReleased(action))
			{
				LoggerManager.LogDebug("Action just released", "", "action", action);
			}
		}
	}

	/********************
	*  Event handlers  *
	********************/
	
	public void _On_InputEvent(InputEvent @e)
	{
		LoggerManager.LogDebug("Input event", "", "event", @e);

		_inputState.Transition(PROCESSING_STATE, true);
	}
}

public class ActionInputState 
{
	private bool _pressed;
	public bool Pressed
	{
		get { return _pressed; }
		set { _pressed = value; }
	}

	private bool _justPressed;
	public bool JustPressed
	{
		get { return _justPressed; }
		set { _justPressed = value; }
	}

	private bool _justReleased;
	public bool JustReleased
	{
		get { return _justReleased; }
		set { _justReleased = value; }
	}

	private InputActionConfig _actionConfig;
	public InputActionConfig Config
	{
		get { return _actionConfig; }
		set { _actionConfig = value; }
	}
}

public class MouseState 
{
	private float _x;
	public float X
	{
		get { return _x; }
		set { _x = value; }
	}

	private float _y;
	public float Y
	{
		get { return _y; }
		set { _y = value; }
	}

	private float _velocityX;
	public float VelocityX
	{
		get { return _velocityX; }
		set { _velocityX = value; }
	}

	private float _velocityY;
	public float VelocityY
	{
		get { return _velocityY; }
		set { _velocityY = value; }
	}

	private bool _leftButtonPressed;
	public bool LeftButtonPressed
	{
		get { return _leftButtonPressed; }
		set { _leftButtonPressed = value; }
	}

	private bool _rightButtonPressed;
	public bool RightButtonPressed
	{
		get { return _rightButtonPressed; }
		set { _rightButtonPressed = value; }
	}

	private bool _middleButtonPressed;
	public bool MiddleButtonPressed
	{
		get { return _middleButtonPressed; }
		set { _middleButtonPressed = value; }
	}

	private bool _wheelUp;
	public bool WheelUp
	{
		get { return _wheelUp; }
		set { _wheelUp = value; }
	}

	private bool _wheelDown;
	public bool WheelDown
	{
		get { return _wheelDown; }
		set { _wheelDown = value; }
	}

	private bool _wheelLeft;
	public bool WheelLeft
	{
		get { return _wheelLeft; }
		set { _wheelLeft = value; }
	}

	private bool _wheelRight;
	public bool WheelRight
	{
		get { return _wheelRight; }
		set { _wheelRight = value; }
	}
}
