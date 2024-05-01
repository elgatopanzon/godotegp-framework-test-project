/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : CharBufferTests
 * @created     : Wednesday May 01, 2024 14:30:11 CST
 */

namespace GodotEGP.Tests.Collections;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using GodotEGP.Collections;

public partial class CharBufferTests : TestContext
{
	[Fact]
	public void CharBufferTests_Test()
	{
		CharBuffer<Buffer16<char>> buffer = new();

		LoggerManager.LogDebug("Buffer", "", "buffer", buffer);

		// verify empty buffer is empty string
		Assert.Equal("", buffer);

		// set directly to a string value
		buffer = "test string";

		LoggerManager.LogDebug("Buffer", "", "buffer", buffer);

		Assert.Equal("test string", buffer);

		// set to a longer value, it should be capped
		buffer = "test string which is much longer";

		LoggerManager.LogDebug("Buffer", "", "buffer", buffer);

		Assert.Equal("test string whic", buffer);
	}
}

