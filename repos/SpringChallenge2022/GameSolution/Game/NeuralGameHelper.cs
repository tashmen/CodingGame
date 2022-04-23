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
        public NeuralNetwork heroNet;

        public NeuralGameHelper(GameState state)
        {
            this.state = state;

            heroNet = new NeuralNetwork(4, new int[] { 354, 177, 60, 15 }, 354);
        }

        public void ImportNetworkFromFile(string fileName)
        {
            using (var reader = new BinaryReader(new FileStream(fileName, FileMode.Open)))
            {
                heroNet = new NeuralNetwork(reader);
            }   
        }

        public void ExportNetworkToFile(string fileName)
        {
            using (var writer = new BinaryWriter(new FileStream(fileName, FileMode.Create)))
            {
                heroNet.Save(writer);
            }
        }

        public Move RunNeuralNetwork()
        {
            var game = state;

            Move move = new Move();

            double[] neuralInputs = new double[354];

            var distToBoardPiece1 = AddNeuralInputsForHero(game.board.myHeroes[0], move, heroNet, 0, ref neuralInputs);
            var distToBoardPiece2 = AddNeuralInputsForHero(game.board.myHeroes[1], move, heroNet, 118, ref neuralInputs);
            var distToBoardPiece3 = AddNeuralInputsForHero(game.board.myHeroes[2], move, heroNet, 236, ref neuralInputs);

            var output = heroNet.output(neuralInputs);

            ConvertOutputToMove(output, 0, move, distToBoardPiece1);
            ConvertOutputToMove(output, 5, move, distToBoardPiece2);
            ConvertOutputToMove(output, 10, move, distToBoardPiece3);

            return move;
        }

        public void ConvertOutputToMove(double[] output, int index, Move move, List<Tuple<double, BoardPiece>> distToBoardPiece)
        {
            int moveType = (int)(output[index++] * 3);
            int x1 = (int)(output[index++] * 17630);
            int y1 = (int)(output[index++] * 9000);
            int spellType = (int)(output[index++] * 3);
            int entityId = distToBoardPiece[(int)(output[index++] * distToBoardPiece.Count)].Item2.id;
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

        public List<Tuple<double, BoardPiece>> AddNeuralInputsForHero(Hero heroIn, Move move, NeuralNetwork heroNetIn, int neuralIndex, ref double[] neuralInputs)
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

            
            neuralInputs[neuralIndex++] = game.board.myBase.health;
            neuralInputs[neuralIndex++] = game.board.myBase.mana;
            neuralInputs[neuralIndex++] = game.board.myBase.x;
            neuralInputs[neuralIndex++] = game.board.myBase.y;
            neuralInputs[neuralIndex++] = game.board.opponentBase.health;
            neuralInputs[neuralIndex++] = game.board.opponentBase.mana;
            neuralInputs[neuralIndex++] = game.board.opponentBase.x;
            neuralInputs[neuralIndex++] = game.board.opponentBase.y;

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

            return distToBoardPiece;
        }
    }
}
