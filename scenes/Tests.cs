using Godot;
using System;

using GodotEGP;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Objects.Extensions;
using GodotEGP.Event.Events;
using GodotEGP.SaveData;

public partial class Tests : Node2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ServiceRegistry.Get<NodeManager>().SubscribeSignal("UITests.Save.Save", "pressed", false, _On_SaveTest_Save_pressed, isHighPriority: true);
		ServiceRegistry.Get<NodeManager>().SubscribeSignal("UITests.Save.Load", "pressed", false, _On_SaveTest_Load_pressed, isHighPriority: true);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void _On_SaveTest_Save_pressed(IEvent e)
	{
		string saveName = GetSaveTestName();

		LoggerManager.LogDebug("Save pressed", "", "saveName", saveName);

		if (ServiceRegistry.Get<SaveDataManager>().GetReady())
		{
			ServiceRegistry.Get<SaveDataManager>().SaveAll();
		}
	}
	public void _On_SaveTest_Load_pressed(IEvent e)
	{
		string saveName = GetSaveTestName();
		LoggerManager.LogDebug("Load pressed", "", "saveName", saveName);


		if (ServiceRegistry.Get<SaveDataManager>().GetReady())
		{
			_On_SaveDataReady(null);
		}
		else
		{
			ServiceRegistry.Get<SaveDataManager>().SubscribeOwner<ServiceReady>(_On_SaveDataReady, oneshot: true);
		}
	}

	public void _On_SaveDataReady(IEvent e)
	{
		string saveName = GetSaveTestName();

		LoggerManager.LogDebug("System data", "", "systemData", ServiceRegistry.Get<SaveDataManager>().Get<SystemData>("System"));

		if (ServiceRegistry.Get<SaveDataManager>().Exists(saveName))
		{
			LoggerManager.LogDebug("Game data", "", "gameData", ServiceRegistry.Get<SaveDataManager>().Get<GameSaveFile>(saveName));
			LoggerManager.LogDebug("Game data endpoint", "", "endpoint", ServiceRegistry.Get<SaveDataManager>().Get<GameSaveFile>(saveName).DataEndpoint);
			LoggerManager.LogDebug("System data endpoint", "", "endpoint", ServiceRegistry.Get<SaveDataManager>().Get<SystemData>("System").DataEndpoint);
		}

	}

	public string GetSaveTestName()
	{
		return ServiceRegistry.Get<NodeManager>().GetNode<TextEdit>("UITests.Save.Name").Text;
	}
}
