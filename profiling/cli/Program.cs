namespace GodotEGP.Profiling.CLI;

using System;

using GodotEGPNonGame.ServiceWorkers;
using GodotEGP.Logging;

using GodotEGP;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Random;
using GodotEGP.Objects.Extensions;
using GodotEGP.Event.Events;
using GodotEGP.Event.Filters;
using Godot;

using GodotEGP.Profiling.CLI.ECSv3;
using GodotEGP.Profiling.CLI.ECSv4;

internal class Program
{
	public static GodotEGP.Main GodotEGP;

    static async Task<int> Main(string[] args)
    {
		// init GodotEGP
		GodotEGP = new GodotEGP.Main();
		SceneTree.Instance.Root.AddChild(GodotEGP);

		// wait for services to be ready
		if (!ServiceRegistry.WaitForServices(
					typeof(EventManager) 
					))
			{
			LoggerManager.LogCritical("Required services never became ready");

			return 0;
		}

		LoggerManager.LogInfo("Services ready");

		// create SceneTree service worker instance
		var serviceWorker = new SceneTreeServiceWorker();
		await serviceWorker.StartAsync(new CancellationToken());

		LoggerManager.LogInfo("GodotEGP ready!");

    	if (args.Length <= 1)
    	{
    		throw new ArgumentException("Must include a valid profiler test!");
    	}

    	string profileName = args[0];
    	string profileSubName = args[1];

    	ProfilingContext profile = null;

    	// run different profiles based on given argument
    	switch (profileName)
    	{
    		case "ECSv3":
    			switch (profileSubName)
    			{
					case "update_1":
						profile = new ECSv3Profile_Update_6(1) {
						};
						break;
					case "update_2":
						profile = new ECSv3Profile_Update_6(2) {
						};
						break;
					case "update_8":
						profile = new ECSv3Profile_Update_6(8) {
						};
						break;
					case "update_16":
						profile = new ECSv3Profile_Update_6(16) {
						};
						break;
					case "update_32":
						profile = new ECSv3Profile_Update_6(32) {
						};
						break;
					case "update_64":
						profile = new ECSv3Profile_Update_6(64) {
						};
						break;
					case "update_128":
						profile = new ECSv3Profile_Update_6(128) {
						};
						break;
					case "update_256":
						profile = new ECSv3Profile_Update_6(256) {
						};
						break;
					case "update_1k":
						profile = new ECSv3Profile_Update_6(1000) {
						};
						break;
					case "update_4k":
						profile = new ECSv3Profile_Update_6(4000) {
						};
						break;
					case "update_16k":
						profile = new ECSv3Profile_Update_6(16000) {
						};
						break;
					case "update_65k":
						profile = new ECSv3Profile_Update_6(65000) {
						};
						break;
					case "update_262k":
						profile = new ECSv3Profile_Update_6(262000) {
						};
						break;
					case "update_1m":
						profile = new ECSv3Profile_Update_6(1000000) {
						};
						break;
					case "update_2m":
						profile = new ECSv3Profile_Update_6(2000000) {
						};
						break;
    				default:
    					break;
    			}
    			break;
    		case "ECSv4":
    			switch (profileSubName)
    			{
					case "update_1":
						profile = new ECSv4Profile_Update_6(1) {
						};
						break;
					case "update_2":
						profile = new ECSv4Profile_Update_6(2) {
						};
						break;
					case "update_8":
						profile = new ECSv4Profile_Update_6(8) {
						};
						break;
					case "update_16":
						profile = new ECSv4Profile_Update_6(16) {
						};
						break;
					case "update_32":
						profile = new ECSv4Profile_Update_6(32) {
						};
						break;
					case "update_64":
						profile = new ECSv4Profile_Update_6(64) {
						};
						break;
					case "update_128":
						profile = new ECSv4Profile_Update_6(128) {
						};
						break;
					case "update_256":
						profile = new ECSv4Profile_Update_6(256) {
						};
						break;
					case "update_1k":
						profile = new ECSv4Profile_Update_6(1000) {
						};
						break;
					case "update_4k":
						profile = new ECSv4Profile_Update_6(4000) {
						};
						break;
					case "update_16k":
						profile = new ECSv4Profile_Update_6(16000) {
						};
						break;
					case "update_65k":
						profile = new ECSv4Profile_Update_6(65000) {
						};
						break;
					case "update_262k":
						profile = new ECSv4Profile_Update_6(262000) {
						};
						break;
					case "update_1m":
						profile = new ECSv4Profile_Update_6(1000000) {
						};
						break;
					case "update_2m":
						profile = new ECSv4Profile_Update_6(2000000) {
						};
						break;
    				default:
    					break;
    			}
    			break;
    		default:
    			break;
    	}

    	throw new Exception("Not a valid profile!");

		return 0;
    }
}
