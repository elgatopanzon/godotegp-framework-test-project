/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : TestContext
 * @created     : Thursday Apr 04, 2024 01:40:46 CST
 */

namespace GodotEGP.Test;

using GodotEGPNonGame.ServiceWorkers;

using Godot;
using GodotEGP;
using GodotEGP.Objects.Extensions;
using GodotEGP.Objects.Validated;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

public partial class TestContext
{
	public GodotEGP.Main GodotEGP { get; set; }
	public TestContext()
	{
		// init GodotEGP
		ServiceRegistry.UseLazyInstance = true;

		GodotEGP = new GodotEGP.Main();
		SceneTree.Instance.Root.AddChild(GodotEGP);

		// wait for services to be ready
		if (!ServiceRegistry.WaitForServices(
					typeof(EventManager)
					))
			{
			LoggerManager.LogCritical("Required services never became ready");

			return;
		}

		LoggerManager.LogInfo("Services ready");

		// create SceneTree service worker instance
		var serviceWorker = new SceneTreeServiceWorker();
		serviceWorker.StartAsync(new CancellationToken());

		LoggerManager.LogInfo("GodotEGP ready!");
		
	}
}

