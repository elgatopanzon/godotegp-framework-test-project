namespace GodotEGP;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.State;
using System;
using System.Collections.Generic;
using GodotEGP.Objects.Validated;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.IO;
using System.Threading;
using System.ComponentModel;

using GodotEGP.Service;
using GodotEGP.Logging;
using GodotEGP.Event;
using GodotEGP.Event.Events;
using GodotEGP.Event.Filter;
using GodotEGP.Config;
using GodotEGP.Data.Operation;

public partial class Main : Node
{
	enum States
	{
		State10,
		State20,
		State30
	}

	string previousCatFact = "";
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		AddChild(new ServiceRegistry());
		ServiceRegistry.Get<ConfigManager>();
		ServiceRegistry.Get<DataService>();

		var lmch = new LoggerManagerConfigHandler();

		// ServiceRegistry.Instance.RegisterService(new EventService(), "Events");
        //
		// ServiceRegistry.Instance.RegisterService(LoggerManager.Instance, "Log");
		// ServiceRegistry.Instance.RegisterService(new RandomService(), "Random");
		// ServiceRegistry.Instance.RegisterService(new SystemService(), "System");
		// ServiceRegistry.Instance.RegisterService(new ObjectPoolService(), "ObjectPool");

		// EventManager testing
		// LoggerManager.LogDebug(ServiceRegistry.Get<EventManager>().ToString());
		//

		// var queue = ServiceRegistry.Get<EventManager>().GetQueue<EventQueueCustom>();
		// ServiceRegistry.Get<EventManager>().Queue<EventQueue>(new Event(this));
		// ServiceRegistry.Get<EventManager>().Queue<EventQueue>(new EventCustom(this));
		// ServiceRegistry.Get<EventManager>().Queue<EventQueue>(new EventCustom(this));
		// ServiceRegistry.Get<EventManager>().Queue<EventQueue>(new EventCustom(this));
		// ServiceRegistry.Get<EventManager>().Queue<EventQueue>(new Event(this));
		// ServiceRegistry.Get<EventManager>().Queue<EventQueue>(new Event(this));
        //
		// LoggerManager.LogDebug("Fetched Event", "", "events", ServiceRegistry.Get<EventManager>().Fetch<EventQueue>(typeof(Event)).Peek().Created);
		// LoggerManager.LogDebug("Fetched Event", "", "events", ServiceRegistry.Get<EventManager>().Fetch<EventQueue>(typeof(Event)).Peek().Created);
		// LoggerManager.LogDebug("Fetched Event", "", "events", ServiceRegistry.Get<EventManager>().Fetch<EventQueue>(typeof(Event)).Peek().Created);
		// // LoggerManager.LogDebug("Fetched Event", "", "events", ServiceRegistry.Get<EventManager>().Fetch<EventQueue>(typeof(Event)).Peek().Created);
        //
		// LoggerManager.LogDebug("Fetched EventCustom", "", "events", ServiceRegistry.Get<EventManager>().Fetch<EventQueue>(typeof(EventCustom)).Peek().Created);
		// LoggerManager.LogDebug("Fetched EventCustom", "", "events", ServiceRegistry.Get<EventManager>().Fetch<EventQueue>(typeof(EventCustom)).Peek().Created);
		// LoggerManager.LogDebug("Fetched EventCustom", "", "events", ServiceRegistry.Get<EventManager>().Fetch<EventQueue>(typeof(EventCustom)).Peek().Created);
		// // LoggerManager.LogDebug("Fetched EventCustom", "", "events", ServiceRegistry.Get<EventManager>().Fetch<EventQueue>(typeof(EventCustom)).Peek().Created);

		// var sub = new EventSubscription<EventServiceRegistered>(this, false);
		// ServiceRegistry.Get<EventManager>().Subscribe(sub);

		var sub = new EventSubscription<ServiceRegistered>(this, __On_EventServiceRegistered, false, false, new List<IFilter> {new SignalType("test")});
		var sub2 = new EventSubscription<ServiceRegistered>(this, __On_EventServiceRegistered_highpriority, true);

