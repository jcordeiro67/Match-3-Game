using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager> {

	public int movesLeft = 30;
	public int scoreGoal = 10000;
	public ScreenFader screenFader;
	public Text levelNameText;
	public Text movesLeftText;

	public MessageWindow messageWindow;

	public Sprite loseIcon;
	public Sprite winIcon;
	public Sprite goalIcon;

	private GameBoard m_board;
	private Scene scene;
	private bool m_isReadyToBegin = false;
	private bool m_isGameOver = false;
	private bool m_isWinner = false;
	private bool m_isReadyToReload = false;

	// Use this for initialization
	void Start () {
	 
		// Find the gameboard and get the gameboard component
		m_board = GameObject.FindObjectOfType<GameBoard>().GetComponent<GameBoard>();

		// set the currentScene to the active scene
		scene = SceneManager.GetActiveScene();

		// if levelNameText not an empty string set the active scene name as the level text
		if (levelNameText != null) {
			levelNameText.text = scene.name;
		}

		UpdateMoves();

		StartCoroutine("ExecuteGameLoop");

	}

	public void UpdateMoves(){

		if (movesLeftText != null) {
			movesLeftText.text = movesLeft.ToString();
		}
	}
	
	IEnumerator ExecuteGameLoop(){

		yield return StartCoroutine("StartGameRoutine");
		yield return StartCoroutine("PlayGameRoutine");
		yield return StartCoroutine("EndGameRoutine");
	}

	public void BeginGame(){

		m_isReadyToBegin = true;
	}

	IEnumerator StartGameRoutine(){

		if (messageWindow != null) {
			messageWindow.GetComponent<RectXformMover>().MoveOn();
			messageWindow.ShowMessage(goalIcon, "SCORE GOAL\n" + scoreGoal.ToString(), scene.name, "Play!");
		}

		while (!m_isReadyToBegin) {
			yield return null;
		}

		// fade out overlay screne
		if(screenFader != null){
			screenFader.FadeOff();
		}

		yield return new WaitForSeconds(0.5f);
		// setup gameboard
		if (m_board != null) {
			m_board.SetupBoard();
		}

	}

	IEnumerator PlayGameRoutine(){
		
		// if isGameOver is false loop until
		// movesLeft reaches 0 and set isGameOver to true and isWinner to false
		while (!m_isGameOver) {
			// check if scoreGoal is met
			if (ScoreManager.Instance != null) {
				if (ScoreManager.Instance.CurrentScore >= scoreGoal) {
					m_isGameOver = true;
					m_isWinner = true;
				}
			}
			// check if no moves left
			if (movesLeft == 0) {
				m_isGameOver = true;
				m_isWinner = false;
			}

			yield return null;
		}
	}

	IEnumerator EndGameRoutine(){

		m_isReadyToReload = false;
		while (!m_board.PlayerInputEnabled) {
			yield return null;
		}

		yield return new WaitForSeconds(1f);
		int currentScore = ScoreManager.Instance.CurrentScore;

		// WINNER
		if (m_isWinner) {
			
			if (messageWindow != null) {
				
				messageWindow.GetComponent<RectXformMover>().MoveOn();
				messageWindow.ShowMessage(winIcon, "YOU WON!\n" + currentScore.ToString(), scene.name, "Next Level");

			}
		} else {
			// LOOSER
			if (messageWindow != null) {
				
				messageWindow.GetComponent<RectXformMover>().MoveOn();
				messageWindow.ShowMessage(loseIcon, "YOU LOST!\n" + currentScore.ToString(), scene.name, "Try Again!");

			}
		}

		//yield return new WaitForSeconds(2f);
		// fade overlay screne
		if(screenFader != null){
			screenFader.FadeOn();
		}
		while (!m_isReadyToReload) {
			yield return null;
		}

		SceneManager.LoadScene(scene.name);

	}

	public void ReloadScene(){
		m_isReadyToReload = true;
	}


}
