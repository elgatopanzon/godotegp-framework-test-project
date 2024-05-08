/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : QueryManagerTests
 * @created     : Tuesday May 07, 2024 14:10:57 CST
 */

namespace GodotEGP.Tests.ECSv3;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using GodotEGP.ECSv3;
using GodotEGP.ECSv3.Queries;
using GodotEGP.ECSv3.Components;

public partial class QueryManagerTests : TestContext
{
	[Fact]
	public void QueryManagerTests_test_query_registration_no_name()
	{
		ECS ecs = new ECS();

		Query query = ecs.CreateQuery()
			.Build();

		EntityHandle q1 = ecs.RegisterQuery(query);
		EntityHandle q2 = ecs.RegisterQuery(query);

		LoggerManager.LogDebug("Query entity id", "", "entity", q1);
		LoggerManager.LogDebug("Query entity id", "", "entity", q2);

		LoggerManager.LogDebug("Query result", "", "result", ecs.Query(q1));

		// the IDs should be different because we're not using a named query
		Assert.NotEqual(q1.Entity, q2.Entity);
	}

	[Fact]
	public void QueryManagerTests_test_query_registration_with_name()
	{
		ECS ecs = new ECS();

		Query query = ecs.CreateQuery()
			.Build();

		EntityHandle q1 = ecs.RegisterQuery(query, "q1");
		EntityHandle q2 = ecs.RegisterQuery(query, "q1");

		LoggerManager.LogDebug("Query entity id", "", "entity", q1);
		LoggerManager.LogDebug("Query entity id", "", "entity", q2);

		LoggerManager.LogDebug("Query result", "", "result", ecs.Query(q1));

		// the IDs should be the same because we used a name
		Assert.Equal(q1.Entity, q2.Entity);
	}

	[Fact]
	public void QueryManagerTests_test_query_execution_by_name()
	{
		ECS ecs = new ECS();

		Query query = ecs.CreateQuery()
			.Has(ecs.Id<EcsQuery>())
			.Build();

		EntityHandle q1 = ecs.RegisterQuery(query, "q1");

		LoggerManager.LogDebug("Query entity id", "", "entity", q1);

		QueryResult res = ecs.Query("q1");
		ArraySegment<EntityHandle> handles = ecs.EntityHandles(res.Entities.ArraySegment.ToArray()).ArraySegment;

		LoggerManager.LogDebug("Query result", "", "res", handles);

		Assert.Equal(1, res.Entities.Count);
		Assert.Contains(q1.Entity, res.Entities.ArraySegment);
	}
}

