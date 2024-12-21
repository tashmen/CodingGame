using Algorithms.GameComponent;
using GameSolution.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GameSolution.Game
{
    public class GameState : IGameState
    {
        public static int MaxTurns = 100;
        public Board Board { get; private set; }
        public int Turn { get; set; }

        public Dictionary<EntityType, int> MyProtein { get; set; }
        public Dictionary<EntityType, int> OppProtein { get; set; }

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
            MyProtein = state.MyProtein.ToDictionary(e => e.Key, e => e.Value);
            OppProtein = state.OppProtein.ToDictionary(e => e.Key, e => e.Value);
            maxMove = state.maxMove;
            minMove = state.minMove;
        }

        public void SetNextTurn(Board board, Dictionary<EntityType, int> myProtein, Dictionary<EntityType, int> oppProtein)
        {
            Turn++;
            this.Board = board;
            MyProtein = myProtein;
            OppProtein = oppProtein;
            UpdateGameState();
        }

        public void UpdateGameState()
        {

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

                Board.ApplyMove(maxMove, true);
                Board.ApplyMove(minMove, false);

                Board.Harvest(true, MyProtein);
                Board.Harvest(false, OppProtein);

                SetNextTurn(Board, MyProtein, OppProtein);
            }
        }

        public void ApplyMove(Move move, Dictionary<EntityType, int> Proteins)
        {
            foreach (MoveAction action in move.Actions)
            {
                if (action.Type == MoveType.GROW)
                {
                    Proteins[EntityType.A]--;
                    Entity entity = Board.GetEntityByLocation(action.Location);
                    if (entity != null)
                    {
                        switch (entity.Type)
                        {
                            case EntityType.A:
                                Proteins[EntityType.A] += 3;
                                break;
                            case EntityType.B:
                                Proteins[EntityType.B] += 3;
                                break;
                            case EntityType.C:
                                Proteins[EntityType.C] += 3;
                                break;
                            case EntityType.D:
                                Proteins[EntityType.D] += 3;
                                break;
                        }
                    }
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

            foreach (EntityType type in MyProtein.Keys)
            {
                if (this.MyProtein[type] != gameState.MyProtein[type])
                    return false;
                if (this.OppProtein[type] != gameState.OppProtein[type])
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

            var myProteinA = MyProtein[EntityType.A];
            var oppProteinA = OppProtein[EntityType.A];
            value = (((double)myEntities - oppEntities) / (myEntities + oppEntities)) + (((double)myProteinA - oppProteinA) / (myProteinA + oppProteinA + 1) * 0.001);

            return value;
        }

        public object GetMove(bool isMax)
        {
            return isMax ? maxMove : minMove;
        }

        public Dictionary<EntityType, int> GetProteins(bool isMine)
        {
            return isMine ? MyProtein : OppProtein;
        }

        public IList GetPossibleMoves(bool isMax)
        {
            var moves = new List<Move>();
            var move = new Move();
            move.AddAction(MoveAction.CreateWait());
            moves.Add(move);

            Dictionary<EntityType, int> proteins = GetProteins(isMax);
            if (proteins[EntityType.A] > 0)
            {
                moves.AddRange(Board.GetGrowMoves(isMax));
            }

            return moves;
        }

        public double? GetWinner()
        {
            double? winner = null;

            var myEntities = Board.GetMyEntityCount();
            var oppEntities = Board.GetOppEntityCount();

            if (Turn < 100)
            {
                if (MyProtein[EntityType.A] == 0 && myEntities < oppEntities)
                    winner = -1;
                if (OppProtein[EntityType.A] == 0 && oppEntities < myEntities)
                    winner = 1;
                if (MyProtein[EntityType.A] == 0 && OppProtein[EntityType.A] == 0 && myEntities == oppEntities)
                    winner = 0;
            }

            if (Turn == 100)
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
                    if (MyProtein[EntityType.A] > OppProtein[EntityType.A])
                    {
                        winner = 1;
                    }
                    else if (MyProtein[EntityType.A] < OppProtein[EntityType.A])
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
            Console.Error.WriteLine(string.Join(',', MyProtein.Values));
            Console.Error.WriteLine(string.Join(',', OppProtein.Values));
            Board.Print();
        }
    }
}
