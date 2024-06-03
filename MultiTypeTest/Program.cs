using ClassLibrary1;

namespace MultiTypeTest;

public class Program
{
    public static void Main()
    {
        var t = new Test();
        t.CallTest(t1);
        t.CallTest(t2);
    }

    public static async Task<string> t1(string input)
    {
        return await Task.FromResult("test1");
    }

    public static async IAsyncEnumerable<string> t2(string input)
    {
        yield return await Task.FromResult("test2");
    }
}

