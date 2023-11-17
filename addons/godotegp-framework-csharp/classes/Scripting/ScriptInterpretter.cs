/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ScriptInterpretter
 * @created     : Wednesday Nov 15, 2023 17:32:08 CST
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

using GodotEGP.State;
using GodotEGP.Resource;
using GodotEGP.Scripting.Functions;

public partial class ScriptInterpretter : Node
{
	// state classes for process state machine
	public class Process : HStateMachine {}
	public class Preparing : HStateMachine {}
	public class Running : HStateMachine {}
	public class Waiting : HStateMachine {}
	public class Finished : HStateMachine {}

	private Process _processState = new Process();
	private Preparing _statePreparing = new Preparing();
	private Running _stateRunning = new Running();
	private Waiting _stateWaiting = new Waiting();
	private Finished _stateFinished = new Finished();

	private const int STATE_RUNNING = 0;
	private const int STATE_WAITING = 1;
	private const int STATE_FINISHED = 2;

	// gamescript related properties
	private Dictionary<string, Resource<GameScript>> _gameScripts;
	private GameScript _gameScript;
	private string[] _currentScriptLinesSplit;
	private int _scriptLineCounter = 0;

	private List<ScriptProcessResult> _scriptLineResults = new List<ScriptProcessResult>();
	private ScriptProcessResult _scriptLineResult;

	private string[] _scriptParams;
	private string _gameScriptName;
	private List<string> _gameScriptFunctionNames = new();

	// script function properties
	private Dictionary<string, IScriptFunction> _scriptFunctions = new Dictionary<string, IScriptFunction>();

	// holds session variables used by the script
	Dictionary<string, object> _scriptVars = new Dictionary<string, object>();

	public Dictionary<string, object> ScriptVars
	{
		get { return _scriptVars; }
		set { _scriptVars = value; }
	}

	// child script properties
	private ScriptInterpretter _childScript;
	private bool _childScriptKeepEnv = false;

	private bool _processFinished;
	public bool ProcessFinished
	{
		get { return _processFinished; }
		set { _processFinished = value; }
	}

	// used by parent to obtain results
	public string Stdout
	{
		get { 
			return string.Join("\n", _scriptLineResults.Select(x => x.Stdout));
		}
	}
	public string Stderr
	{
		get { 
			return string.Join("\n", _scriptLineResults.Select(x => x.Stderr));
		}
	}
	public int ReturnCode
	{
		get { 
			return _scriptLineResult.ReturnCode;
		}
	}

	public ScriptInterpretter(Dictionary<string, Resource<GameScript>> gameScripts, Dictionary<string, IScriptFunction> scriptFuncs, string[] scriptParams = null)
	{
		_scriptParams = scriptParams;
		_gameScripts = gameScripts;
		_scriptFunctions = scriptFuncs;

		// setup process sub-states
		_statePreparing.OnEnter = _State_Preparing_OnEnter;
		_stateRunning.OnUpdate = _State_Running_OnUpdate;
		_stateWaiting.OnUpdate = _State_Waiting_OnUpdate;
		_stateFinished.OnEnter = _State_Finished_OnEnter;

		_processState.AddState(_statePreparing);	
		_processState.AddState(_stateRunning);	
		_processState.AddState(_stateWaiting);	
		_processState.AddState(_stateFinished);	

		// create state transitions
		_processState.AddTransition(_statePreparing, _stateRunning, STATE_RUNNING);
		_processState.AddTransition(_stateRunning, _stateWaiting, STATE_WAITING);
		_processState.AddTransition(_stateWaiting, _stateRunning, STATE_RUNNING);
		_processState.AddTransition(_stateRunning, _stateFinished, STATE_FINISHED);
		_processState.AddTransition(_stateFinished, _statePreparing, STATE_RUNNING);
        //
	}

	/*******************
	*  Godot methods  *
	*******************/
	
	public override void _PhysicsProcess(double delta)
	{
		_processState.Update();
	}

	/****************************
	*  Script running methods  *
	****************************/
	
