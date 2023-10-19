namespace Godot.EGP.State;

using Godot;
using System;

using System.Collections.Generic;

public class StateMachine
{
	// holds object which owns the state
	object _ownerObject;

	// holds list of states
	Dictionary<object, Dictionary<CallbackType, Action>> _states;

	// current state
	object State = null;

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

		_states = new Dictionary<object, Dictionary<CallbackType, Action>>();

		LoggerManager.LogDebug("Creating new instance for object", _ownerObject.GetType().Name);
	}

	public bool Add(object stateName)
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

	public void Init(object stateName)
	{
		SetState(stateName);
		Change(stateName);
	}

	public void SetState(object stateName)
	{
		if (!IsValidState(stateName))
		{
			throw new InvalidStateException(stateName);
		}

		State = stateName;
	}

	public bool IsValidState(object stateName)
	{
		return _states.ContainsKey(stateName);
	}

	public void Change(object newState)
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

	public void _runCallback(object currentState, CallbackType callbackType)
	{
		if (_states[currentState].ContainsKey(callbackType))
		{
			LoggerManager.LogDebug("Running state change callback", _ownerObject.GetType().Name, "callback", $"{currentState} => {callbackType.ToString()}");
			_states[currentState][callbackType]();
		}
	}

	public void RegisterCallback(object stateName, CallbackType callbackType, Action callbackFunc)
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

    public InvalidStateException(object message)
        : base(message.ToString())
    {
    }

    public InvalidStateException(object message, Exception inner)
        : base(message.ToString(), inner)
    {
    }
}
