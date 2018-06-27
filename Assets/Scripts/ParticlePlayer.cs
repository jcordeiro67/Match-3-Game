using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlePlayer : MonoBehaviour {

	public ParticleSystem[] allParticles;
	public float lifeTime = 1;

	// Use this for initialization
	void Start () {
		
		allParticles = GetComponentsInChildren<ParticleSystem>();
		Destroy(gameObject, lifeTime);
	}

	// Plays each particleSystem
	public void Play(){
		foreach (ParticleSystem ps in allParticles) {

			ps.Stop();
			ps.Play();
		}
	}
}
