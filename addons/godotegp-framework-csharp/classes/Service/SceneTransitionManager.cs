/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : SceneTransitionManager
 * @created     : Sunday Nov 12, 2023 18:46:04 CST
 */

namespace GodotEGP.Service;

using System;
using System.Collections.Generic;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

public partial class SceneTransitionManager : Service
{
	ScreenTransitionManager _transitionManager = ServiceRegistry.Get<ScreenTransitionManager>();
	SceneManager _sceneManager = ServiceRegistry.Get<SceneManager>();

	public SceneTransitionManager()
	{
		
	}

	/*******************
	*  Godot methods  *
	*******************/
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_SetServiceReady(true);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}


	/*********************
	*  Service methods  *
	*********************/

	// Called when service is registered in manager
	public override void _OnServiceRegistered()
	{
	}

	// Called when service is deregistered from manager
	public override void _OnServiceDeregistered()
	{
		// LoggerManager.LogDebug($"Service deregistered!", "", "service", this.GetType().Name);
	}

	// Called when service is considered ready
	public override void _OnServiceReady()
	{
	}

	/*****************************************
	*  Scene transition management methods  *
	*****************************************/
	
	public void TransitionScene(string sceneId, string transitionId, bool autoContinue = true)
	{
		if (_sceneManager.IsValidScene(sceneId) && _transitionManager.IsValidTransitionId(transitionId))
		{
			_transitionManager.StartTransition(transitionId);

			// subscribe to transition events
			_transitionManager.SubscribeOwner<ScreenTransitionStarting>((e) => {
				this.Emit(e);
				}, oneshot: true);

			_transitionManager.SubscribeOwner<ScreenTransitionShown>((e) => {
				this.Emit(e);
				_sceneManager.LoadScene(sceneId);

				_sceneManager.SubscribeOwner<SceneLoaded>((e) => {
						if (autoContinue)
						{
							_transitionManager.ContinueTransition();
						}
						}, oneshot: true);
					});


			_transitionManager.SubscribeOwner<ScreenTransitionFinished>((e) => {
					this.Emit(e);
				}, oneshot: true);
		}
		else
		{
			throw new InvalidSceneTransitionException($"The scene ID {sceneId} or transition ID {transitionId} is invalid!");
		}
	}

	/**********************
	*  Callback methods  *
	**********************/
	

	/****************
	*  Exceptions  *
	****************/
	
	public class InvalidSceneTransitionException : Exception
	{
		public InvalidSceneTransitionException() {}
		public InvalidSceneTransitionException(string message) : base(message) {}
		public InvalidSceneTransitionException(string message, Exception inner) : base(message, inner) {}
	}
}

