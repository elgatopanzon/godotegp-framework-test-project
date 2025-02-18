/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : QueryBuilderTests
 * @created     : Friday May 03, 2024 14:20:56 CST
 */

namespace GodotEGP.Tests.ECSv4;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using GodotEGP.Collections;
using GodotEGP.ECSv4;
using GodotEGP.ECSv4.Components;
using GodotEGP.ECSv4.Queries;

using System.Text.RegularExpressions;

public partial class QueryBuilderTestsContext : TestContext
{
	protected ECS _ecs;
	protected PackedDictionary<string, EntityHandle> _entities;

	public QueryBuilderTestsContext()
	{
		_ecs = new ECS();
		_entities = new();

		// register component entities
		_entities.Add("TestTag", _ecs.RegisterComponent<TestTag>());
		_entities.Add("TestTag2", _ecs.RegisterComponent<TestTag2>());
		_entities.Add("TestTag3", _ecs.RegisterComponent<TestTag3>());
		_entities.Add("TestTag4", _ecs.RegisterComponent<TestTag4>());
		_entities.Add("TestData", _ecs.RegisterComponent<TestData>());
		_entities.Add("TestData2", _ecs.RegisterComponent<TestData2>());
		_entities.Add("TestData3", _ecs.RegisterComponent<TestData3>());

		// register a number of test entities
		for (int i = 0; i < 10; i++)
		{
			_entities.Add($"e{i+1}", _ecs.Create($"e{i+1}"));
			_entities[$"e{i+1}"].Add<TestEntity>();
		}

		// add components to entities
		_entities["e1"].Add<TestTag>();

		_entities["e2"].Add<TestTag>();
		_entities["e2"].Add<TestTag2>();

		_entities["e3"].Add<TestTag2>();
		_entities["e3"].Set<TestData>(new TestData());

		_entities["e4"].Set<TestData>(new TestData());
		_entities["e4"].Set<TestData2>(new TestData2());

		_entities["e5"].Set<TestData>(new TestData());

		_entities["e6"].Add<TestTag>();

		_entities["e7"].Set<TestData3>(new TestData3());
		_entities["e7"].Add<TestTag3>();
		_entities["e7"].Add<TestTag4>();

		_entities["e8"].Set<TestData3>(new TestData3());

		_entities["e9"].Set<TestData3>(new TestData3());
		_entities["e9"].Add<TestTag3>();

		_entities["TestTag2"].Add<TestTag3>();
		_entities["TestTag"].Add<TestTag3>();
	}
}

public partial class QueryBuilderTests : QueryBuilderTestsContext
{
	[Fact]
	public void ECSv4QueryBuilderTests_query_has_only()
	{
		// create a query
		Query query = QueryBuilder.Create()
			.Has(_entities["TestTag"])
			.Build();

		// run the query and get results
		QueryEntities res = _ecs.Query(query);

		LoggerManager.LogDebug("Query result", "", "res", res);

		Assert.Equal(3, res.Entities.Count);
		Assert.Contains(_entities["e1"], res.Entities);
		Assert.Contains(_entities["e2"], res.Entities);
		Assert.Contains(_entities["e6"], res.Entities);
	}

	[Fact]
	public void ECSv4QueryBuilderTests_query_and()
	{
		// create a query
		Query query = QueryBuilder.Create()
			.Has(_entities["TestTag"])
			.Has(_entities["TestTag2"])
			.Build();

		// run the query and get results
		QueryEntities res = _ecs.Query(query);

		LoggerManager.LogDebug("Query result", "", "res", res.Entities);

		Assert.Equal(1, res.Entities.Count);
		Assert.Contains(_entities["e2"], res.Entities);
	}

	[Fact]
	public void ECSv4QueryBuilderTests_query_or()
	{
		Query query = QueryBuilder.Create()
			.Has(_entities["TestTag"])
			.Or()
			.Has(_entities["TestTag2"])
			.Build();

		// run the query and get results
		QueryEntities res = _ecs.Query(query);

		LoggerManager.LogDebug("Query result", "", "res", res.Entities);

		Assert.Equal(4, res.Entities.Count);
		Assert.Contains(_entities["e1"], res.Entities);
		Assert.Contains(_entities["e2"], res.Entities);
		Assert.Contains(_entities["e3"], res.Entities);
		Assert.Contains(_entities["e6"], res.Entities);
	}

