using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public Text scoreText;  // Textコンポーネントをアタッチ
    private int score = 0;
    public static ScoreManager Instance;

    public void Initialize()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            score = PlayerPrefs.GetInt("Score", 0);
            scoreText.text = score.ToString() + " pt";
            scoreText.enabled = ConfigLoader.GetConfig().score;
        }
        else
        {
            Destroy(gameObject);
        }

    }

    // private void Update()
    // {
    //     // スコアを表示する
    //     scoreText.text = score.ToString() + " pt";
    // }

    // スコアを増やすメソッド
    public void AddScore(int value)
    {
        score += value;
        scoreText.text = score.ToString() + " pt";
    }

    public void SaveScore()
    {
        PlayerPrefs.SetInt("Score", score);
        PlayerPrefs.Save();
    }
}