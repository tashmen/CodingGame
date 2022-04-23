using GameSolution.Algorithms.NeuralNetwork;
using GameSolution.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSolution.Game
{
    public class NeuralGameHelper
    {
        public GameState state;
        public NeuralNetwork heroNet1;
        public NeuralNetwork heroNet2;
        public NeuralNetwork heroNet3;
        public NeuralNetwork heroNet;

        public NeuralGameHelper(GameState state)
        {
            this.state = state;

            heroNet1 = new NeuralNetwork(4, new int[] { 354, 177, 60, 15 }, 354);

            /*
            heroNet1 = new NeuralNetwork(4, new int[] { 118, 59, 20, 5 }, 118);
            heroNet2 = new NeuralNetwork(4, new int[] { 118, 59, 20, 5 }, 118);
            heroNet3 = new NeuralNetwork(4, new int[] { 118, 59, 20, 5 }, 118);
            */
        }

        public void ImportNetworkFromFile(string fileName)
        {
            using (var reader = new BinaryReader(new FileStream(fileName, FileMode.Open)))
            {
                heroNet = new NeuralNetwork(reader);
                /*
                heroNet1 = new NeuralNetwork(reader);
                heroNet2 = new NeuralNetwork(reader);
                heroNet3 = new NeuralNetwork(reader);
                */
            }   
        }

        public void ExportNetworkToFile(string fileName)
        {
            using (var writer = new BinaryWriter(new FileStream(fileName, FileMode.Create)))
            {
                heroNet1.Save(writer);
                heroNet2.Save(writer);
                heroNet3.Save(writer);
            }
        }

        public Move RunNeuralNetwork()
        {
            var game = state;

            Move move = new Move();
            AddMoveForHero(game.board.myHeroes[0], move, heroNet1);
            AddMoveForHero(game.board.myHeroes[1], move, heroNet2);
            AddMoveForHero(game.board.myHeroes[2], move, heroNet3);

            return move;
        }

        public void AddMoveForHero(Hero heroIn, Move move, NeuralNetwork heroNetIn)
        {
            var game = state;
            var distToBoardPiece = new List<Tuple<double, BoardPiece>>();
            foreach (BoardPiece piece in game.board.boardPieces)
            {
                if (piece is Monster || piece is Hero)
                {
                    distToBoardPiece.Add(new Tuple<double, BoardPiece>(heroIn.GetDistance(piece), piece));
                }
            }

            distToBoardPiece = distToBoardPiece.OrderBy(bp => bp.Item1).ToList();

            double[] neuralInputs = new double[118];
            neuralInputs[0] = game.board.myBase.health;
            neuralInputs[1] = game.board.myBase.mana;
            neuralInputs[2] = game.board.myBase.x;
            neuralInputs[3] = game.board.myBase.y;
            neuralInputs[4] = game.board.opponentBase.health;
            neuralInputs[5] = game.board.opponentBase.mana;
            neuralInputs[6] = game.board.opponentBase.x;
            neuralInputs[7] = game.board.opponentBase.y;

            var neuralIndex = 8;

            for (int i = 0; i < 10; i++)
            {
                if (distToBoardPiece.Count > i)
                {
                    var boardpiece = distToBoardPiece[i].Item2;
                    if (boardpiece is Monster)
                    {
                        var monster = boardpiece as Monster;
                        neuralInputs[neuralIndex++] = monster.health;
                        neuralInputs[neuralIndex++] = monster.id;
                        neuralInputs[neuralIndex++] = monster.isControlled ? 1 : 0;
                        neuralInputs[neuralIndex++] = monster.isNearBase ? 1 : 0;
                        neuralInputs[neuralIndex++] = monster.shieldLife;
                        neuralInputs[neuralIndex++] = monster.vx;
                        neuralInputs[neuralIndex++] = monster.vy;
                        neuralInputs[neuralIndex++] = 0;//Type 0 = monster
                        neuralInputs[neuralIndex++] = monster.threatForMax.HasValue ? monster.threatForMax.Value ? 1 : 2 : 0;
                        neuralInputs[neuralIndex++] = monster.x;
                        neuralInputs[neuralIndex++] = monster.y;
                    }
                    else
                    {
                        var hero = distToBoardPiece[i].Item2;
                        neuralInputs[neuralIndex++] = 0;//heroes don't have health
                        neuralInputs[neuralIndex++] = hero.id;
                        neuralInputs[neuralIndex++] = hero.isControlled ? 1 : 0;
                        neuralInputs[neuralIndex++] = hero.isNearBase ? 1 : 0;
                        neuralInputs[neuralIndex++] = hero.shieldLife;
                        neuralInputs[neuralIndex++] = hero.vx;
                        neuralInputs[neuralIndex++] = hero.vy;
                        neuralInputs[neuralIndex++] = hero.isMax.Value ? 1 : 2;//Type 1 = my hero, 2 = opponent hero
                        neuralInputs[neuralIndex++] = -1;
                        neuralInputs[neuralIndex++] = hero.x;
                        neuralInputs[neuralIndex++] = hero.y;
                    }
                }
                else
                {
                    for (int j = 0; j < 10; j++)
                    {
                        neuralInputs[neuralIndex++] = 0;
                    }
                }
            }
            


            var output1 = heroNetIn.output(neuralInputs);

            int moveType = (int)(output1[0] * 3);
            int x1 = (int)(output1[1] * 17630);
            int y1 = (int)(output1[2] * 9000);
            int spellType = (int)(output1[3] * 3);
            int entityId = distToBoardPiece[(int)(output1[4] * distToBoardPiece.Count)].Item2.id;
            switch (moveType)
            {
                case (int)MoveType.MOVE:
                    move.AddHeroMove(x1, y1);
                    break;
                case (int)MoveType.SPELL:
                    move.AddSpellMove(x1, y1, (SpellType)spellType, entityId);
                    break;
                case (int)MoveType.WAIT:
                    move.AddWaitMove();
                    break;
            }
        }
    }
}