	[Fact]
	public void ECSv4QueryBuilderTests_query_not_only()
	{
		// create a query
		Query query = QueryBuilder.Create()
			.Not(_ecs.Id<EcsTag>()) // exclude all components
			.Not(_ecs.Id<EcsComponent>()) // exclude all components
			.Not(_ecs.Id<EcsProcessPhase>()) // exclude all components
			.Not(_ecs.Id<EcsQuery>()) // exclude all components
			.Not(_entities["TestTag2"]) // exclude all with TestTag2
			.Build();

		// run the query and get results
		QueryEntities res = _ecs.Query(query);

		List<EntityHandle> handles = _ecs.EntityHandles(res.Entities.ToArray());

		LoggerManager.LogDebug("Query result", "", "res", handles);

		Assert.Equal(8, res.Entities.Count);
		Assert.Contains(_entities["e1"], res.Entities);
		Assert.Contains(_entities["e4"], res.Entities);
		Assert.Contains(_entities["e5"], res.Entities);
		Assert.Contains(_entities["e6"], res.Entities);
		Assert.Contains(_entities["e7"], res.Entities);
		Assert.Contains(_entities["e8"], res.Entities);
		Assert.Contains(_entities["e9"], res.Entities);
	}

	[Fact]
	public void ECSv4QueryBuilderTests_query_not_after_has()
	{
		// create a query
		Query query = QueryBuilder.Create()
			.Has(_entities["TestTag"])
			.Not(_ecs.Id<EcsComponent>()) // exclude all components
			.Not(_entities["TestTag2"]) // exclude all with TestTag2
			.Build();

		// run the query and get results
		QueryEntities res = _ecs.Query(query);

		LoggerManager.LogDebug("Query result", "", "res", res.Entities);

		Assert.Equal(2, res.Entities.Count);
		Assert.Contains(_entities["e1"], res.Entities);
		Assert.Contains(_entities["e6"], res.Entities);
	}

	[Fact]
	public void ECSv4QueryBuilderTests_query_has_and_not()
	{
		// create a query
		Query query = QueryBuilder.Create()
			.Has(_entities["TestTag"])
			.Not(_entities["TestTag2"])
			.Build();

		// run the query and get results
		QueryEntities res = _ecs.Query(query);

		LoggerManager.LogDebug("Query result", "", "res", res.Entities);

		Assert.Equal(2, res.Entities.Count);
		Assert.Contains(_entities["e1"], res.Entities);
		Assert.Contains(_entities["e6"], res.Entities);
	}

	[Fact]
	public void ECSv4QueryBuilderTests_query_combined_has_or_not_andnot()
	{
		// create a query
		Query query = QueryBuilder.Create()
			.Has(_entities["TestTag"])
			.Or()
			.Has(_entities["TestTag2"])
			.Not(_entities["TestData"])
			.Not(_entities["TestData2"])
			.Build();

		// run the query and get results
		QueryEntities res = _ecs.Query(query);

		List<EntityHandle> handles = _ecs.EntityHandles(res.Entities.ToArray());

		LoggerManager.LogDebug("Query result", "", "res", handles);

		Assert.Equal(3, res.Entities.Count);
		Assert.Contains(_entities["e1"], res.Entities);
		Assert.Contains(_entities["e2"], res.Entities);
		Assert.DoesNotContain(_entities["e3"], res.Entities);
		Assert.DoesNotContain(_entities["e4"], res.Entities);
		Assert.DoesNotContain(_entities["e5"], res.Entities);
		Assert.Contains(_entities["e6"], res.Entities);
	}

	[Fact]
	public void ECSv4QueryBuilderTests_query_scoped_and_and()
	{
		// create a query
		Query hasTestTag = QueryBuilder.Create()
			.Has(_entities["TestTag"])
			.Build();
		Query hasTestTag2 = QueryBuilder.Create()
			.Has(_entities["TestTag2"])
			.Build();

		Query query = QueryBuilder.Create()
			.Has(hasTestTag)
			.Has(hasTestTag2)
			.Build();


		// run the query and get results
		QueryEntities res = _ecs.Query(query);

		List<EntityHandle> handles = _ecs.EntityHandles(res.Entities.ToArray());

		LoggerManager.LogDebug("Query result", "", "res", handles);

		Assert.Equal(1, res.Entities.Count);
		Assert.Contains(_entities["e2"], res.Entities);
	}