	public void RunScript(string scriptName)
	{
		if (_gameScripts.TryGetValue(scriptName, out Resource<GameScript> gs))
		{
			Reset();

			_gameScript = gs.Value;
			_gameScriptName = scriptName;

			// Start the state machine
			if (_processFinished)
			{
				_processFinished = true;
				_processState.Transition(STATE_RUNNING);
			}
			else {
				_processState.Enter();
			}
		}
		else
		{
			throw new InvalidScriptResourceException($"The game script '{scriptName}' is not a valid GameScript resource!");
		}
	}

	public void RunScriptContent(string scriptContent)
	{
		RegisterFunctionFromContent("eval", scriptContent);

		// run the created resource
		RunScript("eval");
	}

	public void RegisterFunctionFromContent(string func, string scriptContent)
	{
		var scriptResource = new Resource<GameScript>();
		scriptResource.Value = new GameScript();
		scriptResource.Value.ScriptContent = scriptContent;

		_gameScripts[func] = scriptResource;

		_gameScriptFunctionNames.Add(func);
	}

	public bool IsValidScriptName(string script)
	{
		return _gameScripts.ContainsKey(script);
	}

	public bool IsValidFunction(string func)
	{
		return (
			IsValidScriptName(func) || // scripts as function name
			_scriptFunctions.ContainsKey(func) || // function registry
			func == "source" // built in method source (TODO: re-implement this as _builtinFunctions property)
			);
	}

	public void Reset()
	{
		_scriptLineResult = null;
		_scriptLineCounter = 0;
		_scriptLineResults.Clear();
		_currentScriptLinesSplit = new string[]{};
	}

	/****************************
	*  State changed callbacks  *
	****************************/

	public void _State_Preparing_OnEnter()
	{
		// look for funcs in the script
		string scriptContentWithoutFunctions = _gameScript.ScriptContent;

		var scriptFunctions = ParseScriptLineFunction(scriptContentWithoutFunctions);
		if (scriptFunctions.Count > 0)
		{
			foreach (var func in scriptFunctions)
			{
				LoggerManager.LogDebug("Function found in line", "", "func", func);
				RegisterFunctionFromContent(func.FuncName, func.ScriptContent);

				// remove function content from original string
				scriptContentWithoutFunctions = scriptContentWithoutFunctions.Replace(func.RawContent, new String('\n', func.LineCount - 1));
			}
		}


		_currentScriptLinesSplit = scriptContentWithoutFunctions.Split(new char[] {'\n', '\r'}, StringSplitOptions.None);
		LoggerManager.LogDebug("Script line count", "", "count", _currentScriptLinesSplit.Count());


		_processState.Transition(STATE_RUNNING);
	}

	public void _State_Running_OnUpdate()
	{
		if (_scriptLineCounter >= _currentScriptLinesSplit.Count())
		{
			_processState.Transition(STATE_FINISHED); // end of the script
			return;
		}

		// retrive the current script line
		string linestr = _currentScriptLinesSplit[_scriptLineCounter].Trim();

		// skip lines
		if (linestr.StartsWith("#") || linestr.Length == 0)
		{
			_scriptLineCounter++;

			_processState.Update();

			return;
		}

		// process the line if it's not empty
		// TODO: figure out why/how to remove empty lines, or just let them
		// happen
		if (linestr.Length > 0)
		{
			_scriptLineResult = InterpretLine(linestr);

			LoggerManager.LogDebug($"[{_gameScriptName}] Line {_scriptLineCounter +1}", "", "line", $"[{_scriptLineResult.ReturnCode}] {_scriptLineResult.Result}");

			// increase script line after processing
			_scriptLineCounter++;


			if (_scriptLineResult.ResultProcessMode == ResultProcessMode.ASYNC)
			{
				// we are waiting for something, so switch processing mode
				_processState.Transition(STATE_WAITING);

				return;
			}
			else
			{
				// add process result to results list
				_scriptLineResults.Add(_scriptLineResult);

				// trigger another update to process the next line
				_processState.Update();

				return;
			}
		}
	}

