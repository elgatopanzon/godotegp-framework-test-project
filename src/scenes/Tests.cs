using Godot;
using System;
using System.Collections.Generic;

using Newtonsoft.Json;

using GodotEGP;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Objects.Extensions;
using GodotEGP.Event.Events;
using GodotEGP.Event.Filters;
using GodotEGP.SaveData;

using GodotEGP.Config;
using GodotEGP.DAL.Operations;
using GodotEGP.DataBind;

using GodotEGP.Scripting;
using GodotEGP.Resource;

using GodotEGP.Misc;

public partial class Tests : Node2D
{
	private string _scriptingTestScript = "";
	private string _scriptingTestName = "";

	private ScriptInterpretter _scriptingTestInterpretter;

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
		"UITests.STransition.Transition".Connect("pressed", false, _On_SceneTransitionTest_Transition_pressed, isHighPriority: true);
		"UITests.STransition.Chain".Connect("pressed", false, _On_SceneTransitionTest_Chain_pressed, isHighPriority: true);

		"UITests.DataBinding.Label".Node<Label>();

		// var sdm = ServiceRegistry.Get<SaveDataManager>();
		// var db = new DataBinding<Dictionary<string, GodotEGP.Config.Object>>(sdm, 
		// 		sdm.GetSaves,
		// 		(v) => "UITests.DataBinding.Label".Node<Label>().Text = v.Count.ToString()
		// 	);
		// AddChild(db);

		var sdm = ServiceRegistry.Get<SaveDataManager>();
		sdm.Bind<Dictionary<string, GodotEGP.Config.ConfigObject>>(sdm.GetSaves,
				(v) => "UITests.DataBinding.Label".Node<Label>().Text = v.Count.ToString()
			);

		"UITests.DataBinding.Data".BindSignal<TextEdit, string>("text_changed", false,  
				(n) => n.Text,
				(v) => "UITests.DataBinding.Label".Node<Label>().Text = v
			);

		// scripting tests
		// bind to pressed signal of clear button to clear script input field
		"UITests.Scripting.Clear".BindSignal<Button, string>("pressed", false,  
				(n) => null,
				(v) => {
					"UITests.Scripting.Script".Node<TextEdit>().Text = "";
					"UITests.Scripting.Output".Node<TextEdit>().Text = "";
					ServiceRegistry.Get<ScriptService>().DestroySession("testui");
					InitScriptInterpretter();
				}
			);
		// bind to text_changed signal of input field to set script content
		// variable
		"UITests.Scripting.Script".BindSignal<TextEdit, string>("text_changed", false,  
				(n) => n.Text,
				(v) => _scriptingTestScript = v,
				initialSet: true // we want to trigger initial script value to be set
			);
		// bind script name 
		"UITests.Scripting.Name".BindSignal<TextEdit, string>("text_changed", false,  
				(n) => n.Text,
				(v) => _scriptingTestName = v,
				initialSet: true // we want to trigger initial text value to be set
			);

		// connect to pressed signal and wire up to Run callback
		"UITests.Scripting.Eval".Connect("pressed", false, _On_ScriptingTest_Eval_pressed, isHighPriority: true);
		"UITests.Scripting.Run".Connect("pressed", false, _On_ScriptingTest_Run_pressed, isHighPriority: true);

		// input mapping tests
		// InputMap.AddAction(StringNames.Get("inputtest1"));
		// var key = new InputEventKey();
		// key.PhysicalKeycode = Key.A;
		// key.ShiftPressed = true;
		// InputMap.ActionAddEvent(StringNames.Get("inputtest1"), key);
        //
		// LoggerManager.LogDebug("Key event", "", "e", key);

		// test InputManager events
		var im = ServiceRegistry.Get<InputManager>();

		im.SubscribeOwner<InputStateChanged>(e => {
				LoggerManager.LogDebug("Tests: Input state changed", "", "e", e.ActionStates);
			});

		im.SubscribeOwner<InputStateChanged>(e => {
				LoggerManager.LogDebug("Tests: DefaultAction is pressed", "", "e", e.ActionStates);
			}).Filters(new InputStateActionFilter(StringNames.Get("DefaultAction"), InputStateActionFilter.State.Pressed));
		im.SubscribeOwner<InputStateChanged>(e => {
				LoggerManager.LogDebug("Tests: DefaultAction is just pressed", "", "e", e.ActionStates);
			}).Filters(new InputStateActionFilter(StringNames.Get("DefaultAction"), InputStateActionFilter.State.JustPressed));
		im.SubscribeOwner<InputStateChanged>(e => {
				LoggerManager.LogDebug("Tests: DefaultAction is just released", "", "e", e.ActionStates);
			}).Filters(new InputStateActionFilter(StringNames.Get("DefaultAction"), InputStateActionFilter.State.JustReleased));

		im.SubscribeOwner<InputStateJoypadAvailable>(e => {
			LoggerManager.LogDebug("Tests: Input joypad available");
			});
		im.SubscribeOwner<InputStateJoypadUnavailable>(e => {
			LoggerManager.LogDebug("Tests: Input joypad unavailable");
			});

