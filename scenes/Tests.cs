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
		"UITests.Save.Save".Connect("pressed", false, _On_SaveTest_Save_pressed, isHighPriority: true);
		"UITests.Save.Load".Connect("pressed", false, _On_SaveTest_Load_pressed, isHighPriority: true);
		"UITests.Save.Delete".Connect("pressed", false, _On_SaveTest_Delete_pressed, isHighPriority: true);

		"UITests.Save.Copy".Connect("pressed", false, _On_SaveTest_Copy_pressed, isHighPriority: true);
		"UITests.Save.Move".Connect("pressed", false, _On_SaveTest_Move_pressed, isHighPriority: true);
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
	public void _On_SaveTest_Delete_pressed(IEvent e)
	{
		string saveName = GetSaveTestName();

		LoggerManager.LogDebug("Delete pressed", "", "saveName", saveName);

		if (ServiceRegistry.Get<SaveDataManager>().GetReady())
		{
			ServiceRegistry.Get<SaveDataManager>().Remove(saveName);
		}
	}

	public void _On_SaveDataReady(IEvent e)
	{
		string saveName = GetSaveTestName();

		LoggerManager.LogDebug("All saves", "", "systemData", ServiceRegistry.Get<SaveDataManager>().GetSaves());

		LoggerManager.LogDebug("System data", "", "systemData", ServiceRegistry.Get<SaveDataManager>().Get<SystemData>("System"));
		LoggerManager.LogDebug("System data endpoint", "", "endpoint", ServiceRegistry.Get<SaveDataManager>().Get<SystemData>("System").DataEndpoint);

		if (ServiceRegistry.Get<SaveDataManager>().Exists(saveName))
		{
			LoggerManager.LogDebug("Game data", "", "gameData", ServiceRegistry.Get<SaveDataManager>().Get<GameSaveFile>(saveName));
			LoggerManager.LogDebug("Game data endpoint", "", "endpoint", ServiceRegistry.Get<SaveDataManager>().Get<GameSaveFile>(saveName).DataEndpoint);
		}

	}

	public string GetSaveTestName()
	{
		return "UITests.Save.Name".Node<TextEdit>().Text;
	}

	public void _On_SaveTest_Copy_pressed(IEvent e)
	{
		LoggerManager.LogDebug("Copy pressed");

		if (ServiceRegistry.Get<SaveDataManager>().GetReady())
		{
			ServiceRegistry.Get<SaveDataManager>().Copy("UITests.Save.CopyFrom".Node<TextEdit>().Text, "UITests.Save.CopyTo".Node<TextEdit>().Text);
		}
	}
	public void _On_SaveTest_Move_pressed(IEvent e)
	{
		LoggerManager.LogDebug("Move pressed");

		if (ServiceRegistry.Get<SaveDataManager>().GetReady())
		{
			ServiceRegistry.Get<SaveDataManager>().Move("UITests.Save.CopyFrom".Node<TextEdit>().Text, "UITests.Save.CopyTo".Node<TextEdit>().Text);
		}
	}
}
