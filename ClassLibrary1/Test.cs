namespace ClassLibrary1
{
    public partial class Test
    {
        internal void CallTest<T>([MultiType(typeof(Func<string, Task<string>>), typeof(Func<string, IAsyncEnumerable<string>>))] T value)
        {
            Console.WriteLine(value);
        }
    }

    [AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
    public class MultiTypeAttribute : Attribute
    {
        public MultiTypeAttribute(params Type[] types)
        {
        }
    }
}
