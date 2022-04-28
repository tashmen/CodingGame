using GameSolution.Entities;
using System;
using System.Collections;
using Xunit;
using Xunit.Abstractions;

namespace UnitTest
{
    [Collection("MemoryAllocator")]
    public class MoveTests
    {
        public MoveTests(ITestOutputHelper output)
        {
            var converter = new Converter(output);
            Console.SetError(converter);
        }

        [Fact]
        public void Validate_Correct_Information_Tests()
        {
            Move move = new Move();
            move.AddHeroMove(15, 20, 0);
            move.AddWaitMove(1);
            move.AddWindSpellMove(7878, 9000, 2);

            Assert.Equal(15, HeroMove.GetX(move.GetMove(0)));
            Assert.Equal(20, HeroMove.GetY(move.GetMove(0)));
            Assert.Equal(MoveType.MOVE, HeroMove.GetMoveType(move.GetMove(0)));

            Assert.Equal(MoveType.SPELL, HeroMove.GetMoveType(move.GetMove(2)));
            Assert.Equal(7878, HeroMove.GetX(move.GetMove(2)));
            Assert.Equal(9000, HeroMove.GetY(move.GetMove(2)));

            Assert.Equal($"MOVE 15 20{Environment.NewLine}WAIT{Environment.NewLine}SPELL WIND 7878 9000{Environment.NewLine}", move.ToString());
        }
    }
}
