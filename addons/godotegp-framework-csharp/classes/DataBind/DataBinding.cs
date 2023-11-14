/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : DataBinding
 * @created     : Monday Nov 13, 2023 15:26:23 CST
 */

namespace GodotEGP.DataBind;

using System;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Event;
using GodotEGP.Config;

public partial class DataBinding<T> : Node
{
	Action<T> _setterFirstCb;
	Func<T> _getterFirstCb;

	EventSubscription<ObjectChanged> _eventSub;

	public DataBinding(object objectFirst, Func<T> getterFirstCb, Action<T> setterFirstCb)
	{
		_setterFirstCb = setterFirstCb;
		_getterFirstCb = getterFirstCb;

		_eventSub = objectFirst.SubscribeOwner<ObjectChanged>(_On_ObjectFirst_Changed, isHighPriority: true);

		// trigger initial binding
		_On_ObjectFirst_Changed(null);
	}

	~DataBinding()
	{
		// unsubscribe from the changed event to prevent ghost bindings
		ServiceRegistry.Get<EventManager>().Unsubscribe(_eventSub);
	}

	public override void _Ready()
	{
		// AddChild(_bindTimer);
	}

	public void _On_ObjectFirst_Changed(IEvent e)
	{
		LoggerManager.LogDebug("Object first changed");

		_setterFirstCb(_getterFirstCb());	
	}
}
