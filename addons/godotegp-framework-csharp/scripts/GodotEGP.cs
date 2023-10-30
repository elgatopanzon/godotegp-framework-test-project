namespace Godot.EGP;

using Godot;
using Godot.EGP.Extensions;
using Godot.EGP.State;
using System;
using System.Collections.Generic;
using Godot.EGP.ValidatedObject;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.IO;
using System.Threading;
using System.ComponentModel;

public partial class GodotEGP : Node
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

		var sub = new EventSubscription<EventServiceRegistered>(this, __On_EventServiceRegistered, false, false, new List<IEventFilter> {new EventFilterSignal("test")});
		var sub2 = new EventSubscription<EventServiceRegistered>(this, __On_EventServiceRegistered_highpriority, true);

		ServiceRegistry.Get<EventManager>().Subscribe(sub);
		ServiceRegistry.Get<EventManager>().Subscribe(sub2);

		Node obj = ServiceRegistry.Get<ObjectPoolService>().Get<Node>();


		ServiceRegistry.Get<NodeManager>().SubscribeSignal("group_test_group", "timeout", false, __On_Timer_timeout, false, true);

		Godot.Timer timer = new Godot.Timer();
		timer.Name = "timer1";
		timer.AddToGroup("test_group");
		AddChild(timer);
		timer.WaitTime = 1.5;
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

		this.Subscribe<EventServiceReady>((e) => {
			this.Subscribe<EventNodeAdded>((e) => {
					if (ServiceRegistry.Get<NodeManager>().TryGetNode("timer2", out Node n))
					{
						LoggerManager.LogDebug("Timer node exists", "", "timer", n.ToString());
					}
				}, true, false, new List<IEventFilter>() {new EventFilterOwner(timer2)});

			AddChild(timer2);
			}, true, false, new List<IEventFilter>() {new EventFilterOwner(ServiceRegistry.Get<NodeManager>())});

		// validated objects testing
		ValidatedObjectTest vObj = new ValidatedObjectTest();
		// LoggerManager.LogDebug(vObj);
        //
        //
		string vObjJson = Newtonsoft.Json.JsonConvert.SerializeObject(vObj);
		LoggerManager.LogDebug(vObjJson);

		vObjJson = "{'StringListTest':[1,2,3],'DictionarySizeTest':{'a':1,'b':1,'c':1},'StringTest':'string','IntTest':5,'DoubleTest':5.0,'UlongTest':5,'IntArrayTest':[1,2,3],'Vector2Test':{'X':1.0,'Y':1.0}}";
		// vObjJson = "{\"StringListTest\":{\"Value\":[\"d\",\"e\",\"f\"]},\"DictionarySizeTest\":{\"Value\":{\"d\":\"123\",\"e\":1,\"f\":1}},\"StringTest\":{\"Value\":\"string\"},\"IntTest\":{\"Value\":5},\"DoubleTestt\":{\"Value\":5.0},\"UlongTestt\":{\"Value\":5},\"IntArrayTest\":{\"Value\":[\"123\",2,3]},\"Vector2Testt\":{\"Value\":{\"X\":2.0,\"Y\":2.0}}}";
		LoggerManager.LogDebug(vObjJson);

		List<string> errors = new List<string>();

		// ValidatedObjectTest vObj2 = Newtonsoft.Json.JsonConvert.DeserializeObject<ValidatedObjectTest>(vObjJson,
		Newtonsoft.Json.JsonConvert.PopulateObject(vObjJson, vObj,
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

		vObjJson = Newtonsoft.Json.JsonConvert.SerializeObject(vObj);
		LoggerManager.LogDebug(vObjJson);

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

		this.Subscribe<Event>(__On_Event);
	}

	public class MoveState : HStateMachine { }
	public class RunState : HStateMachine { }
	public class JumpState : HStateMachine { }
	public class OtherState : HStateMachine { }
	public class Other2State : HStateMachine { }

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
		LoggerManager.LogDebug("Timeout!");
		LoggerManager.LogDebug($"Previous cat fact: {previousCatFact}");

		// Thread t = new Thread(new ThreadStart(FetchCatFact));
		// t.Start();
		MyBackgroundJob bgj = new MyBackgroundJob();

		// this.Subscribe<EventBackgroundJobComplete>(__On_Event, true, true, new List<IEventFilter> {new EventFilterOwner(bgj)});
		this.Subscribe<EventBackgroundJobComplete>(__On_Event, isHighPriority:false, oneshot:true).Filters(new EventFilterOwner(bgj));

		// bgj.OnComplete = (RunWorkerCompletedEventArgs e) => {
		// 	bgj.Emit<Event>((ev) => ev.SetData(bgj.Result));
		// };

		bgj.Run();
	}

	public void FetchCatFact()
	{
		// testing loading from web urls
		LoggerManager.LogDebug("Fetching cat fact");
		var client = new System.Net.Http.HttpClient();
            
        try
        {
    		var webRequest = new HttpRequestMessage(HttpMethod.Get, "https://catfact.ninja/fact")
    		{
        		Content = new StringContent("{ 'some': 'value' }", Encoding.UTF8, "application/json")
    		};

    		var response = client.Send(webRequest);

    		using var reader = new StreamReader(response.Content.ReadAsStream());
            		
    		var res = reader.ReadToEnd();
    		System.Threading.Thread.Sleep(2000);

			this.Emit<Event>((e) => e.SetData(res));

    		LoggerManager.LogDebug($"Cat fact from thread: {res}");

    		previousCatFact = res;
        }
        catch (System.Exception ex)
        {
			this.Emit<Event>((e) => e.SetData(ex.Message));
        }
	}

	public void __On_Event(IEvent e)
	{
		if (e is EventBackgroundJobComplete ev)
		{
			LoggerManager.LogDebug($"Event data: {ev.RunWorkerCompletedEventArgs.Result}");
		}
	}
}

