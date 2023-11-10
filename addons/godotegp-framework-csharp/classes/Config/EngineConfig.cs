namespace GodotEGP.Config;

using GodotEGP.Objects.Validated;

public partial class EngineConfig : VObject
{
	private readonly VNative<LoggerConfig> _loggerManagerConfig;

	public LoggerConfig LoggerManager
	{
		get { return _loggerManagerConfig.Value; }
		set { _loggerManagerConfig.Value = value; }
	}

	internal readonly VNative<SaveDataManagerConfig> _saveDataManagerConfig;

	public SaveDataManagerConfig SaveDataManager
	{
		get { return _saveDataManagerConfig.Value; }
		set { _saveDataManagerConfig.Value = value; }
	}

	public EngineConfig()
	{
        _loggerManagerConfig = AddValidatedNative<LoggerConfig>(this)
        	.Default(new LoggerConfig(this))
        	.ChangeEventsEnabled();

		_saveDataManagerConfig = AddValidatedNative<SaveDataManagerConfig>(this)
		    .Default(new SaveDataManagerConfig())
		    .ChangeEventsEnabled();
	}
}
