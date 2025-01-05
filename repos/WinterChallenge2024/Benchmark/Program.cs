using BenchmarkDotNet.Running;
using Microsoft.CodeAnalysis;

public class LoopPerformance
{
    private readonly int[] array = Enumerable.Range(0, 10000).ToArray();
    private readonly int[] _tempHolder = Enumerable.Range(0, 10000).ToArray();
    private string[] _intToStringCache = Enumerable.Range(0, 10000).Select(x => x.ToString()).ToArray();
    private static string[,] _boolIntToStringCache;

    static LoopPerformance()
    {
        _boolIntToStringCache = new string[2, 10000];

        for (int i = 0; i < 10000; i++)
        {
            _boolIntToStringCache[0, i] = $"0_{i}";
            _boolIntToStringCache[1, i] = $"1_{i}";
        }
    }

    /*
    [Benchmark]
    public void ForLoop()
    {
        List<int> array1 = new List<int>();
        for (int i = 0; i < array.Length; i++)
        {
            array1.Add(array[i]);
        }
    }

    [Benchmark]
    public void ForeachLoop()
    {
        List<int> array1 = new List<int>();
        foreach (int value in array)
        {
            array1.Add(value);
        }
    }

    [Benchmark]
    public void ForLoopLinq()
    {
        List<int> array1;
        array1 = array.Select(a => a).ToList();
    }
    */

    /*
    [Benchmark]
    public void StringCreation()
    {
        bool isMine = true;
        foreach (int i in array)
        {
            string key = (isMine ? $"opposingtentacle_1_{_intToStringCache[i]}" : $"opposingtentacle_0_{_intToStringCache[i]}");
            isMine = !isMine;
        }
    }

    [Benchmark]
    public void StringCreation4()
    {
        bool isMine = true;
        foreach (int i in array)
        {
            string key = $"opposingtentacle_{_boolIntToStringCache[isMine ? 1 : 0, i]}";
            isMine = !isMine;
        }
    }

    [Benchmark]
    public void StringCreation5()
    {
        bool isMine = true;
        foreach (int i in array)
        {
            string key = _boolIntToStringCache[isMine ? 1 : 0, i];
            isMine = !isMine;
        }
    }

    /*
    [Benchmark]
    public void StringCreation3()
    {
        bool isMine = true;
        foreach (int i in array)
        {
            string key = $"opposingtentacle_{isMine}_{_intToStringCache[i]}";
            isMine = !isMine;
        }
    }
    */

    /*
    [Benchmark]
    public void StringCreation1()
    {
        bool isMine = true;
        foreach (int i in array)
        {
            string key = $"{(isMine ? "opposingtentacle_1_" : "opposingtentacle_0_")}{_intToStringCache[i]}";
            isMine = !isMine;
        }
    }

    [Benchmark]
    public void StringCreation2()
    {
        bool isMine = true;
        foreach (int i in array)
        {
            string key = $"{(isMine ? "opposingtentacle_1_" : "opposingtentacle_0_")}{i}";
            isMine = !isMine;
        }
    }
    */

    /*
    [Benchmark]
    public void ForLoopVsLinq()
    {
        int j = 0;
        for (int i = 0; i < array.Length; i++)
        {
            int baseEntity = array[i];
            if (baseEntity == 6)
            {
                _tempHolder[j++] = baseEntity;
            }
        }
        int[] entities = new int[j];
        for (int i = 0; i < j; i++)
        {
            entities[i] = _tempHolder[i];
        }
    }
    */

    /*
    [Benchmark]
    public void ForLoopVsLinqImproved()
    {
        List<int> validEntities = new List<int>();
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] == 6)
            {
                validEntities.Add(array[i]);
            }
        }
        int[] entities = validEntities.ToArray();
    }
    */

    /*
    [Benchmark]
    public void ForLoopVsLinq2()
    {
        int[] entities = array.Where(e => e == 6).ToArray();
    }
    */

    /*
    [GlobalSetup]
    public void Warmup()
    {
        Board2 board = new Board2();

    }

    [Benchmark]
    public void PooledObject()
    {

        for (int i = 0; i < 1000000; i++)
        {
            using (Board2 board = Board2.Get())
            {

            }
        }

    }

    [Benchmark]
    public void NonPooledObject()
    {

        for (int i = 0; i < 1000000; i++)
        {
            _ = new Board3();
        }

    }
    */

    /*
    [GlobalSetup]
    public void Warmup()
    {
        Entity2 board = new Entity2();

    }

    [Benchmark]
    public void PooledEntityObject()
    {

        for (int i = 0; i < 1000000; i++)
        {
            using (Entity2 board = Entity2.Get())
            {

            }
        }

    }

    [Benchmark]
    public void NonPooledEntityObject()
    {

        for (int i = 0; i < 1000000; i++)
        {
            _ = new Entity3();
        }
    }
    */


    /*
    [GlobalSetup]
    public void Warmup()
    {
        Board2 b = new Board2();
        GameState2 g = new GameState2();
    }
    */

    /*
    [Benchmark]
    public void PooledGameStateObject()
    {

        for (int i = 0; i < 1000000; i++)
        {
            using (GameState2 board = GameState2.Get())
            {

            }
        }

    }
    */

    /*
    [Benchmark]
    public void NonPooledGameStateObject_NonPooledBoard()
    {

        for (int i = 0; i < 1000000; i++)
        {
            _ = new GameState3();
        }
    }
    */

    /*
    [Benchmark]
    public void NonPooledGameState_PooledBoard()
    {
        for (int i = 0; i < 1000000; i++)
        {
            (new GameState4()).Dispose();
        }
    }
    */

    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<LoopPerformance>();
        }
    }
}
