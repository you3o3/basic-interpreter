using System;
using System.Collections.Generic;

internal static class Helper
{
    // indicate the location of error
    internal static string stringWithArrows(string s, Position posStart, Position posEnd)
    {
        string result = "";

        // https://stackoverflow.com/questions/12421160/string-lastindexof-bug
        int idxStart = s.LastIndexOf('\n', s.Length - 1, posStart.idx);
        if (idxStart == -1) idxStart = 0;
        int idxEnd = (posStart.idx + 1 < s.Length) ? s.IndexOf('\n', posStart.idx + 1) : -1;
        if (idxEnd == -1) idxEnd = s.Length;

        int lineCount = posEnd.line - posStart.line + 1;
        for (int i = 0; i < lineCount; i++)
        {
            string line = s.Substring(idxStart, idxEnd - idxStart);
            int colStart = (i == 0) ? posStart.col : 0;
            int colEnd = (i == lineCount - 1) ? posEnd.col : line.Length - 1;

            result += line + '\n';
            result += new string(' ', colStart) + new string('^', colEnd - colStart);

            idxStart = idxEnd;
            idxEnd = (idxStart + 1 < s.Length) ? s.IndexOf('\n', idxStart + 1) : -1;
            if (idxEnd == -1) idxEnd = s.Length;
        }

        return result.Replace("\t", "");
    }

    // https://stackoverflow.com/questions/11689240/linq-list-of-tuples-to-tuple-of-lists
    internal static (List<A>, List<B>) Unpack<A, B>(List<(A, B)> list)
    {
        var listA = new List<A>(list.Count);
        var listB = new List<B>(list.Count);
        foreach (var t in list)
        {
            listA.Add(t.Item1);
            listB.Add(t.Item2);
        }

        return (listA, listB);
    }
}
