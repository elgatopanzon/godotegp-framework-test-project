namespace Godot.EGP;

using Godot;
using System;

public partial class SystemService : Service
{
	public bool ServiceReady { get; set; }

	public bool GetReady()
	{
		return ServiceReady;
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	// Called when service is registered in manager
	public void _OnServiceRegistered()
	{
		LoggerManager.LogDebug($"Service registered!", "", "service", this.GetType().Name);
	}

	// Called when service is deregistered from manager
	public void _OnServiceDeregistered()
	{
	}

	// Called when service is considered ready
	public void _OnServiceReady()
	{
	}
}