		ServiceRegistry.Get<EventManager>().Subscribe(sub);
		ServiceRegistry.Get<EventManager>().Subscribe(sub2);

		Node obj = ServiceRegistry.Get<ObjectPoolService>().Get<Node>();


		ServiceRegistry.Get<NodeManager>().SubscribeSignal("group_test_group", "timeout", false, __On_Timer_timeout, false, true);

		Godot.Timer timer = new Godot.Timer();
		timer.Name = "timer1";
		timer.AddToGroup("test_group");
		AddChild(timer);
		timer.WaitTime = 5;
		timer.Start();

		// ServiceRegistry.Get<EventManager>().SubscribeSignal(timer, "timeout", false, new EventSubscription<EventSignal>(this, __On_Timer_timeout));
		// ServiceRegistry.Get<EventManager>().SubscribeSignal(timer, "timeout", false, new EventSubscription<EventSignal>(this, __On_Timer_timeout));

		// timer.SubscribeSignal("timeout", false, __On_Timer_timeout);
		// timer.SubscribeSignal("timeout", false, __On_Timer_timeout);

		LoggerManager.LogDebug(ServiceRegistry.Get<NodeManager>().GetReady());


		Godot.Timer timer2 = new Godot.Timer();
		timer2.Name = "timer2";
		timer2.AddToGroup("test_group");

		// ServiceRegistry.Get<EventManager>().Subscribe(new EventSubscription<EventServiceReady>(this, (e) => {
		// 	ServiceRegistry.Get<EventManager>().Subscribe(new EventSubscription<EventNodeAdded>(this, (e) => {
		// 			if (ServiceRegistry.Get<NodeManager>().TryGetNode("timer2", out Node n))
		// 			{
		// 				LoggerManager.LogDebug("Timer node exists", "", "timer", n.ToString());
		// 			}
		// 		}, true, new List<IEventFilter>() {new EventFilterOwner(timer2)}));
        //
		// 	AddChild(timer2);
		// 	}, true, new List<IEventFilter>() {new EventFilterOwner(ServiceRegistry.Get<NodeManager>())}));

		this.Subscribe<ServiceReady>((e) => {
			this.Subscribe<NodeAdded>((e) => {
					if (ServiceRegistry.Get<NodeManager>().TryGetNode("timer2", out Node n))
					{
						LoggerManager.LogDebug("Timer node exists", "", "timer", n.ToString());
					}
				}, true, false, new List<IFilter>() {new OwnerObject(timer2)});

			AddChild(timer2);
			}, true, false, new List<IFilter>() {new OwnerObject(ServiceRegistry.Get<NodeManager>())});

		// validated objects testing
		ObjectTest vObj = new ObjectTest();
		// LoggerManager.LogDebug(vObj);
        //
        //
        vObj.ObjectTestt.StringTest = "string200";
		string vObjJson = Newtonsoft.Json.JsonConvert.SerializeObject(vObj);
		LoggerManager.LogDebug(vObjJson);

		// vObjJson = "{'StringListTest':[1,2,3],'DictionarySizeTest':{'a':1,'b':1,'c':1},'StringTest':'string','IntTest':5,'DoubleTest':5.0,'UlongTest':5,'IntArrayTest':[1,2,3],'Vector2Test':{'X':1.0,'Y':1.0}}";
		// vObjJson = "{\"StringListTest\":{\"Value\":[\"d\",\"e\",\"f\"]},\"DictionarySizeTest\":{\"Value\":{\"d\":\"123\",\"e\":1,\"f\":1}},\"StringTest\":{\"Value\":\"string\"},\"IntTest\":{\"Value\":5},\"DoubleTestt\":{\"Value\":5.0},\"UlongTestt\":{\"Value\":5},\"IntArrayTest\":{\"Value\":[\"123\",2,3]},\"Vector2Testt\":{\"Value\":{\"X\":2.0,\"Y\":2.0}}}";
		vObjJson = "{'StringListTest':['a','b','d'],'DictionarySizeTest':{'a':1,'b':1,'d':1},'StringTest':'string','IntTest':6,'DoubleTest':6.0,'UlongTest':5,'IntArrayTest':[1,2,3],'Vector2Test':{'X':1.0,'Y':1.0},'RecursiveTest':[{'Value':{'X':1.0,'Y':1.0}},{'Value':{'X':2.0,'Y':2.0}},{'Value':{'X':3.0,'Y':4.0}}],'ObjectTest':{'IntTest':60}}";
		LoggerManager.LogDebug(vObjJson);