public class BackgroundJob
{
	BackgroundWorker worker = new BackgroundWorker();
	public Action<DoWorkEventArgs> OnWorking;
	public Action<ProgressChangedEventArgs> OnProgress;
	public Action<RunWorkerCompletedEventArgs> OnComplete;

	public BackgroundJob()
	{
	}

	public void _setup()
	{
		worker.DoWork += new DoWorkEventHandler(_On_DoWork);
		worker.ProgressChanged += new ProgressChangedEventHandler(_On_ProgressChanged);
		worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_On_RunWorkerCompleted);
		worker.WorkerReportsProgress = true;
		worker.WorkerSupportsCancellation = true;
	}

	public virtual void Run()
	{
		_setup();
		worker.RunWorkerAsync();
	}

	// handlers for background worker events
	public virtual void _On_DoWork(object sender, DoWorkEventArgs e)
	{
		if (OnWorking != null)
		{
			OnWorking(e);
		}

		this.Emit<EventBackgroundJobWorking>((ev) => ev.SetDoWorkEventArgs(e));

		DoWork(sender, e);
	}

	public virtual void _On_ProgressChanged(object sender, ProgressChangedEventArgs e)
	{
		if (OnProgress != null)
		{
			OnProgress(e);
		}

		this.Emit<EventBackgroundJobProgress>((ev) => ev.SetProgressChangesEventArgs(e));

		ProgressChanged(sender, e);
	}

	public virtual void _On_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
		if (OnComplete != null)
		{
			OnComplete(e);
		}

		RunWorkerCompleted(sender, e);

		this.Emit<EventBackgroundJobComplete>((ev) => ev.SetRunWorkerCompletedEventArgs(e));
	}


	// override these to do the work
	public virtual void DoWork(object sender, DoWorkEventArgs e)
	{
		LoggerManager.LogDebug("Working!");

		System.Threading.Thread.Sleep(1000);

		worker.ReportProgress(50);

		LoggerManager.LogDebug("More work!");

		System.Threading.Thread.Sleep(1000);
	}

	public virtual void ProgressChanged(object sender, ProgressChangedEventArgs e)
	{
		LoggerManager.LogDebug("Job progress", "", "progress", e.ProgressPercentage);
	}

	public virtual void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
		LoggerManager.LogDebug("Done!");
	}
}

public class MyBackgroundJob : BackgroundJob
{
	public override void DoWork(object sender, DoWorkEventArgs e)
	{
		// testing loading from web urls
		LoggerManager.LogDebug("Fetching cat fact");
		var client = new System.Net.Http.HttpClient();
            
        try
        {
    		var webRequest = new HttpRequestMessage(HttpMethod.Get, "https://catfact.ninja/fact")
    		{
        		Content = new StringContent("{ 'some': 'value' }", Encoding.UTF8, "application/json")
    		};

    		var response = client.Send(webRequest);

    		using var reader = new StreamReader(response.Content.ReadAsStream());
            		
    		var res = reader.ReadToEnd();
    		System.Threading.Thread.Sleep(2000);

    		e.Result = res;
        }
        catch (System.Exception ex)
        {
			throw;
        }
	}

	public override void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
    	LoggerManager.LogDebug($"Cat fact from worker: {e.Result}");
	}
}
