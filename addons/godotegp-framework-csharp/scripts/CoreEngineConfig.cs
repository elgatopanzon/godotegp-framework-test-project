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
        _loggerConfig = AddValidatedNative<LoggerConfig>(this)
        	.Default(new LoggerConfig(this))
        	.ChangeEventsEnabled();
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

	public LoggerConfig(ValidatedObject parent = null) : base(parent)
	{
        _logLevel = AddValidatedValue<LoggerMessage.LogLevel>(this)
            .Default(LoggerMessage.LogLevel.Debug)
            .AllowedValues(LoggerMessage.LogLevel.GetValues<LoggerMessage.LogLevel>())
        	.ChangeEventsEnabled()
            ;
	}
}