		List<string> errors = new List<string>();

		ObjectTest vObj2 = Newtonsoft.Json.JsonConvert.DeserializeObject<ObjectTest>(vObjJson,
		// Newtonsoft.Json.JsonConvert.PopulateObject(vObjJson, vObj,
				new JsonSerializerSettings
    				{
        				Error = (object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args) =>
        				{
            				errors.Add(args.ErrorContext.Error.Message);
            				args.ErrorContext.Handled = true;
        				},
        				ObjectCreationHandling = ObjectCreationHandling.Replace
    				}
				);

		string vObj2Json = Newtonsoft.Json.JsonConvert.SerializeObject(vObj2);
		LoggerManager.LogDebug($"Source obj: {vObj2Json}");

		vObjJson = Newtonsoft.Json.JsonConvert.SerializeObject(vObj);
		LoggerManager.LogDebug($"Before merge: {vObjJson}");

		LoggerManager.LogDebug("vObj2.ObjectTest", "", "ObjectTest", vObj2.ObjectTestt);
		LoggerManager.LogDebug("vObj2.GetProperties()[9]", "", "ObjectTest", vObj2.GetProperties()[9]);
		vObj.MergeFrom(vObj2);

		vObjJson = Newtonsoft.Json.JsonConvert.SerializeObject(vObj);
		LoggerManager.LogDebug($"After merge: {vObjJson}");

		LoggerManager.LogDebug(vObj.StringTest);
		// LoggerManager.LogDebug(vObj2.StringTest);
		foreach (string error in errors)
		{
			LoggerManager.LogDebug(error);
		}
        //
		// LoggerManager.LogDebug(vObj.StringTest.Value);
		// try
		// {
		// 	vObj.StringTest.Value = "xxx";
		// }
		// catch (System.Exception)
		// {
		// 	throw;
		// }
		// finally
		// {
		// 	LoggerManager.LogDebug(vObj.StringTest.Value);
		// }
		// LoggerManager.LogDebug(vObj.StringTest.Value);
		// LoggerManager.LogDebug(vObj.IntTest.Value);
		// LoggerManager.LogDebug(vObj.DoubleTest.Value);
		// LoggerManager.LogDebug(vObj.UlongTest.Value);
		// LoggerManager.LogDebug(vObj.Vector2Test.Value);


		// LoggerManager.LogDebug(ServiceRegistry.Get<Service>().GetReady());
		// LoggerManager.LogDebug(ServiceRegistry.Get<TestService>().GetReady());
		// LoggerManager.LogDebug(ServiceRegistry.Instance["Test"].GetReady());

		// ServiceRegistry.Get<ObjectPoolService>().SetPoolConfig<Node>(10, 100);
        //
		// Node obj = ServiceRegistry.Get<ObjectPoolService>().Get<Node>();
        //
		// LoggerManager.LogDebug($"Obj: {obj} {obj.GetType().Name}");
        //
		// ServiceRegistry.Get<ObjectPoolService>().Return<Node>(obj);
        //
		// obj = ServiceRegistry.Get<ObjectPoolService>().Get<Node>();
        //
		// LoggerManager.LogDebug($"Obj: {obj} {obj.GetType().Name}");

