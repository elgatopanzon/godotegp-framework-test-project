namespace Godot.EGP;

using System;

public class TestClass
{
	public TestClass()
	{
		GD.Print("TestClass");
	}

	// Called when the node enters the scene tree for the first time.
	public void _Ready()
	{
		ServiceRegistry.Instance.RegisterService(new Service(), "Base");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public void _Process(double delta)
	{
	}
}

