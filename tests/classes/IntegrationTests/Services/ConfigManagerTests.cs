/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ConfigManagerTests
 * @created     : Thursday Apr 18, 2024 21:56:03 CST
 */

namespace GodotEGP.Tests.Services;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Objects.Validated;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;
using GodotEGP.DAL.Operator;
using GodotEGP.DAL.Endpoints;

public partial class ConfigManagerTests : TestContext
{
	[Fact]
	public async void ConfigManagerTests_auto_discovery_and_loading()
	{
		// setup configmanager service
		ConfigManager configManager = new ConfigManager();

		var filepath = nameof(ConfigManagerTests_auto_discovery_and_loading);
		var testConfigClass = "GodotEGP.Tests.Services.ConfigObjectTest";

		var obj1 = new ConfigObjectTest() {
			IntValue = 10,
			StringValue = "1",
		};
		var obj2 = new ConfigObjectTest() {
			IntValue = 20,
			StringValue = "1",
		};

		// setup the test config directory
		Directory.CreateDirectory(filepath);
		Directory.CreateDirectory(Path.Combine(filepath, testConfigClass));

		var fileOperator = new FileOperator();

		fileOperator.SetDataEndpoint(new FileEndpoint($"{filepath}/Config/{testConfigClass}/test1.json"));
		fileOperator.Save(obj1);

		fileOperator.SetDataEndpoint(new FileEndpoint($"{filepath}/Config/{testConfigClass}/test2.json"));
		fileOperator.Save(obj2);

		// add our config dir
		configManager.AddConfigDataDir(filepath);

		var tcs = new TaskCompletionSource<List<ConfigObject>>();

		configManager.SubscribeOwner<ConfigManagerLoaderCompleted>((e) => {
			tcs.SetResult(e.ConfigObjects);
			}, isHighPriority:true, oneshot:true);
		configManager.SubscribeOwner<ConfigManagerLoaderError>((e) => {
			tcs.SetResult(null);
			throw e.RunWorkerCompletedEventArgs.Error;
			}, isHighPriority:true, oneshot:true);

		// discovery config files
		configManager.DiscoveryConfigFiles();

		// wait for config files to load
		await tcs.Task;

		// validate loaded config is set and merged
		// Assert.Equal(20, (tcs.Task.Result[0] as ConfigObject<ConfigObjectTest>).Value.IntValue);
		// Assert.Equal("1", (tcs.Task.Result[0] as ConfigObject<ConfigObjectTest>).Value.StringValue);
		Assert.Equal(20, configManager.Get<ConfigObjectTest>().IntValue);
		Assert.Equal("1", configManager.Get<ConfigObjectTest>().StringValue);

		// part 2 - config auto reload on changes
		// trigger service ready
		configManager._OnServiceReady();

		// setup loading subscription with new tcs
		var tcs2 = new TaskCompletionSource<List<ConfigObject>>();

		// trigger a process call on directory changed event
		configManager.SubscribeOwner<ConfigManagerDirectoryChanged>((e) => {
			configManager._Process(0);
			}, isHighPriority:true, oneshot:true);

		configManager.SubscribeOwner<ConfigManagerLoaderCompleted>((e) => {
			tcs2.SetResult(e.ConfigObjects);
			}, isHighPriority:true, oneshot:true);
		configManager.SubscribeOwner<ConfigManagerLoaderError>((e) => {
			tcs2.SetResult(null);
			throw e.RunWorkerCompletedEventArgs.Error;
			}, isHighPriority:true, oneshot:true);


		// write config file with new value
		obj2.StringValue = "3";
		fileOperator.SetDataEndpoint(new FileEndpoint($"{filepath}/Config/{testConfigClass}/test2.json"));
		fileOperator.Save(obj2);

		// wait for config file changes to be detected and loaded
		await tcs2.Task;

		// validate config file was reloaded
		Assert.Equal("3", configManager.Get<ConfigObjectTest>().StringValue);

		// part 3 - saving config object to disk by class
		var endpoint = new FileEndpoint($"{filepath}/Config/{testConfigClass}/test2.json");

		var tcs3 = new TaskCompletionSource<ConfigObject>();

		// set and issue config save operation
		configManager.Get<ConfigObjectTest>().StringValue = "string value";
		configManager.Save<ConfigObjectTest>();

		configManager.SubscribeOwner<ConfigManagerSaveCompleted>((e) => {
			tcs3.SetResult(e.ConfigObject);
			}, isHighPriority:true, oneshot:true);
		configManager.SubscribeOwner<ConfigManagerSaveError>((e) => {
			tcs3.SetResult(null);
			throw e.Exception;
			}, isHighPriority:true, oneshot:true);

		// wait for save process to complete
		await tcs3.Task;

		// check directly the content of the file
		Assert.Contains("string value", File.ReadAllText(endpoint.Path));

		// delete directory when finished
		Directory.Delete(filepath, true);
	}
}

public partial class ConfigObjectTest : VConfig
{
	internal readonly VValue<int> _intValue;

	public int IntValue
	{
		get { return _intValue.Value; }
		set { _intValue.Value = value; }
	}

	internal readonly VValue<string> _stringValue;

	public string StringValue
	{
		get { return _stringValue.Value; }
		set { _stringValue.Value = value; }
	}
	
	public ConfigObjectTest()
	{
		_intValue = AddValidatedValue<int>(this)
		    .Default(0)
		    .ChangeEventsEnabled();

		_stringValue = AddValidatedValue<string>(this)
		    .Default("")
		    .ChangeEventsEnabled();
	}
}
