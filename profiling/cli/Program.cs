namespace GodotEGP.Profiling.CLI;

using System;

using GodotEGPNonGame.ServiceWorkers;
using GodotEGP.Logging;

using GodotEGP;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Random;
using GodotEGP.Objects.Extensions;
using GodotEGP.Event.Events;
using GodotEGP.Event.Filters;
using Godot;

using GodotEGP.Profiling.CLI.ECSv3;
using GodotEGP.Profiling.CLI.ECSv4;

using GodotEGP.Collections;
using System.Diagnostics;

internal class Program
{
	public static GodotEGP.Main GodotEGP;

    static async Task<int> Main(string[] args)
    {
		// init GodotEGP
		GodotEGP = new GodotEGP.Main();
		SceneTree.Instance.Root.AddChild(GodotEGP);

		// wait for services to be ready
		if (!ServiceRegistry.WaitForServices(
					typeof(EventManager) 
					))
			{
			LoggerManager.LogCritical("Required services never became ready");

			return 0;
		}

		LoggerManager.LogInfo("Services ready");

		// create SceneTree service worker instance
		var serviceWorker = new SceneTreeServiceWorker();
		await serviceWorker.StartAsync(new CancellationToken());

		LoggerManager.LogInfo("GodotEGP ready!");

    	if (args.Length <= 1)
    	{
    		throw new ArgumentException("Must include a valid profiler test!");
    	}

    	string profileName = args[0];
    	string profileSubName = args[1];

    	ProfilingContext profile = null;

    	// run different profiles based on given argument
    	switch (profileName)
    	{
    		case "ECSv3":
    			switch (profileSubName)
    			{
					case "update_1":
						profile = new ECSv3Profile_Update_6(1) {
						};
						break;
					case "update_2":
						profile = new ECSv3Profile_Update_6(2) {
						};
						break;
					case "update_8":
						profile = new ECSv3Profile_Update_6(8) {
						};
						break;
					case "update_16":
						profile = new ECSv3Profile_Update_6(16) {
						};
						break;
					case "update_32":
						profile = new ECSv3Profile_Update_6(32) {
						};
						break;
					case "update_64":
						profile = new ECSv3Profile_Update_6(64) {
						};
						break;
					case "update_128":
						profile = new ECSv3Profile_Update_6(128) {
						};
						break;
					case "update_256":
						profile = new ECSv3Profile_Update_6(256) {
						};
						break;
					case "update_1k":
						profile = new ECSv3Profile_Update_6(1000) {
						};
						break;
					case "update_4k":
						profile = new ECSv3Profile_Update_6(4000) {
						};
						break;
					case "update_16k":
						profile = new ECSv3Profile_Update_6(16000) {
						};
						break;
					case "update_65k":
						profile = new ECSv3Profile_Update_6(65000) {
						};
						break;
					case "update_262k":
						profile = new ECSv3Profile_Update_6(262000) {
						};
						break;
					case "update_1m":
						profile = new ECSv3Profile_Update_6(1000000) {
						};
						break;
					case "update_2m":
						profile = new ECSv3Profile_Update_6(2000000) {
						};
						break;
    				default:
    					break;
    			}
    			break;
    		case "ECSv4":
    			switch (profileSubName)
    			{
					case "update_1":
						profile = new ECSv4Profile_Update_6(1) {
						};
						break;
					case "update_2":
						profile = new ECSv4Profile_Update_6(2) {
						};
						break;
					case "update_8":
						profile = new ECSv4Profile_Update_6(8) {
						};
						break;
					case "update_16":
						profile = new ECSv4Profile_Update_6(16) {
						};
						break;
					case "update_32":
						profile = new ECSv4Profile_Update_6(32) {
						};
						break;
					case "update_64":
						profile = new ECSv4Profile_Update_6(64) {
						};
						break;
					case "update_128":
						profile = new ECSv4Profile_Update_6(128) {
						};
						break;
					case "update_256":
						profile = new ECSv4Profile_Update_6(256) {
						};
						break;
					case "update_1k":
						profile = new ECSv4Profile_Update_6(1000) {
						};
						break;
					case "update_4k":
						profile = new ECSv4Profile_Update_6(4000) {
						};
						break;
					case "update_16k":
						profile = new ECSv4Profile_Update_6(16000) {
						};
						break;
					case "update_65k":
						profile = new ECSv4Profile_Update_6(65000) {
						};
						break;
					case "update_262k":
						profile = new ECSv4Profile_Update_6(262000) {
						};
						break;
					case "update_1m":
						profile = new ECSv4Profile_Update_6(1000000) {
						};
						break;
					case "update_2m":
						profile = new ECSv4Profile_Update_6(2000000) {
						};
						break;
    				default:
    					break;
    			}
    			break;
    		case "noecs":
    			NoEcs noEcs;

    			switch (profileSubName)
    			{
					case "update_1":
						noEcs = new NoEcs(1) {
						};
						break;
					case "update_2":
						noEcs = new NoEcs(2) {
						};
						break;
					case "update_8":
						noEcs = new NoEcs(8) {
						};
						break;
					case "update_16":
						noEcs = new NoEcs(16) {
						};
						break;
					case "update_32":
						noEcs = new NoEcs(32) {
						};
						break;
					case "update_64":
						noEcs = new NoEcs(64) {
						};
						break;
					case "update_128":
						noEcs = new NoEcs(128) {
						};
						break;
					case "update_256":
						noEcs = new NoEcs(256) {
						};
						break;
					case "update_1k":
						noEcs = new NoEcs(1000) {
						};
						break;
					case "update_4k":
						noEcs = new NoEcs(4000) {
						};
						break;
					case "update_16k":
						noEcs = new NoEcs(16000) {
						};
						break;
					case "update_65k":
						noEcs = new NoEcs(65000) {
						};
						break;
					case "update_262k":
						noEcs = new NoEcs(262000) {
						};
						break;
					case "update_1m":
						noEcs = new NoEcs(1000000) {
						};
						break;
					case "update_2m":
						noEcs = new NoEcs(2000000) {
						};
						break;
    				default:
    					break;
    			}
    			break;

    		default:
    			break;
    	}

    	throw new Exception("Not a valid profile!");

		return 0;
    }
}

