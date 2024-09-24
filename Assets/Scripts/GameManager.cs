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
        StartCoroutine(Helper());
        IEnumerator Helper()
        {

            yield return MazeManager.Instance.CreateMaze();
            yield return DollManager.Instance.SpawnDoll();

            Resources.UnloadUnusedAssets();
        }
    }

    public void FinishGame()
    {
        StartCoroutine(Helper());
        IEnumerator Helper()
        {
            yield return DollManager.Instance.EscapeDoll();
            yield return MazeManager.Instance.FinishMaze();
            //GC
            System.GC.Collect();
            //使ってないアセットをアンロード
            Resources.UnloadUnusedAssets();
            yield return new WaitForSeconds(3f);
            StartGame();
        }
    }

    public void CleanGame()
    {
        StopAllCoroutines();

        MazeManager.Instance.CleanMaze();
        DollManager.Instance.CleanDoll();
        System.GC.Collect();
        Resources.UnloadUnusedAssets();
    }

}
