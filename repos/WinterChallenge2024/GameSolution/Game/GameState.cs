using Algorithms.GameComponent;
using Algorithms.Utility;
using GameSolution.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace GameSolution.Game
{
    public class GameState : PooledObject<GameState>, IGameState
    {
        public static int MaxTurns = 100;

        public Board Board;
        public int Turn;

        public int[] MyProtein;
        public int[] OppProtein;

        public Move? maxMove;
        public Move? minMove;

        static GameState()
        {
            SetInitialCapacity(20000);
        }

        public GameState()
        {
            Turn = 0;
            MyProtein = new int[4];
            OppProtein = new int[4];
            maxMove = null;
            minMove = null;
            _myMoves = new List<Move>();
            _oppMoves = new List<Move>();
        }

        protected override void Reset()
        {
            Board.Dispose();
            _myMoves.Clear();
            _oppMoves.Clear();
        }

        private GameState CopyFrom(GameState state)
        {
            Board = state.Board.Clone();
            Turn = state.Turn;
            Array.Copy(state.MyProtein, MyProtein, state.MyProtein.Length);
            Array.Copy(state.OppProtein, OppProtein, state.OppProtein.Length);
            maxMove = state.maxMove;
            minMove = state.minMove;
            _myMoves.AddRange(state._myMoves);
            _oppMoves.AddRange(state._oppMoves);
            return this;
        }

        public void SetNextTurn(Board board, int[] myProtein, int[] oppProtein)
        {
            Turn++;
            Board = board;
            MyProtein = myProtein;
            OppProtein = oppProtein;
            _myMoves.Clear();
            _oppMoves.Clear();
            UpdateGameState();
        }

        public void UpdateGameState()
        {
            Board.UpdateBoard();
        }

        public void ApplyMove(object move, bool isMax)
        {
            Move m = (Move)move;
            if (isMax)
            {
                maxMove = m;
                minMove = null;
            }
            else
            {
                if (maxMove == null)
                    throw new Exception("Expected max to play first.");
                minMove = m;
            }

            if (maxMove != null && minMove != null)
            {
                ApplyMove(maxMove, MyProtein);
                ApplyMove(minMove, OppProtein);

                Board.ApplyMove(maxMove, minMove);
                //after applymove but before SetNextTurn there is a possibility that we pull an object from the Entity pool that was previously used in a cache.
                //To avoid this, set the cache higher so it doesn't run out.
                Board.Harvest(true, MyProtein);
                Board.Harvest(false, OppProtein);

                Board.Attack();

                SetNextTurn(Board, MyProtein, OppProtein);
            }
        }

        public void ApplyMove(Move move, int[] proteins)
        {
            foreach (MoveAction action in move.Actions)
            {
                if (action.Type == MoveType.GROW || action.Type == MoveType.SPORE)
                {
                    ApplyCost(proteins, action.GetCost());
                    if (action.EntityType == EntityType.BASIC)
                    {
                        Entity entity = Board.GetEntityByLocation(action.Location);
                        if (entity != null)
                        {
                            proteins[entity.Type - EntityType.A] += 3;
                        }
                    }
                }
            }
        }

        public void ApplyCost(int[] proteins, int[] cost)
        {
            for (int i = 0; i < 4; i++)
            {
                proteins[i] -= cost[i];
                if (proteins[i] < 0)
                {
                    throw new Exception("Invalid move played; proteins can't be negative");
                }
            }
        }

        public IGameState Clone()
        {
            GameState cleanState = Get();
            cleanState.CopyFrom(this);
            return cleanState;
        }

        public bool Equals(IGameState state)
        {
            GameState gameState = state as GameState;

            if (this.Turn != gameState.Turn)
                return false;

            if ((maxMove == null && gameState.maxMove != null) || (maxMove != null && gameState.maxMove == null))
                return false;
            if ((minMove == null && gameState.minMove != null) || (minMove != null && gameState.minMove == null))
                return false;

            for (int i = 0; i < 4; i++)
            {
                if (this.MyProtein[i] != gameState.MyProtein[i])
                    return false;
                if (this.OppProtein[i] != gameState.OppProtein[i])
                    return false;
            }

            if (!this.Board.Equals(gameState.Board))
                return false;

            return true;
        }

        public int GetGlobalOrganId()
        {
            return Board.GlobalOrganId;
        }

        public double Evaluate(bool isMax)
        {
            double value;

            int myEntities = Board.GetMyEntityCount();
            int oppEntities = Board.GetOppEntityCount();

            int[] myHarvestProteins = Board.GetHarvestProteins(true);
            double myHarvestProteinsSum = myHarvestProteins.Sum();
            int[] oppHarvestProteins = Board.GetHarvestProteins(false);
            double oppHarvestProteinsSum = oppHarvestProteins.Sum();

            int myNumUniqueProteins = myHarvestProteins.Where(p => p > 0).Count();
            int oppNumUniqueProteins = oppHarvestProteins.Where(p => p > 0).Count();

            int myProteinBoost = myNumUniqueProteins * 5;
            int oppProteinBoost = myNumUniqueProteins * 5;

            double proteinValue = (myHarvestProteinsSum + myProteinBoost - oppProteinBoost - oppHarvestProteinsSum) / (myHarvestProteinsSum + oppHarvestProteinsSum + 1 + myProteinBoost + oppProteinBoost);

            int myEntityLife = Board.GetEntityLifeCount(true);
            int oppEntityLife = Board.GetEntityLifeCount(false);
            int totalLife = Board.GetInitialOpenSpacesCount();
            double lifeScore = (myEntityLife - oppEntityLife) / (double)totalLife;

            int myProtein = MyProtein.Sum();
            int oppProtein = OppProtein.Sum();
            value = ((double)myEntities - oppEntities) / (myEntities + oppEntities + 1) * 0.2;
            value += ((double)myProtein - oppProtein) / (myProtein + oppProtein + 1) * 0.0001;
            value += proteinValue * 0.2;
            value += lifeScore * 0.5;


            if (value >= 1 || value <= -1)
                Console.Error.WriteLine("Evaluation too high");

            return value;
        }

        public object GetMove(bool isMax)
        {
            return isMax ? maxMove : minMove;
        }

        public int[] GetProteins(bool isMine)
        {
            return isMine ? MyProtein : OppProtein;
        }

        private List<Move> _myMoves;
        private List<Move> _oppMoves;

        private List<Move> GetMoves(bool isMax)
        {
            return isMax ? _myMoves : _oppMoves;
        }
        private void SetMoves(List<Move> moves, bool isMax)
        {
            if (isMax)
            {
                _myMoves = moves;
            }
            else
            {
                _oppMoves = moves;
            }
        }
        public IList GetPossibleMoves(bool isMax)
        {
            if (Turn <= 100)
            {
                int[] proteins = GetProteins(isMax);

                List<Move> moves = GetMoves(isMax);
                if (moves.Count == 0)
                {
                    Stopwatch watch = new Stopwatch();
                    watch.Start();
                    moves = Board.GetMoves(proteins, isMax);
                    SetMoves(moves, isMax);
                    watch.Stop();
                    if (watch.ElapsedMilliseconds > 20)
                    {
                        Console.Error.WriteLine($"Move generation: {watch.ElapsedMilliseconds}ms");
                        foreach (Move move in moves)
                        {
                            Console.Error.WriteLine($"{move}");
                        }
                        Print();
                    }
                }

                return moves;
            }
            else return new List<Move>();
        }

        public double? GetWinner()
        {
            double? winner = GetWinnerInternal();

            /*
            if (winner != null)
            {
                Print();
                Console.Error.WriteLine(winner);
            }
            */

            return winner;
        }

        private double? GetWinnerInternal()
        {
            int myEntitiesCount = Board.GetMyEntityCount();
            int oppEntitiesCount = Board.GetOppEntityCount();

            if (Turn < 100)
            {
                if (myEntitiesCount == 0)
                    return CheckGameEnd(myEntitiesCount, oppEntitiesCount);
                else if (oppEntitiesCount == 0)
                    return CheckGameEnd(myEntitiesCount, oppEntitiesCount);

                bool hasNoMyProteinsToBuild = MyProtein[0] == 0 && ((MyProtein[1] == 0 && MyProtein[2] == 0) || (MyProtein[1] == 0 && MyProtein[3] == 0) || (MyProtein[2] == 0 && MyProtein[3] == 0));
                bool hasNoOppProteinsToBuild = OppProtein[0] == 0 && ((OppProtein[1] == 0 && OppProtein[2] == 0) || (OppProtein[1] == 0 && OppProtein[3] == 0) || (OppProtein[2] == 0 && OppProtein[3] == 0));

                if (hasNoMyProteinsToBuild && myEntitiesCount < oppEntitiesCount)
                    return -1;
                if (hasNoOppProteinsToBuild && oppEntitiesCount < myEntitiesCount)
                    return 1;
                if (hasNoMyProteinsToBuild && hasNoOppProteinsToBuild)
                    return CheckGameEnd(myEntitiesCount, oppEntitiesCount);

                double currentWinner = CheckGameEnd(myEntitiesCount, oppEntitiesCount);

                bool hasOppMoves = HasMoves(GetPossibleMoves(false));
                if (!hasOppMoves && currentWinner == 1)
                    return 1;

                bool hasMyMoves = HasMoves(GetPossibleMoves(true));
                if (!hasMyMoves && currentWinner == -1)
                    return -1;

            }

            if (Turn >= 100 || Board.IsFull())
            {
                return CheckGameEnd(myEntitiesCount, oppEntitiesCount);
            }

            return null;
        }

        private bool HasMoves(IList moves)
        {
            if (moves.Count == 1)
            {
                Move? theMove = moves[0] as Move;
                bool isAllWait = true;
                foreach (MoveAction action in theMove.Actions)
                {
                    if (action.Type != MoveType.WAIT)
                    {
                        isAllWait = false;
                        break;
                    }
                }
                return !isAllWait;
            }
            return true;
        }

        private double CheckGameEnd(int myEntitiesCount, int oppEntitiesCount)
        {

            if (myEntitiesCount > oppEntitiesCount)
            {
                return 1;
            }
            else if (myEntitiesCount < oppEntitiesCount)
            {
                return -1;
            }
            else
            {
                int myProteinTotal = MyProtein.Sum();
                int oppProteinTotal = OppProtein.Sum();
                if (myProteinTotal > oppProteinTotal)
                {
                    return 1;
                }
                else if (myProteinTotal < oppProteinTotal)
                {
                    return -1;
                }
                else return 0;
            }
        }

        public void Print()
        {
            Console.Error.WriteLine(Turn);
            Console.Error.WriteLine(string.Join(',', MyProtein));
            Console.Error.WriteLine(string.Join(',', OppProtein));
            Board.Print();
        }
    }
}
