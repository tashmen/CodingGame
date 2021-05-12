using GameSolution.Entities;
using GameSolution.Moves;
using System;
using System.Collections.Generic;
using System.Linq;
using static GameSolution.Constants;

namespace GameSolution.Utility
{
    class GameHelper
    {
        private GameState currentState;
        private List<Move> possibleActions;

        public GameHelper(GameState state, List<Move> possibleActions)
        {
            currentState = state;
            this.possibleActions = possibleActions;
        }

        public Move GetNextMove()
        {
            Move bestAction = null;
            Move treeInShadow = FindShadowedTree();
            if (treeInShadow != null)
            {
                bestAction = treeInShadow;
            }
            else if (currentState.day >= maxTurns - 2)
            {
                bestAction = FindBestCompleteAction();
            }
            else
            {
                bestAction = FindBestGrowAction();

                if (bestAction == null)
                {
                    bestAction = FindBestSeedAction();
                }
            }

            Console.Error.WriteLine($"Move: {bestAction} : {possibleActions.Count}");

            if (bestAction == null)
            {
                return possibleActions.First(a => a.type == Actions.WAIT);
            }

            return bestAction;
        }

        private Move FindBestSeedAction()
        {
            List<Move> actions = possibleActions.Where(a => a.type == Actions.SEED).ToList();
            int treeCount = currentState.trees.Where(t => t.isMine).Count();
            int seedCount = currentState.trees.Where(t => t.isMine && t.size == 0).Count();
            if (seedCount <= 2 && treeCount <= 8)
            {
                int maxPoints = -1;
                Move bestSeedAction = null;
                foreach (Move move in actions)
                {
                    Cell cell = currentState.board.First(c => c.index == move.targetCellIdx);

                    int cost = currentState.GetCostToSeed()

                    int points = cell.richness;
                    points += cell.IsCorner() ? 2 : 0;
                    if (maxPoints < points)
                    {
                        maxPoints = points;
                        bestSeedAction = move;
                    }
                }
                return bestSeedAction;
            }
            return null;
        }

        /// <summary>
        /// Find trees that are better to cut down then to keep standing
        /// </summary>
        /// <returns></returns>
        private Move FindShadowedTree()
        {
            List<Move> actions = possibleActions.Where(a => a.type == Actions.COMPLETE).ToList();
            int sunPowerWithTree = CalculateMySunPowerForGame(new GameState(currentState));

            foreach (Move move in actions)
            {
                Tree tree = currentState.trees.First(t => t.cellIndex == move.targetCellIdx);
                Cell cell = currentState.board.First(c => c.index == move.targetCellIdx);
                
                GameState afterComplete = new GameState(currentState);
                afterComplete.ApplyMove(move);

                int sunPower = CalculateMySunPowerForGame(afterComplete);

                if(sunPowerWithTree < (sunPower - 4))//Cost to cut down is 4 sun power
                {
                    return move;
                }
            }
            return null;
        }

        private int CalculateMySunPowerForGame(GameState state)
        {
            int sunPower = 0;
            int sunPowerTo0 = -1;

            for (int i = 0; i < sunReset; i++)
            {
                state.AdvanceDay();
                if (state.sunDirection == 0)
                {
                    sunPowerTo0 = sunPower;
                }
                sunPower += state.mySunPowerGenerationToday;
            }

            int cycles = (maxTurns - currentState.day) / sunReset;
            sunPower = sunPowerTo0 + cycles * sunPower;

            return sunPower;
        }

        private Move FindBestGrowAction()
        {
            List<Move> actions = possibleActions.Where(a => a.type == Actions.GROW).ToList();
            int maxPoints = -9999;
            Move bestGrowAction = null;
            List<Tree> myTrees = currentState.trees.Where(t => t.isMine).ToList();
            Dictionary<int, int> treeSizeToCount = new Dictionary<int, int>();
            treeSizeToCount[1] = myTrees.Where(t => t.size == 1).Count();
            treeSizeToCount[2] = myTrees.Where(t => t.size == 2).Count();
            treeSizeToCount[3] = myTrees.Where(t => t.size == 3).Count();
            foreach (Move move in actions)
            {
                Tree tree = currentState.trees.First(t => t.cellIndex == move.targetCellIdx);
                Cell cell = currentState.board.First(c => c.index == move.targetCellIdx);
                int treeSizePoints = Math.Max(maxTurns - currentState.day - maxTreeSize + tree.size, 0);
                if (treeSizePoints == 0)
                {
                    continue;
                }

                int cost = currentState.GetCostToGrow(cell);

                GameState afterMove = new GameState(currentState);
                afterMove.ApplyMove(move);

                int sunPower = CalculateMySunPowerForGame(afterMove);                

                int points = sunPower - cost;
                if (maxPoints < points)
                {
                    maxPoints = points;
                    bestGrowAction = move;
                }
            }
            return bestGrowAction;
        }

        private Move FindBestCompleteAction()
        {
            List<Move> actions = possibleActions.Where(a => a.type == Actions.COMPLETE).ToList();
            int maxPoints = -1;
            Move bestAction = null;
            foreach (Move move in actions)
            {
                Tree tree = currentState.trees.First(t => t.cellIndex == move.targetCellIdx);
                Cell cell = currentState.board.First(c => c.index == move.targetCellIdx);
                if (maxPoints < cell.richness)
                {
                    maxPoints = cell.richness;
                    bestAction = move;
                }
            }
            return bestAction;
        }
    }
}