public class NoEcs
{
	public ulong Entities { get; set; }

	public ECSv4.Position[] _position { get; set; }
	public ECSv4.Velocity[] _velocity { get; set; }
	public ECSv4.DataComponent[] _dataComponent { get; set; }
	public ECSv4.Health[] _health { get; set; }
	public ECSv4.Damage[] _damage { get; set; }
	public ECSv4.Sprite[] _sprite { get; set; }

	public NoEcs(ulong entities)
	{
		Entities = entities;

		_position = new ECSv4.Position[(int) entities];
		_velocity = new ECSv4.Velocity[(int) entities];
		_dataComponent = new ECSv4.DataComponent[(int) entities];
		_health = new ECSv4.Health[(int) entities];
		_damage = new ECSv4.Damage[(int) entities];
		_sprite = new ECSv4.Sprite[(int) entities];

		Setup();
		Run();
	}

	public void Setup()
	{
		for (int i = 0; i < (int) Entities; i++)
		{
			_position[i] = new ECSv4.Position() {
			};

			_velocity[i] = new ECSv4.Velocity() {
			};

			_dataComponent[i] = new ECSv4.DataComponent() {
			};

			_health[i] = new ECSv4.Health() {
			};

			_damage[i] = new ECSv4.Damage() {
			};

			_sprite[i] = new ECSv4.Sprite() {
			};
		}
	}