		// LoggerManager.LogTrace("testing");
		// LoggerManager.LogDebug("testing");
		// LoggerManager.LogInfo("testing");
		// LoggerManager.LogWarning("testing");
		// LoggerManager.LogError("testing");
		// LoggerManager.LogCritical("testing");
		// LoggerManager.LogDebug("testing 2", "Custom Name", "node_pool_config", ServiceRegistry.Get<ObjectPoolService>().GetPoolConfig<Node>());
        //
		// LoggerManager.LogDebug("testing long multi-line string again just to see");

		// StateMachine sm = new StateMachine(this);
        //
		// // add some test states
		// sm.Add(States.State10);
		// sm.Add(States.State20);
		// sm.Add(States.State30);
        //
		// // start the FSM
		// sm.Init(States.State10);
		// LoggerManager.LogDebug("Current state", "", "state", sm.State);
        //
		// sm.RegisterCallback(States.State10, StateMachine.CallbackType.OnEnter, (p, n) => {
		// 	LoggerManager.LogDebug("State change callback State10, OnEnter");
		// 	});
		// sm.RegisterCallback(States.State10, StateMachine.CallbackType.OnExit, (p, n) => {
		// 	LoggerManager.LogDebug("State change callback State10, OnExit");
		// 	});
        //
		// sm.RegisterCallback(States.State10, StateMachine.CallbackType.OnChanged, (p, n) => {
		// 	LoggerManager.LogDebug("State change callback State10, OnChanged");
		// 	});
        //
		// sm.RegisterCallback(States.State20, StateMachine.CallbackType.OnEnter, (p, n) => {
		// 	LoggerManager.LogDebug("State change callback State20, OnEnter");
		// 	});
		// sm.RegisterCallback(States.State20, StateMachine.CallbackType.OnExit, (p, n) => {
		// 	LoggerManager.LogDebug("State change callback State20, OnExit");
		// 	});
        //
		// sm.RegisterCallback(States.State20, StateMachine.CallbackType.OnChanged, (p, n) => {
		// 	LoggerManager.LogDebug("State change callback State20, OnChanged");
		// 	});
        //
		// sm.RegisterCallback(States.State30, StateMachine.CallbackType.OnEnter, (p, n) => {
		// 	LoggerManager.LogDebug("State change callback State30, OnEnter");
		// 	});
		// sm.RegisterCallback(States.State30, StateMachine.CallbackType.OnExit, (p, n) => {
		// 	LoggerManager.LogDebug("State change callback State30, OnExit");
		// 	});
        //
		// sm.RegisterCallback(States.State30, StateMachine.CallbackType.OnChanged, (p, n) => {
		// 	LoggerManager.LogDebug("State change callback State30, OnChanged");
		// 	});
        //
		// // test changing state
		// sm.Change(States.State20);
		// LoggerManager.LogDebug("Current state", "", "state", sm.State);
        //
		// sm.Change(States.State30);
		// LoggerManager.LogDebug("Current state", "", "state", sm.State);
        //
		// sm.Push(States.State20);
		// LoggerManager.LogDebug("Current state", "", "state", sm.State);
        //
		// sm.Pop();
		// LoggerManager.LogDebug("Current state", "", "state", sm.State);

