using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileType{

	Normal,
	Obstacle,
	Breakable
}

[RequireComponent(typeof(SpriteRenderer))]

public class TileScript : MonoBehaviour {

	public int xIndex;
	public int yIndex;

	public TileType tileType = TileType.Normal;

	public int breakableValue = 0;
	public Sprite[] breakableSprites;

	[SerializeField] private GameBoard m_board;

	private SpriteRenderer m_spriteRenderer;

	void Awake(){
		m_spriteRenderer = GetComponent<SpriteRenderer>();
	}

	public void Init(int x, int y, GameBoard board){

		xIndex = x;
		yIndex = y;
		m_board = board;

		if (tileType == TileType.Breakable) {
			if (breakableSprites[breakableValue] != null) {
				m_spriteRenderer.sprite = breakableSprites[breakableValue];
			}
		}
	}

	void OnMouseDown(){
		if (m_board != null) {
			m_board.ClickTile(this);
		}
	}

	void OnMouseEnter(){
		if (m_board != null) {
			m_board.DragToTile(this);
		}
	}

	void OnMouseUp(){

		if (m_board != null) {
			m_board.ReleaseTile();
		}
	}

	public void BreakTile(){

		if(tileType != TileType.Breakable){
			return;
		}

		StartCoroutine(BreakTileRoutine());
	}

	IEnumerator BreakTileRoutine(){

		breakableValue = Mathf.Clamp(breakableValue--, 0, breakableValue);

		yield return new WaitForSeconds(0.25f);

		if (breakableSprites[breakableValue] != null) {
			m_spriteRenderer.sprite = breakableSprites[breakableValue];
		}

		if (breakableValue == 0) {

			tileType = TileType.Normal;
			// Either change spriteRenderer color or swap sprite with normal tile from m_board
			m_spriteRenderer.sprite = m_board.tileNormalPrefab.GetComponent<SpriteRenderer>().sprite;
			m_spriteRenderer.color = new Color(m_spriteRenderer.color.r, m_spriteRenderer.color.g, m_spriteRenderer.color.b, 0);
		}
	}
}
