/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : Functions
 * @created     : Thursday Nov 16, 2023 14:32:05 CST
 */

namespace GodotEGP.Scripting.Functions;

using System;
using System.Linq;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

public partial class Echo : IScriptFunction
{
	public ScriptProcessResult Call(ScriptInterpretter i, params object[] p)
	{
		return new ScriptProcessResult(0, (p as string[]).Join(" "));
	}
}

public partial class Source : IScriptFunction
{
	public ScriptProcessResult Call(ScriptInterpretter i, params object[] p)
	{
		LoggerManager.LogDebug("Source called");

		// created a function call as if we are calling this script directly
		string func = (string) p[0];
		p = p.Skip(1).ToArray();

		i.ChildKeepEnv = true;

		return i.ExecuteFunctionCall(func, p as string[]);
	}
}

public partial class Goto : IScriptFunction
{
	public ScriptProcessResult Call(ScriptInterpretter i, params object[] p)
	{
		LoggerManager.LogDebug("Goto called", "", "goto", p[0]);

		i.ScriptLinerCounter = Convert.ToInt32(p[0]) - 1;

		return new ScriptProcessResult(0);
	}
}

public partial class Cat : IScriptFunction
{
	public ScriptProcessResult Call(ScriptInterpretter i, params object[] p)
	{
		string result = "";

		// stdin hack
		if (p.Length == 0 && i.ScriptVars.ContainsKey("STDIN"))
		{
			result = (string) i.ScriptVars["STDIN"];
		}

		return new ScriptProcessResult(0, result);
	}
}

public partial class EvaluateExpression : IScriptFunction
{
	public ScriptProcessResult Call(ScriptInterpretter i, params object[] p)
	{
		System.Data.DataTable table = new System.Data.DataTable();
		var r = table.Compute(p[0].ToString(), null);

		LoggerManager.LogDebug("Eval test result type", "", "resType", r.GetType().Name);

		return new ScriptProcessResult(0, r.ToString(), rawResult: r);
	}
}
