namespace Godot.EGP;

using Godot;
using System;

public partial class TestService : Service
{
	public override void _Ready()
	{
		_SetServiceReady(true);
	}
}
