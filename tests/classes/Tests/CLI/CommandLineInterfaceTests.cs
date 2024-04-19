/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : CommandLineInterfaceTests
 * @created     : Thursday Apr 18, 2024 08:15:50 CST
 */

namespace GodotEGP.Tests.CLI;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using GodotEGP.CLI;

public partial class CommandLineInterfaceTests : TestContext
{
	[Fact]
	public void CommandLineInterface_parses_default_command()
	{
		var cli = new CommandLineInterfaceTestInterface();

		// set the default command
		cli.SetDefaultCommand("default-cmd");

		// test that the command is parsed to the default without passing any
		// args
		Assert.Equal("default-cmd", cli.GetParsedCommand());
	}

	[Fact]
	public void CommandLineInterface_parses_commands_from_args()
	{
		var cli = new CommandLineInterfaceTestInterface();

		// set the args
		cli.SetArgs(new string[] { "test-cmd-no-params" });

		// test that the command is parsed
		Assert.Equal("test-cmd-no-params", cli.GetParsedCommand());
	}

	[Fact]
	public void CommandLineInterface_parses_positional_args()
	{
		var cli = new CommandLineInterfaceTestInterface();

		// set the args
		cli.SetArgs(new string[] { "test-cmd-positional-args", "one", "two", "3" });

		// test that the command parses the args
		Assert.Equivalent(cli.GetPositionalValues(), new List<string> { "one", "two", "3" });
	}

	[Fact]
	public void CommandLineInterface_parses_switches()
	{
		var cli = new CommandLineInterfaceTestInterface();

		// set the args
		cli.SetArgs(new string[] { "test-cmd-switches", "--switch-test" });

		// test that the command parses the args
		Assert.Equivalent(cli.ArgExists("--switch-test"), true);
	}

	[Fact]
	public void CommandLineInterface_parses_switches_with_param()
	{
		var cli = new CommandLineInterfaceTestInterface();

		// set the args
		cli.SetArgs(new string[] { "test-cmd-switches", "--switch-test-param", "param" });

		// test that the command parses the args
		Assert.Equivalent(cli.GetArgumentValue("--switch-test-param"), "param");
	}

	[Fact]
	public void CommandLineInterface_parses_switches_with_multiple_params()
	{
		var cli = new CommandLineInterfaceTestInterface();

		// set the args
		cli.SetArgs(new string[] { "test-cmd-switches", "--switch-test-params", "param1", "param2" });

		// test that the command parses the args
		Assert.Equivalent(cli.GetArgumentValues("--switch-test-params"), new List<string>() { "param1", "param2" });
	}

	[Fact]
	public void CommandLineInterface_parses_options()
	{
		var cli = new CommandLineInterfaceTestInterface();

		// set the args
		cli.SetArgs(new string[] { "test-cmd-options", "-t" });

		// test that the command parses the args
		Assert.Equivalent(cli.ArgExists("-t"), true);
	}

	[Fact]
	public void CommandLineInterface_parses_options_with_param()
	{
		var cli = new CommandLineInterfaceTestInterface();

		// set the args
		cli.SetArgs(new string[] { "test-cmd-options", "-t", "param" });

		// test that the command parses the args
		Assert.Equivalent(cli.GetArgumentValue("-t"), "param");
	}

	[Fact]
	public void CommandLineInterface_parses_options_with_multiple_params()
	{
		var cli = new CommandLineInterfaceTestInterface();

		// set the args
		cli.SetArgs(new string[] { "test-cmd-options", "-t", "param1", "param2" });

		// test that the command parses the args
		Assert.Equivalent(cli.GetArgumentValues("-t"), new List<string>() { "param1", "param2" });
	}
}

public partial class CommandLineInterfaceTestInterface : CommandLineInterface
{
	public CommandLineInterfaceTestInterface(string[] args = null) : base(args)
	{
		AddCommandDefinition("default-cmd", async () => {
			return 0;
			});
		AddCommandDefinition("test-cmd-no-params", async () => {
			return 123;
			});

		AddCommandDefinition("test-cmd-positional-args", TestCommandPositionalArgs);
		AddCommandDefinition("test-cmd-switches", TestCommandSwitches);
		AddCommandDefinition("test-cmd-options", TestCommandOptions);
	}

	public async Task<int> TestCommandPositionalArgs()
	{
		return 0;
	}
	public async Task<int> TestCommandSwitches()
	{
		return 0;
	}
	public async Task<int> TestCommandOptions()
	{
		return 0;
	}
}
