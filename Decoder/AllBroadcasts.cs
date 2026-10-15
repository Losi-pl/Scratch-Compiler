using System.Collections;
using System.Collections.Frozen;
using System.Collections.Immutable;

namespace ScratchCompiler.Decoder;

public readonly struct AllBroadcasts(Project project): IReadOnlySet<Broadcast>
{
    public Enumerator GetEnumerator() => new(project);
    IEnumerator<Broadcast> IEnumerable<Broadcast>.GetEnumerator() => new Enumerator(project);
    IEnumerator IEnumerable.GetEnumerator() => new Enumerator(project);

    internal Broadcast? FindByScratchCode(string scratchCode)
    {
        if (project.Scene.GetBroadcastByCode(scratchCode) is { } broadcast)
            return broadcast;
        foreach (var actor in project.Actors)
            if(actor.GetBroadcastByCode(scratchCode) is { } broadcast2)
                return broadcast2;
        return null;
    }
    
    public struct Enumerator : IEnumerator<Broadcast>
    {
        private FrozenSet<Actor>.Enumerator _actors;

        private ImmutableArray<Broadcast>.Enumerator _enumerator;

        public Enumerator(Project project)
        {
            _actors = project.Actors.GetEnumerator();
            _enumerator = project.Scene.Broadcasts.GetEnumerator();
        }
        
        public bool MoveNext()
        {
            if(_enumerator.MoveNext())
                return true;
            
            while (true)
            {
                if(!_actors.MoveNext())
                    return false;
                _enumerator = _actors.Current.Broadcasts.GetEnumerator();
                if(_enumerator.MoveNext())
                    return true;
            }
        }

        public void Reset() => throw new NotSupportedException();

        public Broadcast Current => _enumerator.Current;

        object IEnumerator.Current => Current;

        void IDisposable.Dispose() { }
    }

    public int Count
    {
        get
        {
            var sum = project.Scene.Broadcasts.Length;
            foreach (var actor in project.Actors)
                sum += actor.Broadcasts.Length;
            return sum;
        }
    }

    public bool Contains(Broadcast item)
    {
        if (project.Scene.Broadcasts.Contains(item))
            return true;
        foreach (var actor in project.Actors)
            if (actor.Broadcasts.Contains(item))
                return true;
        
        return false;
    }

    public bool IsProperSubsetOf(IEnumerable<Broadcast> other)
    {
        var o = other.ToHashSet();
        if(o.Count > this.Count)
            return IsSubsetOf(o);
        return false;
    }

    public bool IsProperSupersetOf(IEnumerable<Broadcast> other)
    {
        var count = 0;
        foreach (var broadcast in other)
        {
            ++count;
            if (!Contains(broadcast))
                return false;
        }

        return count < Count;
    }

    public bool IsSubsetOf(IEnumerable<Broadcast> other)
    {
        var o = other.ToHashSet();
        if(o.Count != Count)
            return false;
        foreach (var broadcast in this)
            if(!o.Contains(broadcast))
                return false;
        return true;
    }

    public bool IsSupersetOf(IEnumerable<Broadcast> other)
    {
        foreach (var broadcast in other)
            if (!Contains(broadcast))
                return false;

        return true;
    }

    public bool Overlaps(IEnumerable<Broadcast> otherEnum)
    {
        foreach (var broadcast in otherEnum)
            if(Contains(broadcast))
                return true;
        return false;
    }

    public bool SetEquals(IEnumerable<Broadcast> otherEnum)
    {
        Span<Broadcast> other = new Broadcast[Count];

        {
            int i = 0;
            using (var @enum = otherEnum.GetEnumerator())
                for (; @enum.MoveNext(); i++)
                    other[i] = @enum.Current;
           
            if (other.Length != i)
                return false;
        }

        foreach (var broadcast in this)
            if (!other.Contains(broadcast))
                return false;
        return true;
    }
}