/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : FileOperationTests
 * @created     : Thursday Apr 18, 2024 14:09:06 CST
 */

namespace GodotEGP.Tests.DAL;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Objects.Validated;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;
using GodotEGP.DAL.Operations;
using GodotEGP.DAL.Endpoints;
using System.ComponentModel;

public partial class FileOperationTests : TestContext
{
	[Fact]
	public async void FileOperationTests_loading_and_deserialising_vobject()
	{
		var filePath = $"{nameof(FileOperationTests_loading_and_deserialising_vobject)}.json";

		File.WriteAllText(filePath, @"{""TestProp"": 123}");

		var tcs = new TaskCompletionSource<FileOperationTestObject>();

		// create the file operation
		var operation = new DataOperationProcessFile<FileOperationTestObject>(filePath, null, onCompleteCb:(e) => {
				if (e is DataOperationComplete ee)
				{
					tcs.SetResult((ee.RunWorkerCompletedEventArgs.Result as DataOperationResult<FileOperationTestObject>).ResultObject);
				}
			},
			onErrorCb:(e) => {
				if (e is DataOperationError ee)
				{
					tcs.SetException(ee.RunWorkerCompletedEventArgs.Error);
				}
			});

		await operation.LoadAsync();

		await tcs.Task;

		if (File.Exists(filePath))
		{
			File.Delete(filePath);
		}

		Assert.Equal(123, tcs.Task.Result.TestProp);
	}

	[Fact]
	public async void FileOperationTests_loading_from_file()
	{
		var filePath = $"{nameof(FileOperationTests_loading_from_file)}";

		File.WriteAllText(filePath, @"basic text file");

		var tcs = new TaskCompletionSource<string>();

		// create the file operation
		var operation = new DataOperationProcessFile<string>(filePath, null, onCompleteCb:(e) => {
				if (e is DataOperationComplete ee)
				{
					tcs.SetResult((ee.RunWorkerCompletedEventArgs.Result as DataOperationResult<string>).ResultObject);
				}
			},
			onErrorCb:(e) => {
				if (e is DataOperationError ee)
				{
					tcs.SetException(ee.RunWorkerCompletedEventArgs.Error);
				}
			});

		await operation.LoadAsync();

		await tcs.Task;

		if (File.Exists(filePath))
		{
			File.Delete(filePath);
		}

		Assert.Equal("basic text file", tcs.Task.Result);
	}

	[Fact]
	public async void FileOperationTests_saving_and_serialising_vobject()
	{
		var filePath = $"{nameof(FileOperationTests_saving_and_serialising_vobject)}.json";

		var obj = new FileOperationTestObject() {
			TestProp = 222,
		};

		var tcs = new TaskCompletionSource();

		// create the file operation
		var operation = new DataOperationProcessFile<FileOperationTestObject>(filePath, obj, onCompleteCb:(e) => {
				if (e is DataOperationComplete ee)
				{
					tcs.SetResult();
				}
			},
			onErrorCb:(e) => {
				if (e is DataOperationError ee)
				{
					tcs.SetException(ee.RunWorkerCompletedEventArgs.Error);
				}
			});

		await operation.SaveAsync();

		await tcs.Task;

		Assert.Equal(@"{  ""TestProp"": 222}", File.ReadAllText(filePath).Replace("\n", ""));

		if (File.Exists(filePath))
		{
			File.Delete(filePath);
		}
	}

	[Fact]
	public async void FileOperationTests_saving_to_file()
	{
		var filePath = $"{nameof(FileOperationTests_saving_to_file)}.txt";

		var stringObj = "saved to file";

		var tcs = new TaskCompletionSource();

		// create the file operation
		var operation = new DataOperationProcessFile<FileOperationTestObject>(filePath, stringObj, onCompleteCb:(e) => {
				if (e is DataOperationComplete ee)
				{
					tcs.SetResult();
				}
			},
			onErrorCb:(e) => {
				if (e is DataOperationError ee)
				{
					tcs.SetException(ee.RunWorkerCompletedEventArgs.Error);
				}
			});

		await operation.SaveAsync();

		await tcs.Task;

		Assert.Equal(stringObj, File.ReadAllText(filePath));

		if (File.Exists(filePath))
		{
			File.Delete(filePath);
		}
	}
}

public partial class FileOperationTestObject : VConfig
{
	internal readonly VValue<int> _testProp;

	public int TestProp
	{
		get { return _testProp.Value; }
		set { _testProp.Value = value; }
	}
	
	public FileOperationTestObject()
	{
		_testProp = AddValidatedValue<int>(this)
	    .Default(1)
	    .ChangeEventsEnabled();
	}
}
