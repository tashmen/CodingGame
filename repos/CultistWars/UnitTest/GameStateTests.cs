using GameSolution.Entities;
using GameSolution.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace UnitTest
{
    public class GameStateTests
    {
        [Fact]
        public void TestBresenham()
        {
            var state = GameHelper.ProblematicShooting();
            var endLocation = state.CheckBulletPath(Board.ConvertPointToLocation(3, 3), Board.ConvertPointToLocation(8, 4));
            Assert.Equal(Board.ConvertPointToLocation(4, 3), endLocation);
            endLocation = state.CheckBulletPath(Board.ConvertPointToLocation(8, 4), Board.ConvertPointToLocation(3, 3));
            Assert.Equal(Board.ConvertPointToLocation(7, 4), endLocation);

            state = GameHelper.ProblematicShooting2();
            endLocation = state.CheckBulletPath(Board.ConvertPointToLocation(0, 4), Board.ConvertPointToLocation(4, 3));
            Assert.Equal(Board.ConvertPointToLocation(2, 3), endLocation);
        }

        [Fact]
        public void TestNeutralMove()
        {
            var state = GameHelper.IncorrectNeutralMove();
            state.MoveNeutralUnit();
            Assert.Equal(Move.ToString(Move.MoveUnit(5, Board.ConvertPointToLocation(3, 4))), Move.ToString(state.NeutralLastMove));

            state = GameHelper.IncorrectNeutralMove2();
            Assert.Equal(Move.ToString(Move.MoveUnit(9, Board.ConvertPointToLocation(6, 2))), Move.ToString(state.NeutralLastMove));
            state.MoveNeutralUnit();
            Assert.Equal(-1, state.NeutralLastMove);

            state = GameHelper.IncorrectNeutralMove3();
            Assert.Equal(-1, state.NeutralLastMove);
            state.MoveNeutralUnit();
            Assert.Equal(Move.ToString(Move.MoveUnit(11, Board.ConvertPointToLocation(10, 4))), Move.ToString(state.NeutralLastMove));
        }
    }
}