	public void _State_Waiting_OnUpdate()
	{
		// TODO: implement transition back to running state after we waited for
		// what we want to wait for
		// _processState.Transition(STATE_RUNNING);

		// if we have a child script, then we're waiting for it
		if (_childScript != null)
		{
			// check if it's finished
			if (_childScript.ProcessFinished)
			{
				// forward compiled stdout/stderr and return code
				AssignVariableValue("func"+_childScript.GetHashCode().ToString(), _childScript.Stdout);

				// _scriptLineResult.Stdout = _childScript.Stdout;
				_scriptLineResults.Add(_scriptLineResult);

				LoggerManager.LogDebug("Child script finished", "", "childRes", _scriptLineResult);

				LoggerManager.LogDebug($"[{_gameScriptName}] Line {_scriptLineCounter} (async)", "", "asyncLine", $"[{_scriptLineResult.ReturnCode}] {_scriptLineResult.Result}");

				// clear child script instance since we're done with it
				_childScript.QueueFree();
				_childScript = null;
				_childScriptKeepEnv = false;

				var resultUnparsedVars = ParseVarSubstitutions(_scriptLineResult.Stdout);
				if (resultUnparsedVars.Count > 0)
				{
					LoggerManager.LogDebug("Async line contains unparsed variables", "", "vars", resultUnparsedVars);
					_scriptLineCounter--;
					_currentScriptLinesSplit[_scriptLineCounter] = _scriptLineResult.Stdout;
					LoggerManager.LogDebug("Async line to reprocess", "", "line", _currentScriptLinesSplit[_scriptLineCounter]);
				}

				// resume execution
				_processState.Transition(STATE_RUNNING);
			}
		}
	}

	public void _State_Finished_OnEnter()
	{
		_processFinished = true;
	}

	/*********************************
	*  Execute script line methods  *
	*********************************/

	// main script process execution functions
	public ScriptProcessResult ExecuteFunctionCall(string func, params string[] funcParams)
	{
		if (func == "source")
		{
			LoggerManager.LogDebug("Source called");

			// created a function call as if we are calling this script directly
			func = funcParams[0];
			funcParams = funcParams.Skip(1).ToArray();

			_childScriptKeepEnv = true;

			return ExecuteFunctionCall(func, funcParams);
		}
		else if (_scriptFunctions.ContainsKey(func))
		{
			ScriptProcessResult res;

			try
			{
				res = _scriptFunctions[func].Call(this, funcParams);
			}
			catch (System.Exception e)
			{
				res = new ScriptProcessResult(127, "", e.Message);
			}

			return res;
		}

		// check if the function name is a valid script
		else if (IsValidScriptName(func))
		{
			LoggerManager.LogDebug("Executing script as function", "", "script", func);

			// create a child script interpreter instance to run the script
			_childScript = new ScriptInterpretter(_gameScripts, _scriptFunctions, scriptParams: funcParams);
			AddChild(_childScript);

			// set child vars to match ours
			if (_childScriptKeepEnv || _gameScriptFunctionNames.Contains(func))
			{
				_childScript._scriptVars = _scriptVars;
			}

			_childScript.RunScript(func);

			// return a wait mode process
			return new ScriptProcessResult(0, "$func"+_childScript.GetHashCode(), resultProcessMode: ResultProcessMode.ASYNC);
		}

		return new ScriptProcessResult(127, "", $"command not found: {func}");
	}

	public ScriptProcessResult ExecuteVariableAssignment(string varName, string varValue)
	{
		// parse variable name and keys
		string varnamePattern = @"\[([^\\]]*)\]";
		MatchCollection matches = Regex.Matches(varName, varnamePattern, RegexOptions.Multiline);

		// TODO: implement nested variable access by parsing key names
		if (matches.Count > 0)
		{
			LoggerManager.LogDebug("TODO: implement nested variable access", "", "varname", varName);

			return new ScriptProcessResult(127, "", "variable key access not implemented");
		}
		else
		{
			AssignVariableValue(varName, varValue);
			return new ScriptProcessResult(0);
		}
	}

	public void AssignVariableValue(string varName, string varValue)
	{
		LoggerManager.LogDebug("Setting variable value", "", "var", $"{varName} = {varValue}");

		_scriptVars[varName] = varValue;
	}

	public ScriptProcessResult ExecuteVariableSubstitution(string varName, ScriptProcessResult res)
	{
		return new ScriptProcessResult(0, res.Result.Replace("$"+varName, GetVariableValue(varName).ToString()));
	}

