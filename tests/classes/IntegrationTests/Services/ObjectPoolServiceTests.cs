/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ObjectPoolServiceTests
 * @created     : Friday Apr 19, 2024 18:22:07 CST
 */

namespace GodotEGP.Tests.Services;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using GodotEGP.Objects.ObjectPool;

public partial class ObjectPoolServiceTests : TestContext
{
	[Fact]
	public void ObjectPoolServiceTests_pool_hit_miss_count()
	{
		var objectPoolService = new ObjectPoolService();		

		var pool = objectPoolService.GetPoolInstance<Event>();

		var eventObj = pool.Get();

		Assert.Equal(0, pool.PoolHitCount);
		Assert.Equal(1, pool.PoolMissCount);

		pool.Return(eventObj);

		var eventObj2 = pool.Get();

		Assert.Equal(1, pool.PoolHitCount);
		Assert.Equal(1, pool.PoolMissCount);
	}
}

