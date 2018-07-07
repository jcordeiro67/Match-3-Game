using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : GamePiece {

	public bool clearedByBomb = false;
	public bool clearedByBottom = true;

	void Start() {
		matchValue = MatchValue.None;
	}

}