		// MoveState moveState = new MoveState();
		// RunState runState = new RunState();
		// JumpState jumpState = new JumpState();
		// OtherState otherState = new OtherState();
		// Other2State other2State = new Other2State();
        //
		// moveState.OnEnter = () => LoggerManager.LogDebug("moveState OnEnter");
		// moveState.OnExit = () => LoggerManager.LogDebug("moveState OnExit");
		// moveState.OnUpdate = () => LoggerManager.LogDebug("moveState OnUpdate");
        //
		// runState.OnEnter = () => LoggerManager.LogDebug("runState OnEnter");
		// runState.OnExit = () => LoggerManager.LogDebug("runState OnExit");
		// runState.OnUpdate = () => LoggerManager.LogDebug("runState OnUpdate");
        //
		// jumpState.OnEnter = () => LoggerManager.LogDebug("jumpState OnEnter");
		// jumpState.OnExit = () => LoggerManager.LogDebug("jumpState OnExit");
		// jumpState.OnUpdate = () => LoggerManager.LogDebug("jumpState OnUpdate");
        //
		// otherState.OnEnter = () => LoggerManager.LogDebug("otherState OnEnter");
		// otherState.OnExit = () => LoggerManager.LogDebug("otherState OnExit");
		// otherState.OnUpdate = () => LoggerManager.LogDebug("otherState OnUpdate");
		// other2State.OnEnter = () => LoggerManager.LogDebug("other2State OnEnter");
		// other2State.OnExit = () => LoggerManager.LogDebug("other2State OnExit");
		// other2State.OnUpdate = () => LoggerManager.LogDebug("other2State OnUpdate");
        //
		// moveState.AddState(runState);
		// moveState.AddState(jumpState);
        //
		// jumpState.AddState(otherState);
		// jumpState.AddState(other2State);
        //
		// moveState.AddTransition(runState, jumpState, 0);
		// moveState.AddTransition(jumpState, runState, 1);
		// jumpState.AddTransition(otherState, other2State, 0);
		// jumpState.AddTransition(other2State, otherState, 3);
        //
		// LoggerManager.LogDebug("Starting move state machine");
		// moveState.Enter();
        //
		// // transition to jump state
		// LoggerManager.LogDebug("Transition to jump state");
		// moveState.Transition(0);
        //
		// // transition to jump.other2state
		// LoggerManager.LogDebug("Transition to jump sub-state other2");
		// jumpState.Transition(0);
        //
		// // transition to run state
		// LoggerManager.LogDebug("Transition to run state");
		// moveState.Transition(1);
        //
		// // moveState.Change(jumpState);
        //
		// // moveState.Change(otherState);
		// // moveState.ChangeTo(other2State);

		// var random1 = ServiceRegistry.Get<RandomService>().Get();
		// LoggerManager.LogDebug($"Random test: randf", "", "value", random1.Randf());
		// LoggerManager.LogDebug($"Random test: randf", "", "value", random1.Randf());
        //
		// LoggerManager.LogDebug($"Random test: randf_range", "", "value", random1.RandfRange(0.0, 10.0));
        //
		// LoggerManager.LogDebug($"Random test: randi", "", "value", random1.Randi());
		// LoggerManager.LogDebug($"Random test: randi", "", "value", random1.Randi());
        //
		// LoggerManager.LogDebug($"Random test: randi_range", "", "value", random1.RandiRange(0, 10));
        //
		// LoggerManager.LogDebug($"Random test: randfn", "", "value", random1.Randfn());
        //
		// var random2 = ServiceRegistry.Get<RandomService>().RegisterInstance(new RandomNumberGeneratorExtended(124432203923092, 3409348273897433484), "test");
		// LoggerManager.LogDebug($"Random seed/state test: randf", "", "value", random2.Randf());
		// LoggerManager.LogDebug($"Random seed/state test: randf", "", "value", random2.Randf());
        //
		// LoggerManager.LogDebug($"Random seed/state test: randf_range", "", "value", random2.RandfRange(0.0, 10.0));
        //
		// LoggerManager.LogDebug($"Random seed/state test: randi", "", "value", random2.Randi());
		// LoggerManager.LogDebug($"Random seed/state test: randi", "", "value", random2.Randi());
        //
		// LoggerManager.LogDebug($"Random seed/state test: randi_range", "", "value", random2.RandiRange(0, 10));
        //
		// LoggerManager.LogDebug($"Random seed/state test: randfn", "", "value", random2.Randfn());

		this.Subscribe<BackgroundJobComplete>(__On_Event);
		this.Subscribe<ValidatedValueChanged>(__On_Event);

