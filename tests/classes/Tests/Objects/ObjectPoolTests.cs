/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ObjectPoolTests
 * @created     : Thursday Apr 18, 2024 21:28:15 CST
 */

namespace GodotEGP.Tests.Objects;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Objects.ObjectPool;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

public partial class ObjectPoolTests : TestContext
{
	[Fact]
	public void ObjectPoolTests_object_creation()
	{
		var objPool = new ObjectPool<TestObject>(10, 100);

		var obj = objPool.Get();
		var objHash = obj.GetHashCode();

		objPool.Return(obj);

		var obj2 = objPool.Get();

		LoggerManager.LogDebug("Object hashes", "", objHash.ToString(), obj2.GetHashCode());

		// verify that the hash of the object we got back is the same as the one
		// we already returned earlier
		Assert.Equal(objHash, obj2.GetHashCode());
	}
}

public partial class TestObject
{

}
