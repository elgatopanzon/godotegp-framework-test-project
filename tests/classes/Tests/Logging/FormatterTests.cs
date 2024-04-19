/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : FormatterTests
 * @created     : Thursday Apr 18, 2024 17:05:26 CST
 */

namespace GodotEGP.Tests.Logging;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using GodotEGP.Logging.Formatters;

public partial class FormatterTests : TestContext
{
	[Fact]
	public void FormatterTests_formatter_TextFormatter()
	{
		var formatter = new TestTextFormatter();

		var formatted = (string) formatter.Format(new Message(level:Message.LogLevel.Debug, text:"log text") {

			});

		LoggerManager.LogDebug("Formatted log message", "", "message", formatted);

		Assert.Contains("[log text]", formatted);
	}
}

public partial class TestTextFormatter : TextFormatter
{
	public TestTextFormatter()
	{
		_propertyFormatterFuncs = new();

		RegisterPropertyFormatter("Text", (s, lm) => $"[{s}]");
	}
}
