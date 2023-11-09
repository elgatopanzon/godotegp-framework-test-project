namespace Godot.EGP;

using Godot;
using System;

public interface ILoggerFormatter
{
	public object Format(LoggerMessage loggerMessage);
}
