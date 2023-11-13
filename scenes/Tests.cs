using Godot;
using System;

using Newtonsoft.Json;

using GodotEGP;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Objects.Extensions;
using GodotEGP.Event.Events;
using GodotEGP.SaveData;

using GodotEGP.Config;
using GodotEGP.Data.Operation;

public partial class Tests : Node2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		"UITests.Test".Connect("pressed", false, _On_Test_pressed, isHighPriority: true);
		
		"UITests.Save.Create".Connect("pressed", false, _On_SaveTest_Create_pressed, isHighPriority: true);
		"UITests.Save.Save".Connect("pressed", false, _On_SaveTest_Save_pressed, isHighPriority: true);
		"UITests.Save.Load".Connect("pressed", false, _On_SaveTest_Load_pressed, isHighPriority: true);
		"UITests.Save.Delete".Connect("pressed", false, _On_SaveTest_Delete_pressed, isHighPriority: true);

		"UITests.Save.Copy".Connect("pressed", false, _On_SaveTest_Copy_pressed, isHighPriority: true);
		"UITests.Save.Move".Connect("pressed", false, _On_SaveTest_Move_pressed, isHighPriority: true);

		"UITests.Save.ListSlots".Connect("pressed", false, _On_SaveTest_ListSlots_pressed, isHighPriority: true);
		"UITests.Save.TimedAutosave".Connect("pressed", false, _On_SaveTest_TimedAutosave_pressed, isHighPriority: true);

		"UITests.Scene.Load".Connect("pressed", false, _On_SceneTest_Load_pressed, isHighPriority: true);
		"UITests.Scene.Reload".Connect("pressed", false, _On_SceneTest_Reload_pressed, isHighPriority: true);
		"UITests.Scene.Unload".Connect("pressed", false, _On_SceneTest_Unload_pressed, isHighPriority: true);
		"UITests.Scene.LoadPrev".Connect("pressed", false, _On_SceneTest_LoadPrev_pressed, isHighPriority: true);

		"UITests.Transition.Start".Connect("pressed", false, _On_TransitionTest_Start_pressed, isHighPriority: true);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	// general testing method hooked to the Test button
	public void _On_Test_pressed(IEvent e)
	{
		LoggerManager.LogDebug("Testing stuff!");

		// resource configs
		// var resourceConfig = ServiceRegistry.Get<ConfigManager>().Get<ResourceDefinitionConfig>();
		// LoggerManager.LogDebug("Resource definitions", "", "resourceDefinitions", resourceConfig);
        //
		// LoggerManager.LogDebug(new AudioEffectHighPassFilter().GetType());
        //
		// var hpfc = resourceConfig.Resources["AudioEffects"];
		// foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
     	// {
        //  	Type type = asm.GetType("Godot.AudioEffectHighPassFilter");
        //  	if (type != null)
        //  	{
        //      	AudioEffectHighPassFilter r = (AudioEffectHighPassFilter) Activator.CreateInstance(type);
		// 		LoggerManager.LogDebug(r);
		// 		LoggerManager.LogDebug(r.CutoffHz);
        //
		// 		OperationResult<AudioEffectHighPassFilter> or = new OperationResult<AudioEffectHighPassFilter>(JsonConvert.SerializeObject(hpfc["testfilter"].Config, Formatting.Indented));
        //
		// 		Newtonsoft.Json.JsonConvert.PopulateObject((string) JsonConvert.SerializeObject(hpfc["testfilter"].Config, Formatting.Indented),
		// 				r,
		// 			new JsonSerializerSettings
    	// 			{
        // 				Error = (object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args) =>
        // 				{
        //     				args.ErrorContext.Handled = true;
        // 				},
        // 				ObjectCreationHandling = ObjectCreationHandling.Replace
    	// 			}
		// 		);
        //
		// 		LoggerManager.LogDebug(r.CutoffHz);
		// 	}
     	// }

		// init ResourceManager service
		// ServiceRegistry.Get<ResourceManager>();
		//
		LoggerManager.LogDebug("Test loading resource", "", "r", ServiceRegistry.Get<ResourceManager>().Get<CompressedTexture2D>("Images", "square"));

		// ServiceRegistry.Get<ResourceManager>().SubscribeOwner<ResourceLoaderCompleted>((e) => {
		// 		LoggerManager.LogDebug("Test loading resource", "", "r", ServiceRegistry.Get<ResourceManager>().Get<CompressedTexture2D>("square"));
		// 	});
	}

	public void _On_SaveTest_TimedAutosave_pressed(IEvent e)
	{
		string saveName = GetSaveTestName();

		LoggerManager.LogDebug("TimedAutosave pressed", "", "saveName", saveName);

		if (ServiceRegistry.Get<SaveDataManager>().GetReady())
		{
			if (ServiceRegistry.Get<SaveDataManager>().Get(saveName).RawValue is Data sd)
			{
				sd.Loaded = true;
			}
		}
	}

	public void _On_SaveTest_Create_pressed(IEvent e)
	{
		string saveName = GetSaveTestName();

		LoggerManager.LogDebug("Create pressed", "", "saveName", saveName);

		if (ServiceRegistry.Get<SaveDataManager>().GetReady())
		{
			ServiceRegistry.Get<SaveDataManager>().Create<GameSaveFile>(saveName);
		}
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
		LoggerManager.LogDebug("System data endpoint", "", "endpoint", ServiceRegistry.Get<SaveDataManager>().Get("System").DataEndpoint);

		if (ServiceRegistry.Get<SaveDataManager>().Exists(saveName))
		{
			LoggerManager.LogDebug("Game data", "", "gameData", ServiceRegistry.Get<SaveDataManager>().Get<GameSaveFile>(saveName));
			LoggerManager.LogDebug("Game data endpoint", "", "endpoint", ServiceRegistry.Get<SaveDataManager>().Get(saveName).DataEndpoint);
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

	public void _On_SaveTest_ListSlots_pressed(IEvent e)
	{
		LoggerManager.LogDebug("ListSlots pressed");

		if (ServiceRegistry.Get<SaveDataManager>().GetReady())
		{
			LoggerManager.LogDebug("List of save slots", "", "saveSlots", ServiceRegistry.Get<SaveDataManager>().GetSlotSaves(10));
		}
	}

	public void _On_SceneTest_Load_pressed(IEvent e)
	{
		string loadScene = "UITests.Scene.Name".Node<TextEdit>().Text;

		LoggerManager.LogDebug("Scene load pressed", "", "scene", loadScene);

		ServiceRegistry.Get<SceneManager>().LoadScene(loadScene);
	}
	public void _On_SceneTest_Reload_pressed(IEvent e)
	{
		LoggerManager.LogDebug("Scene reload pressed");

		ServiceRegistry.Get<SceneManager>().ReloadCurrentScene();
	}
	public void _On_SceneTest_Unload_pressed(IEvent e)
	{
		LoggerManager.LogDebug("Scene unload pressed");

		ServiceRegistry.Get<SceneManager>().UnloadManagedScenes();
	}
	public void _On_SceneTest_LoadPrev_pressed(IEvent e)
	{
		LoggerManager.LogDebug("Scene load prev pressed");

		ServiceRegistry.Get<SceneManager>().LoadPreviousScene();
	}


	public void _On_TransitionTest_Start_pressed(IEvent e)
	{
		string transition = "UITests.Transition.Name".Node<TextEdit>().Text;

		LoggerManager.LogDebug("Screen transition start pressed", "", "scene", transition);

		ServiceRegistry.Get<ScreenTransitionManager>().StartTransition(transition);

		ServiceRegistry.Get<ScreenTransitionManager>().SubscribeOwner<ScreenTransitionShown>((e) => {
			LoggerManager.LogDebug("Do stuff while screen transition is shown");

			ServiceRegistry.Get<ScreenTransitionManager>().ContinueTransition();
			}, oneshot: true);

		ServiceRegistry.Get<ScreenTransitionManager>().SubscribeOwner<ScreenTransitionFinished>((e) => {
			LoggerManager.LogDebug("Screen transition completed!");
			}, oneshot: true);
	}
}
