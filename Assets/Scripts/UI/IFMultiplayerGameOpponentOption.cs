// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

using UnityEngine;
using System.Collections;

public class IFMultiplayerGameOpponentOption : MonoBehaviour {
	
	public enum OpponentType
	{
		Automatch,
		Facebook,
		Username,
		Local
	};
	public OpponentType opponentType;
	
}
