using UnityEngine;
using System;

public static class MyUtil
{
    private static System.Random random = new System.Random();

    public static void Shuffle<T>(T[] array)
    {
        int n = array.Length;

        for (int i = n - 1; i > 0; i--)
        {
            int j = random.Next(0, i + 1);
            T temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }
    }

    public static void Log(params object[] args)
    {
        string buff = "";
        foreach (var v in args)
        {
            buff += v + " @@@ ";
        }
        Debug.Log(buff);
    }

    public static bool IsWrapped()
    {
        // 実行時引数を取得
        string[] commandLineArgs = System.Environment.GetCommandLineArgs();

        // -parentHWND が含まれているかチェック
        bool hasParentHWnd = false;
        for (int i = 0; i < commandLineArgs.Length; i++)
        {
            if (commandLineArgs[i].Contains("parentHWND", StringComparison.OrdinalIgnoreCase))
            {
                hasParentHWnd = true;
                break;
            }
        }
        return hasParentHWnd;
    }
}
