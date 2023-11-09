namespace Godot.EGP;

using Godot;
using System;
using System.Collections.Generic;

public class LoggerDestinationCollection
{
	private List<ILoggerDestination> _loggerDestinations = new List<ILoggerDestination>();

	public void AddDestination(ILoggerDestination destination)
	{
		_loggerDestinations.Add(destination);
	}

	public bool RemoveDestination(ILoggerDestination destination)
	{
		return _loggerDestinations.Remove(destination);
	}

	public List<ILoggerDestination> GetDestinations()
	{
		return _loggerDestinations;
	}
}
