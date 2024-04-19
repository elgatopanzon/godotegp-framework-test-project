/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ConfigObjectTests
 * @created     : Thursday Apr 18, 2024 10:10:12 CST
 */

namespace GodotEGP.Tests.Config;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Objects.Validated;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using GodotEGP.DAL.Endpoints;

public partial class ConfigObjectTests : TestContext
{
	[Fact]
	public async void ConfigObjectTests_object_creation()
	{
		var obj = ConfigObject.Create("GodotEGP.Tests.Config.TestConfigObject");
	}

	[Fact]
	public async void ConfigObjectTests_object_creation_exception()
	{
		// throws an exception when provided with an invalid type
		Assert.Throws<ArgumentNullException>(() => {
				var obj = ConfigObject.Create("GodotEGP.Tests.Config.TestConfigObjectFake");
			});
	}

	[Fact]
	public async void ConfigObjectTests_object_save()
	{
		var filePath = $"{nameof(ConfigObjectTests_object_save)}.json";

		var obj = ConfigObject.Create("GodotEGP.Tests.Config.TestConfigObject");

		obj.DataEndpoint = new FileEndpoint(filePath);
		obj.Save();

		var tcs = new TaskCompletionSource();

		obj.SubscribeOwner<DataOperationComplete>((e) => {
			tcs.SetResult();
			}, isHighPriority:true);

		obj.SubscribeOwner<DataOperationError>((e) => {
				tcs.SetException(e.RunWorkerCompletedEventArgs.Error);
			}, isHighPriority:true);

		await tcs.Task;

		if (File.Exists(filePath))
		{
			File.Delete(filePath);
		}
	}

	[Fact]
	public async void ConfigObjectTests_object_load()
	{
		var filePath = $"{nameof(ConfigObjectTests_object_load)}.json";

		var obj = (ConfigObject<TestConfigObject>) ConfigObject.Create("GodotEGP.Tests.Config.TestConfigObject");

		obj.Value.TestProp = 123;

		obj.DataEndpoint = new FileEndpoint(filePath);
		obj.Save();

		await Task.Run(async () => {
				while (true)
				{
					if (File.Exists(filePath))
					{
						break;
					}

					LoggerManager.LogDebug("Waiting for test file", "", "file", filePath);
					await Task.Delay(100);
				}
			});

		var tcs = new TaskCompletionSource();

		var objLoad = (ConfigObject<TestConfigObject>) ConfigObject.Create("GodotEGP.Tests.Config.TestConfigObject");

		objLoad.DataEndpoint = new FileEndpoint(filePath);
		objLoad.Load();

		objLoad.SubscribeOwner<DataOperationComplete>((e) => {
			tcs.SetResult();
			}, isHighPriority:true);

		objLoad.SubscribeOwner<DataOperationError>((e) => {
				tcs.SetException(e.RunWorkerCompletedEventArgs.Error);
			}, isHighPriority:true);

		await tcs.Task;

		if (File.Exists(filePath))
		{
			File.Delete(filePath);
		}

		Assert.Equal(123, objLoad.Value.TestProp);
	}
}

public partial class TestConfigObject : VConfig
{
	internal readonly VValue<int> _testProp;

	public int TestProp
	{
		get { return _testProp.Value; }
		set { _testProp.Value = value; }
	}
	
	public TestConfigObject()
	{
		_testProp = AddValidatedValue<int>(this)
		    .Default(0)
		    .ChangeEventsEnabled();
	}
}