	[Fact]
	public void ECSv4QueryBuilderTests_query_scoped_and_or()
	{
		// create a query
		Query hasTestTag = QueryBuilder.Create()
			.Has(_entities["TestTag"])
			.Build();
		Query hasTestTag2 = QueryBuilder.Create()
			.Has(_entities["TestTag2"])
			.Build();

		Query query = QueryBuilder.Create()
			.Has(hasTestTag)
			.Or()
			.Has(hasTestTag2)
			.Build();


		// run the query and get results
		QueryEntities res = _ecs.Query(query);

		List<EntityHandle> handles = _ecs.EntityHandles(res.Entities.ToArray());

		LoggerManager.LogDebug("Query result", "", "res", handles);

		Assert.Equal(4, res.Entities.Count);
		Assert.Contains(_entities["e1"], res.Entities);
		Assert.Contains(_entities["e2"], res.Entities);
		Assert.Contains(_entities["e3"], res.Entities);
		Assert.Contains(_entities["e6"], res.Entities);
	}

	[Fact]
	public void ECSv4QueryBuilderTests_query_scoped_has_and_not()
	{
		// create a query
		Query hasTestTag = QueryBuilder.Create()
			.Has(_entities["TestTag"])
			.Build();
		Query notTestTag2 = QueryBuilder.Create()
			.Not(_entities["TestTag2"])
			.Build();

		Query query = QueryBuilder.Create()
			.Has(hasTestTag)
			.Has(notTestTag2)
			.Build();


		// run the query and get results
		QueryEntities res = _ecs.Query(query);

		List<EntityHandle> handles = _ecs.EntityHandles(res.Entities.ToArray());

		LoggerManager.LogDebug("Query result", "", "res", handles);

		Assert.Equal(2, res.Entities.Count);
		Assert.Contains(_entities["e1"], res.Entities);
		Assert.Contains(_entities["e6"], res.Entities);
	}

	[Fact]
	public void ECSv4QueryBuilderTests_query_scoped_combined()
	{
		// create a query
		Query hasTestTag3AndData3 = QueryBuilder.Create()
			.Has(_entities["TestData3"])
			.Has(_entities["TestTag3"])
			.Build();

		Query query = QueryBuilder.Create()
			.Has(hasTestTag3AndData3)
			.Not(_entities["TestTag4"])
			.Build();

		// run the query and get results
		QueryEntities res = _ecs.Query(query);

		List<EntityHandle> handles = _ecs.EntityHandles(res.Entities.ToArray());

		LoggerManager.LogDebug("Query result", "", "res", handles);

		Assert.Equal(1, res.Entities.Count);
		Assert.Contains(_entities["e9"], res.Entities);
	}

	[Fact]
	public void ECSv4QueryBuilderTests_query_scoped_combined_with_scoped_not()
	{
		// create a query
		Query hasTestTag3AndData3 = QueryBuilder.Create()
			.Has(_entities["TestData3"])
			.Not(_entities["TestTag3"])
			.Build();

		Query query = QueryBuilder.Create()
			.Has(hasTestTag3AndData3)
			.Build();

		// run the query and get results
		QueryEntities res = _ecs.Query(query);

		List<EntityHandle> handles = _ecs.EntityHandles(res.Entities.ToArray());

		LoggerManager.LogDebug("Query result", "", "res", handles);

		Assert.Equal(1, res.Entities.Count);
		Assert.Contains(_entities["e8"], res.Entities);
	}

	[Fact]
	public void ECSv4QueryBuilderTests_query_scoped_combined_with_not_scoped()
	{
		// create a query
		Query hasTestData3NotTestTag2 = QueryBuilder.Create()
			.Has(_entities["TestTag"])
			.Not(_entities["TestTag2"])
			.Build();

		Query query2 = QueryBuilder.Create()
			.Not(_ecs.Id<EcsComponent>()) // exclude all components
			.Not(_ecs.Id<EcsTag>()) // exclude all components
			.Not(hasTestData3NotTestTag2)
			.Build();

		Query query = QueryBuilder.Create()
			.Not(query2)
			.Has(QueryBuilder.Create().Has(_ecs.Id<TestEntity>()).Build())
			.Build();

		// run the query and get results
		QueryEntities res = _ecs.Query(query);

		List<EntityHandle> handles = _ecs.EntityHandles(res.Entities.ToArray());

		LoggerManager.LogDebug("Query result", "", "res", handles);

		Assert.Equal(8, res.Entities.Count);
		Assert.Contains(_entities["e2"], res.Entities);
		Assert.Contains(_entities["e3"], res.Entities);
		Assert.Contains(_entities["e4"], res.Entities);
		Assert.Contains(_entities["e5"], res.Entities);
		Assert.Contains(_entities["e7"], res.Entities);
		Assert.Contains(_entities["e8"], res.Entities);
		Assert.Contains(_entities["e9"], res.Entities);
	}

