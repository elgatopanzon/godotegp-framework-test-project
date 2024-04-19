/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : RemoteTransferOperationTests
 * @created     : Thursday Apr 18, 2024 15:29:16 CST
 */

namespace GodotEGP.Tests.DAL;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;
using GodotEGP.Resource;
using GodotEGP.Resource.Resources;

using GodotEGP.DAL.Operations;
using GodotEGP.DAL.Endpoints;

public partial class RemoteTransferOperationTests : TestContext
{
	[Fact]
	public async void RemoteTransferOperationTests_file_download()
	{
		var filePath = $"{nameof(RemoteTransferOperationTests_file_download)}.json";

		var httpEndpoint = new HTTPEndpoint("echo.free.beeceptor.com", port:443, path:"/test-path", requestMethod:HttpMethod.Get, verifySSL:true);
		var fileEndpoint = new FileEndpoint(filePath);

		var tcs = new TaskCompletionSource<ResourceObject<RemoteTransferResult>>();

		var operation = new DataOperationProcessRemoteTransfer<ResourceObject<RemoteTransferResult>>(fileEndpoint:fileEndpoint, httpEndpoint:httpEndpoint, onCompleteCb:(e) => {
				if (e is DataOperationComplete ee)
				{
					tcs.SetResult((ee.RunWorkerCompletedEventArgs.Result as DataOperationResult<ResourceObject<RemoteTransferResult>>).ResultObject);
				}
			},
			onErrorCb:(e) => {
				if (e is DataOperationError ee)
				{
					tcs.SetException(ee.RunWorkerCompletedEventArgs.Error);
				}
			});	

		operation.Save();

		await tcs.Task;

		LoggerManager.LogDebug("Remote transfer result", "", "res", tcs.Task.Result.Value);
		LoggerManager.LogDebug("Remote transfer file content", "", "res", File.ReadAllText(filePath));

		Assert.Contains("echo.free.beeceptor.com", File.ReadAllText(filePath));

		if (File.Exists(filePath))
		{
			File.Delete(filePath);
		}
	}
}

