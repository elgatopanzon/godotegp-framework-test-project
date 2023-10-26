namespace Godot.EGP;

using Godot;
using Godot.EGP.Extensions;
using System;
using System.Collections.Generic;

public partial class NodeManager : Service
{
	private Dictionary<string, List<Node>> _registeredNodes = new Dictionary<string, List<Node>>();
	
	public override void _Ready()
	{
		if (!GetReady())
		{
			LoggerManager.LogDebug("Setting up signals and events");

			// subscribe to Events related to nodes being added and removed with
			// high priority
			// ServiceRegistry.Get<EventManager>().Subscribe(new EventSubscription<EventNodeAdded>(this, __On_EventNodeAdded, true));
			// ServiceRegistry.Get<EventManager>().Subscribe(new EventSubscription<EventNodeRemoved>(this, __On_EventNodeRemoved, true));
			this.Subscribe<EventNodeAdded>(__On_EventNodeAdded, true);
			this.Subscribe<EventNodeRemoved>(__On_EventNodeRemoved, true);

			// connect to SceneTree node_added and node_removed signals
			GetTree().Connect("node_added", new Callable(this, "__On_Signal_node_added"));
			GetTree().Connect("node_removed", new Callable(this, "__On_Signal_node_removed"));

			// retroactively register existing scene tree nodes
			RegisterExistingNodes();

			_SetServiceReady(true);
		}
	}

	// signal callbacks for node_* events, used as rebroadcasters
	public void __On_Signal_node_added(Node nodeObj)
	{
		ServiceRegistry.Get<EventManager>().Emit(new EventNodeAdded(nodeObj, nodeObj));
	}
	public void __On_Signal_node_removed(Node nodeObj)
	{
		ServiceRegistry.Get<EventManager>().Emit(new EventNodeAdded(nodeObj, nodeObj));
	}

	// process node added and removed Event objects
	public void __On_EventNodeAdded(IEvent eventObj)
	{
		EventNodeAdded e = (EventNodeAdded) eventObj;

		RegisterNode(e.Node, GetNodeID(e.Node));
	}
	public void __On_EventNodeRemoved(IEvent eventObj)
	{
		EventNodeAdded e = (EventNodeAdded) eventObj;
		
		DeregisterNode(e.Node);
	}

	public void RegisterNode(Node node, string nodeId, bool registerGroups = true)
	{
		_registeredNodes.TryAdd(nodeId, new List<Node>());

		if (!_registeredNodes[nodeId].Contains(node))
		{
			_registeredNodes[nodeId].Add(node);

			LoggerManager.LogDebug("Registered node", "", "node", new List<string>() {node.GetType().Name, nodeId});
		}

		if (registerGroups)
		{
			foreach (string group in node.GetGroups())
			{
				RegisterNode(node, $"group_{group}", false);
			}

			RegisterNode(node, node.GetPath(), false);
		}
	}

	public void DeregisterNode(Node node)
	{
		foreach (KeyValuePair<string, List<Node>> nodeList in _registeredNodes)
		{
			nodeList.Value.RemoveAll((n) => {
				if (n.Equals(node))
				{
					_registeredNodes[nodeList.Key].Remove(node);

					LoggerManager.LogDebug("Deregistered node", "", "node", new List<string>() {node.GetType().Name, GetNodeID(node), nodeList.Key});
					return true;
				}

				return false;
			});
		}
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

	public string GetNodeID(Node node)
	{
		if (node.HasMeta("id"))
		{
			return (string) node.GetMeta("id");
		}
		else
		{
			return node.Name;
		}
	}

	public bool TryGetNode(string nodeId, out Node node)
	{
		node = null;

		if (_registeredNodes.TryGetValue(nodeId, out List<Node> nodes))
		{
			if (nodes.Count > 0)
			{
				node = nodes[nodes.Count - 1];
			}

			return true;
		}

		return false;
	}
}
