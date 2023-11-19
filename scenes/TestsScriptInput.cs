using Godot;
using System;

using GodotEGP.Logging;
using GodotEGP.Objects.Extensions;

public partial class TestsScriptInput : TextEdit
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public override void _Input(InputEvent @event)
    {
    	if (@event is InputEventKey key)
    	{
    		if (key.Pressed == true && key.KeyLabel == Key.Enter && key.ShiftPressed == true)
    		{
    			AcceptEvent();
    			InsertTextAtCaret("\n");
    		}
    		if (key.Pressed == true && key.KeyLabel == Key.Enter && key.ShiftPressed == false)
    		{
    			"UITests.Scripting.Eval".Node<Button>().EmitSignal(Button.SignalName.Pressed);
    			Text = "";
    			AcceptEvent();
    		}
    	}
    }
}
