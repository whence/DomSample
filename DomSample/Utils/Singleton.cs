namespace DomSample.Utils
{
    /// <summary>
    /// Wrap a class that is intended to be used as singleton. 
    /// Provide thread-safe and lazy instantiation to the single instance based on pattern from
    /// <see cref="http://www.yoda.arachsys.com/csharp/singleton.html"/>.
    /// Although instantiation is thread-safe, the wrapper does not lock the access to the instance. 
    /// Thus it is the responsibility of the singleton instance itself to ensure thread-safe inside the instance.
    /// </summary>
    /// <typeparam name="T">type of the single instance, must have a public default constructor without parameters</typeparam>
    public sealed class Singleton<T>
        where T : new()
    {
        Singleton()
        {
        }

        public static T Instance
        {
            get
            {
                return Nested.instance;
            }
        }

        class Nested
        {
            // Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit
            static Nested()
            {
            }

            internal static readonly T instance = new T();
        }
    }
}
