namespace Godot.EGP;

using Godot;
using Godot.EGP.Extensions;
using System;
using System.Collections.Generic;

public partial class NodeManager : Service
{
	public override void _Process(double delta)
	{
		if (!GetReady())
		{
			LoggerManager.LogDebug("Setting up signals and events");

			// subscribe to Events related to nodes being added and removed with
			// high priority
			ServiceRegistry.Get<EventManager>().Subscribe(new EventSubscription<EventNodeAdded>(this, __On_EventNodeAdded, true));
			ServiceRegistry.Get<EventManager>().Subscribe(new EventSubscription<EventNodeRemoved>(this, __On_EventNodeRemoved, true));
			// connect to SceneTree node_added and node_removed signals
			GetTree().Connect("node_added", new Callable(this, "__On_Signal_node_added"));
			GetTree().Connect("node_removed", new Callable(this, "__On_Signal_node_removed"));

			// retroactively register existing scene tree nodes
			RegisterExistingNodes();

			_SetServiceReady(true);
		}
	}

	// signal callbacks for node_* events, used as rebroadcasters
	public void __On_Signal_node_added(GodotObject nodeObj)
	{
		ServiceRegistry.Get<EventManager>().Emit(new EventNodeAdded(this, nodeObj));
	}
	public void __On_Signal_node_removed(GodotObject nodeObj)
	{
		ServiceRegistry.Get<EventManager>().Emit(new EventNodeAdded(this, nodeObj));
	}

	// process node added and removed Event objects
	public void __On_EventNodeAdded(IEvent eventObj)
	{
		LoggerManager.LogDebug("TODO: register the added node", "", "event", eventObj.ToStringDictionary());
	}
	public void __On_EventNodeRemoved(IEvent eventObj)
	{
		LoggerManager.LogDebug("TODO: deregister the removed node", "", "event", eventObj.ToStringDictionary());
	}

	public void RegisterExistingNodes()
	{
		foreach (Node existingNode in GetSceneTreeNodes())
		{
			__On_Signal_node_added(existingNode);
		}
	}

	public List<Node> GetSceneTreeNodes(Node rootNode = null, List<Node> nodesArray = null)
	{
		if (rootNode == null)
		{
			rootNode = GetTree().Root;
			nodesArray = new List<Node>();
		}

		nodesArray.Add(rootNode);

		foreach (Node childNode in rootNode.GetChildren())
		{
			GetSceneTreeNodes(childNode, nodesArray);
		}

		return nodesArray;
	}

	// func add_scene_tree_nodes():
	// 	for node in get_all_scene_tree_nodes():
	// 		if get_node_id(node):
	// 			_on_node_added(node)
    //
	// func get_all_scene_tree_nodes(node = get_tree().root, all_nodes = []):
	// 	all_nodes.append(node)
	// 	for chid_node in node.get_children():
	// 		get_all_scene_tree_nodes(chid_node, all_nodes)
	// 	return all_nodes
}
