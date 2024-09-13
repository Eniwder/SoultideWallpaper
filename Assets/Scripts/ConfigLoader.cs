using System.IO;
using UnityEngine;

public static class ConfigLoader
{
    // Configのデータクラス
    [System.Serializable]
    public class ConfigData
    {
        public MazeConfig maze;
        public int characters;
        public int fps;
    }
    
    [System.Serializable]
    public class MazeConfig
    {
        public int col;
        public int row;
        public int droprate;
    }

    private static ConfigData config;

    // Configを取得するメソッド（初回読み込み時にデシリアライズ）
    public static ConfigData GetConfig()
    {
        if (config == null)
        {
            LoadConfig();
        }
        return config;
    }

    // StreamingAssetsからconfig.jsonを読み込むメソッド
    private static void LoadConfig()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "config.json");
        // その他のプラットフォームではFile.ReadAllTextを使って同期的に読み込む
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            config = JsonUtility.FromJson<ConfigData>(json);
        }
        else
        {
            Debug.LogError("config.json not found in StreamingAssets");
        }
    }
}
