/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ConfigLoaderTests
 * @created     : Thursday Apr 18, 2024 11:39:04 CST
 */

namespace GodotEGP.Tests.Config;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Objects.Validated;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

public partial class ConfigLoaderTests : TestContext
{
	[Fact]
	public async void ConfigLoaderTests_loading_from_queue()
	{
		var filePath1 = $"{nameof(ConfigLoaderTests_loading_from_queue)}1.json";
		var filePath2 = $"{nameof(ConfigLoaderTests_loading_from_queue)}2.json";

		File.WriteAllText(filePath1, @"{""TestProp"": 123}");
		File.WriteAllText(filePath2, @"{""TestProp"": 456}");

		// create the config loader file queue
		Queue<Dictionary<string, object>> fileQueue = new Queue<Dictionary<string, object>>();

		// create taskcompletionsource to get loader results
		var tcs = new TaskCompletionSource<List<ConfigObject>>();

		fileQueue.Enqueue(new Dictionary<string, object> {{"configType", "GodotEGP.Tests.Config.ConfigLoaderTest" }, {"path", filePath1}});
		fileQueue.Enqueue(new Dictionary<string, object> {{"configType", "GodotEGP.Tests.Config.ConfigLoaderTest" }, {"path", filePath2}});
		
		// init the config loader object
		ConfigLoader configLoader = new ConfigLoader(fileQueue);

		configLoader.SubscribeOwner<ConfigManagerLoaderCompleted>((e) => {
			tcs.SetResult(e.ConfigObjects);
		}, oneshot: true, isHighPriority: true);

		configLoader.SubscribeOwner<ConfigManagerLoaderError>((e) => {
			tcs.SetException(e.RunWorkerCompletedEventArgs.Error);
		}, oneshot: true, isHighPriority: true);

		await tcs.Task;

		if (File.Exists(filePath1))
		{
			File.Delete(filePath1);
		}
		if (File.Exists(filePath2))
		{
			File.Delete(filePath2);
		}

		for (int i = 0; i < tcs.Task.Result.Count; i++)
		{
			var casted = (ConfigObject<ConfigLoaderTest>) tcs.Task.Result[i];

			if (i == 0)
			{
				Assert.Equal(123, casted.Value.TestProp);
			}
			if (i == 1)
			{
				Assert.Equal(456, casted.Value.TestProp);
			}
		}
	}
}

public partial class ConfigLoaderTest : VConfig
{
	internal readonly VValue<int> _testProp;

	public int TestProp
	{
		get { return _testProp.Value; }
		set { _testProp.Value = value; }
	}
	
	public ConfigLoaderTest()
	{
		_testProp = AddValidatedValue<int>(this)
	    .Default(1)
	    .ChangeEventsEnabled();
	}
}
