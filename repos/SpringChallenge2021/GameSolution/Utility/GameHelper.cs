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
        private List<long> possibleActions;

        public GameHelper(GameState state, List<long> possibleActions)
        {
            currentState = state;
            this.possibleActions = possibleActions;
        }

        public long GetNextMove()
        {
            long bestAction = 0;
            long treeInShadow = FindShadowedTree();
            if (treeInShadow != 0)
            {
                bestAction = treeInShadow;
            }
            else if (currentState.day >= maxTurns - 2)
            {
                bestAction = FindBestFinalTurnsCompleteAction();
            }
            
            if(bestAction == 0)
            {
                bestAction = FindBestGrowAction(currentState);

                if (bestAction == 0)
                {
                    bestAction = FindBestSeedAction();
                }
            }

            Console.Error.WriteLine($"Move: {bestAction} : {possibleActions.Count}");

            if (bestAction == 0)
            {
                return Move.CreateMove(Actions.WAIT);
            }

            return bestAction;
        }

        private long FindBestSeedAction()
        {
            List<long> actions = possibleActions.Where(a => Move.GetType(a) == Actions.SEED).ToList();
            int treeCount = currentState.TreeEnumeration.Count(t => t.isMine);
            int seedCount = currentState.TreeEnumeration.Count(t => t.isMine && t.size == 0);

            GameState state = new GameState(currentState);
            SunPower totalSunPower = CalculateSunPowerForGame(state);
            state = new GameState(currentState);
            int costToUpgrade = GetCostToUpgradeTrees(state);

            if (seedCount <= 0 && maxTurns - currentState.day > 4 && totalSunPower.mySunPower > costToUpgrade)
            {
                int maxPoints = -9999;
                long bestSeedAction = 0;
                foreach (long move in actions)
                {
                    Cell cell = currentState.board[Move.GetTargetIndex(move)];
                    Tree tree = currentState.GetTree(Move.GetSourceIndex(move));
                    if(tree.size == 1)
                    {
                        continue;
                    }

                    GameState afterMove = new GameState(currentState);
                    afterMove.ApplyMove(move, afterMove.me);
                    afterMove.AdvanceDay();
                    afterMove.ApplyMove(Move.CreateMove(Actions.GROW, cell.index), afterMove.me);
                    
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
            return 0;
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
        private long FindShadowedTree()
        {
            List<long> actions = possibleActions.Where(a => Move.GetType(a) == Actions.COMPLETE).ToList();
            SunPower sunPowerWithTree = CalculateSunPowerForGame(new GameState(currentState));

            int countLevel3Tree = currentState.TreeEnumeration.Count(t => t.size == maxTreeSize);

            foreach (long move in actions)
            {
                Tree tree = currentState.GetTree(Move.GetTargetIndex(move));
                Cell cell = currentState.board[Move.GetTargetIndex(move)];

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
            return 0;
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

        private long FindBestGrowAction(GameState state)
        {
            List<long> actions = possibleActions.Where(a => Move.GetType(a) == Actions.GROW).ToList();
            int maxPoints = -9999;
            long bestGrowAction = 0;
            List<Tree> myTrees = state.TreeEnumeration.Where(t => t.isMine).ToList();
            Dictionary<int, int> treeSizeToCount = new Dictionary<int, int>();
            treeSizeToCount[1] = myTrees.Where(t => t.size == 1).Count();
            treeSizeToCount[2] = myTrees.Where(t => t.size == 2).Count();
            treeSizeToCount[3] = myTrees.Where(t => t.size == 3).Count();
            foreach (long move in actions)
            {
                Cell cell = state.board[Move.GetTargetIndex(move)];
                Tree tree = state.GetTree(cell.index);
                
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

        private long FindBestFinalTurnsCompleteAction()
        {
            if (maxTurns - currentState.day == 2)
            {
                GameState state = new GameState(currentState);
                long growMove = FindBestGrowAction(currentState);
                if (growMove != 0)
                {
                    Tree growTree = currentState.GetTree(Move.GetTargetIndex(growMove));
                    if (growTree.size == 2)
                    {
                        state.ApplyMove(growMove, state.me);
                        state.AdvanceDay();

                        int power = state.mySunPowerGenerationToday + currentState.me.sun - currentState.GetCostToGrow(growTree);
                        int level3TreeCountOnHighRichness = state.TreeEnumeration.Where(t => t.size == 3 && (state.board[t.cellIndex].richness > 1 || state.nutrients > 3)).Count();
                        Console.Error.WriteLine($"lastTurnPower: {power} treeCount: {level3TreeCountOnHighRichness}");
                        if (power >= (level3TreeCountOnHighRichness) * 4)
                        {
                            return 0;
                        }
                    }
                }
            }
            
            List<long> actions = possibleActions.Where(a => Move.GetType(a) == Actions.COMPLETE).ToList();
            int maxPoints = 0;
            long bestAction = 0;
            foreach (long move in actions)
            {
                Tree tree = currentState.GetTree(Move.GetTargetIndex(move));
                Cell cell = currentState.board[Move.GetTargetIndex(move)];

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
