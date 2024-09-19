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
            scoreText.text = score.ToString();
            scoreText.enabled = ConfigLoader.GetConfig().score;
        }
        else
        {
            Destroy(gameObject);
        }

    }
    public void AddScore(int value)
    {
        score += value;
        scoreText.text = score.ToString();
    }

    public void SaveScore()
    {
        PlayerPrefs.SetInt("Score", score);
        PlayerPrefs.Save();
    }
}