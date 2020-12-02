using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class AIBoard : MonoBehaviour
{
    public static AIBoard Instance { set; get; }
    public Piece[,] pieces = new Piece[9, 9];
    public Piece[] MinimaxBoard = new Piece[81];

    private bool[,] ValidMoves { set; get; }
    private bool[,] ValidJumps { set; get; }

    private bool[] PossibleMoves { set; get; }
    private bool[] PossibleJumps { set; get; }

    public GameObject BluePiecePrefab;
    public GameObject RedPiecePrefab;

    private Vector3 boardOffset = new Vector3(-5.0f, 0, -4.0f);
    private Vector3 pieceOffset = new Vector3(0.5f, 0, 0.5f);

    public bool isBlue;
    private bool isBlueTurn;
    private bool hasJumped;
    private bool aiJumped;
    public bool[,] blueTarget = new bool[9, 9];
    public bool[,] redTarget = new bool[9, 9];

    private Piece selectedPiece;
    private List<Piece> forcedPieces;
    private Piece jumpedPiece;

    private Vector2 mousePosition;
    private Vector2 startDrag;
    private Vector2 endDrag;

    public Dictionary<Piece, bool[]> AiMoves = new Dictionary<Piece, bool[]>();
    public Dictionary<Piece, bool[]> AiJumps = new Dictionary<Piece, bool[]>();

    public GameObject popUpRed;
    public GameObject popUpBlue;

    private void Start()
    {
        Instance = this;
        isBlueTurn = true;
        //GenerateBoard();
        GenerateEndgame();
        GenerateEndTargets();
        MinimaxBoard = FlattenPieces(pieces);

        popUpRed = GameObject.Find("Red Win");
        popUpBlue = GameObject.Find("Blue Win");
        popUpRed.SetActive(false);
        popUpBlue.SetActive(false);
    }

    private void Update()
    {
        UpdateMousePosition();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene("GameMenu");
        }

        if (isBlueTurn)
        {
            int x = (int)mousePosition.x;
            int y = (int)mousePosition.y;

            if (selectedPiece != null)
            {
                UpdatePieceDrag(selectedPiece);
            }
            if (Input.GetMouseButtonDown(0))
            {
                SelectPiece(x, y);
                if(selectedPiece != null)
                {
                    ValidMoves = selectedPiece.ValidMovesAi();
                    ValidJumps = selectedPiece.ValidJumpsAi();
                    if (!hasJumped)
                    {
                        PieceHighlights.Instance.HighlightValidMoves(ValidMoves);
                    }
                    PieceHighlights.Instance.HighlightValidMoves(ValidJumps);
                }
            }
            if (Input.GetMouseButtonUp(0))
            {
                TryMove((int)startDrag.x, (int)startDrag.y, x, y);
                PieceHighlights.Instance.HideHighlights();
            }
            if (Input.GetMouseButtonDown(1) && hasJumped)
            {
                hasJumped = false;
                EndTurn();
            }
        }

        if (!isBlueTurn)
        {
            int currentBest = Minimax(1);

            if (AiMoves.Count != 0)
            {
                int randomMove = Random.Range(0, AiMoves.Count);

                Piece chosenPiece = null;

                chosenPiece = AiMoves.ElementAt(randomMove).Key;
                PossibleMoves = AiMoves.ElementAt(randomMove).Value;

                List<int> flatMoveList = new List<int>();

                int randomFlatMove = Random.Range(0, flatMoveList.Count);

                for (int i = 0; i < 81; i++)
                {
                    if (PossibleMoves[i])
                    {
                        flatMoveList.Add(i);
                    }
                }

                int flatMove = flatMoveList[randomFlatMove];

                //reverse flat position into x, y coords
                int moveX = flatMove / 9;
                int moveY = flatMove % 9;

                if (selectedPiece == null)
                {
                    //AI Select and move
                    AiSelect(chosenPiece.CurrentX, chosenPiece.CurrentY);
                    AiMove(selectedPiece.CurrentX, selectedPiece.CurrentY, moveX, moveY);
                    if (aiJumped)
                    {
                        int jumpBest = MaximiseJumps(chosenPiece);
                        Debug.Log(jumpBest);

                        if(AiJumps.Count != 0 && jumpBest > currentBest)
                        {
                            int randomJump = Random.Range(0, AiJumps.Count);

                            Piece jumpingPiece = AiJumps.ElementAt(randomJump).Key;
                            PossibleJumps = AiJumps.ElementAt(randomJump).Value;

                            List<int> flatJumpList = new List<int>();

                            for (int i = 0; i < 81; i++)
                            {
                                if (PossibleJumps[i])
                                {
                                    flatJumpList.Add(i);
                                }
                            }

                            int flatJump = flatJumpList[0];

                            int jumpX = flatJump / 9;
                            int jumpY = flatJump % 9;

                            AiSelect(chosenPiece.CurrentX, chosenPiece.CurrentY);
                            AiMove(selectedPiece.CurrentX, selectedPiece.CurrentY, jumpX, jumpY);
                        }
                        aiJumped = false;
                        Debug.Log("not jumped");
                    }
                    EndTurn();
                }
            }
        }
    }
    private void UpdateMousePosition()
    {
        if (!Camera.main)
        {
            Debug.Log("no main camera");
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("Board")))
        {
            mousePosition.x = (int)(hit.point.x - boardOffset.x);
            mousePosition.y = (int)(hit.point.z - boardOffset.z);
        }
        else
        {
            mousePosition.x = -1;
            mousePosition.y = -1;
        }
    }
    private void UpdatePieceDrag(Piece p)
    {
        if (!Camera.main)
        {
            Debug.Log("no main camera");
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("Board")))
        {
            p.transform.position = hit.point + Vector3.up;
        }
    }

    private void SelectPiece(int x, int y)
    {
        //out of bounds
        if (x < 0 || x >= 9 || y < 0 || y >= 9)
        {
            return;
        }

        Piece p = pieces[x, y];
        if (p != null && p.isBlue == isBlueTurn && !hasJumped)
        {
            selectedPiece = p;
            startDrag = mousePosition;
        }
        if (p != null && p.isBlue == isBlueTurn && hasJumped && p == jumpedPiece)
        {
            selectedPiece = p;
            startDrag = mousePosition;
        }
    }

    private void TryMove(int x1, int y1, int x2, int y2)
    {
        forcedPieces = ScanForPossibleMove();

        //multiplayer
        startDrag = new Vector2(x1, y1);
        endDrag = new Vector2(x2, y2);
        selectedPiece = pieces[x1, y1];

        //out of bounds
        if (x2 < 0 || x2 >= 9 || y2 < 0 || y2 >= 9)
        {

            if (selectedPiece != null)
            {
                MovePiece(selectedPiece, x1, y1);
            }

            startDrag = Vector2.zero;
            selectedPiece = null;
            return;
        }

        if (selectedPiece != null)
        {
            //if has not moved
            if (endDrag == startDrag)
            {
                MovePiece(selectedPiece, x1, y1);
                startDrag = Vector2.zero;
                selectedPiece = null;
                return;
            }

            //check if valid move
            if (selectedPiece.ValidMove(pieces, x1, y1, x2, y2) && !hasJumped)
            {
                //did we jump
                //if jump
                if (Mathf.Abs(x2 - x1) == 2 || Mathf.Abs(y2 - y1) == 2)
                {
                    hasJumped = true;
                    Debug.Log("jumped");
                }
                pieces[x2, y2] = selectedPiece;
                pieces[x1, y1] = null;
                MovePiece(selectedPiece, x2, y2);

                EndTurn();
            }
            else if (selectedPiece.ValidMove(pieces, x1, y1, x2, y2) && hasJumped)
            {
                if (Mathf.Abs(x2 - x1) == 2 || Mathf.Abs(y2 - y1) == 2)
                {
                    pieces[x2, y2] = selectedPiece;
                    pieces[x1, y1] = null;
                    MovePiece(selectedPiece, x2, y2);

                    EndTurn();
                }
                else
                {
                    MovePiece(selectedPiece, x1, y1);
                    startDrag = Vector2.zero;
                    selectedPiece = null;
                    return;
                }
            }
            else
            {
                MovePiece(selectedPiece, x1, y1);
                startDrag = Vector2.zero;
                selectedPiece = null;
                return;
            }
        }
    }
    private void EndTurn()
    {
        //---for end state check
        int x = (int)endDrag.x;
        int y = (int)endDrag.y;

        

        if (selectedPiece != null)
        {
            x = selectedPiece.CurrentX;
            y = selectedPiece.CurrentY;

            if (selectedPiece.isBlue && !selectedPiece.isEnd && IsBlueTarget(x, y))
            {
                selectedPiece.isEnd = true;
                Debug.Log("reached end point blue");
            }
            else if (!selectedPiece.isBlue && !selectedPiece.isEnd && IsRedTarget(x, y))
            {
                selectedPiece.isEnd = true;
                Debug.Log("reached end point red");
            }

            if (selectedPiece.isBlue && selectedPiece.isEnd && !IsBlueTarget(x, y))
            {
                selectedPiece.isEnd = false;
                Debug.Log("left end point blue");
            }
            else if (!selectedPiece.isBlue && selectedPiece.isEnd && !IsRedTarget(x, y))
            {
                selectedPiece.isEnd = false;
                Debug.Log("left end point red");
            }
        }
        //---

        //PieceHighlights.Instance.HideHighlights();
        selectedPiece = null;
        startDrag = Vector2.zero;

        if (ScanForPossibleMove(selectedPiece, x, y).Count != 0 && hasJumped)
        {
            jumpedPiece = pieces[x, y];
            return;
        }

        isBlueTurn = !isBlueTurn;
        hasJumped = false;
        jumpedPiece = null;
        MinimaxBoard = FlattenPieces(pieces);
        CheckVictory();
    }

    private List<Piece> ScanForPossibleMove(Piece p, int x, int y)
    {
        forcedPieces = new List<Piece>();

        if (pieces[x, y].CanJump(pieces, x, y))
        {
            forcedPieces.Add(pieces[x, y]);
        }

        return forcedPieces;
    }
    private List<Piece> ScanForPossibleMove()
    {
        forcedPieces = new List<Piece>();

        //check all pieces
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                if (pieces[i, j] != null && pieces[i, j].isBlue == isBlueTurn)
                {
                    if (pieces[i, j].CanJump(pieces, i, j))
                    {
                        forcedPieces.Add(pieces[i, j]);
                    }
                }
            }
        }
        return forcedPieces;
    }

    private void GenerateBoard()
    {
        //generate blue
        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 5 - y; x++)
            {
                //generate piece
                GeneratePiece(x, y, BluePiecePrefab);

            }
        }

        //generate red
        for (int y = 8; y > 3; y--)
        {
            for (int x = 8; x > 3 + (8 - y); x--)
            {
                //generate piece
                GeneratePiece(x, y, RedPiecePrefab);
            }
        }
    }
    private void GeneratePiece(int x, int y, GameObject PieceColour)
    {
        GameObject go = Instantiate(PieceColour) as GameObject;
        go.transform.SetParent(transform);
        Piece p = go.GetComponent<Piece>();
        pieces[x, y] = p;
        MovePiece(p, x, y);

    }
    private void MovePiece(Piece p, int x, int y)
    {
        pieces[x, y].SetPosition(x, y);
        //Debug.Log(pieces[x, y].CurrentX);
        p.transform.position = (Vector3.right * x) + (Vector3.forward * y) + boardOffset + pieceOffset;
    }
    private void GenerateEndTargets()
    {
        //fill blue target array
        for (int j = 8; j > 3; j--)
        {
            for (int i = 8; i > 3 + (8 - j); i--)
            {
                blueTarget[i, j] = true;
            }
        }

        //fill red target array
        for (int j = 0; j < 5; j++)
        {
            for (int i = 0; i < 5 - j; i++)
            {
                redTarget[i, j] = true;
            }
        }
    }
    private bool IsBlueTarget(int x, int y)
    {
        if (blueTarget[x, y])
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    private bool IsRedTarget(int x, int y)
    {
        if (redTarget[x, y])
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public void CheckVictory()
    {
        if (BlueVictory())
        {
            Debug.Log("blue wins");
            popUpBlue.SetActive(true);
            
        }

        if (RedVictory())
        {
            Debug.Log("red wins");
            popUpRed.SetActive(true);
        }
    }
    public bool BlueVictory()
    {
        int check = 0;
        foreach (Piece ps in pieces)
        {
            if (ps != null && ps.isBlue && ps.isEnd)
            {
                check++;
            }
        }
        if (check == 15)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public bool RedVictory()
    {
        int check = 0;
        foreach (Piece ps in pieces)
        {
            if (ps != null && !ps.isBlue && ps.isEnd)
            {
                check++;
            }
        }
        if (check == 15)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public bool CanJump(int x, int y)
    {
        bool[,] AllowedJumps = pieces[x, y].ValidJumpsAi();

        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                if (AllowedJumps[i, j])
                {
                    return true;
                }
            }
        }
        return false;
    }
    public bool CanMoveOnce(int x , int y)
    {
        bool[,] AllowedMoves = pieces[x, y].ValidMovesAi();

        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                if (AllowedMoves[i, j])
                {
                    return true;
                }
            }
        }
        return false;
    }
    public bool CanMove(int x, int y)
    {
        if(CanJump(x, y) || CanMoveOnce(x, y))
        {
            return true;
        }
        return false;
    }

    private void AiSelect(int x, int y)
    {
        if(pieces[x, y] == null)
        {
            return;
        }
        if(pieces[x, y].isBlue != isBlueTurn)
        {
            return;
        }

        if(CanMove(x, y))
        {
            selectedPiece = pieces[x, y];
        }
        else
        {
            return;
        }
    }
    private void AiMove(int x1, int y1, int x2, int y2)
    {
        if (Mathf.Abs(x2 - x1) == 2 || Mathf.Abs(y2 - y1) == 2)
        {
            aiJumped = true;
            Debug.Log("ai jumped");
        }
        pieces[x2, y2] = selectedPiece;
        pieces[x1, y1] = null;
        MovePiece(selectedPiece, x2, y2);
        MinimaxBoard = FlattenPieces(pieces);

    }

    private bool[,] TotalValidMoves(Piece p) //possible moves for any Piece p
    {
        bool[,] r = new bool[9, 9];

        for(int i = 0; i < 9; i++)
        {
            for(int j = 0; j < 9; j++)
            {
                if(p.ValidMovesAi()[i, j])
                {
                    r[i, j] = true;
                }
                if (p.ValidJumpsAi()[i, j])
                {
                    r[i, j] = true;
                }
            }
        }
        return r;
    }

    private void Make(int targetPosition, Piece p)//makes moves in search tree
    {
        MinimaxBoard[targetPosition] = p;
        MinimaxBoard[p.FlatPosition] = null;
        p.SetFlatPosition(targetPosition);
    }

    private void UnMake(int targetPosition, int previousPosition, Piece p)//unmakes moves
    {
        MinimaxBoard[targetPosition] = null;
        MinimaxBoard[previousPosition] = p;
        p.SetFlatPosition(previousPosition);
    }

    private int Evaluate()
    {
        int totalWeight = 0;

        foreach(Piece p in MinimaxBoard)
        {
            if(p!= null)
            {
                if (!p.isBlue)
                {
                    totalWeight += p.GetRedWeight();
                }
                if (p.isBlue)
                {
                    totalWeight -= p.GetBlueWeight();
                }
            }
        }
        return totalWeight;
    }

    static Piece[] FlattenPieces(Piece[,] p)
    {
        int size = p.Length;
        Piece[] result = new Piece[size];

        int write = 0;
        for (int i = 0; i <= p.GetUpperBound(0); i++)
        {
            for(int j = 0; j <= p.GetUpperBound(1); j++)
            {
                if (p[i, j] != null)
                {
                    p[i, j].SetFlatPosition(write);
                }
                result[write++] = p[i, j];
            }
        }
        return result;
    }

    static Piece[,] UnflattenPieces(Piece[] p)
    {
        Piece[,] result = new Piece[9, 9];

        int write = 0;
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                result[i, j] = p[write++];
            }
        }
        return result;
    }

    public static void PrintArray(bool[] array)
    {
        int count = 0;
        foreach(bool i in array)
        {
            Debug.Log(count++);
            Debug.Log(i);
        }
    }

    private int MaximiseJumps(Piece p, int bestJumpValue = -10000, int count = 0)
    {
        int previousPosition = p.FlatPosition;
        int jumpValue = Evaluate();

        if(count == 50)
        {
            Debug.Log(count);
            return bestJumpValue;
        }

        if (jumpValue < bestJumpValue)
        {
            Debug.Log(count);
            return bestJumpValue;
        }

        bool[] possibleJumps = p.FlatJumps();
        bool[] bestJumps = new bool[81];

        for(int i = 0; i < 81; i++)
        {
            if (possibleJumps[i])
            {
                Make(i, p);

                jumpValue = MaximiseJumps(p, jumpValue, count + 1);
                
                if (jumpValue > bestJumpValue)
                {
                    bestJumpValue = jumpValue;
                    {
                        AiJumps.Clear();
                        bestJumps = new bool[81];
                        bestJumps[i] = true;
                        AiJumps.Add(p, bestJumps);
                    }
                }
                if (jumpValue == bestJumpValue)
                {
                    bestJumpValue = jumpValue;
                    {
                        bestJumps[i] = true;
                        AiJumps[p] = bestJumps;
                    }
                }
                UnMake(i, previousPosition, p);
            }
        }
        return bestJumpValue;
    }

    private int Minimax(int depth, bool isRedPlayer = true, bool isRedTurn = true, int count = 0)
    {
        int moveValue;

        if(depth == 0)
        {
            moveValue = Evaluate();
            return moveValue;
        }

        int bestValue;

        if (isRedPlayer)
        {
            bestValue = -10000;
        }
        else
        {
            bestValue = 10000;
        }

        foreach (Piece p in MinimaxBoard)
        {
            if(p != null && !p.isBlue == isRedTurn)
            {
                int previousPosition = p.FlatPosition;
                bool[] allPossibleMoves = p.AllFlatMoves();

                bool[] bestPossible = new bool[81];

                for(int i = 0; i < 81; i++)
                {
                    if (allPossibleMoves[i])
                    {
                        Make(i, p);//make move

                        //---RECURSIVE CALL
                        moveValue = Minimax(depth - 1, !isRedPlayer, !isRedTurn, count + 1);
                        //---

                        if (isRedPlayer)//maximising
                        {   
                            if (moveValue > bestValue)
                            {
                                bestValue = moveValue;
                                if(count == 0)
                                {
                                    AiMoves.Clear();
                                    bestPossible = new bool[81];
                                    bestPossible[i] = true;
                                    AiMoves.Add(p, bestPossible);
                                }
                            }
                            if (moveValue == bestValue)
                            {
                                bestValue = moveValue;
                                if (count == 0)
                                {
                                    bestPossible[i] = true;
                                    AiMoves[p] = bestPossible;
                                }
                            }
                        }
                        else if (!isRedPlayer)//minimising
                        {
                            if(moveValue < bestValue)
                            {
                                bestValue = moveValue;
                            }
                            if(moveValue == bestValue)
                            {
                                bestValue = moveValue;
                            }
                        }
                        UnMake(i, previousPosition, p);//unmake move
                    }
                }
            }
        }
        return bestValue;
    }

    public void ReturnButton()
    {
        Debug.Log("Return");
        SceneManager.LoadScene("MainMenu");
    }
    public void ExitButton()
    {
        Debug.Log("Exit");
        Application.Quit();
    }

    public void GenerateEndgame()
    {
        //blue endgame
        for (int y = 8; y > 4; y--)
        {
            for (int x = 8; x > 4 + (8 - y); x--)
            {
                //generate piece
                GeneratePiece(x, y, BluePiecePrefab);
                pieces[x, y].isEnd = true;
            }
        }
        GeneratePiece(3, 1, BluePiecePrefab);
        GeneratePiece(3, 2, BluePiecePrefab);
        GeneratePiece(4, 3, BluePiecePrefab);
        GeneratePiece(6, 3, BluePiecePrefab);
        GeneratePiece(7, 3, BluePiecePrefab);

        //red endgame
        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4 - y; x++)
            {
                //generate piece
                GeneratePiece(x, y, RedPiecePrefab);
                pieces[x, y].isEnd = true;

            }
        }
        GeneratePiece(0, 4, RedPiecePrefab);//end
        pieces[0, 4].isEnd = true;
        GeneratePiece(1, 3, RedPiecePrefab);//end
        pieces[1, 3].isEnd = true;
        GeneratePiece(4, 0, RedPiecePrefab);//end
        pieces[4, 0].isEnd = true;

        GeneratePiece(4, 4, RedPiecePrefab);
        GeneratePiece(5, 5, RedPiecePrefab);
    }
}