	public string GetVariableValue(string varName)
	{
		string varValue = "";

		// check if we have an assigned var
		if (_scriptVars.TryGetValue(varName, out object obj))
		{
			varValue = (string) obj;
		}

		// check if it's a special var
		else
		{
			// positional arguments
			if (int.TryParse(varName, out int i))
			{
				if (i == 0)
				{
					varValue = _gameScriptName;
				}
				else {
					if (_scriptParams.Length >= i)
					{
						varValue = _scriptParams[i-1];
					}
				}
			}
			// arguments as string
			else if (varName == "*")
			{
				varValue = _scriptParams.Join(" ");
			}
			// arguments count
			else if (varName == "#")
			{
				varValue = _scriptParams.Count().ToString();
			}
			// last return code
			else if (varName == "?" && _scriptLineResult != null)
			{
				varValue = _scriptLineResult.ReturnCode.ToString();
			}
		}

		return varValue;
	}

	/************************************
	*  Line interpertretation methods  *
	************************************/

	// accepts a pure string containing the script content to process for
	// interpretation
	public List<ScriptProcessResult> InterpretLines(string scriptLines)
	{
		List<ScriptProcessResult> processes = new List<ScriptProcessResult>();

		_currentScriptLinesSplit = scriptLines.Split(new string[] {"\\n"}, StringSplitOptions.None);

		while (_scriptLineCounter < _currentScriptLinesSplit.Count())
		{
			string linestr = _currentScriptLinesSplit[_scriptLineCounter].Trim();

			if (linestr.Length > 0)
			{
				processes.Add(InterpretLine(linestr));
			}

			_scriptLineCounter++;
		}

		return processes;
	}

	// accepts a single script line and generates a list of process objects to
	// achieve the final rendered result for each line
	public ScriptProcessResult InterpretLine(string line)
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

		ScriptProcessResult lineResult = new ScriptProcessResult(0, line);

		// list of process operations to do to this script line
		List<ScriptProcessOperation> processes = new List<ScriptProcessOperation>();

		// first thing, parse and replace variable names with values
		foreach (ScriptProcessVarSubstitution lineProcess in ParseVarSubstitutions(lineResult.Result))
		{
			lineResult = ExecuteVariableSubstitution(lineProcess.Name, lineResult);
		}
		// processes.AddRange(ParseVarSubstitutions(line));

		// second thing, parse and replace expressions with values
		foreach (ScriptProcessExpression lineProcess in ParseExpressions(lineResult.Result))
		{
			// TODO: implement expression processing
			// lineResult = ExecuteExpression(lineProcess.Expression, lineResult);
			LoggerManager.LogDebug("Expression found", "", "exp", lineProcess);
		}
		// processes.AddRange(ParseExpressions(line));

		// third thing, parse nested script lines and replace values
		lineResult = ParseNestedLines(lineResult.Result);
		if (lineResult.ReturnCode != 0 || lineResult.ResultProcessMode == ResultProcessMode.ASYNC)
		{
			LoggerManager.LogDebug("Nested line process failed or async");
			return lineResult;
		}
		// foreach (ScriptProcessNestedProcess lineProcess in ParseNestdLines(lineResult.Stdout))
		// {
		// 	// TODO: process nested lines
		// 	// lineResult = ExecuteVariableSubstitution(lineProcess.Name, lineResult);
		// }
		// processes.AddRange(ParseNestdLines(line));


		// parse variable assignments
		var varAssignmentProcesses = ParseVarAssignments(lineResult.Result);
		// processes.AddRange(varAssignmentProcesses);
		foreach (ScriptProcessVarAssignment lineProcess in varAssignmentProcesses)
		{
			lineResult = ExecuteVariableAssignment(lineProcess.Name, lineProcess.Value);
		}
		if (lineResult.ReturnCode != 0)
		{
			return lineResult;
		}

		// process if statement
		var ifStatementParsed = ParseBlockStatementOpening(lineResult.Result);
		if (ifStatementParsed != null)
		{

			LoggerManager.LogDebug("Found block statement", "", "if", ifStatementParsed.Stdout);

			if (ifStatementParsed.ReturnCode == 0)
			{
				LoggerManager.LogDebug("Enter block statement!", "", "line", lineResult.Stdout);
			}
			else
			{
				LoggerManager.LogDebug("Skip block statement", "", "line", lineResult.Stdout);
			}

			var parsedBlockLines = ParseBlockStatementLines(ifStatementParsed.ReturnCode);

			LoggerManager.LogDebug("Parsed block lines", "", "lines", parsedBlockLines);
			if (parsedBlockLines.Count > 0)
			{
				string tempFuncName = $"{_scriptLineCounter}_{GetHashCode()}";
				RegisterFunctionFromContent(tempFuncName, parsedBlockLines.ToArray().Join("\n"));
				
				// set line content to the temp function
				lineResult.Stdout = tempFuncName;
			}
		}

