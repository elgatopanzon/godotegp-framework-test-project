namespace Godot.EGP;

using Godot;
using Godot.EGP.State;
using System;

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
		ServiceRegistry.Instance.RegisterService(LoggerManager.Instance, "Log");
		ServiceRegistry.Instance.RegisterService(new Service(), "Base");
		ServiceRegistry.Instance.RegisterService(new TestService(), "Test");
		ServiceRegistry.Instance.RegisterService(new ObjectPoolService(), "ObjectPool");

		GD.Print(ServiceRegistry.Get<Service>().GetReady());
		GD.Print(ServiceRegistry.Get<TestService>().GetReady());
		// GD.Print(ServiceRegistry.Instance["Test"].GetReady());
		
		ServiceRegistry.Get<ObjectPoolService>().SetPoolConfig<Node>(10, 100);

		Node obj = ServiceRegistry.Get<ObjectPoolService>().Get<Node>();

		GD.Print($"Obj: {obj} {obj.GetType().Name}");

		ServiceRegistry.Get<ObjectPoolService>().Return<Node>(obj);

		obj = ServiceRegistry.Get<ObjectPoolService>().Get<Node>();

		GD.Print($"Obj: {obj} {obj.GetType().Name}");

		LoggerManager.LogTrace("testing");
		LoggerManager.LogDebug("testing");
		LoggerManager.LogInfo("testing");
		LoggerManager.LogWarning("testing");
		LoggerManager.LogError("testing");
		LoggerManager.LogCritical("testing");
		LoggerManager.LogDebug("testing 2", "Custom Name", "node_pool_config", ServiceRegistry.Get<ObjectPoolService>().GetPoolConfig<Node>());

		LoggerManager.LogDebug("testing long multi-line string again just to see");

		StateMachine sm = new StateMachine(this);

		// add some test states
		sm.Add(States.State10);
		sm.Add(States.State20);
		sm.Add(States.State30);

		// start the FSM
		sm.Init(States.State10);

		sm.RegisterCallback(States.State20, StateMachine.CallbackType.OnEnter, () => {
			LoggerManager.LogDebug("State change callback state2, OnEnter");
			});
		sm.RegisterCallback(States.State20, StateMachine.CallbackType.OnExit, () => {
			LoggerManager.LogDebug("State change callback state2, OnExit");
			});

		sm.RegisterCallback(States.State20, StateMachine.CallbackType.OnChanged, () => {
			LoggerManager.LogDebug("State change callback state3, OnChanged");
			});

		// test changing state
		sm.Change(States.State20);

		sm.Change(States.State30);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
