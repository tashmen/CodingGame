using Algorithms.GameComponent;
using GameSolution.Entities;
using System;
using System.Collections;
using System.Linq;

namespace GameSolution.Game
{
    public class GameState : IGameState
    {
        public static int MaxTurns = 100;
        public Board Board { get; private set; }
        public int Turn { get; set; }

        public int[] MyProtein { get; set; }
        public int[] OppProtein { get; set; }

        public Move? maxMove { get; set; }
        public Move? minMove { get; set; }

        public GameState()
        {
            Turn = 0;
            maxMove = null;
            minMove = null;
        }

        public GameState(GameState state)
        {
            Board = state.Board.Clone();
            Turn = state.Turn;
            MyProtein = state.MyProtein.ToArray();
            OppProtein = state.OppProtein.ToArray();
            maxMove = state.maxMove;
            minMove = state.minMove;
        }

        public void SetNextTurn(Board board, int[] myProtein, int[] oppProtein)
        {
            Turn++;
            this.Board = board;
            MyProtein = myProtein;
            OppProtein = oppProtein;
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
            return new GameState(this);
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

            var myEntities = Board.GetMyEntityCount();
            var oppEntities = Board.GetOppEntityCount();

            var myHarvestProteins = Board.GetHarvestProteins(true);
            double myHarvestProteinsSum = myHarvestProteins.Sum();
            var oppHarvestProteins = Board.GetHarvestProteins(false);
            double oppHarvestProteinsSum = oppHarvestProteins.Sum();

            int myNumUniqueProteins = myHarvestProteins.Where(p => p > 1).Count();
            int oppNumUniqueProteins = oppHarvestProteins.Where(p => p > 1).Count();

            int myProteinBoost = myNumUniqueProteins * 5;
            int oppProteinBoost = myNumUniqueProteins * 5;

            var proteinValue = (myHarvestProteinsSum + myProteinBoost - oppProteinBoost - oppHarvestProteinsSum) / (myHarvestProteinsSum + oppHarvestProteinsSum + 1 + myProteinBoost + oppProteinBoost) * 0.2;


            var myProtein = MyProtein.Sum();
            var oppProtein = OppProtein.Sum();
            value = (((double)myEntities - oppEntities) / (myEntities + oppEntities + 1) * 0.2) + (((double)myProtein - oppProtein) / (myProtein + oppProtein + 1) * 0.0001) + proteinValue;

            if (value > 1 || value < -1)
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

        public IList GetPossibleMoves(bool isMax)
        {
            int[] proteins = GetProteins(isMax);

            return Board.GetMoves(proteins, isMax);
        }

        public double? GetWinner()
        {
            double? winner = null;

            var myEntities = Board.GetMyEntityCount();
            var oppEntities = Board.GetOppEntityCount();

            if (Turn < 100)
            {
                if (myEntities == 0 && oppEntities > 0)
                    winner = -1;
                else if (myEntities > 0 && oppEntities == 0)
                    winner = 1;
                else if (myEntities == 0 && oppEntities == 0)
                    winner = 0;

                bool hasNoMyProteinsToBuild = MyProtein[0] == 0 && ((MyProtein[1] == 0 && MyProtein[2] == 0) || (MyProtein[1] == 0 && MyProtein[3] == 0) || (MyProtein[2] == 0 && MyProtein[3] == 0));
                bool hasNoOppProteinsToBuild = OppProtein[0] == 0 && ((OppProtein[1] == 0 && OppProtein[2] == 0) || (OppProtein[1] == 0 && OppProtein[3] == 0) || (OppProtein[2] == 0 && OppProtein[3] == 0));

                if (hasNoMyProteinsToBuild && myEntities < oppEntities)
                    winner = -1;
                if (hasNoOppProteinsToBuild && oppEntities < myEntities)
                    winner = 1;
                if (hasNoMyProteinsToBuild && hasNoOppProteinsToBuild && myEntities == oppEntities)
                    winner = 0;
            }

            if (Turn == 100 || Board.IsFull())
            {
                if (myEntities > oppEntities)
                {
                    winner = 1;
                }
                else if (myEntities < oppEntities)
                {
                    winner = -1;
                }
                else
                {
                    if (MyProtein.Sum() > OppProtein.Sum())
                    {
                        winner = 1;
                    }
                    else if (MyProtein.Sum() < OppProtein.Sum())
                    {
                        winner = -1;
                    }
                    else winner = 0;
                }
            }

            return winner;
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
