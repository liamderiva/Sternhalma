using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameBoard : MonoBehaviour {

    public static GameBoard Instance { set; get; }
    public Piece[,] pieces = new Piece[9, 9];
    private bool[,] ValidMoves { set; get; }
    private bool[,] ValidJumps { set; get; }
    public GameObject BluePiecePrefab;
    public GameObject RedPiecePrefab;

    private Vector3 boardOffset = new Vector3(-5.0f, 0, -4.0f);
    private Vector3 pieceOffset = new Vector3(0.5f, 0, 0.5f);

    public bool isBlue;
    private bool isBlueTurn;
    private bool hasJumped;
    public bool[,] blueTarget = new bool[9, 9];
    public bool[,] redTarget = new bool[9, 9];

    private Piece selectedPiece;
    private List<Piece> forcedPieces;
    private Piece jumpedPiece;

    private Vector2 mousePosition;
    private Vector2 startDrag;
    private Vector2 endDrag;

    public GameObject popUpRed;
    public GameObject popUpBlue;

    private void Start()
    {
        Instance = this;
        isBlueTurn = true;
        GenerateBoard();
        GenerateEndTargets();
        popUpRed = GameObject.Find("Red Win");
        popUpBlue = GameObject.Find("Blue Win");
        popUpRed.SetActive(false);
        popUpBlue.SetActive(false);
    }

    private void Update()
    {
        UpdateMousePosition();

        //if my turn (or blue turn)
        {
            int x = (int)mousePosition.x;
            int y = (int)mousePosition.y;

            if(selectedPiece != null)
            {
                UpdatePieceDrag(selectedPiece);
            }

            if (Input.GetMouseButtonDown(0))
            {
                SelectPiece(x, y);
                if(selectedPiece != null)
                {
                    ValidMoves = selectedPiece.ValidMoves();
                    ValidJumps = selectedPiece.ValidJumps();
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
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SceneManager.LoadScene("GameMenu");
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
        if(x < 0 || x >= 9 || y < 0 || y >= 9)
        {
            return;
        }

        Piece p = pieces[x, y];
        if(p != null && p.isBlue == isBlueTurn && !hasJumped)
        {
            selectedPiece = p;
            startDrag = mousePosition;
        }
        if(p!= null && p.isBlue == isBlueTurn && hasJumped && p == jumpedPiece)
        {
            selectedPiece = p;
            startDrag = mousePosition;
        }
        if(jumpedPiece != null)
        Debug.Log("cheese");
    }

    private void TryMove(int x1, int y1, int x2, int y2)
    {
        forcedPieces = ScanForPossibleMove();

        //multiplayer
        startDrag = new Vector2(x1, y1);
        endDrag = new Vector2(x2, y2);
        selectedPiece = pieces[x1, y1];

        //out of bounds
        if(x2 < 0 || x2 >= 9 || y2 < 0 || y2>= 9)
        {

            if(selectedPiece != null)
            {
                MovePiece(selectedPiece, x1, y1);
            }

            startDrag = Vector2.zero;
            selectedPiece = null;
            return;
        }

        if(selectedPiece != null)
        {
            //if has not moved
            if(endDrag == startDrag)
            {
                MovePiece(selectedPiece, x1, y1);
                startDrag = Vector2.zero;
                selectedPiece = null;
                return;
            }

            //check if valid move
            if(selectedPiece.ValidMove(pieces, x1, y1, x2, y2) && !hasJumped)
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
            else if(selectedPiece.ValidMove(pieces, x1, y1, x2, y2) && hasJumped)
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

        if(selectedPiece != null)
        {
            if(selectedPiece.isBlue && !selectedPiece.isEnd && IsBlueTarget(x, y))
            {
                selectedPiece.isEnd = true;
                Debug.Log("reached end point blue");
            }
            else if (!selectedPiece.isBlue && !selectedPiece.isEnd && IsRedTarget(x, y))
            {
                selectedPiece.isEnd = true;
                Debug.Log("reached end point red");
            }

            if(selectedPiece.isBlue && selectedPiece.isEnd && !IsBlueTarget(x, y))
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
        for(int i = 0; i < 9; i++)
        {
            for(int j = 0; j < 9; j++)
            {
                if(pieces[i,j] != null && pieces[i,j].isBlue == isBlueTurn)
                {
                    if(pieces[i,j].CanJump(pieces, i, j))
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
            if(ps != null && ps.isBlue && ps.isEnd)
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
}
