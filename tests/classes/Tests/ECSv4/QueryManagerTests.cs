/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : QueryManagerTests
 * @created     : Tuesday May 07, 2024 14:10:57 CST
 */

namespace GodotEGP.Tests.ECSv4;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using GodotEGP.ECSv4;
using GodotEGP.ECSv4.Queries;
using GodotEGP.ECSv4.Components;

public partial class QueryManagerTests : TestContext
{
	[Fact]
	public void ECSv4QueryManagerTests_test_query_registration_no_name()
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
	public void ECSv4QueryManagerTests_test_query_registration_with_name()
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
	public void ECSv4QueryManagerTests_test_query_execution_by_name()
	{
		ECS ecs = new ECS();

		Query query = ecs.CreateQuery()
			.Has(ecs.Id<EcsComponent>())
			.Build();

		EntityHandle q1 = ecs.RegisterQuery(query, "q1");

		LoggerManager.LogDebug("Query entity id", "", "entity", q1);

		QueryResult res = ecs.Query("q1");
		ArraySegment<EntityHandle> handles = ecs.EntityHandles(res.Entities.ArraySegment.ToArray()).ArraySegment;

		LoggerManager.LogDebug("Query result", "", "res", handles);

		Assert.Equal(1, res.Entities.Count);
		Assert.Contains(EcsComponentConfig.Id, res.Entities.ArraySegment);
	}

	[Fact]
	public void ECSv4QueryManagerTests_test_query_cached_results()
	{
		ECS ecs = new ECS();

		// create a test entity
		EntityHandle e1 = ecs.Create("test entity");
		e1.Add<TestTag>();

		// create a query matching the entity
		Query query = ecs.CreateQuery()
			.Has(ecs.Id<TestTag>())
			.Build();

		EntityHandle q1 = ecs.RegisterQuery(query, "test query");

		LoggerManager.LogDebug("Query entity id", "", "entity", q1);

		// run the query
		QueryResult res = ecs.Query("test query");
		ArraySegment<EntityHandle> handles = ecs.EntityHandles(res.Entities.ArraySegment.ToArray()).ArraySegment;

		LoggerManager.LogDebug("Query result", "", "res", handles);
		LoggerManager.LogDebug("Query result hash", "", "hash", res.GetHashCode());

		QueryResult result = ecs.QueryResults("test query");

		LoggerManager.LogDebug("Query result obtained hash", "", "hash", result.GetHashCode());

		Assert.Equal(1, res.Entities.Count);
		Assert.Contains(e1.Entity, res.Entities.ArraySegment);

		// disable automatic query updating
		ecs.Config.KeepQueryResultsUpdated = false;

		// remove the testtag from the entity
		// the query results shouldn't update
		e1.Remove<TestTag>();

		// get the results without running the query
		QueryResult resultAfter = ecs.QueryResults("test query");

		LoggerManager.LogDebug("Query result after", "", "res", resultAfter);

		// verify the results still contain the outdated entity
		Assert.Equal(1, resultAfter.Entities.Count);
		Assert.Contains(e1.Entity, resultAfter.Entities.ArraySegment);

		// manually update the query results for the entity
		ecs.Config.KeepQueryResultsUpdated = true;
		ecs._updateQueryResults(e1.Entity);

		LoggerManager.LogDebug("Query result after update", "", "res", resultAfter);

		// verify query results are empty
		QueryResult resultAfterUpdate = ecs.QueryResults("test query");

		Assert.Equal(0, resultAfterUpdate.Entities.Count);
	}
}

