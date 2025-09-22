using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ListExt
{
    public static IList<T> Shuffle<T>(this IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; --i)
        {
            // Random.Range‚ÌÅ‘å’l‚Í‘æ‚Qˆø”–¢–‚È‚Ì‚ÅA+1‚·‚é‚±‚Æ‚É’ˆÓ
            int j = Random.Range(0, i + 1);
            // i”Ô–Ú‚Æj”Ô–Ú‚Ì—v‘f‚ğŒğŠ·‚·‚é
            T tmp = list[i];
            list[i] = list[j];
            list[j] = tmp;
        }
        return list;
    }

    public static List<List<T>> SplitToSublists<T>(this List<T> source, int count)
    {
        return source.Select((x, i) => new { Index = i, Value = x })
                     .GroupBy(x => x.Index / count)
                     .Select(x => x.Select(v => v.Value).ToList())
                     .ToList();
    }
}
