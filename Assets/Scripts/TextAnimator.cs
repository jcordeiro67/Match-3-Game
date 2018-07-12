using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Timeline;

[RequireComponent(typeof(Text))]
public class TextAnimator : MonoBehaviour {

	private Text text;

	void Awake(){

		text = gameObject.GetComponent<Text>();

	}

	public void SetPointText(int score){

		text.text = score.ToString();
	}

}
