namespace Godot.EGP;

using Godot;
using System;

public interface ILoggerDestination
{
	bool Enabled { get; set; }
	public bool Process(LoggerMessage loggerMessage);
}
