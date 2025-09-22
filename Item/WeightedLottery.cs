using System.Linq;
using UnityEngine;

public static class WeightedLottery
{
    //重み付き籤を引く
    public static T Draw<T>(Map<T>[] map) {
        float total = map.Sum(v => v.probability);
        float randomPoint = Random.value * total;
        foreach (Map<T> m in map) {
            if (randomPoint < m.probability)
                return m.lottery;
            randomPoint -= m.probability;
        }
        //ここに到達することはない
        return map[0].lottery;
    }

    //出現階層内の重み付き籤を引く
    public static T DrawInRange<T>(int currentStage, Table<T>[] tables) {
        float total = tables.Where(table => table.startStage <= currentStage && currentStage < table.endStage)
                               .Sum(table => table.probability);
        float randomPoint = Random.value * total;
        foreach (Table<T> table in tables) {
            if (table.startStage <= currentStage && currentStage < table.endStage) {
                if (randomPoint < table.probability)
                    return table.lottery;
                randomPoint -= table.probability;
            }
        }
        //ここに到達することはない
        return tables[0].lottery;
    }
}

[System.Serializable]
public class Map<T> {
	public T lottery;
	public float probability;
}

[System.Serializable]
public class Table<T> : Map<T> {
	public int startStage;
	public int endStage;
}
