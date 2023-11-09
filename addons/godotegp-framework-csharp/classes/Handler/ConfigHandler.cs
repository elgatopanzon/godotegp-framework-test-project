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
		ServiceRegistry.Get<ConfigManager>().Get<EngineConfig>().LoggerConfig.SubscribeOwner<ValidatedValueChanged>(_On_ConfigManager_ValueChanged, isHighPriority: true);

	}

	private void _On_ConfigManager_ValueChanged(IEvent e)
	{
		if (e is ValidatedValueChanged ev)
		{
			if (ev.Owner is LoggerConfig cec)
			{
				LoggerManager.Instance.SetConfig(cec);
			}
		}
	}
}