		// config object test
		// LoggerManager.LogDebug("Creating CoreEngineConfig instance");
		// CoreEngineConfig engineConfig = new CoreEngineConfig();
        //
		// string engineConfigJson = Newtonsoft.Json.JsonConvert.SerializeObject(engineConfig);
		// LoggerManager.LogDebug(engineConfigJson);
        //
		// CoreEngineConfig engineConfig2 = Newtonsoft.Json.JsonConvert.DeserializeObject<CoreEngineConfig>(engineConfigJson,
		// // Newtonsoft.Json.JsonConvert.PopulateObject(vObjJson, vObj,
		// 		new JsonSerializerSettings
    	// 			{
        // 				Error = (object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args) =>
        // 				{
        //     				errors.Add(args.ErrorContext.Error.Message);
        //     				args.ErrorContext.Handled = true;
        // 				},
        // 				ObjectCreationHandling = ObjectCreationHandling.Replace
    	// 			}
		// 		);
        //
		// engineConfigJson = Newtonsoft.Json.JsonConvert.SerializeObject(engineConfig);
		// LoggerManager.LogDebug(engineConfigJson);

	}

	public partial class MoveState : HStateMachine { }
	public partial class RunState : HStateMachine { }
	public partial class JumpState : HStateMachine { }
	public partial class OtherState : HStateMachine { }
	public partial class Other2State : HStateMachine { }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void StateOnEnter()
	{
		LoggerManager.LogDebug("State OnEnter");
	}

	public void __On_EventServiceRegistered(IEvent eventObj)
	{
		LoggerManager.LogDebug("Received event!", "", "eventType", eventObj.GetType().Name);		
		LoggerManager.LogDebug("", "", "eventOwner", eventObj.Owner.GetType().Name);		
	}

	public void __On_EventServiceRegistered_highpriority(IEvent eventObj)
	{
		LoggerManager.LogDebug("Received high priority event!", "", "eventType", eventObj.GetType().Name);		
		LoggerManager.LogDebug("", "", "eventOwner", eventObj.Owner.GetType().Name);		
	}

	public void __On_Timer_timeout(IEvent eventObj)
	{
		LoggerManager.LogDebug("DataLoadTest: requesting load object");

		ServiceRegistry.Get<DataService>().LoadFromFile<EngineConfig>("config/CoreEngineConfig.json", onCompleteCb: (e) => {
				if (e is DataOperationComplete ee)
				{
					LoggerManager.LogDebug("DataLoadTest: My loaded object!", "", "object", ee.RunWorkerCompletedEventArgs.Result);

					if (ee.RunWorkerCompletedEventArgs.Result is OperationResult<EngineConfig> cecr)
					{
						cecr.ResultObject.LoggerConfig.LogLevel = Logging.Message.LogLevel.Info;

						// data save operation
						// DataOperation dataOperation = ServiceRegistry.Get<DataService>()
						// 	.SaveToFile<CoreEngineConfig>("config/CoreEngineConfig2.json", 
						// 		dataObject: cecr.ResultObject
						// 	);

						// ServiceRegistry.Get<DataService>().HTTPRequest<ValidatedObject>("catfact.ninja", 443, "/fact", HttpMethod.Get);
						// ServiceRegistry.Get<DataService>().HTTPRequest<ValidatedObject>("echo.free.beeceptor.com", 443, path: "/fact", requestMethod: HttpMethod.Get, dataObject: new StringContent("Some content"));
						ServiceRegistry.Get<DataService>().HTTPRequest<HTTPEchoResult>("echo.free.beeceptor.com", 443, path: "/fact", requestMethod: HttpMethod.Get, dataObject: cecr.ResultObject);
						// ServiceRegistry.Get<DataService>().HTTPRequest<ValidatedObject>("echo.free.beeceptor.com", 443, path: "/fact", requestMethod: HttpMethod.Get, urlParams: new Dictionary<string, object> { {"someParam", "someValue"},{"someParam2", "someValue2"} });

						VValue vv = CreateValidatedValue("System.String");

						vv.RawValue = "asd";

						LoggerManager.LogDebug("VVtest", "", "value", vv);
					}
				}
			});
	}

	public VValue CreateValidatedValue(string parameterTypeName)
    {
        Type parameterType = Type.GetType(parameterTypeName);
        Type genericType = typeof(VValue<>).MakeGenericType(parameterType);
        return (VValue) Activator.CreateInstance(genericType);
    }

	public void __On_Event(IEvent e)
	{
		if (e is BackgroundJobComplete ev)
		{
			LoggerManager.LogDebug($"Event data: {ev.RunWorkerCompletedEventArgs.Result}");
		}
		if (e is ValidatedValueChanged evv)
		{
			LoggerManager.LogDebug($"Event data: {evv.Owner}", "", "data", evv.Owner.GetType());
			LoggerManager.LogDebug($"Event data: {evv.Value}", "", "data", evv);
			LoggerManager.LogDebug($"Event data: {evv.PrevValue}");
		}
	}
}

