using System;
using System.Collections.Generic;

public class WeightedRandom<T>
{

    private readonly List<WeightedItem> _items;

    public WeightedRandom(int capacity)
    {
        _items = new List<WeightedItem>(capacity);
    }

    public void Add(T item, float weight)
    {
        _items.Add(new WeightedItem(item, weight));
    }

    public T GetRandomItem()
    {
        float randomWeight = Randomize.Float(0f, CalculateTotalWeight());

        for (int i = 0; i < _items.Count; i++)
        {
            var item = _items[i];

            if (randomWeight < item.Weight)
                return item.Item;

            randomWeight -= item.Weight;
        }

        throw new Exception("Something went wrong with weighted random");
    }

    private float CalculateTotalWeight()
    {
        float total = 0f;

        for (int i = 0; i < _items.Count; i++)
        {
            total += _items[i].Weight;
        }

        return total;
    }

    private readonly struct WeightedItem
    {
        public readonly T Item;
        public readonly float Weight;

        public WeightedItem(T item, float weight)
        {
            Item = item;
            Weight = weight;
        }

    }

}
