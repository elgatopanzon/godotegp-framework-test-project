namespace Godot.EGP.Config;

using System;
using System.Collections.Generic;
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

	private readonly ValidatedValue<Dictionary<string, LoggerMessage.LogLevel>> _logLevelOverrides;

	public Dictionary<string, LoggerMessage.LogLevel> LogLevelOverrides
	{
		get { return _logLevelOverrides.Value; }
		set { _logLevelOverrides.Value = value; }
	}

	public LoggerConfig(ValidatedObject parent = null) : base(parent)
	{
        _logLevel = AddValidatedValue<LoggerMessage.LogLevel>(this)
            .Default((OS.IsDebugBuild()) ? LoggerMessage.LogLevel.Debug : LoggerMessage.LogLevel.Info)
            .AllowedValues(LoggerMessage.LogLevel.GetValues<LoggerMessage.LogLevel>())
        	.ChangeEventsEnabled()
            ;

        _logLevelOverrides = AddValidatedValue<Dictionary<string, LoggerMessage.LogLevel>>(this)
            .Default(new Dictionary<string, LoggerMessage.LogLevel>())
        	.ChangeEventsEnabled()
            ;
	}

	public LoggerMessage.LogLevel GetMatchingLogLevelOverride(string match)
	{
		foreach (KeyValuePair<string, LoggerMessage.LogLevel> levelOverride in LogLevelOverrides)
		{
			if (match.Contains(levelOverride.Key))
			{
				return levelOverride.Value;
			}
		}

		return LogLevel;
	}
}
