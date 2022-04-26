using Algorithms.Space;
using System.Collections.Generic;
using System.Linq;

namespace GameSolution.Entities
{
    public class Board
    {
        public IList<BoardPiece> boardPieces { get; set; }
        public Base myBase { get; set; }
        public Base opponentBase { get; set; }
        public IList<Monster> monsters { get; set; }
        public IList<Hero> myHeroes { get; set; }
        public IList<Hero> opponentHeroes { get; set; }


        public Board(IList<BoardPiece> boardPieces )
        {
            this.boardPieces = boardPieces;
            SetupBoard();
        }

        public Board(Board board)
        {
            boardPieces = board.boardPieces.Select(bp => bp.Clone()).ToList();
            SetupBoard();
        }

        public void SetupBoard()
        {
            monsters = new List<Monster>();
            myHeroes = new List<Hero>();
            opponentHeroes = new List<Hero>();
            foreach(BoardPiece piece in boardPieces)
            {
                if(piece is Base)
                {
                    Base b = piece as Base;
                    if (b.isMax.Value)
                    {
                        myBase = b;
                    }
                    else
                    {
                        opponentBase = b;
                    }
                }
                if(piece is Monster)
                {
                    monsters.Add(piece as Monster);
                }
                if(piece is Hero)
                {
                    Hero h = piece as Hero;
                    if (h.isMax.Value)
                    {
                        myHeroes.Add(h);
                    }
                    else
                    {
                        opponentHeroes.Add(h);
                    }
                }
            }
        }

        public void ApplyMove(Move move, bool isMax)
        {
            var heroes = isMax ? myHeroes : opponentHeroes;
            
            foreach(Hero hero in heroes)
            {
                HeroMove heroMove = move.GetMove(hero.id);
                switch (heroMove.moveType)
                {
                    case MoveType.MOVE:
                        var point = heroMove.point;
                        var newPoint = Space2d.TranslatePoint(hero.point, point, hero.speed);
                        hero.point = newPoint;
                        break;
                    case MoveType.SPELL:
                        break;
                }
            }
        }


        public Board Clone()
        {
            return new Board(this);
        }

        public double? GetWinner()
        {
            if(myBase.health == 0)
            {
                return -1;
            }
            else if (opponentBase.health == 0)
            {
                return 1;
            }

            return null;
        }
    }
}
