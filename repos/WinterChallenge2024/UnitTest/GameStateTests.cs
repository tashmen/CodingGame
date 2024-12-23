using Algorithms.Space;
using GameSolution.Entities;
using GameSolution.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace UnitTest
{

    public class GameStateTests
    {
        private GameState state;
        public GameStateTests(ITestOutputHelper output)
        {
            var converter = new Converter(output);
            Console.SetError(converter);

            state = GameBuilder.BuildEmptyGame();
        }

        [Fact]
        public void TestGameState()
        {
            state = GameBuilder.BuildBasicGame();

        }

        [Fact]
        public void TestStartMoves()
        {
            state = GameBuilder.BuildBasicGame();
            List<Move> myMoves = (List<Move>)state.GetPossibleMoves(true);
            Assert.Equal(4, myMoves.Count);

            List<Move> oppMoves = (List<Move>)state.GetPossibleMoves(false);
            Assert.Equal(4, oppMoves.Count);

            Move myMove = myMoves.First(m => m.Actions[0].Type == MoveType.GROW && m.Actions[0].Location.Equals(new Point2d(2, 2)));
            Move oppMove = oppMoves.First(m => m.Actions[0].Type == MoveType.GROW && m.Actions[0].Location.Equals(new Point2d(2, 6)));

            GameState oldState = state.Clone() as GameState;

            state.ApplyMove(myMove, true);
            state.ApplyMove(oppMove, false);

            Assert.Equal(5, state.GetGlobalOrganId());

            oldState.Board.GetEntities()[oldState.Board.GetNodeIndex(2, 2)] = new Entity(2, 2, "BASIC", 1, 3, "N", 1, 1);
            oldState.Board.GetEntities()[oldState.Board.GetNodeIndex(2, 6)] = new Entity(2, 6, "BASIC", 0, 4, "N", 2, 2);
            oldState.Board.GlobalOrganId = 5;
            oldState.Turn++;
            oldState.MyProtein[0]--;
            oldState.OppProtein[0]--;
            oldState.minMove = oppMove;
            oldState.maxMove = myMove;

            Assert.True(oldState.Equals(state));
        }

        [Fact]
        public void TestStartMoves_Wood2()
        {
            state = GameBuilder.BuildWood2Game();
            List<Move> myMoves = (List<Move>)state.GetPossibleMoves(true);
            Assert.Equal(3, myMoves.Count);
        }

    }
}
