namespace Godot.EGP;

using Godot;
using Godot.EGP.Extensions;
using Godot.EGP.State;
using System;
using System.Collections.Generic;

public partial class GodotEGP : Node
{
	enum States
	{
		State10,
		State20,
		State30
	}
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

		var sub = new EventSubscription<EventServiceRegistered>(this, __On_EventServiceRegistered, false, new List<IEventFilter> {new EventFilterSignal("test")});
		var sub2 = new EventSubscription<EventServiceRegistered>(this, __On_EventServiceRegistered_highpriority, true);

		ServiceRegistry.Get<EventManager>().Subscribe(sub);
		ServiceRegistry.Get<EventManager>().Subscribe(sub2);

		Node obj = ServiceRegistry.Get<ObjectPoolService>().Get<Node>();

		Timer timer = new Timer();
		AddChild(timer);
		timer.WaitTime = 1.5;
		timer.Start();

		ServiceRegistry.Get<EventManager>().SubscribeSignal(timer, "timeout", false, new EventSubscription<EventSignal>(this, __On_Timer_timeout));
		ServiceRegistry.Get<EventManager>().SubscribeSignal(timer, "timeout", false, new EventSubscription<EventSignal>(this, __On_Timer_timeout));

		LoggerManager.LogDebug(ServiceRegistry.Get<NodeManager>().GetReady());

		Timer timer2 = new Timer();
		timer2.Name = "timer2";

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
				}, true, new List<IEventFilter>() {new EventFilterOwner(timer2)});

			AddChild(timer2);
			}, true, new List<IEventFilter>() {new EventFilterOwner(ServiceRegistry.Get<NodeManager>())});


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
	}
}
