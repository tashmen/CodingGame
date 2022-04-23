using GameSolution.Algorithms.Genetic;
using GameSolution.Algorithms.NeuralNetwork;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralTrainer
{
    public class Program
    {
        public static string pathToCodingGame = "C:\\Users\\jnorc\\CodingGame\\";
        public static string pathToDataFolder = pathToCodingGame + "data\\";
        public static string pathToReferee = pathToCodingGame + "SpringChallenge2022\\target\\spider-attack-spring-2022-1.0-SNAPSHOT.jar";
        public static string pathToBrutalTester = pathToCodingGame + "cg-brutaltester-1.0.0.jar";
        public static string pathToLogs = pathToCodingGame + "logs\\";
        public static string pathToGame = "C:\\Users\\jnorc\\source\\repos\\SpringChallenge2022\\GameSolution\\bin\\Debug\\GameSolution.exe";
        public static string pathTolog4j = pathToCodingGame + "log4j2.xml";
        public static string dataFileType = ".data";

        public static int gamesToPlay = 100;
        public static int populationSize = 100;
        public static void Main(string[] args)
        {
            Population population = new Population(populationSize);

            bool isFirstLoad = false;

            if (isFirstLoad)
            {
                ExportNetwork(population);
            }
            else
            {
                ImportNetwork(population);
            }

            GeneticAlgorithm genetic = new GeneticAlgorithm(population, 0.01, 0.05, 0.7);

            while (!Console.KeyAvailable)
            {
                for(var p = 0; p<populationSize/2; p++)
                {
                    int player1Index = p;
                    int player2Index = p+populationSize/2;

                    Individual player1 = population.getIndividual(player1Index);
                    Individual player2 = population.getIndividual(player2Index);

                    ProcessStartInfo processStartInfo = new ProcessStartInfo("cmd", $"/C java -Dlog4j.configuration=\"{pathTolog4j}\" -jar {pathToBrutalTester} -r \"java -Dleague.level=4 -jar {pathToReferee}\" -p1 \"{pathToGame} 1 {pathToDataFolder}{player1Index}{dataFileType}\" -p2 \"{pathToGame} 1 {pathToDataFolder}{player2Index}{dataFileType}\" -t 3 -n {gamesToPlay} -l {pathToLogs}");

                    processStartInfo.UseShellExecute = false;

                    Process process = new Process();
                    process.StartInfo = processStartInfo;

                    process.Start();

                    process.WaitForExit();

                    for(int g = 0; g<gamesToPlay; g++)
                    {
                        using (StreamReader stream = new StreamReader($"{pathToLogs}game{g+1}.json"))
                        {
                            var json = stream.ReadToEnd();
                            GameResultDto gameResult = JsonConvert.DeserializeObject<GameResultDto>(json);
                            var player1Score = gameResult.scores[0];
                            var player2Score = gameResult.scores[1];
                            if(player1Score > player2Score)
                            {
                                player1.SetFitness(player1.GetFitness() + 1);
                            }
                            else if(player2Score > player1Score)
                            {
                                player2.SetFitness(player2.GetFitness() + 1);
                            }
                        }
                    }
                }

                population = genetic.runOnce();
                Console.Error.WriteLine("Finished Generation: " + genetic.generationCounter);
                ExportNetwork(population);
            }

        }

        public static void ExportNetwork(Population population)
        {
            for (int i = 0; i < populationSize; i++)
            {
                NeuralNetwork net = new NeuralNetwork(4, new int[] { 354, 177, 60, 15 }, 354);
                using (var writer = new BinaryWriter(new FileStream(pathToDataFolder + i + dataFileType, FileMode.Create)))
                {
                    net.Save(writer);
                }
                population.addIndividual(net);
            }
        }

        public static void ImportNetwork(Population population)
        {
            for (int i = 0; i < 100; i++)
            {
                NeuralNetwork net;
                using (var reader = new BinaryReader(new FileStream(pathToDataFolder + i + dataFileType, FileMode.Open)))
                {
                    net = new NeuralNetwork(reader);
                }
                population.addIndividual(net);
            }
        }
    }
}
