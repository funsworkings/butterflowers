using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;

public class ScriptableDatabase<E> : ScriptableObject where E:ScriptableObject
{
    [SerializeField] E[] m_items = new E[]{};
    public E[] items {
        get{
            return m_items;
        }
    }

    public bool Add(E item){
        if(Contains(item)) return false;

        List<E> temp = m_items.ToList();
        temp.Add(item);

        m_items = temp.ToArray();
        return true;
    }

    public bool Remove(E item){
        if(!Contains(item)) return false;

        List<E> temp = m_items.ToList();
        temp.Remove(item);

        m_items = temp.ToArray();
        return true;
    }

    public virtual bool Contains(E item){
        if(item == null || m_items == null)
            return false;

        return (m_items.Contains(item));
    }
    
    public E FetchItem(int index){
        if(m_items.Length == 0)
            return null;

        E item = null;
        int i = (int)Mathf.Repeat((float)index, (float)(m_items.Length));
            item = m_items[i];
        
        return item;
    }

    public E FetchRandomItem(){
        if(m_items.Length == 0)
            return null;

        return items[Random.Range(0, items.Length)];
    }

    public int GetItemIndex(E item)
    {
        int index = -1;
        for(int i = 0; i < items.Length; i++)
        {
            if(items[i] == item)
            {
                index = i;
                break;
            }
        }

        return index;
    }
}