		// var blockProcess = ParseBlockProcessLine(line, _currentScriptLinesSplit);
		// if (blockProcess != null)
		// {
		// 	// processes.AddRange(new List<ScriptProcessOperation>() {blockProcess});
		// 	LoggerManager.LogDebug("Evaluating block procress line", "", "line", blockProcess);
		// }
		// else
		// {
			// if var assignments are 0, then try to match function calls
			// NOTE: this is because the regex matches both var assignments in lower
			// case AND function calls
			if (varAssignmentProcesses.Count == 0)
			{
				foreach (ScriptProcessFunctionCall lineProcess in ParseFunctionCalls(lineResult.Result))
				{
					lineResult = ExecuteFunctionCall(lineProcess.Function, lineProcess.Params.ToArray());
				}
				// processes.AddRange(ParseFunctionCalls(line));
			}
		// }

		// if there's no processes until now, just return the plain object with
		// no processing attached
		// if (processes.Count == 0)
		// {
		// 	processes.Add(new ScriptProcessOperation(line));
		// }
		
		// LoggerManager.LogDebug("Line result", "", "res", lineResult);

		return lineResult;
	}


	/**************************************
	*  Parse if/while/for block methods  *
	**************************************/

	public List<string> ParseBlockStatementLines(int currentLineReturnCode = 0)
	{
		List<string> parsedLines = new();

		// look ahead to extract the lines inside the if block, and the lines
		// inside any else block, until we reach a fi/done
		
		int blockProcessState = -1;

		int tempLineCounter = _scriptLineCounter + 1;
		while (tempLineCounter < _currentScriptLinesSplit.Count())
		{
			string nextLine = _currentScriptLinesSplit[tempLineCounter].Trim();

			// look for then/do, and continue
			if (blockProcessState == -1)
			{
				if (nextLine == "then" || nextLine == "do")
				{
					blockProcessState++;
				}
			}

			// look for lines or end
			// 1 = if content
			else if (blockProcessState >= 0)
			{
				// if its an end, stop the loop
				if (nextLine == "fi" || nextLine == "done")
				{
					_scriptLineCounter = tempLineCounter;
					break;
				}

				if (nextLine == "else")
				{
					blockProcessState++;
					tempLineCounter++;
					continue;
				}

				if (blockProcessState == currentLineReturnCode)
				{
					parsedLines.Add(nextLine);
				}
			}

			tempLineCounter++;
		}

		return parsedLines;
	}

	public ScriptProcessResult ParseBlockStatementOpening(string line)
	{
		string patternBlockStatement = @"^(if|while|for)\[?(.+)*\]*";
		Match isBlockStatement = Regex.Match(line, patternBlockStatement, RegexOptions.Multiline);

		if (isBlockStatement.Groups.Count >= 3)
		{
			string statementType = isBlockStatement.Groups[1].Value;
			string statementCondition = isBlockStatement.Groups[2].Value.Trim();

			var conditions = ParseProcessBlockConditions(statementCondition);

			LoggerManager.LogDebug($"Parsed {statementType} start", "", "conditions", conditions);

			int conditionTrueCount = 0;

			int andOr = -1;
			for (int i = 0; i < conditions.Count; i++)
			{
				var condition = conditions[i];
				ScriptProcessOperation functionParse = condition.Item1[0];

				// set andOr type to AND or OR mode
				if (andOr == -1 && (condition.Item2 == "||"))
				{
					// LoggerManager.LogDebug($"Condition {i} compare type", "", "compareType", "OR");
					andOr = 1;
				}
				else if (andOr == -1 && (condition.Item2 == ""))
				{
					// LoggerManager.LogDebug($"Condition {i} compare type", "", "compareType", "AND");
					andOr = 0;
				}

				if (functionParse is ScriptProcessFunctionCall fc)
				{
					List<string> conditionParams = fc.Params;
					string conditionType = fc.Function;

					// LoggerManager.LogDebug($"Condition {i} {conditionType}", "", "condition", conditionParams);

					if (conditionType == "expr")
					{
						var conditionParseRes = (dynamic) ParseBlockStatementCondition(conditionParams[0]);
						LoggerManager.LogDebug($"Condition {i} expr result", "", "res", conditionParseRes);

						if (conditionParseRes is bool rb && rb == true)
						{
							conditionTrueCount++;
						}
					}
				}
			}

			bool conditionTrue = false;
			if ((conditionTrueCount == conditions.Count && andOr == 0))
			{
				conditionTrue = true;
			}
			else if ((conditionTrueCount >= 1 && andOr == 1))
			{
				conditionTrue = true;
			}

			return new ScriptProcessResult((conditionTrue) ? 0 : 1, line);
		}

		return null;
	}

	public object ParseBlockStatementCondition(string condition)
	{
		System.Data.DataTable table = new System.Data.DataTable();
		return table.Compute(condition.ToString(), null);
	}

	// parse a line starting with if/while/for as a block of script to be
	// treated up the stack as a single process object
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
				string forwardScriptLineRaw = scriptLines[_scriptLineCounter];
				string forwardScriptLine = forwardScriptLineRaw.Trim();

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

				fullScriptLine += forwardScriptLineRaw+"\n";

				// if we have conditional matches, it's an elif or a nested if
				if (forwardLineConditions.Count > 0)
				{
					LoggerManager.LogDebug("Block conditions found in line", "", "line", forwardScriptLine);

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
					// currentBlockProcesses.Add(new List<ScriptProcessOperation> {new ScriptProcessOperation(InterpretLine(forwardScriptLine).Result)});
					currentBlockProcesses.Add(new List<ScriptProcessOperation> {new ScriptProcessOperation(forwardScriptLine)});
				}


			}

		}

		return null;
	}

	// return the processed conditions from an if/while/for block
	public List<(List<ScriptProcessOperation>, string)> ParseProcessBlockConditions(string scriptLine)
	{
		string patternBlockProcessCondition = @"\[(.*?)\] ?(\|?\|?)";
		string patternBlockProcessConditionExpression = @"\(\((.*?)\)\) ?(\|?\|?)";

		MatchCollection blockProcessConditionMatches = Regex.Matches(scriptLine, patternBlockProcessCondition, RegexOptions.Multiline);
		MatchCollection blockProcessConditionMatchesExpressions = Regex.Matches(scriptLine, patternBlockProcessConditionExpression, RegexOptions.Multiline);

		List<(List<ScriptProcessOperation>, string)> conditionsList = new List<(List<ScriptProcessOperation>, string)>();

		// if (scriptLine.StartsWith("for "))
		// {
		// 	conditionsList.Add((new List<ScriptProcessOperation> {new ScriptProcessOperation(InterpretLine(scriptLine.Replace("for ", "")).Stdout)}, ""));
		// }

		// process condition brackets
		foreach (Match match in blockProcessConditionMatches)
		{
			string blockConditionInside = match.Groups[1].Value;
			string blockConditionCompareType = match.Groups[2].Value;

			// var interpretted = InterpretLine(blockConditionInside.Trim());
			conditionsList.Add((ParseFunctionCalls("condition " + InterpretLine(blockConditionInside.Trim()).Stdout, verifyFunctionName: false), blockConditionCompareType.Trim()));
		}

		// process expression matches
		foreach (Match match in blockProcessConditionMatchesExpressions)
		{
			string blockConditionInside = match.Groups[1].Value;
			string blockConditionCompareType = match.Groups[2].Value;

			// var interpretted = InterpretLine(blockConditionInside.Trim());
			conditionsList.Add((ParseFunctionCalls("expr \"" + InterpretLine(blockConditionInside.Trim()).Stdout+ "\"", verifyFunctionName: false), blockConditionCompareType.Trim()));
		}

		return conditionsList;
	}


	/********************************
	*  Parse script lines methods  *
	********************************/

	// parse function name and content in script line
	public List<(string FuncName, string ScriptContent, int LineCount, string RawContent)> ParseScriptLineFunction(string lines)
	{
		string patternScriptFunction = @"^([a-zA-Z0-9_]+)\(\) \{\n(^[^{}\r]+$)*\n\}";
		MatchCollection matches = Regex.Matches(lines, patternScriptFunction, RegexOptions.Multiline);

		List<(string FuncName, string ScriptContent, int LineCount, string RawContent)> funcs = new();

		foreach (Match match in matches)
		{
			Match m = match;

			if (match.Groups.Count >= 2)
			{
				funcs.Add((match.Groups[1].Value.Trim(), match.Groups[2].Value, 2 + match.Groups[2].Value.Split('\n').Length, match.Groups[0].Value));
			}

			m = m.NextMatch();
		}

		return funcs;
	}

	// return script processed lines from nested $(...) lines in a script line
	public ScriptProcessResult ParseNestedLines(string line)
	{
		List<(string, ScriptProcessResult)> processes = new List<(string, ScriptProcessResult)>();

		// string patternNestedLine = @"((?<=\$\()[^""\n]*(?=\)))";
		// string patternNestedLine = @"((?<=\$\()[^""\n](?=\)))|((?<=\$\()[^""\n]*(?=\)))";
		// string patternNestedLine = @"((?<=\$\()[^""\n](?=\)))|((?<=\$\()[^""\n\)\(]*(?=\)))";
		string patternNestedLine = @"((?<=\$\()[^\n](?=\)))|((?<=\$\()[^\n\)\(]*(?=\)))";

		ScriptProcessResult lineResult = new ScriptProcessResult(0, line);

		MatchCollection nl = Regex.Matches(line, patternNestedLine, RegexOptions.Multiline);
		foreach (Match match in nl)
		{
			if (match.Groups.Count >= 1)
			{
				string nestedLine = match.Groups[0].Value;

				List<string> nestedLines = new List<string>();

				// LoggerManager.LogDebug("Nested line matche", "", "nestedLine", $"{nestedLine}");

				ScriptProcessResult lineResultInner = InterpretLine(nestedLine);
				processes.Add((nestedLine, lineResultInner));

				// stop processing them so we can process one async call at once
				if (lineResultInner.ResultProcessMode == ResultProcessMode.ASYNC)
				{
					break;
				}
			}
		}

		if (processes.Count > 0)
		{
			foreach ((string, ScriptProcessResult) nestedRes in processes)
			{
				lineResult.Stdout = lineResult.Result.Replace($"$({nestedRes.Item1})", nestedRes.Item2.Result);
				lineResult.Stderr = nestedRes.Item2.Stderr;
				lineResult.ReturnCode = nestedRes.Item2.ReturnCode;
				lineResult.ResultProcessMode = nestedRes.Item2.ResultProcessMode;

				if (lineResult.ReturnCode != 0 || lineResult.ResultProcessMode == ResultProcessMode.ASYNC)
				{
					break;
				}
			}
			
			LoggerManager.LogDebug("Nested lines result", "", "res", lineResult);
		}

		return lineResult;
	}

	// parse list of required variable substitutions in a script line
	public List<ScriptProcessOperation> ParseVarSubstitutions(string line)
	{
		List<ScriptProcessOperation> processes = new List<ScriptProcessOperation>();

		string patternVarSubstitution = @"\$([a-zA-Z0-9_\[\]'#?@*]+)";
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

	// parse expressions in a script line
	public List<ScriptProcessOperation> ParseExpressions(string line)
	{
		List<ScriptProcessOperation> processes = new List<ScriptProcessOperation>();

		string patternExpression = @"^\(\((.+)\)\)";
		MatchCollection expressionMatches = Regex.Matches(line, patternExpression);

		foreach (Match match in expressionMatches)
		{
			processes.Add(new ScriptProcessExpression(line, match.Groups[1].Value));
		}

		return processes;
	}

	// parse variable assignments with names and values in a script line
	public List<ScriptProcessOperation> ParseVarAssignments(string line)
	{
		List<ScriptProcessOperation> processes = new List<ScriptProcessOperation>();

		line = line.Replace("\n", "##NL##");

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

					varValue = varValue.Replace("##NL##", "\n");

					processes.Add(new ScriptProcessVarAssignment(line, varName, varValue));
					// LoggerManager.LogDebug("Variable assignment match", "", "assignment", execLine);
				}
			}
		}

		return processes;
	}

	// parse function calls with params in a script line
	public List<ScriptProcessOperation> ParseFunctionCalls(string line, bool verifyFunctionName = true)
	{
		List<ScriptProcessOperation> processes = new List<ScriptProcessOperation>();

		// string patternFuncCall = @"(^\b[a-z_]+) (""[^""]+"")$"; // matches function calls with single param in quotes but not without
		// string patternFuncCall = @"(^\b[a-z_]+) ((""[^""]+"")$|(\w.+))"; // matches function calls with single param in quotes, and multiple param without quotes as single string (can be split?)
		// string patternFuncCall = @"(^\b[a-z_]+) ((""[^""]+"")$|(\w.+)|(([\w.]+)|.+))"; // matches single param
		string patternFuncCall = @"(^\b[a-z0-9_]+) *((""[^""]+"")*$|(\w.+)|(([\w.]+)|.+))"; // matches single param

		MatchCollection fm = Regex.Matches(line, patternFuncCall, RegexOptions.Multiline);
		foreach (Match match in fm)
		{
			if (match.Groups.Count >= 3)
			{
				string funcName = match.Groups[1].Value;
				string funcParamsStr = match.Groups[2].Value;

				List<string> funcParams = new List<string>();

				// foreach (Match fmatches in Regex.Matches(funcParamsStr, @"(?<="")[^""\n]*(?="")|[\w]+"))
				// foreach (Match fmatches in Regex.Matches(funcParamsStr, @"((?<="")[^""\n]*(?=""))|([\S]+)"))
				foreach (Match fmatches in Regex.Matches(funcParamsStr, @"((?<="")[^""\n]*(?=""))|([\w\d!£\$%\^&*\(\)-=+_'><?/\\;,.\n]+)"))
				{
					Match nm = fmatches;

					if (nm.Groups[0].Value != " ")
					{
						funcParams.Add(nm.Groups[0].Value);
					}

					nm = nm.NextMatch();
				}

				if (IsValidFunction(funcName) || verifyFunctionName == false)
				{
					processes.Add(new ScriptProcessFunctionCall(line, funcName, funcParams));
					// LoggerManager.LogDebug("Function call match", "", "call", $"func name: {funcName}, params: [{string.Join("|", funcParams.ToArray())}]");
				}

			}
		}

		return processes;
	}

	/****************
	*  Exceptions  *
	****************/
	
	public class InvalidScriptResourceException : Exception
	{
		public InvalidScriptResourceException() {}
		public InvalidScriptResourceException(string message) : base(message) {}
		public InvalidScriptResourceException(string message, Exception inner) : base(message, inner) {}
	}
}

