/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : SaveDataManagerTests
 * @created     : Friday Apr 19, 2024 18:38:23 CST
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
using GodotEGP.SaveData;

public partial class SaveDataManagerTests : TestContext
{
	[Fact]
	public async void SaveDataManagerTests_saving_data()
	{
		// setup savedatamanger service
		var saveDataManager = new SaveDataManager();

		var filepath = OS.GetUserDataDir();
		var testConfigClass = "GodotEGP.Tests.Services.SaveDataTestObject";
		var filepathFull = Path.Combine(filepath, "Save", testConfigClass);

		Directory.CreateDirectory(filepathFull);

		var tcs = new TaskCompletionSource<ConfigObject>();

		saveDataManager.SubscribeOwner<SaveDataSaveComplete>((e) => {
			tcs.SetResult(e.SaveData);
			}, isHighPriority:true, oneshot:true);
		saveDataManager.SubscribeOwner<SaveDataSaveError>((e) => {
			tcs.SetResult(null);
			throw e.Exception;
			}, isHighPriority:true, oneshot:true);

		// create a save data instance
		saveDataManager.Create<SaveDataTestObject>("test-save", saveCreated:true);
		var save = saveDataManager.Get<SaveDataTestObject>("test-save");

		await tcs.Task;

		var saveFileContent = File.ReadAllText(filepathFull+"/test-save.json");

		LoggerManager.LogDebug("Save data", "", "save", save);
		LoggerManager.LogDebug("Save data file content", "", "save", saveFileContent);

		Assert.NotNull(save);
		Assert.Contains(@"SaveVersion"": "+save.SaveVersion, saveFileContent);

		// part 2 - saving save files
		save.SaveVersion = 2;

		var tcs2 = new TaskCompletionSource<ConfigObject>();

		saveDataManager.SubscribeOwner<SaveDataSaveComplete>((e) => {
			tcs2.SetResult(e.SaveData);
			}, isHighPriority:true, oneshot:true);
		saveDataManager.SubscribeOwner<SaveDataSaveError>((e) => {
			tcs2.SetResult(null);
			throw e.Exception;
			}, isHighPriority:true, oneshot:true);

		saveDataManager.Save("test-save");

		await tcs2.Task;

		var saveFileContent2 = File.ReadAllText(filepathFull+"/test-save.json");

		LoggerManager.LogDebug("Save data file content", "", "save", saveFileContent2);

		Assert.NotNull(save);
		Assert.Contains(@"SaveVersion"": "+save.SaveVersion, saveFileContent2);

		// part 3 - copying save data
		saveDataManager.Copy("test-save", "test-save2");

		saveDataManager.Get<SaveDataTestObject>("test-save2").SaveVersion = 3;

		Assert.Equal(2, saveDataManager.Get<SaveDataTestObject>("test-save").SaveVersion);
		Assert.Equal(3, saveDataManager.Get<SaveDataTestObject>("test-save2").SaveVersion);

		// part 4 - moving save data
		saveDataManager.Move("test-save2", "test-save-moved");

		Assert.Equal(2, saveDataManager.Get<SaveDataTestObject>("test-save").SaveVersion);
		Assert.Equal(3, saveDataManager.Get<SaveDataTestObject>("test-save-moved").SaveVersion);
		Assert.Throws<SaveDataNotFoundException>(() => saveDataManager.Get<SaveDataTestObject>("test-save2"));

		// part 5 - deleting save data
		saveDataManager.Remove("test-save-moved");
		saveDataManager.Remove("test-save");

		Assert.Throws<SaveDataNotFoundException>(() => saveDataManager.Get<SaveDataTestObject>("test-save-moved"));
		Assert.Throws<SaveDataNotFoundException>(() => saveDataManager.Get<SaveDataTestObject>("test-save"));

		// delete directory when finished
		Directory.Delete(filepath, true);
	}
}

public partial class SaveDataTestObject : GameSaveFile
{

}
