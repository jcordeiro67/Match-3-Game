﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameBoard : MonoBehaviour {

	public int width;						// Width of the gameboard in spaces
	public int height;						// Height of gameboard in spaces
	public int borderSize;					// Border around gameboard for orthoSize
	public float swapTime = 0.5f;			// Time to complete swaping gamePieces
	public int minSearchPieces = 3;			// Number of spaces to search for matches
	public GameObject tileNormalPrefab;		// Prefab gameboard tile piece
	public GameObject tileObstaclePrefab;	// Prefab Obstacle tile piece

	public GameObject[] gamePiecePrefabs;	// Array to hold prefabs to be used in the level


	private TileScript[,] m_allTiles;		// Array to hold cordinates of boardTiles
	private GamePiece[,] m_allGamePieces;	// Array to hold cordinates of gamePieces

	private TileScript m_clickedTile;
	private TileScript m_targetTile;

	private bool m_playerInputEnabled = true;

	public StartingTile[] startingTiles;

	[System.Serializable]
	public class StartingTile{

		public GameObject tilePrefab;
		public int x;
		public int y;
		public int z;
	}

	// Use this for initialization
	void Start() {

		m_allTiles = new TileScript[width, height];
		m_allGamePieces = new GamePiece[width, height];

		SetupTiles();
		SetupCamera();
		FillBoard(10, 1f);

	}

	void Update(){
		//HighlightMatches();
	}

	// set the camera position and orthosize to center the board onscreen with a border.
	void SetupCamera(){

		Camera.main.transform.position = new Vector3((float)(width-1) / 2f, (float)(height-1) / 2f, -10f);

		float aspectRatio = (float)Screen.width / (float)Screen.height;

		float verticalSize = (float)height / 2f + (float)borderSize;
		float horizontalSize = ((float)width / 2f + (float)borderSize) / aspectRatio;

		Camera.main.orthographicSize = (verticalSize > horizontalSize) ? verticalSize : horizontalSize;

	}

	void MakeTile(GameObject prefab, int x, int y, int z = 0)
	{
		if (prefab != null) {
			
			GameObject tile = Instantiate(prefab, new Vector3(x, y, z), Quaternion.identity) as GameObject;
			tile.name = "Tile (" + x + "," + y + ")";
			m_allTiles[x, y] = tile.GetComponent<TileScript>();
			tile.transform.parent = transform;
			m_allTiles[x, y].Init(x, y, this);
		}
	}

	/// <summary>
	/// Setups the tiles for the gameboard.
	/// </summary>
	void SetupTiles() {

		foreach (StartingTile sTile in startingTiles) {
			
			if (sTile != null) {
				MakeTile(sTile.tilePrefab, sTile.x, sTile.y, sTile.z);
			}
		}

		for (int i = 0; i < width; i++) {
			
			for (int j = 0; j < height; j++) {

				if (m_allTiles[i,j] == null) {
					MakeTile(tileNormalPrefab, i, j);
				}
			}
		}
	}

	/// <summary>
	/// Gets a random game piece from the prefab array.
	/// </summary>
	/// <returns>The random game piece.</returns>
	GameObject GetRandomGamePiece(){
		
		int randomIndex = Random.Range(0, gamePiecePrefabs.Length);

		if (gamePiecePrefabs[randomIndex] == null) {
			Debug.LogWarning("BOARD: " + randomIndex + " does not contain a valid prefab!");
		}

		return gamePiecePrefabs[randomIndex];
	}

	/// <summary>
	/// Places the game piece.
	/// </summary>
	/// <param name="gamePiece">Game piece.</param>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	public void PlaceGamePiece(GamePiece gamePiece, int x, int y){
		if (gamePiece == null) {
			Debug.LogWarning("BOARD: Invalid GamePiece!" );
		}

		gamePiece.transform.position = new Vector3(x, y, 0);
		gamePiece.transform.rotation = Quaternion.identity;

		if (IsWithinBounds(x,y)) {
			m_allGamePieces[x, y] = gamePiece;
		}

		gamePiece.SetCoord(x,y);

	}

	// Determines whether this instance is within bounds the specified x y.
	bool IsWithinBounds(int x, int y){

		return (x >= 0 && x < width && y >= 0 && y < height);
	}


	// Places a random gamePiece at X Y with an optional Y offset and move time.
	GamePiece FillRandomAt(int x, int y, int falseYOffset = 0, float moveTime = 0.1f){
		GameObject randomPiece = Instantiate(GetRandomGamePiece(), Vector3.zero, Quaternion.identity) as GameObject;
		if (randomPiece != null) {
			randomPiece.GetComponent<GamePiece>().Init(this);
			PlaceGamePiece(randomPiece.GetComponent<GamePiece>(), x, y);

			if (falseYOffset != 0) {
				randomPiece.transform.position = new Vector3(x, y + falseYOffset, 0);
				randomPiece.GetComponent<GamePiece>().Move(x, y, moveTime);
			}

			randomPiece.transform.parent = transform;
			return randomPiece.GetComponent<GamePiece>();
		}
		return null;
	}

	// Fills the empty spaces on the board with an optional Y offset to make the pieces drop into place.
	void FillBoard(int falseYOffset = 0, float moveTime = 0.1f){

		int maxIterations = 100;
		int iterations = 0;

		// loop through all the spaces on the board
		for (int i = 0; i < width; i++) {
			
			for (int j = 0; j < height; j++) {
				// if the space is unoccupied and does not contain and Obstacle tile
				if (m_allGamePieces[i,j]==null && m_allTiles[i,j].tileType != TileType.Obstacle) {
					// Fill the space with a random prefab piece
					GamePiece piece = FillRandomAt(i,j, falseYOffset, moveTime);
					iterations = 0;

					// if we form a match while filling in new space
					while (HasMatchOnFill(i,j)) {
						// clear the piece and fill with new random until now match
						ClearPieceAt(i,j);
						piece = FillRandomAt(i, j, falseYOffset, moveTime);
						iterations++;
						// exit the loop if we hit max iterations
						if (iterations >= maxIterations) {

							Debug.Log("Break ============================="); 
							break;
						}
					}
				}
			}
		}
	}



	// Determines whether this instance has match on fill at the specified x y with in minLength.
	// finds matches in the left and down positions as the board fills </description>
	// returns true if this instance has match on fill at the specified x y; otherwise, false.
	bool HasMatchOnFill(int x, int y, int minLength = 3){

		List<GamePiece> leftMatches = FindMatches(x, y, new Vector2(-1, 0), minLength);
		List<GamePiece> downwardMatches = FindMatches(x, y, new Vector2(0, -1), minLength);

		if (leftMatches == null) {
			leftMatches = new List<GamePiece>();
		}

		if (downwardMatches == null) {
			downwardMatches = new List<GamePiece>();
		}
		return (leftMatches.Count > 0 || downwardMatches.Count > 0);
	}

	/// <summary>
	/// Player Clicks a tile.
	/// </summary>
	/// <param name="tile">Tile.</param>
	public void ClickTile(TileScript tile){
		if (m_clickedTile == null) {
			m_clickedTile = tile;
			//Debug.Log("clicked tile: " + tile.name);
		}
	}

	/// <summary>
	/// Player Drags to adjacent tile.
	/// </summary>
	/// <param name="tile">Tile.</param>
	public void DragToTile(TileScript tile){
		if (m_clickedTile != null && IsNextToo(tile, m_clickedTile)) {
			m_targetTile = tile;
			//Debug.Log("draged to tile: " + tile.name);
		}
	}

	/// <summary>
	/// Player Releases the tile.
	/// </summary>
	public void ReleaseTile(){
		if (m_clickedTile != null && m_targetTile != null) {
			SwitchTiles(m_clickedTile, m_targetTile);

			// Release the tiles
			m_clickedTile = null;
			m_targetTile = null;

			//Debug.Log("Tile Released"); 
		}
	}


	// Switchs the clicked tile with the target tile.
	void SwitchTiles(TileScript clickedTile, TileScript targetTile){

		StartCoroutine(SwitchedTilesRoutine(clickedTile, targetTile));

	}

	// coroutine for switching tiles
	IEnumerator SwitchedTilesRoutine(TileScript clickedTile, TileScript targetTile){

		if (m_playerInputEnabled) {
			
			GamePiece clickedPiece = m_allGamePieces[clickedTile.xIndex, clickedTile.yIndex];
			GamePiece targetPiece = m_allGamePieces[targetTile.xIndex, targetTile.yIndex];

			if (targetPiece !=null && clickedPiece != null) {
				// Swap clickedPiece transform with targetPiece
				clickedPiece.Move(targetTile.xIndex, targetTile.yIndex, swapTime);

				// Swap targetPiece transfrom with clickedPiece
				targetPiece.Move(clickedTile.xIndex, clickedTile.yIndex, swapTime);

				yield return new WaitForSeconds(swapTime);

				List<GamePiece> clickedPieceMatches = FindMatchesAt(clickedTile.xIndex, clickedTile.yIndex);
				List<GamePiece> targetPieceMatches = FindMatchesAt(targetTile.xIndex, targetTile.yIndex);

				// If no match return pieces to original spot
				if (targetPieceMatches.Count == 0 && targetPieceMatches.Count == 0) {
					clickedPiece.Move(clickedTile.xIndex, clickedTile.yIndex, swapTime);
					targetPiece.Move(targetTile.xIndex, targetTile.yIndex, swapTime);
				} 
				else {

					yield return new WaitForSeconds(swapTime);

					ClearAndRefillBoard(clickedPieceMatches.Union(targetPieceMatches).ToList());
				}
			}
		}
	}


	// Determines whether this instance is next too the specified start end.
	// returns true if this instance is next too the specified start end; otherwise, false.
	bool IsNextToo(TileScript start, TileScript end){

		if (Mathf.Abs(start.xIndex - end.xIndex) == 1 && start.yIndex == end.yIndex) {
			
			return true;
		}

		if (Mathf.Abs(start.yIndex - end.yIndex) == 1 && start.xIndex == end.xIndex) {

			return true;
		}

		return false;
	}

	// loops through entire board and checks for any matches on the board
	List<GamePiece> FindAllMatches(){

		List<GamePiece> combinedMatches = new List<GamePiece>();
		for (int i = 0; i < width; i++) {
			for (int j = 0; j < height; j++) {
				List<GamePiece> matches = FindMatchesAt(i, j);
				combinedMatches = combinedMatches.Union(matches).ToList();
			}
		}
		return combinedMatches;
	}

	// general search method; specify a starting coordinate (startX, startY) and use a Vector2 for direction
	// i.e. (1,0) = right, (-1,0) = left, (0,1) = up, (0,-1) = down; minLength is minimum number to be considered
	// a match
	List<GamePiece> FindMatches(int startX, int startY, Vector2 searchDirection, int minLength = 3)
	{
		List<GamePiece> matches = new List<GamePiece>();

		GamePiece startPiece = null;

		if (IsWithinBounds(startX, startY))
		{
			startPiece = m_allGamePieces[startX, startY];
		}

		if (startPiece !=null)
		{
			matches.Add(startPiece);
		}

		else
		{
			return null;
		}

		int nextX;
		int nextY;

		int maxValue = (width > height) ? width: height;

		for (int i = 1; i < maxValue - 1; i++)
		{
			nextX = startX + (int) Mathf.Clamp(searchDirection.x,-1,1) * i;
			nextY = startY + (int) Mathf.Clamp(searchDirection.y,-1,1) * i;

			if (!IsWithinBounds(nextX, nextY))
			{
				break;
			}

			GamePiece nextPiece = m_allGamePieces[nextX, nextY];

			if (nextPiece == null) {
				break;
			} 

			else
			{
				if (nextPiece.matchValue == startPiece.matchValue && !matches.Contains(nextPiece))
				{
					matches.Add(nextPiece);
				}

				else
				{
					break;
				}
			}
		}

		if (matches.Count >= minLength)
		{
			return matches;
		}

		return null;

	}

	/// <summary>
	/// Finds the vertical matches.
	/// </summary>
	/// <returns>The vertical matches.</returns>
	/// <param name="startX">Start x.</param>
	/// <param name="startY">Start y.</param>
	/// <param name="minLength">Minimum length.</param>
	List<GamePiece> FindVerticalMatches(int startX, int startY, int minLength = 3)
	{
		List<GamePiece> upwardMatches = FindMatches(startX, startY, new Vector2(0,1), 2);
		List<GamePiece> downwardMatches = FindMatches(startX, startY, new Vector2(0,-1), 2);

		if (upwardMatches == null)
		{
			upwardMatches = new List<GamePiece>();
		}

		if (downwardMatches == null)
		{
			downwardMatches = new List<GamePiece>();
		}

		var combinedMatches = upwardMatches.Union(downwardMatches).ToList();

		return (combinedMatches.Count >= minLength) ? combinedMatches : null;

	}

	/// <summary>
	/// Finds the horizontal matches.
	/// </summary>
	/// <returns>The horizontal matches.</returns>
	/// <param name="startX">Start x.</param>
	/// <param name="startY">Start y.</param>
	/// <param name="minLength">Minimum length.</param>
	List<GamePiece> FindHorizontalMatches(int startX, int startY, int minLength = 3)
	{
		List<GamePiece> rightMatches = FindMatches(startX, startY, new Vector2(1,0), 2);
		List<GamePiece> leftMatches = FindMatches(startX, startY, new Vector2(-1,0), 2);

		if (rightMatches == null)
		{
			rightMatches = new List<GamePiece>();
		}

		if (leftMatches == null)
		{
			leftMatches = new List<GamePiece>();
		}

		var combinedMatches = rightMatches.Union(leftMatches).ToList();

		return (combinedMatches.Count >= minLength) ? combinedMatches : null;

	}

	/// <summary>
	/// Finds the matches at i, j and minLength.
	/// </summary>
	/// <returns>The <see cref="System.Collections.Generic.List`1[[GamePiece]]"/>.</returns>
	/// <param name="i">The index.</param>
	/// <param name="j">J.</param>
	/// <param name="minLength">Minimum length.</param>
	List<GamePiece> FindMatchesAt(int i, int j, int minLength = 3){
		List<GamePiece> horizMatches = FindHorizontalMatches(i, j, minLength);
		List<GamePiece> vertMatches = FindVerticalMatches(i, j, minLength);
		if (horizMatches == null) {
			horizMatches = new List<GamePiece>();
		}
		if (vertMatches == null) {
			vertMatches = new List<GamePiece>();
		}
		var combinedMatches = horizMatches.Union(vertMatches).ToList();
		return combinedMatches;
	}

	// find the matches with in the min length and build list of matches
	// returns: the list of matches
	List<GamePiece> FindMatchesAt(List<GamePiece> gamePieces, int minLength = 3){

		List<GamePiece> matches = new List<GamePiece>();

		foreach (GamePiece piece in gamePieces) {
			matches = matches.Union(FindMatchesAt(piece.xIndex, piece.yIndex, 3)).ToList();
		}
		return matches;
	}

	void HighlightTileOff(int x, int y){

		if (m_allTiles[x,y].tileType != TileType.Breakable) {

			SpriteRenderer spriteRenderer = m_allTiles[x,y].GetComponent<SpriteRenderer>();
			spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0);
		}
	}

	void HighlightTileOn(int x, int y, Color col){
		
		if (m_allTiles[x,y].tileType != TileType.Breakable) {
			
			SpriteRenderer spriteRenderer = m_allTiles[x,y].GetComponent<SpriteRenderer>();
			spriteRenderer.color = col;
		}
	}

	void HighlightMatchesAt(int x, int y){
		
		HighlightTileOff(x, y);
		var combinedMatches = FindMatchesAt(x, y);
		if (combinedMatches.Count > 0) {
			foreach (GamePiece piece in combinedMatches) {
				HighlightTileOn(piece.xIndex, piece.yIndex, piece.GetComponent<SpriteRenderer>().color);
			}
		}
	}

	void HighlightMatches(){
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				HighlightMatchesAt(i,j);

			}
		}
	}

	// highlights matching pieces
	void HighlightPieces(List<GamePiece> gamePieces){

		foreach (GamePiece piece in gamePieces) {
			if (piece != null) {
				HighlightTileOn(piece.xIndex, piece.yIndex, piece.GetComponent<SpriteRenderer>().color);
			}
		}
	}


	// Clears the piece at x and y.
	void ClearPieceAt(int x, int y){

		GamePiece pieceToClear = m_allGamePieces[x, y];

		if (pieceToClear != null) {
			m_allGamePieces[x, y] = null;
			Destroy(pieceToClear.gameObject);
		}

		HighlightTileOff(x,y);
	}

	// Clears the piece at gamePieces.
	void ClearPieceAt(List<GamePiece> gamePieces){
		
		foreach (GamePiece piece in gamePieces) {
			if (piece != null) {
				ClearPieceAt(piece.xIndex, piece.yIndex);
			}
		}
	}

	// Breaks a Tile piece at x and y
	void BreakTileAt(int x, int y){

		TileScript tileToBreak = m_allTiles[x, y];

		if (tileToBreak != null) {
			tileToBreak.BreakTile();
		}
	}

	void BreakTileAt(List<GamePiece> gamePiece){

		foreach (GamePiece piece in gamePiece) {

			if (piece != null) {
				BreakTileAt(piece.xIndex, piece.yIndex);
			}
		}
	}

	// collapses the column with empty pieces
	// returns the list of moving pieces
	List<GamePiece> CollapseColumn(int column, float collapseTime = 0.1f){

		List<GamePiece> movingPieces = new List<GamePiece>();

		for (int i = 0; i < height -1; i++) {
			
			if (m_allGamePieces[column, i] == null && m_allTiles[column, i].tileType != TileType.Obstacle) {
				for (int j = i+1; j < height; j++) {
					
					if (m_allGamePieces[column,j] !=null) {
						m_allGamePieces[column,j].Move(column, i, collapseTime* (j - i));
						m_allGamePieces[column,i] = m_allGamePieces[column, j];
						m_allGamePieces[column,i].SetCoord(column,i);

						if (!movingPieces.Contains(m_allGamePieces[column,i])) {
							movingPieces.Add(m_allGamePieces[column,i]);
						}

						m_allGamePieces[column,j] = null;
						break;
					}
				}
			}
		}

		return movingPieces;
	}

	// collapse the column
	// returns: list of moving pieces
	List<GamePiece> CollapseColumn(List<GamePiece> gamePieces){

		List<GamePiece> movingPieces = new List<GamePiece>();
		List<int> columnsToCollapse = GetColumns(gamePieces);

		foreach (int column in columnsToCollapse) {
			movingPieces = movingPieces.Union(CollapseColumn(column)).ToList();
		}

		return movingPieces;
	}

	// gets the columns of game pieces
	// returns: the columns of pieces
	List <int> GetColumns(List<GamePiece> gamePieces){

		List<int> columns = new List<int>();

		foreach (GamePiece piece in gamePieces) {
			if (!columns.Contains(piece.xIndex)) {
				columns.Add(piece.xIndex);
			}
		}

		return columns;
	}

	// clears entire board of all game pieces
	void ClearBoard(){

		for (int i = 0; i < width; i++) {
			for (int j = 0; j < height; j++) {
				ClearPieceAt(i,j);
			}
		}
	}


	void ClearAndRefillBoard(List<GamePiece> gamePieces){

		StartCoroutine(ClearAndRefilBoardRoutine(gamePieces));
	}

	IEnumerator ClearAndRefilBoardRoutine(List<GamePiece> gamePieces){

		m_playerInputEnabled = false;

		List<GamePiece> matches = gamePieces;

		do {
			//Clear and Collapse board
			yield return StartCoroutine(ClearAndCollapseRoutine(matches));
			yield return null;

			//Refill Board
			yield return StartCoroutine(RefillRoutine());

			matches = FindAllMatches();
			// time to wait to restart clearAndCollapseRoutine
			yield return new WaitForSeconds(0.5f);
		} 

		while (matches.Count != 0);
		m_playerInputEnabled = true;
	}

	// refils the board with a optional offset
	IEnumerator RefillRoutine(){
		FillBoard(10, 0.5f);
		yield return null;
	}

	// clears and collapses the column
	// returns: null
	IEnumerator ClearAndCollapseRoutine(List<GamePiece> gamePieces){

		List<GamePiece> movingPieces = new List<GamePiece>();
		List<GamePiece> matches = new List<GamePiece>();

		HighlightPieces(gamePieces);
		// length the pieces remain highlighted before clearing
		yield return new WaitForSeconds(0.5f);
		bool isFinished = false;

		while (!isFinished) {
			// clear game pieces
			ClearPieceAt(gamePieces);
			BreakTileAt(gamePieces);
			// length of wait after pieces are removed before collapse starts
			yield return new WaitForSeconds(0.15f);
			movingPieces = CollapseColumn(gamePieces);

			while (!IsCollapsed(movingPieces)) {
				yield return null;
			}

			yield return new WaitForSeconds(0.2f);
			matches = FindMatchesAt(movingPieces);

			if (matches.Count == 0) {

				isFinished = true;
				break;
			} 

			else {
				yield return StartCoroutine(ClearAndCollapseRoutine(matches));
			}
		}

		yield return null;
	}

	// determines if the column has finished collapsing
	// returns: true of piece index Y is within offset, otherwise false.
	bool IsCollapsed(List<GamePiece> gamePieces){
		foreach (GamePiece piece in gamePieces) {
			if (piece != null) {
				if (piece.transform.position.y - (float) piece.yIndex > 0.001f) {
					return false;
				}
			}
		}

		return true;
	}
}
