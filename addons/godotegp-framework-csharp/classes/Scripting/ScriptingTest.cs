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
	private string[] _currentScriptLinesSplit;

	private int _scriptLineCounter = 0;

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

			// if statements
			_script += @"if [ 1 -gt 100]\n";
			_script += @"then\n";
			_script += @"echo omg such a large number\n";
			_script += @"fi\n";

			_script += @"if [ 1 -gt 100] || [ 1 -le 100]\n";
			_script += @"then\n";
			_script += @"echo uh ok\n";
			_script += @"fi\n";

			_script += @"if [ ""2"" == ""2"" ]\n";
			_script += @"then\n";
			_script += @"echo omg such a large number\n";
			_script += @"fi\n";

			_script += @"if [ ""$SOMEVARVAL"" = ""1"" ]\n";
			_script += @"then\n";
			_script += @"echo It's equal to 1 yay\n";
			_script += @"elif [ ""$SOMEVARVAL"" = ""$(somefunccall random_param_1 another_param)"" ]\n";
			_script += @"then\n";
			_script += @"echo did you know? $(echo this is nested!)\n";
			_script += @"else\n";
			_script += @"echo eh it's actually ""$SOMEVARVAL""\n";
			_script += @"fi\n";
            //
			// // while loops
			_script += @"counter=1\n";
			_script += @"while [ $counter -le 10 ]\n";
			_script += @"do\n";
			_script += @"echo count: $counter\n";
			_script += @"((counter++))\n";
			_script += @"done\n";
            //
			// // for loops
			_script += @"names=""name1 name2 name3""\n";
			_script += @"for name in $names\n";
			_script += @"do\n";
			_script += @"echo name: $name\n";
			_script += @"done\n";
            //
			// // for loops range
			_script += @"for val in {1..5}\n";
			_script += @"do\n";
			_script += @"echo val: $val\n";
			_script += @"done\n";
            //
			// // multiline with commas
			// _script += @"echo one; echo two; echo three\n";
			// _script += @"echo one; echo ""$(echo a; echo b)""; echo three\n";

			// nested if else else
			_script += @"if [ ""2"" = ""2"" ]\n";
			_script += @"then\n";
			_script += @"if [ ""a"" = ""a"" ]\n";
			_script += @"then\n";
			_script += @"echo omg such a large number\n";
			_script += @"else\n";
			_script += @"echo not a large number...\n";
			_script += @"fi\n";
			_script += @"else\n";
			_script += @"echo it's an else\n";
			_script += @"fi\n";

			// some var setting tests
			_script += @"c=""$(a)$(b)""\n";
			_script += @"c=$( ((a + b)) )\n";
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

		_currentScriptLinesSplit = scriptLines.Split(new string[] {"\\n"}, StringSplitOptions.None);

		while (_scriptLineCounter < _currentScriptLinesSplit.Count())
		{
			string linestr = _currentScriptLinesSplit[_scriptLineCounter].Trim();

			processes.Add(InterpretLine(linestr));

			_scriptLineCounter++;
		}

		return processes;
	}

	public ScriptProcessOperation ParseBlockProcessLine(string line, string[] scriptLines)
	{
		string patternBlockProcess = @"^(if|while|for)\[?(.+)*\]*";
		Match isBlockProcess = Regex.Match(line, patternBlockProcess, RegexOptions.Multiline);

		string fullScriptLine = "";

		if (isBlockProcess.Groups.Count >= 3)
		{
			string blockProcessType = isBlockProcess.Groups[1].Value;
			string blockProcessCondition = isBlockProcess.Groups[2].Value.Trim();

			List<(List<(List<ScriptProcessOperation>, string)>, List<List<ScriptProcessOperation>>)> blockConditions = new List<(List<(List<ScriptProcessOperation>, string)>, List<List<ScriptProcessOperation>>)>();

			// look over the next lines and build up the process block
			List<List<ScriptProcessOperation>> currentBlockProcesses = new List<List<ScriptProcessOperation>>();
			List<(List<ScriptProcessOperation>, string)> currentBlockCondition = null;
			List<(List<ScriptProcessOperation>, string)> prevBlockCondition = null;

			while (true)
			{
				string forwardScriptLine = scriptLines[_scriptLineCounter];

				var forwardLineConditions = ParseProcessBlockConditions(forwardScriptLine);

				_scriptLineCounter++;

				if (Regex.Match(forwardScriptLine, patternBlockProcess, RegexOptions.Multiline).Groups.Count >= 3 && forwardScriptLine != line)
				{
					LoggerManager.LogDebug("Nested if found!", "", "line", $"{forwardScriptLine} {line}");
					_scriptLineCounter--;
					var parsedNestedBlock = ParseBlockProcessLine(forwardScriptLine, scriptLines);
					currentBlockProcesses.Add(new List<ScriptProcessOperation> {parsedNestedBlock});
					fullScriptLine += parsedNestedBlock.ScriptLine;
					_scriptLineCounter++;

					continue;
				}

				fullScriptLine += forwardScriptLine+"\n";

				// if we have conditional matches, it's an elif or a nested if
				if (forwardLineConditions.Count > 0)
				{
					LoggerManager.LogDebug("Block conditions found in line", "", "line", forwardScriptLine);

					// if (line != forwardScriptLine)
					// {
					// 	var nestedIfProcess = ParseBlockProcessLine(forwardScriptLine, scriptLines);
                    //
					// 	if (nestedIfProcess != null)
					// 	{
					// 		LoggerManager.LogDebug("Nested if process!", "", "nestedIf", nestedIfProcess);
					// 	}
					// }
                    //
					// set current condition to the one we just found
					if (currentBlockProcesses.Count > 0)
					{
						blockConditions.Add((currentBlockCondition, currentBlockProcesses));
					}

					currentBlockProcesses = new List<List<ScriptProcessOperation>>();
					currentBlockCondition = forwardLineConditions;
				}

				// expected when we have entered a conditional statement block
				else if (forwardScriptLine == "else")
				{
					LoggerManager.LogDebug("else line");


					// reset current processes list to account for the next
					// upcoming lines
					blockConditions.Add((currentBlockCondition, currentBlockProcesses));
					currentBlockProcesses = new List<List<ScriptProcessOperation>>();
					currentBlockCondition = null;

					continue;
				}

				// expected when we have entered a conditional statement block
				else if (forwardScriptLine == "then" || forwardScriptLine == "do")
				{
					LoggerManager.LogDebug("then/do line", "", "conditions", currentBlockCondition);


					// reset current processes list to account for the next
					// upcoming lines
					currentBlockProcesses = new List<List<ScriptProcessOperation>>();

					continue;
				}

				// end of the current block, let's exit the loop
				else if (forwardScriptLine == "fi" || forwardScriptLine == "done")
				{
					LoggerManager.LogDebug("fi/done line, reached end of block");

					// // add previous condition processes if there are any
					if (currentBlockProcesses.Count > 0)
					{
						blockConditions.Add((currentBlockCondition, currentBlockProcesses));
					}

					_scriptLineCounter--;

					LoggerManager.LogDebug("Block conditions list", "", "blockConditions", blockConditions);
					return new ScriptProcessBlockProcess(fullScriptLine, blockProcessType, blockConditions);
				}

				// we should be capturing lines as processes here
				else
				{
					currentBlockProcesses.Add(InterpretLine(forwardScriptLine));
				}


			}

		}

		return null;
	}

	public List<(List<ScriptProcessOperation>, string)> ParseProcessBlockConditions(string scriptLine)
	{
		string patternBlockProcessCondition = @"\[(.*?)\] ?(\|?\|?)";

		MatchCollection blockProcessConditionMatches = Regex.Matches(scriptLine, patternBlockProcessCondition, RegexOptions.Multiline);

		List<(List<ScriptProcessOperation>, string)> conditionsList = new List<(List<ScriptProcessOperation>, string)>();

		if (scriptLine.StartsWith("for "))
		{
			conditionsList.Add((InterpretLine(scriptLine.Replace("for ", "")), ""));
		}

		foreach (Match match in blockProcessConditionMatches)
		{
			string blockConditionInside = match.Groups[1].Value;
			string blockConditionCompareType = match.Groups[2].Value;

			var interpretted = InterpretLine(blockConditionInside.Trim());

			conditionsList.Add((interpretted, blockConditionCompareType.Trim()));
		}

		return conditionsList;
	}

	public List<ScriptProcessOperation> InterpretLine(string line)
	{
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

		var blockProcess = ParseBlockProcessLine(line, _currentScriptLinesSplit);
		if (blockProcess != null)
		{
			processes.AddRange(new List<ScriptProcessOperation>() {blockProcess});
		}
		else
		{
			// if var assignments are 0, then try to match function calls
			// NOTE: this is because the regex matches both var assignments in lower
			// case AND function calls
			if (varAssignmentProcesses.Count == 0)
			{
				processes.AddRange(ParseFunctionCalls(line));
			}
		}

		// if there's no processes until now, just return the plain object with
		// no processing attached
		if (processes.Count == 0)
		{
			processes.Add(new ScriptProcessOperation(line));
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
				foreach (Match fmatches in Regex.Matches(funcParamsStr, @"((?<="")[^""\n]*(?=""))|([\w-_'\:]+)"))
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

	private string _type;
	public string ProcessType
	{
		get { return _type; }
		set { _type = value; }
	}

	public ScriptProcessOperation(string scriptLine)
	{
		_scriptLine = scriptLine;
		_type = this.GetType().Name;
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

public class ScriptProcessBlockProcess : ScriptProcessOperation
{
	private string _blockType;
	public string Type
	{
		get { return _blockType; }
		set { _blockType = value; }
	}

	private List<(List<(List<ScriptProcessOperation>, string)>, List<List<ScriptProcessOperation>> BlockProcesses)> _blockProcesses;
	public List<(List<(List<ScriptProcessOperation>, string)>, List<List<ScriptProcessOperation>>)> Processes
	{
		get { return _blockProcesses; }
		set { _blockProcesses = value; }
	}

	public ScriptProcessBlockProcess(string scriptLine, string blockType, List<(List<(List<ScriptProcessOperation>, string)>, List<List<ScriptProcessOperation>>)> blockProcesses) : base(scriptLine)
	{
		_blockType = blockType;
		_blockProcesses = blockProcesses;
	}
}
