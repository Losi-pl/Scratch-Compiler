using System.Collections;
using System.Collections.Frozen;
using System.Collections.Immutable;

namespace ScratchCompiler.Parser;

public readonly struct AllVariables(Project project) : IReadOnlySet<Variable>
{
    public Enumerator GetEnumerator() => new(project);
    IEnumerator<Variable> IEnumerable<Variable>.GetEnumerator() => new Enumerator(project);
    IEnumerator IEnumerable.GetEnumerator() => new Enumerator(project);
    
    public struct Enumerator : IEnumerator<Variable>
    {
        private FrozenSet<Actor>.Enumerator _actors;

        private ImmutableArray<Variable>.Enumerator _enumerator;

        public Enumerator(Project project)
        {
            _actors = project.Actors.GetEnumerator();
            _enumerator = project.Scene.Variables.GetEnumerator();
        }
        
        public bool MoveNext()
        {
            if(_enumerator.MoveNext())
                return true;
            
            while (true)
            {
                if(!_actors.MoveNext())
                    return false;
                _enumerator = _actors.Current.Variables.GetEnumerator();
                if(_enumerator.MoveNext())
                    return true;
            }
        }

        public void Reset() => throw new NotSupportedException();

        public Variable Current => _enumerator.Current;

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

    internal Variable? FindByScratchCode(string scratchCode)
    {
        if(project.Scene.GetVariableByCode(scratchCode) is { } var)
            return var;
        foreach (var actor in project.Actors)
            if(actor.GetVariableByCode(scratchCode) is { } var2)
                return var2;
        return null;
    }
    
    public bool Contains(Variable item)
    {
        if (project.Scene.Variables.Contains(item))
            return true;
        foreach (var actor in project.Actors)
            if (actor.Variables.Contains(item))
                return true;
        
        return false;
    }

    public bool IsProperSubsetOf(IEnumerable<Variable> other)
    {
        var o = other.ToHashSet();
        if(o.Count > this.Count)
            return IsSubsetOf(o);
        return false;
    }

    public bool IsProperSupersetOf(IEnumerable<Variable> other)
    {
        var count = 0;
        foreach (var variable in other)
        {
            ++count;
            if (!Contains(variable))
                return false;
        }

        return count < Count;
    }

    public bool IsSubsetOf(IEnumerable<Variable> other)
    {
        var o = other.ToHashSet();
        if(o.Count != Count)
            return false;
        foreach (var variable in this)
            if(!o.Contains(variable))
                return false;
        return true;
    }

    public bool IsSupersetOf(IEnumerable<Variable> other)
    {
        foreach (var variable in other)
            if (!Contains(variable))
                return false;

        return true;
    }

    public bool Overlaps(IEnumerable<Variable> otherEnum)
    {
        foreach (var variable in otherEnum)
            if(Contains(variable))
                return true;
        return false;
    }

    public bool SetEquals(IEnumerable<Variable> otherEnum)
    {
        Span<Variable> other = new Variable[Count];

        {
            int i = 0;
            using (var @enum = otherEnum.GetEnumerator())
                for (; @enum.MoveNext(); i++)
                    other[i] = @enum.Current;
           
            if (other.Length != i)
                return false;
        }

        foreach (var variable in this)
            if (!other.Contains(variable))
                return false;
        return true;
    }
}