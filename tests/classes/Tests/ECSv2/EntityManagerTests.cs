/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : EntityManagerTests
 * @created     : Saturday Apr 27, 2024 22:31:09 CST
 */

namespace GodotEGP.Tests.ECSv2;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using GodotEGP.ECSv2;

public partial class EntityManagerTests : TestContext
{
	[Fact]
	public void EntityManagerTests_test_entity_id_recycling()
	{
		var entityManager = new EntityManager();

		// create new id
		Entity entity = entityManager.Create();

		LoggerManager.LogDebug("Initial id", "", "entity", entity);
		LoggerManager.LogDebug("Initial version", "", "version", entity.Version);

		entityManager.Destroy(entity.RawId);
		entity = entityManager.Create();

		LoggerManager.LogDebug("Recycled id", "", "entity", entity);
		LoggerManager.LogDebug("Recycled id int", "", "id", entity.Id);
		LoggerManager.LogDebug("Recycled version", "", "version", entity.Version);

		entity++;

		LoggerManager.LogDebug("Recycled id ++", "", "entity", entity);
		LoggerManager.LogDebug("Recycled version ++", "", "version", entity.Version);

		Assert.True(((ulong) 8589934593 == (ulong) entity.RawId));
		Assert.True((1 == entity.Id));
	}

	[Fact]
	public void EntityManagerTests_test_entity_pair_id()
	{
		var entityManager = new EntityManager();

		// create new id
		Entity entity = entityManager.Create();

		LoggerManager.LogDebug("Initial id", "", "id", entity);

		uint pairId = 12345;

		LoggerManager.LogDebug("Add pair id", "", "pairId", pairId);

		entity.PairId = pairId;

		LoggerManager.LogDebug("Id with pair id", "", "id", entity);
		LoggerManager.LogDebug("Left id", "", "leftId", entity.Id);
		LoggerManager.LogDebug("Right id", "", "rightId", entity.PairId);

		Assert.True((1 == entity.Id));
		Assert.True((pairId == entity.PairId));
	}

	[Fact]
	public void EntityManagerTests_test_entity_archetypes()
	{
		var entityManager = new EntityManager();

		// create new id
		Entity entity = entityManager.Create();
		Entity componentId = entityManager.Create();

		entityManager.AddArchetypeId(entity, componentId);

		var archetype = entityManager.GetArchetype(entity);

		LoggerManager.LogDebug("Archetype", "", "archetype", archetype);

		entityManager.RemoveArchetypeId(entity, componentId);
		archetype = entityManager.GetArchetype(entity);

		LoggerManager.LogDebug("Archetype", "", "archetype", archetype);

		Assert.Empty(archetype.ArraySegment);
	}
}

