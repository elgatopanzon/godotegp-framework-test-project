namespace Godot.EGP.Config;

using System;
using Godot.EGP.ValidatedObjects;

public class CoreEngineConfig : ValidatedObject
{
	private readonly ValidatedNative<LoggerConfig> _loggerConfig;

	public LoggerConfig LoggerConfig
	{
		get { return _loggerConfig.Value; }
		set { _loggerConfig.Value = value; }
	}

	public CoreEngineConfig()
	{
        _loggerConfig = AddValidatedNative<LoggerConfig>()
        	.Default(new LoggerConfig());
	}
}

public class LoggerConfig : ValidatedObject
{
	private readonly ValidatedValue<LoggerMessage.LogLevel> _logLevel;

	public LoggerMessage.LogLevel LogLevel
	{
		get { return _logLevel.Value; }
		set { _logLevel.Value = value; }
	}

	public LoggerConfig()
	{
        _logLevel = AddValidatedValue<LoggerMessage.LogLevel>()
            .Default(LoggerMessage.LogLevel.Debug)
            .AllowedValues(LoggerMessage.LogLevel.GetValues<LoggerMessage.LogLevel>())
            ;
	}
}
