using GameSolution.Entities;
using GameSolution.Moves;
using System;
using System.Collections.Generic;
using System.Linq;
using static GameSolution.Constants;

namespace GameSolution.Utility
{
    public class GameHelper
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
                bestAction = FindBestFinalTurnsCompleteAction();
            }
            
            if(bestAction == null)
            {
                bestAction = FindBestGrowAction(currentState);

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
            int treeCount = currentState.TreeEnumeration.Count(t => t.isMine);
            int seedCount = currentState.TreeEnumeration.Count(t => t.isMine && t.size == 0);

            GameState state = new GameState(currentState);
            SunPower totalSunPower = CalculateSunPowerForGame(state);
            state = new GameState(currentState);
            int costToUpgrade = GetCostToUpgradeTrees(state);

            if (seedCount <= 0 && maxTurns - currentState.day > 4 && totalSunPower.mySunPower > costToUpgrade)
            {
                int maxPoints = -9999;
                Move bestSeedAction = null;
                foreach (Move move in actions)
                {
                    Cell cell = currentState.board[move.targetCellIdx];
                    Tree tree = currentState.GetTree(move.sourceCellIdx);
                    if(tree.size == 1)
                    {
                        continue;
                    }

                    GameState afterMove = new GameState(currentState);
                    afterMove.ApplyMove(move, afterMove.me);
                    afterMove.AdvanceDay();
                    afterMove.ApplyMove(new Move(Actions.GROW, cell.index), afterMove.me);
                    
                    SunPower sunPower = CalculateSunPowerForGame(afterMove);


                    int points = cell.richness;
                    points += sunPower.GetDifference();
                    points -= currentState.GetCostToSeed();
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
        /// Determines cost in sunpower to upgrade every tree in the most efficient way possible
        /// </summary>
        /// <param name="state">The state of the game.  Note the state will be mutated to perform the calculation</param>
        /// <returns>The minimum upgrade cost</returns>
        private int GetCostToUpgradeTrees(GameState state)
        {
            int totalCost = 0;
            foreach(Tree tree in state.TreeEnumeration)
            {
                if(tree.isMine && tree.size == 2)
                {
                    while (tree.size < maxTreeSize)
                    {
                        totalCost += state.GetCostToGrow(tree);
                        tree.Grow();
                    }
                }
            }

            foreach (Tree tree in state.TreeEnumeration)
            {
                if (tree.isMine && tree.size == 1)
                {
                    while (tree.size < maxTreeSize)
                    {
                        totalCost += state.GetCostToGrow(tree);
                        tree.Grow();
                    }
                }
            }

            foreach (Tree tree in state.TreeEnumeration)
            {
                if (tree.isMine && tree.size == 0)
                {
                    while (tree.size < maxTreeSize)
                    {
                        totalCost += state.GetCostToGrow(tree);
                        tree.Grow();
                    }
                }
            }

            return totalCost;
        }

        /// <summary>
        /// Find trees that are better to cut down then to keep standing
        /// </summary>
        /// <returns></returns>
        private Move FindShadowedTree()
        {
            List<Move> actions = possibleActions.Where(a => a.type == Actions.COMPLETE).ToList();
            SunPower sunPowerWithTree = CalculateSunPowerForGame(new GameState(currentState));

            int countLevel3Tree = currentState.TreeEnumeration.Count(t => t.size == maxTreeSize);

            foreach (Move move in actions)
            {
                Tree tree = currentState.GetTree(move.targetCellIdx);
                Cell cell = currentState.board[move.targetCellIdx];

                int scoreOnCut = currentState.GetTreeCutScore(cell);
                scoreOnCut -= 1;//take away one point for the sunpower cost
                
                GameState afterComplete = new GameState(currentState);
                afterComplete.ApplyMove(move, afterComplete.me);

                SunPower sunPower = CalculateSunPowerForGame(afterComplete);

                //If the amount of power gained keeping the tree is greater than the score we receive from cutting then keep the tree.
                if ((sunPowerWithTree.mySunPower - sunPower.mySunPower) / 3 > scoreOnCut)
                {
                    continue;
                }


                if(sunPowerWithTree.GetDifference() - countLevel3Tree  <= (sunPower.GetDifference() - 4))//Cost to cut down is 4 sun power
                {
                    return move;
                }
            }
            return null;
        }

        public static SunPower CalculateSunPowerForGame(GameState state)
        {
            SunPower power = new SunPower();
            SunPower powerTo0 = new SunPower();

            int startingDay = state.day;

            for (int i = 0; i < sunReset; i++)
            {
                state.AdvanceDay();
                if (state.sunDirection == 0)
                {
                    powerTo0.mySunPower = power.mySunPower;
                    powerTo0.oppSunPower = power.oppSunPower;
                }
                if (state.day >= maxTurns)
                {
                    break;
                }
                power.mySunPower += state.mySunPowerGenerationToday;
                power.oppSunPower += state.opponentSunPowerGenerationToday;
            }

            int cycles = (maxTurns - startingDay) / sunReset;
            power.mySunPower = powerTo0.mySunPower + cycles * power.mySunPower;
            power.oppSunPower = powerTo0.oppSunPower + cycles * power.oppSunPower;

            return power;
        }

        private Move FindBestGrowAction(GameState state)
        {
            List<Move> actions = possibleActions.Where(a => a.type == Actions.GROW).ToList();
            int maxPoints = -9999;
            Move bestGrowAction = null;
            List<Tree> myTrees = state.TreeEnumeration.Where(t => t.isMine).ToList();
            Dictionary<int, int> treeSizeToCount = new Dictionary<int, int>();
            treeSizeToCount[1] = myTrees.Where(t => t.size == 1).Count();
            treeSizeToCount[2] = myTrees.Where(t => t.size == 2).Count();
            treeSizeToCount[3] = myTrees.Where(t => t.size == 3).Count();
            foreach (Move move in actions)
            {
                Cell cell = state.board[move.targetCellIdx];
                Tree tree = cell.tree;
                
                int treeSizePoints = Math.Max(maxTurns - state.day - maxTreeSize + tree.size, 0);
                Console.Error.WriteLine($"treesizepoints: {treeSizePoints}");
                if (treeSizePoints == 0)
                {
                    continue;
                }

                int cost = state.GetCostToGrow(tree);

                GameState afterMove = new GameState(state);
                afterMove.ApplyMove(move, afterMove.me);

                SunPower sunPower = CalculateSunPowerForGame(afterMove);

                int points = sunPower.GetDifference();
                points -= cost;
                points += cell.richness;
                if (maxPoints < points)
                {
                    maxPoints = points;
                    bestGrowAction = move;
                }
            }
            return bestGrowAction;
        }

        private Move FindBestFinalTurnsCompleteAction()
        {
            if (maxTurns - currentState.day == 2)
            {
                GameState state = new GameState(currentState);
                Move growMove = FindBestGrowAction(currentState);
                if (growMove != null)
                {
                    Cell growCell = currentState.board[growMove.targetCellIdx];
                    if (growCell.tree.size == 2)
                    {
                        state.ApplyMove(growMove, state.me);
                        state.AdvanceDay();

                        int power = state.mySunPowerGenerationToday + currentState.me.sun - currentState.GetCostToGrow(growCell.tree);
                        int level3TreeCountOnHighRichness = state.board.Where(c => c.HasTree && c.tree.size == 3 && (c.richness > 1 || state.nutrients > 3)).Count();
                        Console.Error.WriteLine($"lastTurnPower: {power} treeCount: {level3TreeCountOnHighRichness}");
                        if (power >= (level3TreeCountOnHighRichness) * 4)
                        {
                            return null;
                        }
                    }
                }
            }
            
            List<Move> actions = possibleActions.Where(a => a.type == Actions.COMPLETE).ToList();
            int maxPoints = 0;
            Move bestAction = null;
            foreach (Move move in actions)
            {
                Tree tree = currentState.GetTree(move.targetCellIdx);
                Cell cell = currentState.board[move.targetCellIdx];

                int points = currentState.GetTreeCutScore(cell);
                points -= 1;//you lose one point (4 sunpower) to chop a tree
                if (maxPoints < points)
                {
                    maxPoints = cell.richness;
                    bestAction = move;
                }
            }
            return bestAction;
        }
    }
}
