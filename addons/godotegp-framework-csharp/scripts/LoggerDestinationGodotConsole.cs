namespace Godot.EGP;

using Godot;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

public class LoggerDestinationGodotConsole : ILoggerDestination
{
	public bool Enabled { get; set; }

	private ILoggerFormatter _loggerFormatter;

	public LoggerDestinationGodotConsole(ILoggerFormatter loggerFormatter = null)
	{
		if (loggerFormatter == null)
		{
			loggerFormatter = new LoggerFormatterGodotRich();
		}
		_loggerFormatter = loggerFormatter;
		Enabled = true; // enabled by default
	}

	public bool Process(LoggerMessage loggerMessage)
	{
		if (Enabled)
		{
			// var jsonString = JsonConvert.SerializeObject(
        	// loggerMessage, Formatting.Indented,
        	// new JsonConverter[] {new StringEnumConverter()});
            //
        	// GD.Print(jsonString);
        	GD.PrintRich(_loggerFormatter.Format(loggerMessage));
        	return true;
		}

		return false;
	}
}
