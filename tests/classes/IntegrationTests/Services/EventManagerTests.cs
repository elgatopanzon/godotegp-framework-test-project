/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : EventManagerTests
 * @created     : Friday Apr 19, 2024 12:51:26 CST
 */

namespace GodotEGP.Tests.Services;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

public partial class EventManagerTests : TestContext
{
	[Fact]
	public async void EventManagerTests_emit_stress_test()
	{
		int eventCount = 100;
		LoggerManager.Instance.SetConfig(new LoggerConfig());
		LoggerManager.SetLogLevel(Message.LogLevel.Warning);

		var objs = new List<TestObject>();

		int receivedAlreadyCount = 0;

		for (int i = 0; i < eventCount; i++)
		{
			var obj = new TestObject() {
				Id = i,
			};

			obj.SubscribeOwner<Event>((e) => {
				var testObj = (e.Owner as TestObject);
				if (testObj.EventReceived)
				{
					receivedAlreadyCount++;
				}

				testObj.EventReceived = true;
			}, isHighPriority:true);

			objs.Add(obj);
		}

		foreach (var item in objs)
		{
			await item.EmitEvent();
		}

		if (receivedAlreadyCount > 0)
		{
			throw new Exception($"Multiple receives for same object: {receivedAlreadyCount}");
		}

		foreach (var item in objs)
		{
			if (!item.EventReceived)
			{
				throw new Exception("Event didn't reach receiver");
			}
		}

		LoggerManager.SetLogLevel(Message.LogLevel.Debug);
	}

	[Fact]
	public async void EventManagerTests_emit_stress_test_async()
	{
		int eventCount = 100;
		LoggerManager.Instance.SetConfig(new LoggerConfig());
		LoggerManager.SetLogLevel(Message.LogLevel.Warning);

		var objs = new List<TestObject>();
		var tasks = new List<Task>();

		int receivedAlreadyCount = 0;

		for (int i = 0; i < eventCount; i++)
		{
			var obj = new TestObject() {
				Id = i,
			};

			obj.SubscribeOwner<Event>((e) => {
				var testObj = (e.Owner as TestObject);
				if (testObj.EventReceived)
				{
					receivedAlreadyCount++;
				}

				testObj.EventReceived = true;
			}, isHighPriority:true);

			objs.Add(obj);
			tasks.Add(new Task(async () => {
				await obj.EmitEvent();
			}));
			// tasks.Add(Task.Run(async () => {
			// 	await objs[i].EmitEvent();
			// }));

		}

		foreach (var task in tasks)
		{
			task.Start();
		}

		Task.WaitAll(tasks.ToArray(), 9999999);

		if (receivedAlreadyCount > 0)
		{
			throw new Exception($"Multiple receives for same object: {receivedAlreadyCount}");
		}

		foreach (var item in objs)
		{
			if (!item.EventReceived)
			{
				throw new Exception("Event didn't reach receiver");
			}
		}

		LoggerManager.SetLogLevel(Message.LogLevel.Debug);
	}
}

public partial class TestObject
{
	public int Id { get; set; } = 0;
	public bool EventReceived { get; set; } = false;

	public async Task EmitEvent()
	{
		this.Emit<Event>();
	}
}
