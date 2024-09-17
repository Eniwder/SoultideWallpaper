using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour
{
    private static System.Random random = new System.Random();
    public static GameManager Instance;
    [SerializeField] private MazeManager maze;
    [SerializeField] private DollManager doll;
    [SerializeField] private ScoreManager score;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            maze.Initialize();
            doll.Initialize();
            score.Initialize();
            StartGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartGame()
    {
        StartCoroutine(helper());
        IEnumerator helper()
        {
            yield return MazeManager.Instance.CreateMaze();
            yield return DollManager.Instance.SpawnDoll();
        }
    }

    public void FinishGame()
    {
        StartCoroutine(helper());
        IEnumerator helper()
        {
            yield return DollManager.Instance.EscapeDoll();
            yield return MazeManager.Instance.DeleteMaze();
            StartGame();
        }

    }

    public void ResetGame()
    {
        MazeManager.Instance.CleanMaze();
        DollManager.Instance.CleanDoll();
        StartGame();
    }


}
