namespace Godot.EGP;

using System;

public interface IEvent
{
	DateTime Created { get; set; }
	object Owner { get; set; }
	object Data { get; set; }
}
