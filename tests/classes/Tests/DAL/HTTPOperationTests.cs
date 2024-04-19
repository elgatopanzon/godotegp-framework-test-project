/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : HTTPOperationTests
 * @created     : Thursday Apr 18, 2024 14:53:01 CST
 */

namespace GodotEGP.Tests.DAL;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Objects.Validated;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using System.Net.Http;

using GodotEGP.DAL.Operations;
using GodotEGP.DAL.Endpoints;

public partial class HTTPOperationTests : TestContext
{
	[Fact]
	public async void HTTPOperationTests_GET()
	{
		var tcs = new TaskCompletionSource<HTTPOperationTestResultObject>();

		var endpoint = new HTTPEndpoint("echo.free.beeceptor.com", port:443, path:"/test-path", requestMethod:HttpMethod.Get, verifySSL:true);
		var operation = new DataOperationProcessHTTP<HTTPOperationTestResultObject>(endpoint, null, onCompleteCb:(e) => {
				if (e is DataOperationComplete ee)
				{
					tcs.SetResult((ee.RunWorkerCompletedEventArgs.Result as DataOperationResult<HTTPOperationTestResultObject>).ResultObject);
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

		LoggerManager.LogDebug("Http request result", "", "res", tcs.Task.Result);

		Assert.Equal("GET", tcs.Task.Result.Method);
	}

	[Fact]
	public async void HTTPOperationTests_GET_query_params()
	{
		var tcs = new TaskCompletionSource<HTTPOperationTestResultObject>();

		var endpoint = new HTTPEndpoint("echo.free.beeceptor.com", port:443, path:"/test-path", requestMethod:HttpMethod.Get, verifySSL:true, urlParams:new() { { "test-param", "test-value" } });
		var operation = new DataOperationProcessHTTP<HTTPOperationTestResultObject>(endpoint, null, onCompleteCb:(e) => {
				if (e is DataOperationComplete ee)
				{
					tcs.SetResult((ee.RunWorkerCompletedEventArgs.Result as DataOperationResult<HTTPOperationTestResultObject>).ResultObject);
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

		LoggerManager.LogDebug("Http request result", "", "res", tcs.Task.Result);

		Assert.Equivalent(new Dictionary<string, object>() { { "test-param", "test-value" } }, tcs.Task.Result.ParsedQueryParams);
	}

	[Fact]
	public async void HTTPOperationTests_POST_json_data()
	{
		var tcs = new TaskCompletionSource<HTTPOperationTestResultObject>();

		var endpoint = new HTTPEndpoint("echo.free.beeceptor.com", port:443, path:"/test-path", requestMethod:HttpMethod.Post, verifySSL:true);
		var operation = new DataOperationProcessHTTP<HTTPOperationTestResultObject>(endpoint, new HTTPOperationTestPayload(), onCompleteCb:(e) => {
				if (e is DataOperationComplete ee)
				{
					tcs.SetResult((ee.RunWorkerCompletedEventArgs.Result as DataOperationResult<HTTPOperationTestResultObject>).ResultObject);
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

		LoggerManager.LogDebug("Http request result", "", "res", tcs.Task.Result);

		Assert.Equal(1, Convert.ToInt32(tcs.Task.Result.ParsedBody["Prop"]));
	}
}

public partial class HTTPOperationTestResultObject : VObject
{
	internal readonly VValue<string> _method;

	public string Method
	{
		get { return _method.Value; }
		set { _method.Value = value; }
	}

	internal readonly VValue<string> _path;

	public string Path
	{
		get { return _path.Value; }
		set { _path.Value = value; }
	}

	internal readonly VValue<Dictionary<string, object>> _parsedQueryParams;

	public Dictionary<string, object> ParsedQueryParams
	{
		get { return _parsedQueryParams.Value; }
		set { _parsedQueryParams.Value = value; }
	}

	internal readonly VValue<Dictionary<string, object>> _parsedBody;

	public Dictionary<string, object> ParsedBody
	{
		get { return _parsedBody.Value; }
		set { _parsedBody.Value = value; }
	}


	public HTTPOperationTestResultObject()
	{
		_method = AddValidatedValue<string>(this)
		    .Default("")
		    .ChangeEventsEnabled();

		_path = AddValidatedValue<string>(this)
		    .Default("")
		    .ChangeEventsEnabled();

		_parsedQueryParams = AddValidatedValue<Dictionary<string, object>>(this)
		    .Default(new Dictionary<string, object>())
		    .ChangeEventsEnabled();

		_parsedBody = AddValidatedValue<Dictionary<string, object>>(this)
		    .Default(new Dictionary<string, object>())
		    .ChangeEventsEnabled();
	}
}

public partial class HTTPOperationTestPayload : VObject
{
	internal readonly VValue<int> _prop;

	public int Prop
	{
		get { return _prop.Value; }
		set { _prop.Value = value; }
	}

	public HTTPOperationTestPayload()
	{
		_prop = AddValidatedValue<int>(this)
		    .Default(1)
		    .ChangeEventsEnabled();
	}
}
