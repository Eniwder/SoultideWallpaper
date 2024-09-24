using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
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

            // タスクバーの位置を考慮してスコアの位置をずらす
            if (MyUtil.IsWrapped())
            {
                List<DisplayInfo> displayLayout = new List<DisplayInfo>();
                Screen.GetDisplayLayout(displayLayout);
                RectTransform rectTransform = scoreText.GetComponent<RectTransform>();
                rectTransform.anchoredPosition += new Vector2(displayLayout[0].workArea.width - displayLayout[0].width + displayLayout[0].workArea.x,
                  displayLayout[0].height - displayLayout[0].workArea.height - displayLayout[0].workArea.y);
            }
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