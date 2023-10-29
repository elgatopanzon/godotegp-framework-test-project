namespace Godot.EGP;

using System;
using Godot;

public class EventNodeAdded : Event
{
	public Node Node { get; set; }

	public EventNodeAdded(object ownerObj, Node nodeObj) : base(ownerObj, nodeObj)
	{
		Node = nodeObj;
	}
}

