using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour {

    public bool isBlue; //isWhite
    public bool isEnd;
    public int CurrentX { set; get; }   //current x of piece
    public int CurrentY { set; get; }   //current y of piece
    public int FlatPosition { set; get; }    //current position in 1d array

    public int[] redWeights = new int[] 
    {25, 23, 21, 20, 19, 17, 12, 10, 8,
     23, 22, 20, 19, 17, 13, 11, 9,  7,
     21, 20, 19, 16, 14, 12, 10, 8,  6,
     20, 19, 16, 15, 13, 11, 9,  7,  5,
     19, 17, 14, 13, 12, 10, 8,  6,  4,
     17, 13, 12, 11, 10, 9,  7,  5,  3,
     12, 11, 10, 9,  8,  7,  6,  4,  2,
     10, 9,  8,  7,  6,  5,  4,  3,  1,
     8,  7,  6,  5,  4,  3,  2,  1,  0};

    public int[] blueWeights = new int[] 
    {0, 1,  2,  3,  4,  5,  6,  7,  8,
     1, 3,  4,  5,  6,  7,  8,  9,  10,
     2, 4,  6,  7,  8,  9,  10, 11, 12,
     3, 5,  7,  9,  10, 11, 12, 13, 14,
     4, 6,  8,  10, 12, 13, 14, 15, 17,
     5, 7,  9,  11, 13, 15, 16, 18, 19,
     6, 8,  10, 12, 14, 16, 19, 20, 21,
     7, 9,  11, 13, 15, 18, 20, 22, 23,
     8, 10, 12, 14, 17, 19, 21, 23, 25};

    public void SetPosition(int x, int y)   //sets the properties CurrentX/Y to ints x/y
    {
        CurrentX = x;
        CurrentY = y;
    }

    public void SetFlatPosition(int i)
    {
        FlatPosition = i;
    }

    public bool CanJump(Piece[,] board, int x, int y)
    {
        if (isBlue || !isBlue)
        {
            //top left
            if(x >= 2 && y <= 6)
            {
                Piece p = board[x - 1, y + 1];  //piece to jump
                if(p != null)   //if there is a piece there
                {
                    //check if possible to land after jump
                    if(board[x - 2, y + 2] == null)
                    {
                        return true;
                    }
                }
            }
            //left
            if (x >= 2)
            {
                Piece p = board[x - 1, y];  //piece to jump
                if (p != null)   //if there is a piece there
                {
                    //check if possible to land after jump
                    if (board[x - 2, y] == null)
                    {
                        return true;
                    }
                }
            }
            //down
            if (y >= 2)
            {
                Piece p = board[x, y - 1];  //piece to jump
                if (p != null)   //if there is a piece there
                {
                    //check if possible to land after jump
                    if (board[x, y - 2] == null)
                    {
                        return true;
                    }
                }
            }
            //bottom right
            if (x <= 6 && y >= 2)
            {
                Piece p = board[x + 1, y - 1];  //piece to jump
                if (p != null)   //if there is a piece there
                {
                    //check if possible to land after jump
                    if (board[x + 2, y - 2] == null)
                    {
                        return true;
                    }
                }
            }
            //right
            if (x <= 6)
            {
                Piece p = board[x + 1, y];  //piece to jump
                if (p != null)   //if there is a piece there
                {
                    //check if possible to land after jump
                    if (board[x + 2, y] == null)
                    {
                        return true;
                    }
                }
            }
            //up
            if (y <= 6)
            {
                Piece p = board[x, y + 1];  //piece to jump
                if (p != null)   //if there is a piece there
                {
                    //check if possible to land after jump
                    if (board[x, y + 2] == null)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }
    public bool ValidMove(Piece[,] board, int x1, int y1, int x2, int y2)
    {
        //if you are moving onto another piece return false
        if (board[x2, y2] != null)
        {
            return false;
        }

        //if moving in y = -x line return false
        int diagonalCheckX = (x1 - x2);
        int diagonalCheckY = (y1 - y2);
        if (diagonalCheckX == diagonalCheckY)
        {
            return false;
        }

        int deltaMoveX = Mathf.Abs(x1 - x2); //for x axis movement
        int deltaMoveY = Mathf.Abs(y1 - y2); //for y axis movement
        if (isBlue)
        {
            //---single movement
            if(deltaMoveX == 1 && deltaMoveY == 0)
            {
                return true;
            }
            if (deltaMoveY == 1 && deltaMoveX == 0)
            {
                return true;
            }
            if((x1-x2) == -(y1-y2) && deltaMoveX == 1 && deltaMoveY == 1)
            {
                return true;
            }

            //---jumping movement
            if(deltaMoveX == 2)
            {
                if (deltaMoveY == 2)
                {
                    Piece p = board[(x1 + x2) / 2, (y1 + y2) / 2];
                    if(p != null)
                    {
                        return true;
                    }
                }
            }
            if (deltaMoveY == 2 && deltaMoveX == 0)
            {
                Piece p = board[(x1 + x2) / 2, (y1 + y2) / 2];
                if (p != null)
                {
                    return true;
                }
            }
            if (deltaMoveX == 2 && deltaMoveY == 0)
            {
                Piece p = board[(x1 + x2) / 2, (y1 + y2) / 2];
                if (p != null)
                {
                    return true;
                }
            }
        }

        if (!isBlue)
        {
            //---single movement
            if (deltaMoveX == 1 && deltaMoveY == 0)
            {
                return true;
            }
            if (deltaMoveY == 1 && deltaMoveX == 0)
            {
                return true;
            }
            if ((x1 - x2) == -(y1 - y2) && deltaMoveX == 1 && deltaMoveY == 1)
            {
                return true;
            }

            //---jumping movement
            //diagonal
            if (deltaMoveX == 2)
            {
                if (deltaMoveY == 2)
                {
                    Piece p = board[(x1 + x2) / 2, (y1 + y2) / 2];
                    if (p != null)
                    {
                        return true;
                    }
                }
            }
            //y-axis
            if (deltaMoveY == 2 && deltaMoveX == 0)
            {
                Piece p = board[(x1 + x2) / 2, (y1 + y2) / 2];
                if (p != null)
                {
                    return true;
                }
            }
            //x-axis
            if (deltaMoveX == 2 && deltaMoveY == 0)
            {
                Piece p = board[(x1 + x2) / 2, (y1 + y2) / 2];
                if (p != null)
                {
                    return true;
                }
            }
        }

        return false;
    }
    
    public bool[,] ValidMovesAi()
    {
        bool[,] r = new bool[9, 9];

        Piece p;
        int x, y;

        x = CurrentX;
        y = CurrentY;

        //for left
        if(x != 0)
        {
            p = AIBoard.Instance.pieces[x - 1, y];//target position
            if(p == null)//no piece at target
            {
                r[x - 1, y] = true;//move is legal
            }
            else if(p != null)//piece at target
            {
                r[x - 1, y] = false;//move not legal
            }
        }
        //for right
        if (x != 8)
        {
            p = AIBoard.Instance.pieces[x + 1, y];//target position
            if (p == null)//no piece at target
            {
                r[x + 1, y] = true;//move is legal
            }
            else if (p != null)//piece at target
            {
                r[x + 1, y] = false;//move not legal
            }
        }
        //for down
        if (CurrentY != 0)
        {
            p = AIBoard.Instance.pieces[x, y - 1];//target position
            if (p == null)//no piece at target
            {
                r[x, y - 1] = true;//move is legal
            }
            else if (p != null)//piece at target
            {
                r[x, y - 1] = false;//move not legal
            }
        }
        //for up
        if (y != 8)
        {
            p = AIBoard.Instance.pieces[x, CurrentY + 1];//target position
            if (p == null)//no piece at target
            {
                r[x, y + 1] = true;//move is legal
            }
            else if (p != null)//piece at target
            {
                r[x, y + 1] = false;//move not legal
            }
        }
        //for down-right
        if (x != 8 && y != 0)
        {
            p = AIBoard.Instance.pieces[x + 1, y - 1];//target position
            if (p == null)//no piece at target
            {
                r[x + 1, y - 1] = true;//move is legal
            }
            else if (p != null)//piece at target
            {
                r[x + 1, y - 1] = false;//move not legal
            }
        }
        //for up-left
        if (x != 0 && y != 8)
        {
            p = AIBoard.Instance.pieces[x - 1, y + 1];//target position
            if (p == null)//no piece at target
            {
                r[x - 1, y + 1] = true;//move is legal
            }
            else if (p != null)//piece at target
            {
                r[x - 1, y + 1] = false;//move not legal
            }
        }
        return r;
    }

    public bool[,] ValidJumpsAi()
    {
        bool[,] r = new bool[9, 9];

        Piece p;
        int x, y;

        x = CurrentX;
        y = CurrentY;

        //top-left
        if (x >= 2 && y <= 6)
        {
            p = AIBoard.Instance.pieces[x - 1, y + 1];  //piece to jump
            if (p != null)   //if there is a piece there
            {
                //check if possible to land after jump
                if (AIBoard.Instance.pieces[x - 2, y + 2] == null)
                {
                    r[x - 2, y + 2] = true;
                }
            }
        }
        //left
        if (x >= 2)
        {
            p = AIBoard.Instance.pieces[x - 1, y];  //piece to jump
            if (p != null)   //if there is a piece there
            {
                //check if possible to land after jump
                if (AIBoard.Instance.pieces[x - 2, y] == null)
                {
                    r[x - 2, y] = true;
                }
            }
        }
        //down
        if (y >= 2)
        {
            p = AIBoard.Instance.pieces[x, y - 1];  //piece to jump
            if (p != null)   //if there is a piece there
            {
                //check if possible to land after jump
                if (AIBoard.Instance.pieces[x, y - 2] == null)
                {
                    r[x, y - 2] = true;
                }
            }
        }
        //bottom right
        if (x <= 6 && y >= 2)
        {
            p = AIBoard.Instance.pieces[x + 1, y - 1];  //piece to jump
            if (p != null)   //if there is a piece there
            {
                //check if possible to land after jump
                if (AIBoard.Instance.pieces[x + 2, y - 2] == null)
                {
                    r[x + 2, y - 2] = true;
                }
            }
        }
        //right
        if (x <= 6)
        {
            p = AIBoard.Instance.pieces[x + 1, y];  //piece to jump
            if (p != null)   //if there is a piece there
            {
                //check if possible to land after jump
                if (AIBoard.Instance.pieces[x + 2, y] == null)
                {
                    r[x + 2, y] = true;
                }
            }
        }
        //up
        if (y <= 6)
        {
            p = AIBoard.Instance.pieces[x, y + 1];  //piece to jump
            if (p != null)   //if there is a piece there
            {
                //check if possible to land after jump
                if (AIBoard.Instance.pieces[x, y + 2] == null)
                {
                    r[x, y + 2] = true;
                }
            }
        }
        return r;
    }

    public bool[,] ValidMoves()
    {
        bool[,] r = new bool[9, 9];

        Piece p;
        int x, y;

        x = CurrentX;
        y = CurrentY;

        //for left
        if (x != 0)
        {
            p = GameBoard.Instance.pieces[x - 1, y];//target position
            if (p == null)//no piece at target
            {
                r[x - 1, y] = true;//move is legal
            }
            else if (p != null)//piece at target
            {
                r[x - 1, y] = false;//move not legal
            }
        }
        //for right
        if (x != 8)
        {
            p = GameBoard.Instance.pieces[x + 1, y];//target position
            if (p == null)//no piece at target
            {
                r[x + 1, y] = true;//move is legal
            }
            else if (p != null)//piece at target
            {
                r[x + 1, y] = false;//move not legal
            }
        }
        //for down
        if (CurrentY != 0)
        {
            p = GameBoard.Instance.pieces[x, y - 1];//target position
            if (p == null)//no piece at target
            {
                r[x, y - 1] = true;//move is legal
            }
            else if (p != null)//piece at target
            {
                r[x, y - 1] = false;//move not legal
            }
        }
        //for up
        if (y != 8)
        {
            p = GameBoard.Instance.pieces[x, CurrentY + 1];//target position
            if (p == null)//no piece at target
            {
                r[x, y + 1] = true;//move is legal
            }
            else if (p != null)//piece at target
            {
                r[x, y + 1] = false;//move not legal
            }
        }
        //for down-right
        if (x != 8 && y != 0)
        {
            p = GameBoard.Instance.pieces[x + 1, y - 1];//target position
            if (p == null)//no piece at target
            {
                r[x + 1, y - 1] = true;//move is legal
            }
            else if (p != null)//piece at target
            {
                r[x + 1, y - 1] = false;//move not legal
            }
        }
        //for up-left
        if (x != 0 && y != 8)
        {
            p = GameBoard.Instance.pieces[x - 1, y + 1];//target position
            if (p == null)//no piece at target
            {
                r[x - 1, y + 1] = true;//move is legal
            }
            else if (p != null)//piece at target
            {
                r[x - 1, y + 1] = false;//move not legal
            }
        }
        return r;
    }

    public bool[,] ValidJumps()
    {
        bool[,] r = new bool[9, 9];

        Piece p;
        int x, y;

        x = CurrentX;
        y = CurrentY;

        //top-left
        if (x >= 2 && y <= 6)
        {
            p = GameBoard.Instance.pieces[x - 1, y + 1];  //piece to jump
            if (p != null)   //if there is a piece there
            {
                //check if possible to land after jump
                if (GameBoard.Instance.pieces[x - 2, y + 2] == null)
                {
                    r[x - 2, y + 2] = true;
                }
            }
        }
        //left
        if (x >= 2)
        {
            p = GameBoard.Instance.pieces[x - 1, y];  //piece to jump
            if (p != null)   //if there is a piece there
            {
                //check if possible to land after jump
                if (GameBoard.Instance.pieces[x - 2, y] == null)
                {
                    r[x - 2, y] = true;
                }
            }
        }
        //down
        if (y >= 2)
        {
            p = GameBoard.Instance.pieces[x, y - 1];  //piece to jump
            if (p != null)   //if there is a piece there
            {
                //check if possible to land after jump
                if (GameBoard.Instance.pieces[x, y - 2] == null)
                {
                    r[x, y - 2] = true;
                }
            }
        }
        //bottom right
        if (x <= 6 && y >= 2)
        {
            p = GameBoard.Instance.pieces[x + 1, y - 1];  //piece to jump
            if (p != null)   //if there is a piece there
            {
                //check if possible to land after jump
                if (GameBoard.Instance.pieces[x + 2, y - 2] == null)
                {
                    r[x + 2, y - 2] = true;
                }
            }
        }
        //right
        if (x <= 6)
        {
            p = GameBoard.Instance.pieces[x + 1, y];  //piece to jump
            if (p != null)   //if there is a piece there
            {
                //check if possible to land after jump
                if (GameBoard.Instance.pieces[x + 2, y] == null)
                {
                    r[x + 2, y] = true;
                }
            }
        }
        //up
        if (y <= 6)
        {
            p = GameBoard.Instance.pieces[x, y + 1];  //piece to jump
            if (p != null)   //if there is a piece there
            {
                //check if possible to land after jump
                if (GameBoard.Instance.pieces[x, y + 2] == null)
                {
                    r[x, y + 2] = true;
                }
            }
        }
        return r;
    }

    public bool[] FlatMoves()
    {
        bool[] r = new bool[81];

        Piece p;
        int x = FlatPosition;

        //for left
        if(x > 8)
        {
            p = AIBoard.Instance.MinimaxBoard[x - 9];//target position
            if (p == null)//no piece at target
            {
                r[x - 9] = true;//move is legal
            }
            else if (p != null)//piece at target
            {
                r[x - 9] = false;//move not legal
            }
        }
        //for right
        if (x < 72)
        {
            p = AIBoard.Instance.MinimaxBoard[x + 9];
            if (p == null)
            {
                r[x + 9] = true;
            }
            else if (p != null)
            {
                r[x + 9] = false;
            }
        }
        //for up
        if ((x % 9) != 8)
        {
            p = AIBoard.Instance.MinimaxBoard[x + 1];
            if (p == null)
            {
                r[x + 1] = true;
            }
            else if (p != null)
            {
                r[x + 1] = false;
            }
        }
        //for down
        if ((x % 9) != 0)
        {
            p = AIBoard.Instance.MinimaxBoard[x - 1];
            if (p == null)
            {
                r[x - 1] = true;
            }
            else if (p != null)
            {
                r[x - 1] = false;
            }
        }
        //for down-right
        if ((x % 9) != 0 && x < 72)
        {
            p = AIBoard.Instance.MinimaxBoard[x + 8];
            if (p == null)
            {
                r[x + 8] = true;
            }
            else if (p != null)
            {
                r[x + 8] = false;
            }
        }
        //for up-left
        if ((x % 9) != 8 && x > 8)
        {
            p = AIBoard.Instance.MinimaxBoard[x - 8];
            if (p == null)
            {
                r[x - 8] = true;
            }
            else if (p != null)
            {
                r[x - 8] = false;
            }
        }
        return r;
    }
    public bool[] FlatJumps()
    {
        bool[] r = new bool[81];

        Piece p, q;
        int x = FlatPosition;

        //left
        if(x > 17)
        {
            p = AIBoard.Instance.MinimaxBoard[x - 9];   //piece to jump
            if(p != null)   //if there is a piece there
            {
                q = AIBoard.Instance.MinimaxBoard[x - 18];  //target position
                if (q == null)  //no piece at target
                {
                    r[x - 18] = true;   //move is legal
                }
            }
        }
        //right
        if (x < 63)
        {
            p = AIBoard.Instance.MinimaxBoard[x + 9];
            if(p != null)
            {
                q = AIBoard.Instance.MinimaxBoard[x + 18];
                if (q == null)
                {
                    r[x + 18] = true;
                }
            }
        }
        //up
        if ((x % 9) < 7)
        {
            p = AIBoard.Instance.MinimaxBoard[x + 1];
            if(p != null)
            {
                q = AIBoard.Instance.MinimaxBoard[x + 2];
                if (q == null)
                {
                    r[x + 2] = true;
                }
            }
        }
        //down
        if ((x % 9) > 1)
        {
            p = AIBoard.Instance.MinimaxBoard[x - 1];
            if(p != null)
            {
                q = AIBoard.Instance.MinimaxBoard[x - 2];
                if (q == null)
                {
                    r[x - 2] = true;
                }
            }
        }
        //down-right
        if ((x % 9) > 1 && x < 63)
        {
            p = AIBoard.Instance.MinimaxBoard[x + 8];
            if(p != null)
            {
                q = AIBoard.Instance.MinimaxBoard[x + 16];
                if (q == null)
                {
                    r[x + 16] = true;
                }
            }
        }
        //up-left
        if ((x % 9) < 7 && x > 17)
        {
            p = AIBoard.Instance.MinimaxBoard[x - 8];
            if(p != null)
            {
                q = AIBoard.Instance.MinimaxBoard[x - 16];
                if (q == null)
                {
                    r[x - 16] = true;
                }
            }
        }
        return r;
    }
    public bool[] AllFlatMoves()
    {
        bool[] r = new bool[81];

        Piece p = AIBoard.Instance.MinimaxBoard[FlatPosition];

        for (int i = 0; i < 81; i++)
        {
            if(p.FlatMoves()[i] || p.FlatJumps()[i])
            {
                r[i] = true;
            }
        }
        return r;
    }

    public int GetRedWeight()
    {
        Piece p = AIBoard.Instance.MinimaxBoard[FlatPosition];
        int weight = 0;

        weight = redWeights[p.FlatPosition];

        return weight;
    }
    public int GetBlueWeight()
    {
        Piece p = AIBoard.Instance.MinimaxBoard[FlatPosition];
        int weight = 0;

        weight = blueWeights[p.FlatPosition];

        return weight;
    }

    public bool CanFlatJump(int previousPosition)
    {
        Piece p = AIBoard.Instance.MinimaxBoard[FlatPosition];

        for (int i = 0; i < 81; i++)
        {
            if (p.FlatJumps()[i] && i != previousPosition) //prevent jumping back
            {
                return true;
            }
        }
        return false;
    }
}
