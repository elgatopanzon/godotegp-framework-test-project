namespace GodotEGP.Logging.Destination;

using Godot;
using System;

public interface IDestination
{
	bool Enabled { get; set; }
	public bool Process(Logging.Message loggerMessage);
}
