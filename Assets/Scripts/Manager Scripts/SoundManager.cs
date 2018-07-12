using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : Singleton<SoundManager> {

	public AudioClip[] musicClips;
	public AudioClip[] winClips;
	public AudioClip[] loosClips;
	public AudioClip[] bonusClips;
	public AudioClip[] tileClips;
	public AudioClip[] pieceClips;
	public AudioClip[] pointsClips;
	public AudioClip[] clickClips;

	[Range(0,1)]
	public float musicVolume = 0.5f;

	[Range(0,1)]
	public float fxVolume = 1.0f;

	public float lowPitch = 0.95f;
	public float highPitch = 1.05f;
	// Use this for initialization
	void Start () {

		PlayRandomMusic();
	}

	public AudioSource PlayClipAtPoint(AudioClip clip, Vector3 position, float volume = 1f, bool loop = false){

		if (clip != null) {
			GameObject go = new GameObject("SoundFX" + clip.name);
			go.transform.position = position;

			AudioSource source = go.AddComponent<AudioSource>();
			source.clip = clip;

			float randomPitch = Random.Range(lowPitch, highPitch);
			source.pitch = randomPitch;

			if (loop) {
				source.loop = true;
			}

			source.volume = volume;
			source.Play();
			Destroy(go, clip.length);
			return source;
		}

		return null;
	}

	public AudioSource PlayRandom(AudioClip[] clips, Vector3 position, float volume = 1f, bool loop = false){

		if (clips != null) {
			if (clips.Length != 0) {

				int randomIndex = Random.Range(0, clips.Length);

				if (clips[randomIndex] != null) {
					AudioSource source = PlayClipAtPoint(clips[randomIndex], position, volume, loop);
					return source;
				}
			}
		}
		return null;
	}

	public void PlayClickClip(){

		PlayRandom(clickClips, Vector3.zero, fxVolume);
	}

	public void PlayRandomMusic(){

		PlayRandom(musicClips, Vector3.zero, musicVolume, true);

	}

	public void PlayRandomWinClip(){

		PlayRandom(winClips, Vector3.zero, fxVolume);
	}

	public void PlayRandomLooseClip(){

		PlayRandom(loosClips, Vector3.zero, fxVolume);
	}

	public void PlayRandomBonusClip(){

		PlayRandom(bonusClips, Vector3.zero, fxVolume);
	}

	public void PlayRandomPointsClip(){

		PlayRandom(pointsClips, Vector3.zero, fxVolume);
	}

	public void PlayRandomTileClip(int x, int y){

		Vector3 tilePosition = new Vector3(x, y, 0f);
		PlayRandom(tileClips, tilePosition, fxVolume);
	}

	public void PlayRandomPieceClip(int x, int y){

		Vector3 piecePosition = new Vector3(x, y, 0f);
		PlayRandom(pieceClips, piecePosition, fxVolume);
	}

}
