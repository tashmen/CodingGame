using Algorithms.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace GameSolution.Entities
{
    public class Board
    {
        public long MyPieces;
        public long OppPieces;

        public long EmptySpacesMap;

        public int Id;

        private double? Winner = null;
        private IList EmptySpaces = null;

        public static long[] WinningMasks = new long[] { 7, 56, 448, 73, 146, 292, 273, 84 };
        public IList[] EmptySpacesMapToMoves = null;
        public static double?[] PiecesToWinningMap = null;


        public Board(int id)
        {
            Id = id;

            if (EmptySpacesMapToMoves == null)
            {
                EmptySpacesMapToMoves = new IList[512];

                for (int c = 0; c < 512; c++)
                {
                    EmptySpacesMap = c;
                    EmptySpacesMapToMoves[c] = GetEmptySpacesInternal();
                }
            }

            if (PiecesToWinningMap == null)
            {
                PiecesToWinningMap = new double?[262144];
                for (MyPieces = 0; MyPieces < 512; MyPieces++)
                {
                    for(OppPieces = 0; OppPieces < 512; OppPieces++)
                    {
                        if(BitFunctions.NumberOfSetBits(MyPieces & OppPieces) == 0)
                        {
                            long joined = MyPieces | (OppPieces << 9);
                            PiecesToWinningMap[joined] = GetWinnerInternal();
                        }
                    }
                }
            }

            MyPieces = 0;
            OppPieces = 0;
            EmptySpacesMap = 511;
        }

        public Board(Board board)
        {
            MyPieces = board.MyPieces;
            OppPieces = board.OppPieces;
            Id = board.Id;
            Winner = board.Winner;

            EmptySpacesMap = board.EmptySpacesMap;
            EmptySpaces = null;
            EmptySpacesMapToMoves = board.EmptySpacesMapToMoves;
        }

        public static int ConvertRowColToIndex(int row, int col)
        {
            return row * 3 + col;
        }

        public void ApplyMove(Move move, bool isMax)
        {
            if(Id != move.BoardNumber)
            {
                throw new Exception("Wrong board for this move" + move.ToString());
            }

            int location = move.Row * 3 + move.Col;
            if (isMax)
            {
                MyPieces = BitFunctions.SetBit(MyPieces, location);
            }
            else
            {
                OppPieces = BitFunctions.SetBit(OppPieces, location);
            }
            EmptySpacesMap = BitFunctions.ClearBit(EmptySpacesMap, location);
            EmptySpaces = null;
        }

        public void ApplyDraw(Move move)
        {
            int location = move.Row * 3 + move.Col;
            EmptySpacesMap = BitFunctions.ClearBit(EmptySpacesMap, location);
            EmptySpaces = null;
        }

        public Board Clone()
        {
            return new Board(this);
        }

        public bool Equals(Board board)
        {
            return board.MyPieces == MyPieces && board.OppPieces == OppPieces;
        }

        public IList GetEmptySpaces()
        {
            if(EmptySpaces == null)
            {
                EmptySpaces = EmptySpacesMapToMoves[EmptySpacesMap];
            }
            return EmptySpaces;
        }

        private IList GetEmptySpacesInternal()
        {
            IList emptySpaces = new List<Move>();

            if(EmptySpacesMap == 0)
            {
                return new List<Move>();
            }

            for (int r = 0; r < 3; r++)
            {
                int row = r * 3;
                for (int c = 0; c < 3; c++)
                {
                    int location = row + c;
                    if (BitFunctions.IsBitSet(EmptySpacesMap, location))
                    {
                        emptySpaces.Add(new Move(r, c, Id));
                    }
                }
            }

            return emptySpaces;
        }

        public double? GetWinner()
        {
            if(Winner ==  null)
            {
                long joined = MyPieces | (OppPieces << 9);
                Winner = PiecesToWinningMap[joined];

                if (Winner == null && EmptySpacesMap == 0)
                {
                    Winner = 0;   
                }
            }

            return Winner;
        }

        private double? GetWinnerInternal()
        {
            int bitCount;
            int myBitCount = BitFunctions.NumberOfSetBits(MyPieces);
            int oppBitCount = BitFunctions.NumberOfSetBits(OppPieces);
            bitCount = myBitCount + oppBitCount;
            if (myBitCount < 3 && oppBitCount < 3)
            {
                return null;
            }

            foreach (long winningMask in WinningMasks)
            {
                if (myBitCount >= 3 && BitFunctions.NumberOfSetBits(MyPieces & winningMask) == 3)
                {
                    return 1;
                }
                else if (oppBitCount >= 3 && BitFunctions.NumberOfSetBits(OppPieces & winningMask) == 3)
                {
                    return -1;
                }
            }

            if (bitCount == 9)
            {
                return 0;//No spaces left so it's a draw
            }

            return null;//Game isn't over
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int r = 0; r < 3; r++)
            {
                int row = r * 3;
                for (int c = 0; c < 3; c++)
                {
                    int location = row + c;
                    if (BitFunctions.IsBitSet(MyPieces, location))
                    {
                        stringBuilder.Append("X");
                    }
                    else if(BitFunctions.IsBitSet(OppPieces, location))
                    {
                        stringBuilder.Append("O");
                    }
                    else
                    {
                        stringBuilder.Append("-");
                    }
                }
                stringBuilder.Append(Environment.NewLine);
            }
            return stringBuilder.ToString();
        }
    }
}