public partial class HTTPEchoResult : VObject
{
	private readonly VValue<string> _method;

	public string Method
	{
		get { return _method.Value; }
		set { _method.Value = value; }
	}

	private readonly VValue<string> _protocol;
	public string Protocol
	{
		get { return _protocol.Value; }
		set { _protocol.Value = value; }
	}

	private readonly VValue<string> _host;
	public string Host
	{
		get { return _host.Value; }
		set { _host.Value = value; }
	}

	private readonly VValue<string> _ip;
	public string IP
	{
		get { return _ip.Value; }
		set { _ip.Value = value; }
	}

	public HTTPEchoResult()
	{
        _method = AddValidatedValue<string>(this)
            // .AllowedValues()
            ;
        _protocol = AddValidatedValue<string>(this)
            // .AllowedValues()
            ;
        _host = AddValidatedValue<string>(this)
            // .AllowedValues()
            ;
        _ip = AddValidatedValue<string>(this)
            // .AllowedValues()
            ;
	}
}


// 	public void FetchCatFact()
// 	{
// 		// testing loading from web urls
// 		LoggerManager.LogDebug("Fetching cat fact");
// 		var client = new System.Net.Http.HttpClient();
//             
//         try
//         {
//     		var webRequest = new HttpRequestMessage(HttpMethod.Get, "https://catfact.ninja/fact")
//     		{
//         		Content = new StringContent("{ 'some': 'value' }", Encoding.UTF8, "application/json")
//     		};
//
//     		var response = client.Send(webRequest);
//
//     		using var reader = new StreamReader(response.Content.ReadAsStream());
//             		
//     		var res = reader.ReadToEnd();
//     		System.Threading.Thread.Sleep(2000);
//
// 			this.Emit<Event>((e) => e.SetData(res));
//
//     		LoggerManager.LogDebug($"Cat fact from thread: {res}");
//
//     		previousCatFact = res;
//         }
//         catch (System.Exception ex)
//         {
// 			this.Emit<Event>((e) => e.SetData(ex.Message));
//         }
// 	}
//
// public partial class MyBackgroundJob : BackgroundJob
// {
// 	public override void DoWork(object sender, DoWorkEventArgs e)
// 	{
// 		// testing loading from web urls
// 		LoggerManager.LogDebug("Fetching cat fact");
// 		var client = new System.Net.Http.HttpClient();
//             
//         try
//         {
//     		var webRequest = new HttpRequestMessage(HttpMethod.Get, "https://catfact.ninja/fact")
//     		{
//         		Content = new StringContent("{ 'some': 'value' }", Encoding.UTF8, "application/json")
//     		};
//
//     		var response = client.Send(webRequest);
//
//     		using var reader = new StreamReader(response.Content.ReadAsStream());
//             		
//     		var res = reader.ReadToEnd();
//     		System.Threading.Thread.Sleep(2000);
//
//     		e.Result = res;
//         }
//         catch (System.Exception ex)
//         {
// 			throw;
//         }
// 	}
//
// 	public override void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
// 	{
//     	LoggerManager.LogDebug($"Cat fact from worker: {e.Result}");
// 	}
// }
