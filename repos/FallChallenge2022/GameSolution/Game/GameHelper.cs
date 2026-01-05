using GameSolution.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameSolution.Game
{
    public class GameHelper
    {
        GameState State { get; set; }
        public GameHelper(GameState state)
        {
            State = state;
        }

        public Move GetMove()
        {
            Move move = new Move();

            var cells = State.Board.Cells;
            var myUnits = GetMyUnits(cells);
            var oppTiles = GetOpponentTilesWithoutRecycler(cells);
            var oppUnits = GetOppUnits(cells);

            foreach (var myUnit in myUnits)
            {
                var minDist = 99999;
                Cell target = null;
                foreach(var oppTile in oppTiles) 
                {
                    var dist = (int)oppTile.Point.GetDistance(myUnit.Point);
                    if(minDist > dist)
                    {
                        minDist = dist;
                        target = oppTile;
                    }
                }
                if(target!= null)
                    move.UnitActions.Add(MoveAction.CreateMove(myUnit.Units, myUnit.x, myUnit.y, target.x, target.y));
            }

            var myBuildableTiles = GetMyBuildableTiles(cells);
            if(myBuildableTiles.Count > 0 && State.myMatter >= 10 && myUnits.Sum(c => c.Units) <= oppUnits.Sum(c => c.Units))
            {
                var location = myBuildableTiles[0];
                move.UnitActions.Add(MoveAction.CreateBuild(location.x, location.y));
                State.myMatter -= 10;
            }

            if (State.myMatter >= 10)
            {
                var mySpawnableTiles = GetMySpawnableTiles(cells);
                if (mySpawnableTiles.Count > 0)
                {
                    var minDist = 9999;
                    Cell target = null;
                    foreach (var oppTile in oppTiles)
                    {
                        foreach (var myTile in mySpawnableTiles)
                        {
                            var dist = (int)myTile.Point.GetDistance(oppTile.Point);
                            if (minDist > dist)
                            {
                                minDist = dist;
                                target = myTile;
                            }
                        }
                    }

                    if(target != null)
                    {
                        move.UnitActions.Add(MoveAction.CreateSpawn(target.x, target.y, State.myMatter / 10));
                    }
                }
            }

            if (move.UnitActions.Count == 0)
                move.UnitActions.Add(MoveAction.CreateWait());

            return move;
        }


        public IList<Cell> GetOpponentTilesWithoutRecycler(IList<Cell> cells)
        {
            return cells.Where(c => c.Owner == Owner.Opponent && !c.HasRecycler).ToList();
        }
        public IList<Cell> GetOpponentTiles(IList<Cell> cells)
        {
            return cells.Where(c => c.Owner == Owner.Opponent).ToList();
        }

        public IList<Cell> GetMySpawnableTiles(IList<Cell> cells)
        {
            return cells.Where(c => c.Owner == Owner.Me && c.CanSpawn).ToList();
        }

        public IList<Cell> GetMyBuildableTiles(IList<Cell> cells)
        {
            return cells.Where(c => c.Owner == Owner.Me && c.CanBuild && !c.InRangeOfRecycler).ToList();
        }

        public IList<Cell> GetMyUnits(IList<Cell> cells)
        {
            return cells.Where(c => c.Owner == Owner.Me && c.Units > 0).ToList();
        }

        public IList<Cell> GetOppUnits (IList<Cell> cells)
        {
            return cells.Where(c => c.Owner == Owner.Opponent && c.Units > 0).ToList();
        }
    }
}
