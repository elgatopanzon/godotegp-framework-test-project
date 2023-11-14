/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ScriptingTest
 * @created     : Monday Nov 13, 2023 22:56:15 CST
 */

namespace GodotEGP.Scripting;

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

public partial class ScriptingTest
{
	private string _script;

	private Dictionary<string, Func<int, string>> _functionDefinitions = new Dictionary<string, Func<int, string>>();

	public ScriptingTest(string script)
	{
		_script = script;

		if (_script.Length == 0)
		{
			_script += @"echo ""this text should act like a simple print statement""\n";
			_script += @"echo ""testing: setting variables content""\n";
			_script += @"VARNAME=""some string value""\n";
			_script += @"echo ""testing: echoing variables: $VARNAME""\n";
			_script += @"echo ""testing: setting variables to content with variables inside""\n";
			_script += @"VARNAME=""home: $HOME""\n";
			_script += @"logdebug ""logging to debug log""\n";
			_script += @"echo ""testing"" ""multiple"" ""echo"" ""params""\n";
			_script += @"echo echo without quotes\n";
			_script += @"echo 1 2 3\n";
			_script += @"echo ""testing: setting variables to number types""\n";
			_script += @"VARINT=1\n";
			_script += @"VARFLOAT=1.1\n";
			_script += @"echo ""testing: enclosed script lines content""\n";
			_script += @"echo ""$(echo this should return this string)""\n";
			_script += @"echo ""$(echo this is part)"" ""$(echo of multiple)"" ""$(echo nested lines)""\n";
			_script += @"echo ""testing: accessing array elements""\n";
			_script += @"echo ""$VARARRAY[0]""\n";
			_script += @"echo ""$VARARRAY[1]""\n";
			_script += @"echo ""testing: accessing dictionary elements""\n";
			_script += @"echo ""$VARARRAY['key']""\n";

			// // if statements
			// _script += @"if [ 1 -gt 100]; then\n";
			// _script += @"echo omg such a large number\n";
			// _script += @"fi\n";
			// _script += @"if [ 1 -gt 100] || [ 1 -le 100]; then\n";
			// _script += @"echo uh ok\n";
			// _script += @"fi\n";
			// _script += @"if [ ""2"" == ""2"" ]; then\n";
			// _script += @"echo omg such a large number\n";
			// _script += @"fi\n";
            //
			// // while loops
			// _script += @"counter=1\n";
			// _script += @"while [ $counter -le 10 ]; do\n";
			// _script += @"echo count: $counter\n";
			// _script += @"((counter++))\n";
			// _script += @"done\n";
            //
			// // for loops
			// _script += @"names=""name1 name2 name3""\n";
			// _script += @"for name in $names; do\n";
			// _script += @"echo name: $name\n";
			// _script += @"done\n";
            //
			// // for loops range
			// _script += @"for val in {1..5}; do\n";
			// _script += @"echo val: $val\n";
			// _script += @"done\n";
            //
			// // multiline with commas
			// _script += @"echo one; echo two; echo three\n";
			// _script += @"echo one; echo ""$(echo a; echo b)""; echo three\n";
		}
		LoggerManager.LogDebug(_script);

		// creates a list of lines, each with a list of processes for each line
		var interprettedLines = InterpretLines(_script);

		for (int i = 0; i < interprettedLines.Count; i++)
		{
			for (int ii = 0; ii < interprettedLines[i].Count; ii++)
			{
				if (ii == 0)
				{
					LoggerManager.LogDebug($"Line {i}", "", "line", interprettedLines[i][ii].ScriptLine);
				}
				LoggerManager.LogDebug($"Line {i} process {ii} {interprettedLines[i][ii].GetType().Name}", "", "process", interprettedLines[i][ii]);
			}
		}
	}

	public List<List<ScriptProcessOperation>> InterpretLines(string scriptLines)
	{
		List<List<ScriptProcessOperation>> processes = new List<List<ScriptProcessOperation>>();

		foreach (string line in scriptLines.Split(new string[] {"\\n"}, StringSplitOptions.None))
		{
			processes.Add(InterpretLine(line.Trim()));
		}

		return processes;
	}

