using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectXformMover))]
public class MessageWindow : MonoBehaviour {

	public Text levelTextTxt;
	public Text messageText;
	public Image messageIcon;
	public Text buttonText;

	public void ShowMessage(Sprite sprite = null, string message = "", string levelName = "", string buttonMsg = "Play!")
	{

		if (messageIcon != null) {
			messageIcon.sprite = sprite;
		}

		if (messageText != null) {
			messageText.text = message;
		}

		if (levelTextTxt != null) {
			levelTextTxt.text = levelName;
		}

		if (buttonText != null) {
			buttonText.text = buttonMsg;
		}
	}
}
