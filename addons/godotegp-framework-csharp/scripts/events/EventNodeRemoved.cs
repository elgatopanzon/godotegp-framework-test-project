namespace Godot.EGP;

using System;
using Godot;

public class EventNodeRemoved : Event
{
	public Node Node { get; set; }

	public EventNodeRemoved(object ownerObj, Node nodeObj) : base(ownerObj, nodeObj)
	{
		Node = nodeObj;
	}
}

