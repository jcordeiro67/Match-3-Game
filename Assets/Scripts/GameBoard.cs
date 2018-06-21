using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameBoard : MonoBehaviour {

	public int width;						// Width of the gameboard in spaces
	public int height;						// Height of gameboard in spaces
	public int borderSize;					// Border around gameboard for orthoSize
	public float swapTime = 0.5f;			// Time to complete swaping gamePieces
	public int minSearchPieces = 3;			// Number of spaces to search for matches
	public GameObject tilePrefab;			// Prefab gameboard tile piece

	public GameObject[] gamePiecePrefabs;	// Array to hold prefabs to be used in the level


	private TileScript[,] m_allTiles;		// Array to hold cordinates of boardTiles
	private GamePiece[,] m_allGamePieces;	// Array to hold cordinates of gamePieces

	private TileScript m_clickedTile;
	private TileScript m_targetTile;

	// Use this for initialization
	void Start() {

		m_allTiles = new TileScript[width, height];
		m_allGamePieces = new GamePiece[width, height];

		SetupTiles();
		SetupCamera();
		FillRandom();
		//HighlightMatches();

	}

	void Update(){
		//HighlightMatches();
	}

	/// <summary>
	/// Setups the camera.
	/// </summary>
	void SetupCamera(){

		Camera.main.transform.position = new Vector3((float)(width-1) / 2f, (float)(height-1) / 2f, -10f);

		float aspectRatio = (float)Screen.width / (float)Screen.height;

		float verticalSize = (float)height / 2f + (float)borderSize;
		float horizontalSize = ((float)width / 2f + (float)borderSize) / aspectRatio;

		Camera.main.orthographicSize = (verticalSize > horizontalSize) ? verticalSize : horizontalSize;

	}

	/// <summary>
	/// Setups the tiles for the gameboard.
	/// </summary>
	void SetupTiles() {

		for (int i = 0; i < width; i++) {
			
			for (int j = 0; j < height; j++) {

				GameObject tile = Instantiate(tilePrefab, new Vector3(i, j, 0), Quaternion.identity) as GameObject;
				tile.name = "Tile (" + i + "," + j + ")";
				m_allTiles[i, j] = tile.GetComponent<TileScript>();
				tile.transform.parent = transform;
				m_allTiles[i, j].Init(i, j, this);
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

	/// <summary>
	/// Determines whether this instance is within bounds the specified x y.
	/// </summary>
	/// <returns><c>true</c> if this instance is within bounds the specified x y; otherwise, <c>false</c>.</returns>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	bool IsWithinBounds(int x, int y){

		return (x >= 0 && x < width && y >= 0 && y < height);
	}

	/// <summary>
	/// Fills the random.
	/// </summary>
	void FillRandom(){

		for (int i = 0; i < width; i++) {
			for (int j = 0; j < height; j++) {

				GameObject randomPiece = Instantiate(GetRandomGamePiece(), Vector3.zero, Quaternion.identity) as GameObject;

				if (randomPiece != null) {
					randomPiece.GetComponent<GamePiece>().Init(this);
					PlaceGamePiece(randomPiece.GetComponent<GamePiece>(), i, j);
					randomPiece.transform.parent = transform;
				}
			}
			
		}
	}

	public void ClickTile(TileScript tile){
		if (m_clickedTile == null) {
			m_clickedTile = tile;
			//Debug.Log("clicked tile: " + tile.name);
		}
	}

	public void DragToTile(TileScript tile){
		if (m_clickedTile != null && IsNextToo(tile, m_clickedTile)) {
			m_targetTile = tile;
			//Debug.Log("draged to tile: " + tile.name);
		}
	}

	public void ReleaseTile(){
		if (m_clickedTile != null && m_targetTile != null) {
			SwitchTiles(m_clickedTile, m_targetTile);

			// Release the tiles
			m_clickedTile = null;
			m_targetTile = null;

			//Debug.Log("Tile Released"); 
		}
	}

	/// <summary>
	/// Switchs the tiles.
	/// </summary>
	/// <param name="clickedTile">Clicked tile.</param>
	/// <param name="targetTile">Target tile.</param>
	void SwitchTiles(TileScript clickedTile, TileScript targetTile){

		StartCoroutine(SwitchedTilesRoutine(clickedTile, targetTile));

	}

	IEnumerator SwitchedTilesRoutine(TileScript clickedTile, TileScript targetTile){

		GamePiece clickedPiece = m_allGamePieces[clickedTile.xIndex, clickedTile.yIndex];
		GamePiece targetPiece = m_allGamePieces[targetTile.xIndex, targetTile.yIndex];

		// Swap clickedPiece transform with targetPiece
		clickedPiece.Move(targetTile.xIndex, targetTile.yIndex, swapTime);

		// Swap targetPiece transfrom with clickedPiece
		targetPiece.Move(clickedTile.xIndex, clickedTile.yIndex, swapTime);

		yield return new WaitForSeconds(swapTime);
		HighlightMatchesAt(clickedTile.xIndex, clickedTile.yIndex);
		HighlightMatchesAt(targetTile.xIndex, targetTile.yIndex);

	}

	/// <summary>
	/// Determines whether this instance is next too the specified start end.
	/// </summary>
	/// <returns><c>true</c> if this instance is next too the specified start end; otherwise, <c>false</c>.</returns>
	/// <param name="start">Start.</param>
	/// <param name="end">End.</param>
	bool IsNextToo(TileScript start, TileScript end){

		if (Mathf.Abs(start.xIndex - end.xIndex) == 1 && start.yIndex == end.yIndex) {
			
			return true;
		}

		if (Mathf.Abs(start.yIndex - end.yIndex) == 1 && start.xIndex == end.xIndex) {

			return true;
		}

		return false;
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

			if (nextPiece.matchValue == startPiece.matchValue && !matches.Contains(nextPiece))
			{
				matches.Add(nextPiece);
			}

			else
			{
				break;
			}
		}

		if (matches.Count >= minLength)
		{
			return matches;
		}

		return null;

	}

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

	List<GamePiece> FindMatchesAt(int i, int j, int minLength =3)
	{
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

	void HighlightTileOff(int x, int y){
		SpriteRenderer spriteRenderer = m_allTiles[x,y].GetComponent<SpriteRenderer>();
		spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0);
	}

	void HighlightTileOn(int x, int y, Color col){
		SpriteRenderer spriteRenderer = m_allTiles[x,y].GetComponent<SpriteRenderer>();
		spriteRenderer.color = col;
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
}
