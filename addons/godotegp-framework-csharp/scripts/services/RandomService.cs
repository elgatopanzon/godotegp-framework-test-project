namespace Godot.EGP;

using Godot;
using System;
using System.Collections.Generic;

public partial class RandomService : Service
{
	private Dictionary<string, RandomNumberGeneratorExtended> _randomInstances = new Dictionary<string, RandomNumberGeneratorExtended>();

	public RandomNumberGeneratorExtended Get(string instanceName = "default")
	{
		if (!_randomInstances.TryGetValue(instanceName, out RandomNumberGeneratorExtended randomInstance))
		{
			// instead of throwing an error, we just create a basic instance
			randomInstance = _CreateRandomInstance();

			RegisterInstance(randomInstance, instanceName);
		}

		return randomInstance;
	}

	private RandomNumberGeneratorExtended _CreateRandomInstance(ulong seed = 0, ulong state = 0)
	{
		return new RandomNumberGeneratorExtended(seed, state);
	}

	public RandomNumberGeneratorExtended RegisterInstance(RandomNumberGeneratorExtended randomInstance, string instanceName)
	{
		if (!_randomInstances.TryAdd(instanceName, randomInstance))
		{
			throw new RandomInstanceExistsException($"Instance with name {instanceName} already exists");		
		}

		LoggerManager.LogDebug("Registering new instance", "", "instance", instanceName);

		return randomInstance;
	}
}

public class RandomInstanceExistsException : Exception
{
	public RandomInstanceExistsException(string message) : base(message)
	{
		
	}
}
