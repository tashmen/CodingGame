using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

public class LoopPerformance
{
    private readonly int[] array = Enumerable.Range(0, 10000).ToArray();

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
}

class Program
{
    static void Main(string[] args)
    {
        BenchmarkRunner.Run<LoopPerformance>();
    }
}
