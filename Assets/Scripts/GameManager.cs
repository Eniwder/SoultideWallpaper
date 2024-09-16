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

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            maze.Initialize();
            doll.Initialize();
            StartCoroutine(StartGame());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public IEnumerator StartGame()
    {
        yield return MazeManager.Instance.CreateMaze();
        yield return DollManager.Instance.SpawnDoll();
    }

    public void ResetGame()
    {
        MazeManager.Instance.CleanMaze();
        DollManager.Instance.CleanDoll();
        StartCoroutine(StartGame());
    }


}
