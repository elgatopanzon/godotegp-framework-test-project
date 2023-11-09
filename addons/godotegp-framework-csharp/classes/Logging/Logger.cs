namespace Godot.EGP;

using Godot;
using System;

public class Logger
{
	public LoggerMessage.LogLevel LogLevel = LoggerMessage.LogLevel.Debug; // debug by default

	public LoggerDestinationCollection LoggerDestinationCollection { set; get; }

	public Logger(LoggerDestinationCollection loggerDestinationCollection)
	{
		LoggerDestinationCollection = loggerDestinationCollection;
	}

	public void ProcessLoggerMessage(LoggerMessage loggerMessage)
	{
        foreach (ILoggerDestination loggerDestination in LoggerDestinationCollection.GetDestinations())
        {
        	loggerDestination.Process(loggerMessage);
        }
	}
}
