namespace Godot.EGP.State;

using Godot;
using System;

using System.Collections.Generic;

public class StateMachine
{
	// holds object which owns the state
	object _ownerObject;

	// holds list of states
	Dictionary<string, Dictionary<CallbackType, Action>> _states;

	// current state
	string State = "";

	// enum for valid state change callbacks
	public enum CallbackType
	{
		OnEnter, // pre-changed
		OnChanged, // actually changed
		OnExit // pre-exit
	}

	public StateMachine(object ownerObject)
	{
		_ownerObject = ownerObject;

		_states = new Dictionary<string, Dictionary<CallbackType, Action>>();

		LoggerManager.LogDebug("Creating new instance for object", _ownerObject.GetType().Name);
	}

	public bool Add(string stateName)
	{
		// try to add the dictionay for the added state
		// will fail if state with this key name already exists
		bool res = _states.TryAdd(stateName, new Dictionary<CallbackType, Action>());

		if (res)
		{
			LoggerManager.LogDebug("Adding state", _ownerObject.GetType().Name, "state", stateName);
		}
		else
		{
			LoggerManager.LogError("Adding state failed", _ownerObject.GetType().Name, "state", stateName);
		}

		return res;
	}

	public void Init(string stateName)
	{
		SetState(stateName);
		Change(stateName);
	}

	public void SetState(string stateName)
	{
		if (!IsValidState(stateName))
		{
			throw new InvalidStateException(stateName);
		}

		State = stateName;
	}

	public bool IsValidState(string stateName)
	{
		return _states.ContainsKey(stateName);
	}

	public void Change(string newState)
	{
		// run state change callbacks
		if (IsValidState(newState))
		{
			LoggerManager.LogDebug("Changing state", _ownerObject.GetType().Name, "stateChange", $"{State} => {newState}");

			_runCallback(State, CallbackType.OnExit);
			_runCallback(newState, CallbackType.OnEnter);

			SetState(newState);

			// run final changed callback
			_runCallback(newState, CallbackType.OnChanged);
		}
	}

	public void _runCallback(string currentState, CallbackType callbackType)
	{
		if (_states[currentState].ContainsKey(callbackType))
		{
			LoggerManager.LogDebug("Running state change callback", _ownerObject.GetType().Name, "callback", $"{currentState} => {callbackType.ToString()}");
			_states[currentState][callbackType]();
		}
	}

	public void RegisterCallback(string stateName, CallbackType callbackType, Action callbackFunc)
	{
		if (IsValidState(stateName))
		{
			_states[stateName].TryAdd(callbackType, callbackFunc);
		}
	}
}

public class InvalidStateException : Exception
{
    public InvalidStateException()
    {
    }

    public InvalidStateException(string message)
        : base(message)
    {
    }

    public InvalidStateException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
