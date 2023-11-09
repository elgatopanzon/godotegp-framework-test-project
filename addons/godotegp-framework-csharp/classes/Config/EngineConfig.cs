namespace GodotEGP.Config;

using GodotEGP.Objects.Validated;

public class EngineConfig : VObject
{
	private readonly VNative<LoggerConfig> _loggerConfig;

	public LoggerConfig LoggerConfig
	{
		get { return _loggerConfig.Value; }
		set { _loggerConfig.Value = value; }
	}

	public EngineConfig()
	{
        _loggerConfig = AddValidatedNative<LoggerConfig>(this)
        	.Default(new LoggerConfig(this))
        	.ChangeEventsEnabled();
	}
}
