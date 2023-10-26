namespace Godot.EGP;

using System;
using Godot;

public class EventNodeRemoved : Event
{
	public object Node { get; set; }

	public EventNodeRemoved(object ownerObj, object nodeObj) : base(ownerObj)
	{
		Node = nodeObj;
	}
}

