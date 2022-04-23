using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using GameSolution.Entities;
using GameSolution.Game;
using System.Diagnostics;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Player
{
    static void Main(string[] args)
    {
        bool runNeural = false;
        bool isLive = true;
        string fileName = null;
        if (args.Length > 0)
        {
            runNeural = args[0] == "1";
            isLive = false;
        }
        if(args.Length > 1)
        {
            fileName = args[1];
        }

        List<BoardPiece> pieces = new List<BoardPiece>();
        GameState state = new GameState();

        string[] inputs;
        inputs = Console.ReadLine().Split(' ');
        int baseX = int.Parse(inputs[0]); // The corner of the map representing your base
        int baseY = int.Parse(inputs[1]);
        int heroesPerPlayer = int.Parse(Console.ReadLine()); // Always 3
        int enemyX = baseX == 0 ? 17630 : 0;
        int enemyY = baseY == 0 ? 9000 : 0;

        bool isFirstRound = true;


        // game loop
        while (true)
        {
            pieces.Clear();

            for (int i = 0; i < 2; i++)
            {
                
                inputs = Console.ReadLine().Split(' ');
                int health = int.Parse(inputs[0]); // Your base health
                int mana = int.Parse(inputs[1]); // Ignore in the first league; Spend ten mana to cast a spell
                Base b;
                if(i == 0)
                {
                    b = new Base(-1, baseX, baseY, true, health, mana);
                }
                else
                {
                    b = new Base(-2, enemyX, enemyY, false, health, mana);
                }
                pieces.Add(b);
            }

            int entityCount = int.Parse(Console.ReadLine()); // Amount of heros and monsters you can see
            for (int i = 0; i < entityCount; i++)
            {
                BoardPiece p = null;
                inputs = Console.ReadLine().Split(' ');
                int id = int.Parse(inputs[0]); // Unique identifier
                int type = int.Parse(inputs[1]); // 0=monster, 1=your hero, 2=opponent hero
                int x = int.Parse(inputs[2]); // Position of this entity
                int y = int.Parse(inputs[3]);
                int shieldLife = int.Parse(inputs[4]); // Ignore for this league; Count down until shield spell fades
                int isControlled = int.Parse(inputs[5]); // Ignore for this league; Equals 1 when this entity is under a control spell
                int health = int.Parse(inputs[6]); // Remaining health of this monster
                int vx = int.Parse(inputs[7]); // Trajectory of this monster
                int vy = int.Parse(inputs[8]);
                int nearBase = int.Parse(inputs[9]); // 0=monster with no target yet, 1=monster targeting a base
                int threatFor = int.Parse(inputs[10]); // Given this monster's trajectory, is it a threat to 1=your base, 2=your opponent's base, 0=neither

                switch (type)
                {
                    case 0:
                        bool? isThreatForMax = null;
                        if (threatFor == 1)
                            isThreatForMax = true;
                        else if (threatFor == 2)
                            isThreatForMax = false;
                        
                        p = new Monster(id, x, y, null, health, shieldLife, isControlled == 1, vx, vy, nearBase == 1, isThreatForMax);
                        break;
                    case 1:
                        p = new Hero(id, x, y, true, shieldLife, isControlled == 1, vx, vy, BoardPiece.GetDistance(baseX, x, baseY, y) < 5000);
                        break;
                    case 2:
                        p = new Hero(id, x, y, false, shieldLife, isControlled == 1, vx, vy, BoardPiece.GetDistance(baseX, x, baseY, y) < 5000);
                        break;
                }

                if(p != null)
                    pieces.Add(p);

            }

            state.SetNextTurn(new Board(pieces));

            /*
            Stopwatch watch = new Stopwatch();
            watch.Start();
            Console.Error.WriteLine("Move count: " + state.GetPossibleMoves(true).Count);
            watch.Stop();
            Console.Error.WriteLine(watch.ElapsedMilliseconds);
            */
            Move move;
            if (!runNeural)
            {
                GameHelper game = new GameHelper();
                move = game.GetBestMove(state);
            }
            else
            {
                NeuralGameHelper game = new NeuralGameHelper(state);
                if(!isLive && isFirstRound)
                {
                    game.ImportNetworkFromFile(fileName);
                }

                move = game.RunNeuralNetwork();
            }
            

            int limit = isFirstRound ? 998 : 48;

            Console.Write(move);

            isFirstRound = false;
        }
    }
}