using System.Collections;
using System.Collections.Generic;

public class EnumerableWrapper<T> : IEnumerable<T>
{
    private IEnumerator<T> enumerator;
    public EnumerableWrapper(IEnumerator<T> aEnumerator)
    {
        enumerator = aEnumerator;
    }
    public IEnumerator<T> GetEnumerator()
    {
        return enumerator;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}