namespace Godot.EGP;

using System;
using Godot;

public class EventNodeAdded : Event
{
	public object Node { get; set; }

	public EventNodeAdded(object ownerObj, object nodeObj) : base(ownerObj)
	{
		Node = nodeObj;
	}
}

