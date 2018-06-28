using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GamePiece : MonoBehaviour {

	public int xIndex;
	public int yIndex;

	public InterpType interpolation = InterpType.SmootherStep;
	public MatchValue matchValue;

	public Color matchValueRBG;

	private GameBoard m_board;
	private bool m_isMoving = false;


	public enum InterpType{
		Linear,
		EaseOut,
		EaseIn,
		SmoothStep,
		SmootherStep
	}

	public enum MatchValue{
		Blue,
		Cyan,
		Green,
		Magenta,
		Orange,
		Pink,
		Purple,
		Red,
		Teal,
		White,
		Yellow,
		Brown,
		Wild
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

//		if(Input.GetKeyDown(KeyCode.RightArrow)){
//
//			Move((int)transform.position.x +1, (int)transform.position.y, 0.5f);
//		}
//
//		if(Input.GetKeyDown(KeyCode.LeftArrow)){
//
//			Move((int)transform.position.x -1, (int)transform.position.y, 0.5f);
//		}
//
//		if(Input.GetKeyDown(KeyCode.UpArrow)){
//
//			Move((int)transform.position.x, (int)transform.position.y +3, 0.5f);
//		}
//
//		if(Input.GetKeyDown(KeyCode.DownArrow)){
//
//			Move((int)transform.position.x, (int)transform.position.y -3, 0.5f);
//		}
	}

	public void Init(GameBoard board){

		m_board = board;
	}
		
	/// <summary>
	/// Sets the coordinate.
	/// </summary>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	public void SetCoord(int x, int y){

		xIndex = x;
		yIndex = y;

	}

	/// <summary>
	/// Move the specified destX, destY and timeToMove.
	/// </summary>
	/// <param name="destX">Destination x.</param>
	/// <param name="destY">Destination y.</param>
	/// <param name="timeToMove">Time to move.</param>
	public void Move(int destX, int destY, float timeToMove){

		if (!m_isMoving) {
			StartCoroutine(MoveRoutine(new Vector3(destX, destY, 0), timeToMove));
		}

	}

	/// <summary>
	/// Moves the gamePiece.
	/// </summary>
	/// <returns>Back to The routine.</returns>
	/// <param name="destination">Destination.</param>
	/// <param name="timeToMove">Time to move.</param>
	IEnumerator MoveRoutine(Vector3 destination, float timeToMove){

		Vector3 startPos = transform.position;
		bool reachedDestination = false;

		float elapsedTime = 0f;
		m_isMoving = true;


		while (!reachedDestination) {
			// if piece is close to destination
			if (Vector3.Distance(transform.position, destination) < 0.01f) {
				
				reachedDestination = true;

				if (m_board != null) {
					m_board.PlaceGamePiece(this, (int)destination.x, (int)destination.y);
				}
				break;

			}

			// track the total running time for the piece
			elapsedTime += Time.deltaTime;

			// calculate the lerp value
			float t = Mathf.Clamp(elapsedTime / timeToMove, 0f, 1f);

			// Set the Interpolation of the lerp curve
			switch (interpolation) {
				
				case InterpType.Linear:
					break;

				case InterpType.EaseIn:
					t = 1 - Mathf.Cos(t * Mathf.PI * 0.5f);	// Cosine Ease in Curve
					break;

				case InterpType.EaseOut:
					t = Mathf.Sin(t * Mathf.PI * 0.5f);	// Sine Ease out curve
					break;

				case InterpType.SmoothStep:
					t = t * t * (3 - 2 * t);	// SmoothStep Method
					break;

				case InterpType.SmootherStep:
					t = t * t * t * (t * (t * 6 - 15) + 10);	//SmootherStep Method
					break;
			}

			// move the game piece
			transform.position = Vector3.Lerp(startPos, destination, t);

			// wait for next frame
			yield return null;
		}

		m_isMoving = false;
	}


	public void ChangeColor(GamePiece pieceToMatch){
		
		SpriteRenderer rendererToChange = GetComponent<SpriteRenderer>();

		Color colorToMatch = Color.clear;

		if (pieceToMatch != null) {
			
			SpriteRenderer rendererToMatch = pieceToMatch.GetComponent<SpriteRenderer>();

			if (rendererToMatch != null && rendererToChange != null) {
				
				rendererToChange.color = rendererToMatch.color;
			}
			matchValue = pieceToMatch.matchValue;
		}
	}

	public void ChangeSprite(GamePiece pieceToMatch){

		Sprite spriteToChange = null;

		SpriteRenderer rendererToChange = GetComponent<SpriteRenderer>();

		if(pieceToMatch != null){

			SpriteRenderer rendererToMatch = pieceToMatch.GetComponent<SpriteRenderer>();

			if (rendererToMatch != null && rendererToChange != null) {
				
				rendererToChange.sprite = rendererToMatch.sprite;
			}

			matchValue = pieceToMatch.matchValue;
		}
	}
}
