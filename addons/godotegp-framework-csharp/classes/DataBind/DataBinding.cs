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
	Action<T> _bindToMethod;
	Func<T> _bindFromMethod;

	EventSubscription<ObjectChanged> _eventSub;

	public DataBinding(object fromObject, Func<T> bindFromMethod, Action<T> bindToMethod)
	{
		_bindToMethod = bindToMethod;
		_bindFromMethod = bindFromMethod;

		_eventSub = fromObject.SubscribeOwner<ObjectChanged>(_On_FromObject_Changed, isHighPriority: true);

		// trigger initial binding
		_On_FromObject_Changed(null);
	}

	public override void _Ready()
	{
		// AddChild(_bindTimer);
	}

	public void _On_FromObject_Changed(IEvent e)
	{
		LoggerManager.LogDebug("Data binding setting value");

		_bindToMethod(_bindFromMethod());	
	}
}