	public List<ScriptProcessOperation> InterpretLine(string line)
	{
		LoggerManager.LogDebug("Evaluating script line", "", "line", line);

		// TODO: split and process lines with ; and pipes

		// execution and parse order
		// 1. parse printed vars to real values in unparsed line
			// parse var names in expressions (( )) and replace with actual
			// values e.g. number or string
		// 2. parse nested lines as a normal line, replacing the executed result
		// 3. parse variable assignments
		// 4. parse function calls
		// 5. parse if/while/for calls

		// list of process operations to do to this script line
		List<ScriptProcessOperation> processes = new List<ScriptProcessOperation>();
		
		// first thing, parse and replace variable names with values
		processes.AddRange(ParseVarSubstitutions(line));

		// second thing, parse and replace expressions with values
		processes.AddRange(ParseExpressions(line));

		// third thing, parse nested script lines and replace values
		processes.AddRange(ParseNestdLines(line));

		// parse variable assignments
		var varAssignmentProcesses = ParseVarAssignments(line);
		processes.AddRange(varAssignmentProcesses);

		// if var assignments are 0, then try to match function calls
		// NOTE: this is because the regex matches both var assignments in lower
		// case AND function calls
		if (varAssignmentProcesses.Count == 0)
		{
			processes.AddRange(ParseFunctionCalls(line));
		}

		return processes;
	}

	public List<ScriptProcessOperation> ParseNestdLines(string line)
	{
		List<ScriptProcessOperation> processes = new List<ScriptProcessOperation>();

		string patternNestedLine = @"((?<=\$\()[^""\n]*(?=\)))";

		MatchCollection nl = Regex.Matches(line, patternNestedLine, RegexOptions.Multiline);
		foreach (Match match in nl)
		{
			if (match.Groups.Count >= 1)
			{
				string nestedLine = match.Groups[0].Value;

				List<string> nestedLines = new List<string>();

				// LoggerManager.LogDebug("Nested line matche", "", "nestedLine", $"{nestedLine}");

				processes.Add(new ScriptProcessNestedProcess(line, InterpretLine(nestedLine)));
			}
		}

		return processes;
	}

	public List<ScriptProcessOperation> ParseVarSubstitutions(string line)
	{
		List<ScriptProcessOperation> processes = new List<ScriptProcessOperation>();

		string patternVarSubstitution = @"\$([a-zA-Z0-9_\[\]']+)";
		MatchCollection varSubstitutionMatches = Regex.Matches(line, patternVarSubstitution);

		foreach (Match match in varSubstitutionMatches)
		{
			if (match.Groups.Count >= 2)
			{
				processes.Add(new ScriptProcessVarSubstitution(line, match.Groups[1].Value));
			}
		}

		return processes;
	}

	public List<ScriptProcessOperation> ParseExpressions(string line)
	{
		List<ScriptProcessOperation> processes = new List<ScriptProcessOperation>();

		string patternExpression = @"\(\((.+)\)\)";
		MatchCollection expressionMatches = Regex.Matches(line, patternExpression);

		foreach (Match match in expressionMatches)
		{
			processes.Add(new ScriptProcessExpression(line, match.Groups[1].Value));
		}

		return processes;
	}

	public List<ScriptProcessOperation> ParseVarAssignments(string line)
	{
		List<ScriptProcessOperation> processes = new List<ScriptProcessOperation>();
		
		string patternIsVarAssignment = @"^[a-zA-Z0-9_]+=.+";
		Match isVarAssignment = Regex.Match(line, patternIsVarAssignment);

		// check if it's a variable
		if (isVarAssignment.Success)
		{
			// string patternVars = @"(^\b[A-Z]+)=([\w.]+)"; // matches VAR assignments without quotes
			// string patternVars = @"(^\b[A-Z]+)=[""](\w.+)[""|\w.^]"; //
			// string patternVars = @"(^\b[A-Z_]+)=(([\w.]+)|""(.+)"")"; // matches VAR assignments with and without quotes
			string patternVars = @"(^\b[a-zA-Z0-9_]+)=(([\w.]+)|.+)"; // matches VAR assignments with and without quotes, keeping the quotes

			// matches VAR assignments in strings
			MatchCollection m = Regex.Matches(line, patternVars, RegexOptions.Multiline);
			foreach (Match match in m)
			{
				if (match.Groups.Count >= 3)
				{
					string varName = match.Groups[1].Value;
					string varValue = match.Groups[2].Value;

					if (varValue.StartsWith("\"") && varValue.EndsWith("\""))
					{
						varValue = varValue.Trim('\"');
					}

					processes.Add(new ScriptProcessVarAssignment(line, varName, varValue));
					// LoggerManager.LogDebug("Variable assignment match", "", "assignment", execLine);
				}
			}
		}

		return processes;
	}

