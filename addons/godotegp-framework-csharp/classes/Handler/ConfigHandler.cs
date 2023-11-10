namespace GodotEGP.Handler;

using GodotEGP.Service;
using GodotEGP.Config;
using GodotEGP.Logging;
using GodotEGP.Event.Events;
using GodotEGP.Objects.Extensions;

public partial class ConfigHandler : Handler
{
	public ConfigHandler()
	{
		ServiceRegistry.Get<ConfigManager>().Get<EngineConfig>().SubscribeOwner<ValidatedValueChanged>(_On_EngineConfig_ValueChanged, isHighPriority: true);
	}

	public void _On_EngineConfig_ValueChanged(IEvent e)
	{
		if (e is ValidatedValueChanged ev)
		{
			if (ev.Owner is EngineConfig ec)
			{
				// set configs
				LoggerManager.Instance.SetConfig(ec.LoggerManager);
				ServiceRegistry.Get<SaveDataManager>().SetConfig(ec.SaveDataManager);
			}
		}
	}
}
