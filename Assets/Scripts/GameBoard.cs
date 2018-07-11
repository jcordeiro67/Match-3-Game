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
	public int fillOffset = 10;				// Distance the pieces start above the board
	public float fillMoveTime = 0.5f;		// Time it takes a gamepiece to travel fromt the fillOfset into place
	public GameObject tileNormalPrefab;		// Prefab gameboard tile piece
	public GameObject tileObstaclePrefab;	// Prefab Obstacle tile piece
	public GameObject[] gamePiecePrefabs;	// Array to hold prefabs to be used in the level
	public GameObject adjacentBombPrefab;
	public GameObject colorBombPrefab;
	public GameObject columnBombPrefab;
	public GameObject rowBombPrefab;

	public int maxColectibles = 3;
	public int collectibleCount = 0;
	private int m_scoreMultiplier = 0;

	[Range(0,1)]
	public float chanceForCollectible = 0.1f;
	public Vector3 collectibleRotation = new Vector3(0f, 0f, -45f);
	public GameObject[] collectiblePrefabs;

	private TileScript[,] m_allTiles;		// Array to hold cordinates of boardTiles
	private GamePiece[,] m_allGamePieces;	// Array to hold cordinates of gamePieces

	private TileScript m_clickedTile;
	private TileScript m_targetTile;

	private GameObject m_clickedTileBomb;
	private GameObject m_targetTileBomb;

	private bool m_playerInputEnabled = true;

	public bool PlayerInputEnabled{

		get{
			return m_playerInputEnabled;
		}
	}

	public StartingObject[] startingTiles;
	public StartingObject[] startingGamePieces;

	private ParticleManager m_particleManager;

	[System.Serializable]
	public class StartingObject{

		public GameObject prefab;
		public int x;
		public int y;
		public int z;
	}

	// Use this for initialization
	void Start() {

		m_allTiles = new TileScript[width, height];
		m_allGamePieces = new GamePiece[width, height];
		m_particleManager = GameObject.FindObjectOfType<ParticleManager>();


	}

	public void SetupBoard()
	{
		SetupTiles();
		SetupGamePieces();
		List<GamePiece> startingCollectibles = FindAllCollectibles();
		collectibleCount = startingCollectibles.Count;
		SetupCamera();
		FillBoard(fillOffset, fillMoveTime);
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

	// Makes the tile.
	void MakeTile(GameObject prefab, int x, int y, int z = 0)
	{
		if (prefab != null && IsWithinBounds(x,y)) {
			
			GameObject tile = Instantiate(prefab, new Vector3(x, y, z), Quaternion.identity) as GameObject;
			tile.name = "Tile (" + x + "," + y + ")";
			m_allTiles[x, y] = tile.GetComponent<TileScript>();
			tile.transform.parent = transform;
			m_allTiles[x, y].Init(x, y, this);
		}
	}


	// Makes the game piece.
	void MakeGamePiece(GameObject prefab, int x, int y, int falseYOffset = 0, float moveTime = 0.1f)
	{
		if (prefab != null && IsWithinBounds(x,y)) {
			prefab.GetComponent<GamePiece>().Init(this);
			PlaceGamePiece(prefab.GetComponent<GamePiece>(), x, y);
			if (falseYOffset != 0) {
				prefab.transform.position = new Vector3(x, y + falseYOffset, 0);
				prefab.GetComponent<GamePiece>().Move(x, y, moveTime);
			}

			prefab.transform.parent = transform;
		}
	}

	/// <summary>
	/// Makes a bomb game piece.
	/// </summary>
	/// <param name="prefab">Prefab.</param>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	GameObject MakeBomb(GameObject prefab, int x, int y){

		if (prefab != null && IsWithinBounds(x,y)) {
			GameObject bomb = Instantiate(prefab, new Vector3(x, y, 0), Quaternion.identity) as GameObject;
			bomb.GetComponent<Bomb>().Init(this);
			bomb.GetComponent<Bomb>().SetCoord(x,y);
			bomb.transform.parent = transform;

			return bomb;
		}

		return null;
	}

	// Setups the tiles for the gameboard.
	void SetupTiles() {

		foreach (StartingObject sTile in startingTiles) {
			
			if (sTile != null) {
				MakeTile(sTile.prefab, sTile.x, sTile.y, sTile.z);
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

	void SetupGamePieces(){

		foreach (StartingObject sPiece in startingGamePieces) {
			if (sPiece != null) {
				GameObject piece = Instantiate(sPiece.prefab, new Vector3(sPiece.x, sPiece.y, sPiece.z), Quaternion.identity) as GameObject;
				Collectible collectibleComponent = piece.GetComponent<Collectible>();
				if(collectibleComponent != null){
					piece.transform.Rotate(collectibleRotation);
				}
				MakeGamePiece(piece, sPiece.x, sPiece.y, fillOffset, fillMoveTime);
			}
		}
	}

	// Gets a random object from the passed in array of GameObjects.
	// <returns>The random GameObject.</returns>
	GameObject GetRandomObject(GameObject[] objectArray){

		int randomIndex = Random.Range(0, objectArray.Length);
		if (objectArray[randomIndex] == null) {
			Debug.LogWarning("BOARD.GetRandomObject at index " + randomIndex + " does not contain a valid GameObject");
			
		}

		return objectArray[randomIndex];
	}
		
	// Gets a random game piece from the GamePiece prefab array.
	// <returns>The random GamePiece.</returns>
	GameObject GetRandomGamePiece(){
		
		return GetRandomObject(gamePiecePrefabs);
	}

	// Gets a random collectible from the collectible prefab array.
	// <returns>The random collectible piece.</returns>
	GameObject GetRandomCollectible(){

		return GetRandomObject(collectiblePrefabs);
	}

	// Places the game piece.
	public void PlaceGamePiece(GamePiece gamePiece, int x, int y){
		if (gamePiece == null) {
			Debug.LogWarning("BOARD: Invalid GamePiece!" );
		}

		gamePiece.transform.position = new Vector3(x, y, 0);
		//gamePiece.transform.rotation = Quaternion.identity;

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
	// Returns the randomPiece if with in bounds of the board else returns null.
	GamePiece FillRandomGamePieceAt(int x, int y, int falseYOffset = 0, float moveTime = 0.1f){

		if (IsWithinBounds(x,y)) {
			// generate random piece
			GameObject randomPiece = Instantiate(GetRandomGamePiece(), Vector3.zero, Quaternion.identity) as GameObject;
			// place piece on the board
			MakeGamePiece(randomPiece, x, y, falseYOffset, moveTime);
			return randomPiece.GetComponent<GamePiece>();
		}

		return null;
	}

	GamePiece FillRandomCollectibleAt(int x, int y, int falseYOffset = 0, float moveTime = 0.1f){
		
		if (IsWithinBounds(x, y)) {
			// generate the random collectible
			// set the rotation of the collectible
			GameObject randomCollectible = Instantiate(GetRandomCollectible(), Vector3.zero, Quaternion.Euler(collectibleRotation)) as GameObject;
			// place the collectible on the board
			MakeGamePiece(randomCollectible, x, y, falseYOffset, moveTime);
			return randomCollectible.GetComponent<Collectible>();
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
					
					GamePiece piece = null;

					if (j == height -1 && CanAddCollectible()) {
						piece = FillRandomCollectibleAt(i, j, falseYOffset, moveTime);
						collectibleCount++;
					} 

					else {
						// Fill the space with a random prefab piece
						piece = FillRandomGamePieceAt(i,j, falseYOffset, moveTime);
						iterations = 0;

						// if we form a match while filling in new space
						while (HasMatchOnFill(i,j)) {
							// clear the piece and fill with new random until now match
							ClearPieceAt(i,j);
							piece = FillRandomGamePieceAt(i, j, falseYOffset, moveTime);
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

	// Player Clicks a tile.
	public void ClickTile(TileScript tile){
		if (m_clickedTile == null) {
			m_clickedTile = tile;
			//Debug.Log("clicked tile: " + tile.name);
		}
	}

	// Player Drags to adjacent tile.
	public void DragToTile(TileScript tile){
		if (m_clickedTile != null && IsNextToo(tile, m_clickedTile)) {
			m_targetTile = tile;
			//Debug.Log("draged to tile: " + tile.name);
		}
	}
		
	// Player Releases the tile.
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
				List<GamePiece> colorMatches = new List<GamePiece>();

				// Determine if one of the switched pieces was a colorBomb
				// If colorBomb is clickedPiece match clickedPiece color
				if (IsColorBomb(clickedPiece) && !IsColorBomb(targetPiece)) {
					clickedPiece.matchValue = targetPiece.matchValue;
					colorMatches = FindAllMatchValue(clickedPiece.matchValue);
				}
				// If colorBomb is targetPiece match targetPieces color
				else if (IsColorBomb(targetPiece) && !IsColorBomb(clickedPiece)) {
					targetPiece.matchValue = clickedPiece.matchValue;
					colorMatches = FindAllMatchValue(targetPiece.matchValue);
				}
				// If both pieces are colorBombs search all gamePieces
				// if not in colorMatches list add to list
				else if (IsColorBomb(clickedPiece) && IsColorBomb(targetPiece)) {
					
					foreach (GamePiece piece in m_allGamePieces) {
						
						if (!colorMatches.Contains(piece)) {
							colorMatches.Add(piece);
						}
					}
				}

				// If no valid match has been made move pieces back to original location
				if (targetPieceMatches.Count == 0 && targetPieceMatches.Count == 0 && colorMatches.Count == 0) {
					clickedPiece.Move(clickedTile.xIndex, clickedTile.yIndex, swapTime);
					targetPiece.Move(targetTile.xIndex, targetTile.yIndex, swapTime);
				}

				else {

					// Deduct 1 from movesLeft
					if (GameManager.Instance != null) {
						GameManager.Instance.movesLeft--;
						GameManager.Instance.UpdateMoves();
					}

					yield return new WaitForSeconds(swapTime);

					// determine swapDirection for bomb placement
					Vector2 swapDirection = new Vector2(targetTile.xIndex - clickedTile.xIndex, targetTile.yIndex - clickedTile.yIndex);
					m_clickedTileBomb = DropBomb(clickedTile.xIndex, clickedTile.yIndex, swapDirection, clickedPieceMatches);
					m_targetTileBomb = DropBomb(targetTile.xIndex, targetTile.yIndex, swapDirection, targetPieceMatches);

					// Change the bombs sprite to match either the target piece or the clicked piece
					if (m_clickedTileBomb != null && targetPiece != null) {
						
						// match the target piece
						GamePiece clickedBomPiece = m_clickedTileBomb.GetComponent<GamePiece>();

						if (!IsColorBomb(clickedBomPiece)) {
							clickedBomPiece.ChangeSprite(targetPiece);
						}
					}

					if (m_targetTileBomb != null && clickedPiece != null) {
						
						// match the clicked piece
						GamePiece targetBombPiece = m_targetTileBomb.GetComponent<GamePiece>();

						if (!IsColorBomb(targetBombPiece)) {
							targetBombPiece.ChangeSprite(clickedPiece);
						}
					}

					// Appends the lists colorMatches and TargetPieceMatches to clickedPieceMatches
					ClearAndRefillBoard(clickedPieceMatches.Union(targetPieceMatches).ToList().Union(colorMatches).ToList());
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
				if (nextPiece.matchValue == startPiece.matchValue && !matches.Contains(nextPiece) && nextPiece.matchValue != MatchValue.None)
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

	// Finds the vertical matches.
	// <returns>The vertical matches.</returns>
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
		
	// Finds the horizontal matches.
	// <returns>The horizontal matches.</returns>
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

	// Finds the matches at i, j and minLength.
	// <returns>The <see cref="System.Collections.Generic.List`1[[GamePiece]]"/>.</returns>
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

		//HighlightTileOff(x,y);
	}

	// Clears the gamePieces and plays the proper clear or bomb FX.
	// Scores the pieces points
	void ClearPieceAt(List<GamePiece> gamePieces, List<GamePiece> bombedPieces){
		
		foreach (GamePiece piece in gamePieces) {
			
			if (piece != null) {
				
				ClearPieceAt(piece.xIndex, piece.yIndex);

				int bonus = 0;
				if (gamePieces.Count >= 4) {
					bonus = 20;
				}

				piece.ScorePoints(m_scoreMultiplier, bonus);

				if (m_particleManager != null) {
					
					// play bombFX 
					if (bombedPieces.Contains(piece)) {
						// get bomb component from gameObject
						// check that bomb component is not null
						// add bombType to paramaters
						m_particleManager.BombFXAt(piece.xIndex, piece.yIndex);
					}

					else{
						// play clearFX
						m_particleManager.ClearPieceFXAt(piece.xIndex, piece.yIndex);
					}

				}
			}
		}
	}

	// Breaks a Tile piece at x and y
	void BreakTileAt(int x, int y){

		TileScript tileToBreak = m_allTiles[x, y];

		if (tileToBreak != null && tileToBreak.tileType == TileType.Breakable) {
			
			if (m_particleManager != null) {
				m_particleManager.BreakTileFXAt(tileToBreak.breakableValue, x, y, 0);
			}

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

	IEnumerator ClearAndRefilBoardRoutine(List<GamePiece> gamePieces){

		m_playerInputEnabled = false;

		List<GamePiece> matches = gamePieces;
		m_scoreMultiplier = 0;

		do {
			// Increases the score multiplier by 1 evertime a collapse is made before 
			// the player moves another piece
			m_scoreMultiplier ++;

			//Clear and Collapse board
			yield return StartCoroutine(ClearAndCollapseRoutine(matches));
			yield return null;

			//Refill Board
			yield return StartCoroutine(RefillRoutine());

			matches = FindAllMatches();
			// time to wait to restart clearAndCollapseRoutine
			yield return new WaitForSeconds(0.2f);
		} 

		while (matches.Count != 0);
		m_playerInputEnabled = true;
	}

	// refils the board with a optional offset
	IEnumerator RefillRoutine(){
		FillBoard(fillOffset, fillMoveTime);
		yield return null;
	}

	// clears and collapses the column
	// returns: null
	IEnumerator ClearAndCollapseRoutine(List<GamePiece> gamePieces){

		List<GamePiece> movingPieces = new List<GamePiece>();
		List<GamePiece> matches = new List<GamePiece>();
		List<GamePiece> bombedPieces = new List<GamePiece>();

		//HighlightPieces(gamePieces);
		// length the pieces remain highlighted before clearing
		yield return new WaitForSeconds(0.2f);
		bool isFinished = false;

		while (!isFinished) {

			// find gamepieces affected by bombs
			bombedPieces = GetBombedPieces(gamePieces);
			// merge bombedPieces list with gamePieces
			gamePieces = gamePieces.Union(bombedPieces).ToList();

			// buld a list of collectible pieces at row 0 and add to 
			// list of gamePieces to be removed by ClearPieces
			List<GamePiece> collectedPieces = FindCollectiblesAt(0, true);

			List<GamePiece> allCollectibles = FindAllCollectibles();
			List<GamePiece> blockers = gamePieces.Intersect(allCollectibles).ToList();
			collectedPieces = collectedPieces.Union(blockers).ToList();
			collectibleCount -= collectedPieces.Count;

			gamePieces = gamePieces.Union(collectedPieces).ToList();

			// string bombs together
			bombedPieces = GetBombedPieces(gamePieces);
			gamePieces = gamePieces.Union(bombedPieces).ToList();

			// clear game pieces and bombed pieces
			ClearPieceAt(gamePieces, bombedPieces);
			// clear tile pieces
			BreakTileAt(gamePieces);

			// add bombs here
			if (m_clickedTileBomb != null) {
				ActivateBomb(m_clickedTileBomb);
				m_clickedTileBomb = null;
			}

			if (m_targetTileBomb != null) {
				ActivateBomb(m_targetTileBomb);
				m_targetTileBomb = null;
			}

			// length of wait after pieces are removed before collapse starts
			yield return new WaitForSeconds(0.15f);
			movingPieces = CollapseColumn(gamePieces);

			while (!IsCollapsed(movingPieces)) {
				yield return null;
			}

			yield return new WaitForSeconds(0.2f);

			// finds and new matching pieces after a column collapse
			matches = FindMatchesAt(movingPieces);
			// Finds collectible pieces that collapse to bottom row
			collectedPieces = FindCollectiblesAt(0, true);
			matches = matches.Union(collectedPieces).ToList();

			// if no matches or collectibles found
			if (matches.Count == 0) {
				// exit loop
				isFinished = true;
				break;
			} 

			else {
				
				// Increase scoreMultiplier
				m_scoreMultiplier++;
				// return to this coroutine
				yield return StartCoroutine(ClearAndCollapseRoutine(matches));
			}
		}
		// exit coroutine
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

	// gets a list of rown pieces if a bomb was present and triggered
	// returns a list of gamePieces within the row
	List<GamePiece> GetRowPieces(int row){
		
		List<GamePiece> gamePieces = new List<GamePiece>();

		for (int i = 0; i < width; i++)
		{
			if (m_allGamePieces[i, row] !=null)
			{
				gamePieces.Add(m_allGamePieces[i, row]);
			}
		}

		return gamePieces;
	}

	// gets a list of column pieces if a bomb was present and triggered
	// returns a list of gamePieces within the column
	List<GamePiece> GetColumnPieces(int column){
		
		List<GamePiece> gamePieces = new List<GamePiece>();

		for (int i = 0; i < height; i++)
		{
			if (m_allGamePieces[column,i] !=null)
			{
				gamePieces.Add(m_allGamePieces[column,i]);
			}
		}
		return gamePieces;
	}

	// gets a list of adjacent pieces if a bomb was present and triggered
	// returns a list of gamePieces within the offset range
	List<GamePiece> GetAdjacentPieces(int x, int y, int offset = 1)
	{
		List<GamePiece> gamePieces = new List<GamePiece>();

		for (int i = x - offset; i <= x + offset; i++)
		{
			for (int j = y - offset; j <= y + offset; j++)
			{
				if (IsWithinBounds(i,j))
				{
					gamePieces.Add(m_allGamePieces[i,j]);
				}

			}
		}

		return gamePieces;
	}

	List<GamePiece> GetBombedPieces(List<GamePiece> gamePieces){
		
		List<GamePiece> allPiecesToClear = new List<GamePiece>();

		foreach (GamePiece piece in gamePieces)
		{
			if (piece !=null)
			{
				List<GamePiece> piecesToClear = new List<GamePiece>();

				Bomb bomb = piece.GetComponent<Bomb>();

				if (bomb !=null)
				{
					switch (bomb.bombType)
					{
						case BombType.Column:
							piecesToClear = GetColumnPieces(bomb.xIndex);
							break;
						case BombType.Row:
							piecesToClear = GetRowPieces(bomb.yIndex);
							break;
						case BombType.Adjacent:
							piecesToClear = GetAdjacentPieces(bomb.xIndex, bomb.yIndex, 1);
							break;
						case BombType.Color:

							break;
					}

					// List of all pieces to clear
					allPiecesToClear = allPiecesToClear.Union(piecesToClear).ToList();
					// remove collectibles from list to clear if not of clearedByBomb type 
					allPiecesToClear = RemoveCollectibles(allPiecesToClear);
				}
			}
		}

		return allPiecesToClear;
	}
		
	bool IsCornerMatch(List<GamePiece> gamePieces){

		bool vertical = false;
		bool horizontal = false;
		int xStart = -1;
		int yStart = -1;

		foreach (GamePiece piece in gamePieces) {
			
			if (piece != null) {
				
				if (xStart == -1 || yStart == -1) {

					xStart = piece.xIndex;
					yStart = piece.yIndex;
					continue;
				}

				if (piece.xIndex != xStart && piece.yIndex == yStart) {
					horizontal = true;
				}

				if(piece.xIndex == xStart && piece.yIndex != yStart){
					vertical = true;
				}
			}
		}
		return (horizontal && vertical);
	}

	// Searches the list of gamePieces for more then 4 in a row and 
	// Drops a bomb at X, Y and determines which bombType to drop by swapDirection
	// returns the bomb
	GameObject DropBomb(int x, int y, Vector2 swapDirection, List<GamePiece> gamePieces){

		GameObject bomb = null;

		if (gamePieces.Count >= 4) {

			if (IsCornerMatch(gamePieces)) {
				// drop and adjacent bomb
				if (adjacentBombPrefab != null) {
					bomb = MakeBomb(adjacentBombPrefab, x, y);

				}
			}
			else {

				if (gamePieces.Count >= 5) {
					if (colorBombPrefab != null) {
						bomb = MakeBomb(colorBombPrefab, x, y);
					}
				}

				else {
					
					// row bomb
					if (swapDirection.x != 0) {
						if (rowBombPrefab != null) {
							bomb = MakeBomb(rowBombPrefab, x, y);
						}
					}

					else {
						
						if (columnBombPrefab != null) {
							// column bomb
							bomb = MakeBomb(columnBombPrefab, x,y);
						}
					}
				}
			}
		}

		return bomb;
	}

	// Activates the bomb after it has been dropped
	// adds the bomb to the list m_allGamePieces as a gamePiece
	void ActivateBomb(GameObject bomb){

		int x = (int)bomb.transform.position.x;
		int y = (int)bomb.transform.position.y;

		if (IsWithinBounds(x,y)) {
			m_allGamePieces[x, y] = bomb.GetComponent<GamePiece>();
		}
	}

	// Finds matching gamePieces by MatchValue
	// returns a list of foundPieces with same value
	List<GamePiece> FindAllMatchValue(MatchValue mValue){

		List<GamePiece> foundPieces = new List<GamePiece>();

		for (int i = 0; i < width; i++) {
			for (int j = 0; j < height; j++) {
				if(m_allGamePieces[i,j] != null){
					if (m_allGamePieces[i,j].matchValue == mValue) {
						foundPieces.Add(m_allGamePieces[i,j]);
					}
				}
			}
		}

		return foundPieces;
	}

	// Checks if a bomb piece is swapped with a color piece
	// returns true if bombType is color false if not
	bool IsColorBomb(GamePiece gamePiece){

		Bomb bomb = gamePiece.GetComponent<Bomb>();

		if (bomb != null) {
			return (bomb.bombType == BombType.Color);
		}

		return false;
	}

	List<GamePiece> FindCollectiblesAt(int row, bool clearedAtBottomOnly = false){

		List<GamePiece> foundCollectibles = new List<GamePiece>();

		for (int i = 0; i < width; i++) {
			if (m_allGamePieces[i,row] != null) {
				
				Collectible collectibleComponent = m_allGamePieces[i, row].GetComponent<Collectible>();

				if (collectibleComponent != null) {
					if (!clearedAtBottomOnly || (clearedAtBottomOnly && collectibleComponent.clearedByBottom)) {
						foundCollectibles.Add(m_allGamePieces[i,row]);
					}
				}
			}
		}

		return foundCollectibles;
	}

	List<GamePiece> FindAllCollectibles(){

		List<GamePiece> foundCollectibles = new List<GamePiece>();

		for (int i = 0; i < height; i++) {
			List<GamePiece> collectibleRow = FindCollectiblesAt(i);
			foundCollectibles = foundCollectibles.Union(collectibleRow).ToList();

		}

		return foundCollectibles;
	}

	bool CanAddCollectible(){

		return (Random.Range(0f, 1f) <= chanceForCollectible && collectiblePrefabs.Length > 0 
			&& collectibleCount < maxColectibles);
	}

	List<GamePiece> RemoveCollectibles(List<GamePiece> bombedPieces){

		List<GamePiece> collectiblePieces = FindAllCollectibles();
		List<GamePiece> piecesToRemove = new List<GamePiece>();

		foreach (GamePiece piece in collectiblePieces) {
			Collectible collectibleComponent = piece.GetComponent<Collectible>();

			if (collectibleComponent != null) {
				if (!collectibleComponent.clearedByBomb) {
					piecesToRemove.Add(piece);
				}
			}
		}

		return bombedPieces.Except(piecesToRemove).ToList();
	}
}