	[Fact]
	public void ECSv4QueryBuilderTests_query_is_equal()
	{
		// create a query
		Query query = QueryBuilder.Create()
			.Is(_entities["e5"])
			.Build();

		// run the query and get results
		QueryEntities res = _ecs.Query(query);

		List<EntityHandle> handles = _ecs.EntityHandles(res.Entities.ToArray());

		LoggerManager.LogDebug("Query result", "", "res", handles);

		Assert.Equal(1, res.Entities.Count);
		Assert.Contains(_entities["e5"], res.Entities);
	}

	[Fact]
	public void ECSv4QueryBuilderTests_query_is_not_equal()
	{
		// create a query
		Query query = QueryBuilder.Create()
			.Has(_ecs.Id<EcsComponent>()) // include data components
			.IsNot(_ecs.Id<EcsComponentConfig>()) // include data components
			.IsNot(_ecs.Id<EcsSystem>()) // include data components
			.IsNot(_entities["TestData2"])
			.Build();

		// run the query and get results
		QueryEntities res = _ecs.Query(query);

		List<EntityHandle> handles = _ecs.EntityHandles(res.Entities.ToArray());

		LoggerManager.LogDebug("Query result", "", "res", handles);

		Assert.Equal(2, res.Entities.Count);
		Assert.Contains(_entities["TestData"], res.Entities);
		Assert.Contains(_entities["TestData3"], res.Entities);
	}

	[Fact]
	public void ECSv4QueryBuilderTests_query_name_is()
	{
		// create a query
		Query query = QueryBuilder.Create()
			.NameIs("TestData2")
			.Build();

		// run the query and get results
		QueryEntities res = _ecs.Query(query);

		List<EntityHandle> handles = _ecs.EntityHandles(res.Entities.ToArray());

		LoggerManager.LogDebug("Query result", "", "res", handles);

		Assert.Equal(1, res.Entities.Count);
		Assert.Contains(_entities["TestData2"], res.Entities);
	}

	[Fact]
	public void ECSv4QueryBuilderTests_query_name_matches()
	{
		// create a query
		Query query = QueryBuilder.Create()
			.NameMatches(new Regex("TestData"))
			.Build();

		// run the query and get results
		QueryEntities res = _ecs.Query(query);

		List<EntityHandle> handles = _ecs.EntityHandles(res.Entities.ToArray());

		LoggerManager.LogDebug("Query result", "", "res", handles);

		Assert.Equal(3, res.Entities.Count);
		Assert.Contains(_entities["TestData"], res.Entities);
		Assert.Contains(_entities["TestData2"], res.Entities);
		Assert.Contains(_entities["TestData3"], res.Entities);
	}
	
	[Fact]
	public void ECSv4QueryBuilderTests_query_inand()
	{
		// create a query
		Query query = QueryBuilder.Create()
			.InAnd(_ecs.GetEntityArchetype(_entities["e1"]))
			.Build();

		// run the query and get results
		QueryEntities res = _ecs.Query(query);

		List<EntityHandle> handles = _ecs.EntityHandles(res.Entities.ToArray());

		LoggerManager.LogDebug("Query result", "", "res", handles);

		Assert.Equal(3, res.Entities.Count);
		Assert.Contains(_entities["e1"], res.Entities);
		Assert.Contains(_entities["e2"], res.Entities);
		Assert.Contains(_entities["e6"], res.Entities);
	}

	[Fact]
	public void ECSv4QueryBuilderTests_query_wildcard()
	{
		// create a query
		Query query = QueryBuilder.Create()
			.Has(_entities["TestTag"])
			.Has(Entity.CreateFrom(0)) // matches basically any other component
			.Build();

		// run the query and get results
		QueryEntities res = _ecs.Query(query);

		List<EntityHandle> handles = _ecs.EntityHandles(res.Entities.ToArray());

		LoggerManager.LogDebug("Query result", "", "res", handles);

		Assert.Equal(3, res.Entities.Count);
		Assert.Contains(_entities["e2"], res.Entities);
	}
}

public struct TestEntity : ITagComponent {public static int Id { get; set; }}