/**************************************
*  Script process operation classes  *
**************************************/

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


/**********************************
*  Script process result classes  *
**********************************/

public enum ResultProcessMode
{
	NORMAL,
	ASYNC
}

public class ScriptProcessResult
{
	private int _returnCode;
	public int ReturnCode
	{
		get { return _returnCode; }
		set { _returnCode = value; }
	}

	private string _stdout;
	public string Stdout
	{
		get { return _stdout; }
		set { _stdout = value; }
	}

	private string _stderr;
	public string Stderr
	{
		get { return _stderr; }
		set { _stderr = value; }
	}

	public string Result
	{
		get { return GetResult(); }
	}

	private object _rawResult;
	public object RawResult
	{
		get { return _rawResult; }
		set { _rawResult = value; }
	}

	private ResultProcessMode _resultProcessMode;
	public ResultProcessMode ResultProcessMode
	{
		get { return _resultProcessMode; }
		set { _resultProcessMode = value; }
	}

	public ScriptProcessResult(int returnCode, string stdout = "", string stderr = "", object rawResult = null, ResultProcessMode resultProcessMode = ResultProcessMode.NORMAL)
	{
		_returnCode = returnCode;
		_stdout = stdout;
		_stderr = stderr;
		_resultProcessMode = resultProcessMode;
	}

	public string GetResult()
	{
		if (_returnCode == 0)
		{
			return _stdout;
		}
		else
		{
			return $"err {_returnCode}: {_stderr}";
		}
	}
}
