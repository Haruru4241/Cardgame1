using System.Collections.Generic;
using UnityEngine;// 덱 및 핸드 관리
public class Pile
{
    public string Name { get; }
    public List<CardInstance> Cards { get; } = new List<CardInstance>();

    public Pile(string name) { Name = name; }

    public void Add(CardInstance ci)
    {
        if (!Cards.Contains(ci))
        {
            Cards.Add(ci);
            ci.CurrentPile = this;
        }
    }

    public void Remove(CardInstance ci)
    {
        if (Cards.Remove(ci))
            ci.CurrentPile = null;
    }

    public List<CardInstance> FindAll(System.Predicate<CardInstance> predicate)
    {
        return Cards.FindAll(predicate);
    }
    public void Shuffle()
    {
        int n = Cards.Count;
        for (int i = n - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);  // 0 <= j <= i
            var temp = Cards[i];
            Cards[i] = Cards[j];
            Cards[j] = temp;
        }
    }
}