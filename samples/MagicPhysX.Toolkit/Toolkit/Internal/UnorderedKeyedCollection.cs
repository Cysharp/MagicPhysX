using System.Runtime.InteropServices;

namespace MagicPhysX.Toolkit.Internal;

internal sealed class UnorderedKeyedCollection<T>
    where T : class
{
    readonly Dictionary<T, int> dict; // Value is index of list.
    readonly List<T> list;

    public UnorderedKeyedCollection()
        : this(4)
    {
    }

    public UnorderedKeyedCollection(int capacity)
    {
        dict = new Dictionary<T, int>(capacity, ReferenceEqualityComparer.Instance);
        list = new List<T>(capacity);
    }

    public int Count => list.Count;

    public void Add(T value)
    {
        var index = list.Count;
        dict.Add(value, index);
        list.Add(value);
    }

    public void Clear()
    {
        dict.Clear();
        list.Clear();
    }

    public void Remove(T value)
    {
        if (dict.Remove(value, out var index))
        {
            // swap remove and update new-index
            var span = CollectionsMarshal.AsSpan(list);
            (span[index], span[span.Length - 1]) = (span[span.Length - 1], span[index]);

            dict[span[index]] = index;
            list.RemoveAt(list.Count - 1);
        }
    }

    public ReadOnlySpan<T> AsSpan()
    {
        return CollectionsMarshal.AsSpan(list);
    }

    public T[] ToArray()
    {
        return list.ToArray();
    }
}
