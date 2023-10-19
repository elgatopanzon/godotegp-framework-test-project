namespace Godot.EGP.State;

using Godot;
using System;

using System.Collections.Generic;

public class StateMachine
{
	// holds object which owns the state
	object _ownerObject;

	// holds list of states
	Dictionary<string, Dictionary<CallbackType, Func>> _states;

	// current state
	string State = "";

	// enum for valid state change callbacks
	enum CallbackType
	{
		OnEnter, // pre-changed
		OnChanged, // actually changed
		OnExit // pre-exit
	}

	public StateMachine(object ownerObject)
	{
		_ownerObject = ownerObject;

		_states = new Dictionary<string, Dictionary<CallbackType, Func>>();
	}

	public AddState(string stateName)
	{
		// try to add the dictionay for the added state
		// will fail if state with this key name already exists
		_states.TryAdd(stateName, new Dictionary<CallbackType, Func>());
	}

	public void ChangeState(string newState)
	{
		if (!_states.ContainsKey(newState))
		{
			raise InvalidStateException();
		}

		// run state change callbacks
		_runStateChangeCallback(State, CallbackType.OnExit);
		_runStateChangeCallback(newState, CallbackType.OnEnter);

		// set new state
		State = newState;

		// run final changed callback
		_runStateChangeCallback(newState, newState, CallbackType.OnChanged);
	}

	public void _runStateChangeCallback(string currentState, CallbackType callbackType)
	{
		if (_states[stateName].ContainsKey(callbackType))
		{
			_states[stateName][callbackType]();
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
