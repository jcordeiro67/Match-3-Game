using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour {

	public GameObject clearFXPrefab;
	public GameObject breakFXPrefab;
	public GameObject doubleBreakFXPrefab;

	public void ClearPieceFXAt(int x, int y, int z=0){

		if (clearFXPrefab != null) {
			GameObject clearFX = Instantiate(clearFXPrefab, new Vector3(x, y, z), Quaternion.identity) as GameObject;

			ParticlePlayer particlePlayer = clearFX.GetComponent<ParticlePlayer>();
			if (particlePlayer != null) {
				particlePlayer.Play();
			}
		}
	}

	//Determines which breakable fx prefab to play acording to breakable value
	public void BreakTileFXAt(int breakableValue, int x, int y, int z=0){

		GameObject breakFx = null;
		ParticlePlayer particlePlayer = null;

		// if breakable value greater then 1 play doubleBreakFXPrefab
		// else play breakFXPrefab as breakFx
		if (breakableValue > 1) {
			if (doubleBreakFXPrefab != null) {
				breakFx = Instantiate(doubleBreakFXPrefab, new Vector3(x, y, z), Quaternion.identity) as GameObject;
			}
			
		} else {
			if (breakFXPrefab != null) {
				breakFx = Instantiate(breakFXPrefab, new Vector3(x, y, z), Quaternion.identity) as GameObject;
			}
		}

		if (breakFx != null) {
			particlePlayer = breakFx.GetComponent<ParticlePlayer>();
			if ( particlePlayer != null) {
				particlePlayer.Play();
			}
		}
	}
}
