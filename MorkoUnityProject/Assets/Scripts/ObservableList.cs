using System;
using System.Collections.Generic;

public class ObservableList<T>
{
    private readonly List<T> _data;
    
    public event Action OnAdd;
    public event Action OnRemove;

    public ObservableList()
    {
        _data = new List<T>();
    }

    public T this[int index]
    {
        get => _data[index];
    }

    public void Add(T value)
    {
        OnAdd?.Invoke();
        this._data.Add(value);
    }

    public void RemoveAt(int index, bool fromEnd = false)
    {
        OnRemove?.Invoke();
        this._data.RemoveAt(fromEnd ? _data.Count - index : index);
    }
    
    public int Count()
    {
        return this._data.Count;
    }
}