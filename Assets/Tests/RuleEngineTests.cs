// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using SharpUnit;
using System.Data;
using Mono.Data.Sqlite;

public class RuleEngineTests : TestCase
{
	IFGameController gameController;

	public override void SetUp()
	{
		gameController = IFGameController.Create("Game for Test");
	}
	
	public override void TearDown()
	{
		if(gameController.questionController != null) {
			NGUITools.Destroy(gameController.questionController.gameObject);
		}
		if(gameController.questionHUDController != null) {
			NGUITools.Destroy(gameController.questionHUDController.gameObject);	
		}
		NGUITools.Destroy(gameController.gameObject);
	}
	
	[UnitTest]
	public void TestStartNewGameInNormalMode()
	{
		IFGameLevel level = new IFGameLevel(1, 4, 5f);
		gameController.StartNewGame(level, IFGame.GameMode.Normal);
		
		Assert.Equals(level, gameController.Game.Level);
	}
	
}

