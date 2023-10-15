namespace Godot.EGP;

using Godot;
using System;

public partial class GodotEGP : Node
{
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
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