	public List<ScriptProcessOperation> ParseFunctionCalls(string line)
	{
		List<ScriptProcessOperation> processes = new List<ScriptProcessOperation>();

		// string patternFuncCall = @"(^\b[a-z_]+) (""[^""]+"")$"; // matches function calls with single param in quotes but not without
		// string patternFuncCall = @"(^\b[a-z_]+) ((""[^""]+"")$|(\w.+))"; // matches function calls with single param in quotes, and multiple param without quotes as single string (can be split?)
		// string patternFuncCall = @"(^\b[a-z_]+) ((""[^""]+"")$|(\w.+)|(([\w.]+)|.+))"; // matches single param
		string patternFuncCall = @"(^\b[a-z_]+) *((""[^""]+"")*$|(\w.+)|(([\w.]+)|.+))"; // matches single param

		MatchCollection fm = Regex.Matches(line, patternFuncCall, RegexOptions.Multiline);
		foreach (Match match in fm)
		{
			if (match.Groups.Count >= 3)
			{
				string funcName = match.Groups[1].Value;
				string funcParamsStr = match.Groups[2].Value;

				List<string> funcParams = new List<string>();

				// foreach (Match fmatches in Regex.Matches(funcParamsStr, @"(?<="")[^""\n]*(?="")|[\w]+"))
				foreach (Match fmatches in Regex.Matches(funcParamsStr, @"((?<="")[^""\n]*(?=""))|([\w-_]+)"))
				{
					Match nm = fmatches;

					if (nm.Groups[0].Value != " ")
					{
						funcParams.Add(nm.Groups[0].Value);
					}

					nm = nm.NextMatch();
				}

				processes.Add(new ScriptProcessFunctionCall(line, funcName, funcParams));
				// LoggerManager.LogDebug("Function call match", "", "call", $"func name: {funcName}, params: [{string.Join("|", funcParams.ToArray())}]");
			}
		}

		return processes;
	}
}





public class ScriptProcessOperation
{
	private string _scriptLine;
	public string ScriptLine
	{
		get { return _scriptLine; }
		set { _scriptLine = value; }
	}

	public ScriptProcessOperation(string scriptLine)
	{
		_scriptLine = scriptLine;
	}
}

public class ScriptProcessVarAssignment : ScriptProcessOperation
{
	private string _varName;
	public string Name
	{
		get { return _varName; }
		set { _varName = value; }
	}

	private string _varValue;
	public string Value
	{
		get { return _varValue; }
		set { _varValue = value; }
	}

	public ScriptProcessVarAssignment(string scriptLine, string varName, string varValue) : base(scriptLine)
	{
		_varName = varName;
		_varValue = varValue;
	}
}

public class ScriptProcessVarSubstitution : ScriptProcessOperation
{
	private string _varName;
	public string Name
	{
		get { return _varName; }
		set { _varName = value; }
	}

	public ScriptProcessVarSubstitution(string scriptLine, string varName) : base(scriptLine)
	{
		_varName = varName;
	}
}

public class ScriptProcessNestedProcess : ScriptProcessOperation
{
	private List<ScriptProcessOperation> _nestedProcesses;
	public List<ScriptProcessOperation> Processes
	{
		get { return _nestedProcesses; }
		set { _nestedProcesses = value; }
	}

	public ScriptProcessNestedProcess(string scriptLine, List<ScriptProcessOperation> nestedProcesses) : base(scriptLine)
	{
		_nestedProcesses = nestedProcesses;
	}
}

public class ScriptProcessExpression : ScriptProcessOperation
{
	private string _expression;
	public string Expression
	{
		get { return _expression; }
		set { _expression = value; }
	}

	public ScriptProcessExpression(string scriptLine, string expression) : base(scriptLine)
	{
		_expression = expression;
	}
}

public class ScriptProcessFunctionCall : ScriptProcessOperation
{
	private string _funcName;
	public string Function
	{
		get { return _funcName; }
		set { _funcName = value; }
	}

	private List<string> _funcParams;
	public List<string> Params
	{
		get { return _funcParams; }
		set { _funcParams = value; }
	}

	public ScriptProcessFunctionCall(string scriptLine, string funcName, List<string> funcParams) : base(scriptLine)
	{
		_funcName = funcName;
		_funcParams = funcParams;
	}
}