	public void Run()
	{
		DateTime lastUpdate = DateTime.Now;
		float deltaTime = 0;

		DateTime lastFrameCount = DateTime.Now;
		int frames = 0;
		int fps = 0;

		PackedArray<int> fpsSamples = new();

		Stopwatch stopWatch = new Stopwatch();

		float elapsedTime = 0;

		while (true)
		{
			DateTime timeNow = DateTime.Now;
			deltaTime = (timeNow.Ticks - lastUpdate.Ticks) / 10000000f;
			// stopWatch.Start();

			// do update logic
			for (int i = 0; i < (int) Entities; i++)
			{
				ECSv4.Position position = _position[i];
				ECSv4.Velocity velocity = _velocity[i];
				ECSv4.Health health = _health[i];
				ECSv4.Damage damage = _damage[i];
				ECSv4.DataComponent dataComponent = _dataComponent[i];;
				ECSv4.Sprite sprite = _sprite[i];


				/*********************
				*  DirectionSystem  *
				*********************/
				if (dataComponent.RandomInt % 10 == 0)
				{
					if (position.X > position.Y)
					{
						velocity.X = Random.Shared.Next(3, 19) - 10;
						velocity.Y = Random.Shared.Next(0, 5);
					}
					else
					{
						velocity.X = Random.Shared.Next(0, 5);
						velocity.Y = Random.Shared.Next(3, 19) - 10;
					}
				}


				/********************
				*  MovementSystem  *
				********************/
				position.X += (velocity.X * deltaTime);
				position.Y += (velocity.Y * deltaTime);

				/******************
				*  HealthSystem  *
				******************/
				if (health.Hp <= 0 && health.Status != 0) 
				{
      				health.Hp = 0;
      				health.Status = 0;
    			} 
    			else if (health.Status == 0 && health.Hp == 0)
    			{
      				health.Hp = health.HpMax;
      				health.Status = 1;
    			} 
    			else if (health.Hp >= health.HpMax && health.Status != 2)
    			{
      				health.Hp = health.HpMax;
      				health.Status = 2;
    			} 
    			else 
    			{
      				health.Status = 2;
    			}
				

				/******************
				*  DamageSystem  *
				******************/
				int total = damage.Attack - damage.Defense;
				if (total <= 0)
				{
					damage.Attack = Random.Shared.Next(10, 40);
					damage.Defense = Random.Shared.Next(10, 40);
					health.HpMax = Random.Shared.Next(100, 200);
				}

				if (health.Hp > 0 && total > 0)
				{
					health.Hp = Math.Max(0, health.Hp - total);
				}


				/****************
				*  DataSystem  *
				****************/
				dataComponent.RandomInt = Random.Shared.Next();
				dataComponent.RandomDouble = Random.Shared.NextDouble();

				/******************
				*  SpriteSystem  *
				******************/
				switch (health.Status)
				{
					case 0:
						sprite.SpriteId = '_';
						break;
					case 1:
						sprite.SpriteId = '/';
						break;
					case 2:
						if (health.Hp == health.HpMax)
						{
							sprite.SpriteId = '+';
						}
						else {
							sprite.SpriteId = '|';
						}
						break;
					default:
						break;
				}


				// write components back in case they changed
				_position[i] = position;
				_velocity[i] = velocity;
				_health[i] = health;
				_damage[i] = damage;
				_dataComponent[i] = dataComponent;
				_sprite[i] = sprite;
			}

			// end update logic

			// deltaTime = ((float) stopWatch.ElapsedMilliseconds) / 1000f;
			elapsedTime += deltaTime;

			lastUpdate = timeNow;

			// updates fps
			frames++;

			if ((timeNow - lastFrameCount).TotalSeconds >= 1)
			{
				fps = frames;
				fpsSamples.Add(fps);

				LoggerManager.LogInfo("FPS", "", "fps", $"{fps} @ {Entities.ToString()}e (avg:{Convert.ToInt32(fpsSamples.Span.ToArray().TakeLast(50).Average())}) [({deltaTime * 1000}ms) ({deltaTime * 1000000}us) ({deltaTime * 1000000000}ns)] cpe:{(int) ((deltaTime * 1000000000) / Entities)}ns)");
				// Console.WriteLine($"FPS fps={fps} @ {Entities.ToString()}e (avg:{Convert.ToInt32(fpsSamples.Span.ToArray().TakeLast(50).Average())}) [({deltaTime * 1000}ms) ({deltaTime * 1000000}us) ({deltaTime * 1000000000}ns)] cpe:{(int) ((deltaTime * 1000000000) / Entities)}ns)");

				frames = 0;

				lastFrameCount = timeNow;
				lastUpdate = lastFrameCount;
			}

		}
	}
}
