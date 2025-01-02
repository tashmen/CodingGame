using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

public class LoopPerformance
{
    private readonly int[] array = Enumerable.Range(0, 10000).ToArray();

    [Benchmark]
    public void ForLoop()
    {
        for (int i = 0; i < array.Length; i++)
        {
            int value = array[i];
        }
    }

    [Benchmark]
    public void ForeachLoop()
    {
        foreach (int value in array)
        {
            int temp = value;
        }
    }
}

class Program
{
    static void Main(string[] args)
    {
        BenchmarkRunner.Run<LoopPerformance>();
    }
}
