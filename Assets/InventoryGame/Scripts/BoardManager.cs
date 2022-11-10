using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardManager : MonoBehaviour
{
    [Tooltip("How many turns before calculating the final score.")]
    [SerializeField] private int turns;
    [SerializeField] private Text turnRemainingText;
    [SerializeField] private Transform canvas;
    
    [Tooltip("Transform for the board itself for normalizing the tile positions.")]
    [SerializeField] private RectTransform boardTransform;

    [Tooltip("Game object parent that will hold the placed pieces.")]
    [SerializeField] private Transform placedTiledParent;

    [Space(10)]
    [SerializeField] private GameObject[] pieces;
    [SerializeField] private RectTransform spawnPlace;
    [SerializeField] private GameObject blockedTilePrefab;

    [Header("Scorboard")]
    [SerializeField] private GameObject scorboardPanel;
    [SerializeField] private Text scoreText;

    // The board [row, col] is flipped compared to the visual representation in the scene
    private int[,] board = new int[4,5];
    private Vector2 bottomLeftCorner;

    private List<GameObject> bagOfPieces;
    private GamePiece currentPiece;
    private List<GameObject> blockedObjectList = new List<GameObject>();
    private int currentTurn;

    private void Awake()
    {
        bottomLeftCorner = boardTransform.anchoredPosition;
    }

    private void Start()
    {
        StartGame();
    }

    /// <summary>
    /// Initializes the board and starts the game.
    /// </summary>
    public void StartGame()
    {
        // Clear the board, both the data and any placed tiles from a previous game.
        for (int r = 0; r < board.GetLength(0); r++)
        {
            for (int c = 0; c < board.GetLength(1); c++)
            {
                board[r, c] = 0;
            }
        }

        foreach (Transform t in placedTiledParent)
        {
            Destroy(t.gameObject);
        }

        scorboardPanel.SetActive(false);
        currentTurn = turns;
        turnRemainingText.text = currentTurn.ToString();

        bagOfPieces = new List<GameObject>(pieces);
        Shuffle(bagOfPieces);

        SpawnPiece();
    }

    /// <summary>
    /// Randomly select a piece from the pool of pieces and create it in the selection area.
    /// </summary>
    public void SpawnPiece()
    {
        var piece = Instantiate(bagOfPieces[0], canvas);
        var pieceTransform = piece.GetComponent<RectTransform>();
        pieceTransform.anchoredPosition = spawnPlace.anchoredPosition;

        currentPiece = piece.GetComponent<GamePiece>();
        currentPiece.BoardManager = this;

        bagOfPieces.RemoveAt(0);
    }

    /// <summary>
    /// Places the piece on the board and creates blocked tiles if any.
    /// </summary>
    /// <param name="piece">The <see cref="GamePiece"/> being placed on the board.</param>
    public void TryPlacePiece(GamePiece piece)
    {
        ClearBlockedTiles();

        // Snap the piece to the closest coordinates based on the top left corner of the piece
        Vector2Int boardIndex = GetBoardIndex(piece);
        
        // Check to see if the blocks are intersecting with other blocks currently on the board and within bounds.
        // If not, create a blocked tile at that location to show
        foreach(RectTransform t in piece.BlockTransforms)
        {
            int blockCol = (int)t.anchoredPosition.x / GamePiece.TileSize;
            int blockRow = (int)t.anchoredPosition.y / GamePiece.TileSize;
            if (boardIndex.y + blockRow < 0 || boardIndex.x + blockCol < 0 ||
                boardIndex.y + blockRow >= board.GetLength(0) || boardIndex.x + blockCol >= board.GetLength(1) ||
                board[boardIndex.y + blockRow, boardIndex.x + blockCol] != 0)
            {
                var pos = new Vector2(bottomLeftCorner.x + ((boardIndex.x + blockCol) * GamePiece.TileSize),
                    bottomLeftCorner.y + ((boardIndex.y + blockRow) * GamePiece.TileSize));

                var obj = Instantiate(blockedTilePrefab, canvas);
                blockedObjectList.Add(obj);
                obj.GetComponent<RectTransform>().anchoredPosition = pos;
            }
        }

        var snappedPos = new Vector2(bottomLeftCorner.x + (boardIndex.x * GamePiece.TileSize),
            bottomLeftCorner.y + (boardIndex.y * GamePiece.TileSize));

        var pieceTransform = piece.GetComponent<RectTransform>();
        pieceTransform.anchoredPosition = snappedPos;
    }

    /// <summary>
    /// Button callback to set the current piece in the board.
    /// </summary>
    public void ConfirmPiece()
    {
        // If there's any blocked tiles, the piece can't be placed
        if (blockedObjectList.Count > 0)
        {
            Debug.LogWarning($"There are {blockedObjectList.Count} blocked spaces, can't place piece.");
            return;
        }

        // Add the block value to the board and move it to be parented under the tileParent object
        Vector2Int boardIndex = GetBoardIndex(currentPiece);
        foreach (RectTransform t in currentPiece.BlockTransforms)
        {
            int blockCol = (int)t.anchoredPosition.x / GamePiece.TileSize;
            int blockRow = (int)t.anchoredPosition.y / GamePiece.TileSize;
            board[boardIndex.y + blockRow, boardIndex.x + blockCol] = currentPiece.TileValue;
        }

        currentPiece.transform.SetParent(placedTiledParent);
        currentPiece.GetComponent<DraggableUI>().enabled = false;

        AdvanceTurn();
    }

    /// <summary>
    /// Button callback to discard the current piece and select a new one.
    /// </summary>
    public void DiscardPiece()
    {
        // Destroy any blocked piece indicators as well as the current object
        foreach (var block in blockedObjectList)
        {
            Destroy(block);
        }

        blockedObjectList.Clear();
        Destroy(currentPiece.gameObject);

        AdvanceTurn();
    }

    /// <summary>
    /// Button callback to rotate the current piece and attempt to place it.
    /// </summary>
    public void RotatePiece()
    {
        currentPiece.RotatePiece();
        TryPlacePiece(currentPiece);
    }

    /// <summary>
    /// Clears the blocked tiles from the board.
    /// </summary>
    public void ClearBlockedTiles()
    {
        foreach (GameObject obj in blockedObjectList)
        {
            Destroy(obj);
        }

        blockedObjectList.Clear();
    }

    /// <summary>
    /// Advances the turn counter and updates the UI. If there's no more turns, then show the leaderboard.
    /// </summary>
    private void AdvanceTurn()
    {
        currentTurn--;
        turnRemainingText.text = currentTurn.ToString();

        if (currentTurn > 0)
        {
            SpawnPiece();
        }
        else
        {
            CalculateScore();
        }
    }

    /// <summary>
    /// Calculates the final score, displays the ending screen while submitting the score to the leaderboard.
    /// </summary>
    private void CalculateScore()
    {
        int score = 0;

        // For now, just add up the number of blocks filled in the board
        for (int r = 0; r < board.GetLength(0); r++)
        {
            for (int c = 0; c < board.GetLength(1); c++)
            {
                if (board[r,c] > 0)
                {
                    score++;
                }
            }
        }

        scoreText.text = $"Score: {score}";
        scorboardPanel.SetActive(true);
    }

    /// <summary>
    /// Gets the board index position of the piece.
    /// </summary>
    /// <param name="piece"><see cref="GamePiece"/> to get the board position.</param>
    /// <returns>A <see cref="Vector2Int"/> of col/row (x/y) position in the board.</returns>
    private Vector2Int GetBoardIndex(GamePiece piece)
    {
        var pieceTransform = piece.GetComponent<RectTransform>();
        Vector2 offset = pieceTransform.anchoredPosition - bottomLeftCorner;
        int column = (int)(offset.x / GamePiece.TileSize);
        int row = (int)(offset.y / GamePiece.TileSize);
        if (offset.x % GamePiece.TileSize > GamePiece.TileSize / 2)
        {
            column++;
        }

        if (offset.y % GamePiece.TileSize > GamePiece.TileSize / 2)
        {
            row++;
        }

        return new Vector2Int(column, row);
    }

    /// <summary>
    /// Fisher-Yates algorithm for shuffing an array.
    /// https://stackoverflow.com/questions/108819/best-way-to-randomize-an-array-with-net
    /// </summary>
    private void Shuffle(List<GameObject> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            int k = Random.Range(0, n--);
            var temp = list[n];
            list[n] = list[k];
            list[k] = temp;
        }
    }
}