		// test joypad action strength for axis
		im.SubscribeOwner<InputStateChanged>(e => {
				LoggerManager.LogDebug("Tests: AxisTestX", "", "e", e.ActionStates["AxisTestX"]);
			}).Filters(new InputStateActionFilter(StringNames.Get("AxisTestX"), InputStateActionFilter.State.Pressed));
		im.SubscribeOwner<InputStateChanged>(e => {
				LoggerManager.LogDebug("Tests: AxisTestY", "", "e", e.ActionStates["AxisTestY"]);
			}).Filters(new InputStateActionFilter(StringNames.Get("AxisTestY"), InputStateActionFilter.State.Pressed));

		im.SubscribeOwner<InputStateChanged>(e => {
				LoggerManager.LogDebug("Tests: TriggerTest", "", "e", e.ActionStates["TriggerTest"]);
			}).Filters(new InputStateActionFilter(StringNames.Get("TriggerTest"), InputStateActionFilter.State.Pressed));
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		// if (Input.IsActionPressed("inputtest1"))
		// {
		// 	LoggerManager.LogDebug("Pressed");
		// }

		var s = "UITests.Mouse.Sprite".Node<Sprite2D>();
		s.Position = ServiceRegistry.Get<InputManager>().MouseState.Position;
	}

	public override void _Input(InputEvent @e)
	{
		var s2 = "UITests.Mouse.Sprite2".Node<Sprite2D>();
		if (@e is InputEventMouseMotion em)
		{
			s2.Position = em.Position;
		}
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
			if (ServiceRegistry.Get<SaveDataManager>().Get(saveName).RawValue is SaveDataBase sd)
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

	public void _On_SceneTransitionTest_Transition_pressed(IEvent e)
	{
		string scene = "UITests.STransition.SName".Node<TextEdit>().Text;
		string transition = "UITests.STransition.TName".Node<TextEdit>().Text;

		LoggerManager.LogDebug("Scene transition start pressed", "", "transition", $"{scene} {transition}");

		ServiceRegistry.Get<SceneTransitionManager>().TransitionScene(scene, transition);

		ServiceRegistry.Get<SceneTransitionManager>().SubscribeOwner<ScreenTransitionShown>((e) => {
			LoggerManager.LogDebug("Do stuff while scene transition is shown");
			}, oneshot: true, isHighPriority: true);

		ServiceRegistry.Get<SceneTransitionManager>().SubscribeOwner<ScreenTransitionFinished>((e) => {
			LoggerManager.LogDebug("Scene transition completed!");
			}, oneshot: true, isHighPriority: true);
	}

	public void _On_SceneTransitionTest_Chain_pressed(IEvent e)
	{
		ServiceRegistry.Get<SceneTransitionManager>().SubscribeOwner<SceneTransitionChainContinued>((e) => {
				LoggerManager.LogDebug("Continuing transition chain");
			ServiceRegistry.Get<SceneTransitionManager>().ContinueChain();
			});

		ServiceRegistry.Get<SceneTransitionManager>().StartChain("testchain");
	}

	public void _On_ScriptingTest_Eval_pressed(IEvent e)
	{
		InitScriptInterpretter();

		LoggerManager.LogDebug("Scripting test eval pressed");
		LoggerManager.LogDebug("Script content", "", "script", _scriptingTestScript);

		// _scriptingTestInterpretter.RunScriptContent(_scriptingTestScript);
		ServiceRegistry.Get<ScriptService>().GetSession("testui").RunScriptContent(_scriptingTestScript);
	}

	public void _On_ScriptingTest_Run_pressed(IEvent e)
	{
		InitScriptInterpretter();

		LoggerManager.LogDebug("Scripting test run pressed");
		LoggerManager.LogDebug("Script name", "", "scriptName", _scriptingTestName);

		// _scriptingTestInterpretter.RunScript(_scriptingTestName);
		ServiceRegistry.Get<ScriptService>().GetSession("testui").RunScript(_scriptingTestName);
	}

	public void InitScriptInterpretter()
	{
		if (!ServiceRegistry.Get<ScriptService>().SessionExists("testui"))
		{
			var ses = ServiceRegistry.Get<ScriptService>().CreateSession("testui");

			ServiceRegistry.Get<EventManager>().Unsubscribe("UITests.Scripting.Output");
			ServiceRegistry.Get<ScriptService>().SubscribeOwner<ScriptOutput>((e) => {
					"UITests.Scripting.Output".Node<TextEdit>().Text += e.Result.Output+"\n";
					"UITests.Scripting.Output".Node<TextEdit>().ScrollVertical = 9999990;
				}, isHighPriority: true, groupName: "UITests.Scripting.Output");

			ServiceRegistry.Get<ScriptService>().RegisterFunctionCallback(ScriptCbFunctionTest, "cbfunctiontest");
		}
		// if (_scriptingTestInterpretter == null)
		// {
		// 	var gameScripts = ServiceRegistry.Get<ResourceManager>().GetResources<GameScript>();
		// 	var scriptFuncs = ServiceRegistry.Get<ScriptService>().ScriptFunctions;
		// 	_scriptingTestInterpretter = new ScriptInterpretter(gameScripts, scriptFuncs);
		// 	AddChild(_scriptingTestInterpretter);
		// }

	}

	public ScriptProcessResult ScriptCbFunctionTest(ScriptInterpretter i, object[] p)
	{
		return new ScriptProcessResult(0, "this was returned from a C# object method registered at runtime as a callback!");
	}
}
