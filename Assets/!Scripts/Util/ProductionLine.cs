using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class ProductionLine<TValue>
{
    private List<Func<TValue, TValue>> m_operations = new List<Func<TValue, TValue>>();

    private TValue m_initial;
    [SerializeField, ReadOnly] private List<TValue> m_productionState = new List<TValue>();
    
    public ProductionLine(TValue initial)
    {
        // add first operation as padding so both lists start changing at index 1
        // this is to help visualise a production line at 0 operations will return an unchanged input
        m_initial = initial;
        m_productionState.Add(initial);
        m_operations.Add((v) => v);
    }

    public ProductionLine(TValue initial, params Func<TValue, TValue>[] operations) : this(initial)
    {
        foreach (var operation in operations)
            m_operations.Add(operation);
    }

    public TValue GetResult()
    {
        return GetResultAtPoint(m_operations.Count);
    }

    public TValue GetResultAtPoint(int productionIndex)
    {
        // index <= 0 || no operations to perform
        if (productionIndex <= 0 || m_operations.Count < 1)
            return m_initial;
        // requested index greater than operations available
        else if (productionIndex > m_operations.Count)
            productionIndex = m_operations.Count;
        
        // requested production index already exists
        if (productionIndex < m_productionState.Count && m_productionState[productionIndex] != null)
            return m_productionState[productionIndex];

        // otherwise, iterate through operations, storing each iteration inside production state list

        TValue v = m_initial; // m_productionStates[0]
        for (int i = 1; i < productionIndex; i++)
        {
            // apply operation to v recursively
            v = m_operations[i](v);

            //check if current index is within range of current list
            if (i < m_productionState.Count)
            {
                // if space within list exists, fill with v
                if (m_productionState[i] == null)
                    m_productionState[i] = v;
            }
            // current index exists outside range of current list
            // this means there are more operations than stored states
            else
            {
                m_productionState.Add(v);
            }
        }

        return v;
    }

    public void InsertOperation(Func<TValue, TValue> operation, int index, bool unique = false)
    {
        if (unique && m_operations.Contains(operation))
            return;

        if (index < 1) index = 1;

        m_operations.Add(m_operations[^1]);
        for (int i = m_operations.Count - 1; i > index && i > 0; i--)
        {
            m_operations[i] = m_operations[i - 1];
        }

        m_operations[index] = operation;
    }
    public void AppendOperation(Func<TValue, TValue> operation, bool unique = false)
    {
        if (unique && m_operations.Contains(operation))
            return;
        else
            m_operations.Add(operation);
    }

    public bool RemoveOperation(Func<TValue, TValue> operation)
    {
        return m_operations.Remove(operation);
    }

    public void SetInitial(TValue initial) 
    {
        m_initial = initial;
        m_productionState.Clear();
        m_productionState.Add(m_initial);
    }
}
