/*
Sampo Tuisku
GitHub: SAMPROO

This class crates a generic list with the functionality of adding, removing at index and returning the size.
When Add() or RemoveAt(), Event is triggered.
*/

using System;
using System.Collections.Generic;

public class ObservableList<T>
{
    private readonly List<T> _data;
    
    public event Action OnAdd;
    public event Action OnRemove;

    public ObservableList() => _data = new List<T>();
    
    public T this[int index] => _data[index];
    
    public int Count => this._data.Count;

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
}
