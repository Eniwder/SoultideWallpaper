using UnityEngine;
using Spine.Unity;
using Spine.Unity.AttachmentTools;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
public class DollManager : MonoBehaviour
{
    private static System.Random random = new System.Random();
    public static DollManager Instance;
    public string[] dollList;
    public List<GameObject> dolls;
    public RectTransform canvasRectTransform;
    private bool closing = false;

    public void Initialize()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    public void Update()
    {
        if (Instance == null || dolls.Count < 1 || closing) return;
        SortDollsByPosition();
    }

    public void CleanDoll()
    {
        foreach (var doll in dolls)
        {
            Destroy(doll);
        }
        dolls.Clear();
    }

    // Y座標に基づいてキャラクターをソートする TODO パフォーマンスへの影響
    private void SortDollsByPosition()
    {

        var sortedDolls = dolls.OrderByDescending(c => c.GetComponent<RectTransform>().anchoredPosition.y).ToArray();

        for (int i = 0; i < sortedDolls.Length; i++)
        {
            sortedDolls[i].transform.SetSiblingIndex(i);
        }
    }

    public IEnumerator SpawnDoll()
    {
        closing = false;
        int dollNum = ConfigLoader.GetConfig().doll.num < 1 ? getFitDollNum() : ConfigLoader.GetConfig().doll.num;
        dollList = getDolls(dollNum);
        var walkableGridPosList = new List<Vector2Int>();
        for (int y = 0; y < MazeManager.Instance.row; y++)
        {
            for (int x = 0; x < MazeManager.Instance.col; x++)
            {
                if (MazeManager.Instance.walkableGrid[y, x])
                {
                    walkableGridPosList.Add(new Vector2Int(x, y));
                }
            }
        }

        var walkableGridPos = walkableGridPosList.ToArray();
        MyUtil.Shuffle(walkableGridPos);
        for (int i = 0; i < dollList.Length && i < walkableGridPos.Length; i++)
        {
            GameObject prefab = Resources.Load<GameObject>("Prefabs/Spine/" + dollList[i]);
            GameObject doll = Instantiate(prefab, canvasRectTransform);
            DollBehaviour dollScript = doll.GetComponent<DollBehaviour>();
            dollScript.currentPos = new Vector2Int(walkableGridPos[i].x, walkableGridPos[i].y);
            RectTransform trt = MazeManager.Instance.tiles[walkableGridPos[i].y, walkableGridPos[i].x].GetComponent<RectTransform>();
            doll.transform.position = trt.position;
            dolls.Add(doll);
            yield return new WaitForSeconds(0.3f);
        }

        int getFitDollNum()
        {
            return (int)((MazeManager.Instance.col + MazeManager.Instance.row) * 0.5);
        }
    }

    public IEnumerator EscapeDoll()
    {
        closing = true;
        foreach (var doll in dolls)
        {
            DollBehaviour dollScript = doll.GetComponent<DollBehaviour>();
            dollScript.escape = true;
        }
        bool allEscape = false;
        while (!allEscape)
        {
            allEscape = true;
            foreach (var doll in dolls)
            {
                allEscape = allEscape && (doll == null);
            }
            yield return null;
        }
        dolls.Clear();
    }

    private string[] getDolls(int num)
    {
        var prefixGroups = ConfigLoader.GetConfig().doll.cand.GroupBy(item => GetPrefix(item)).ToList();
        var selectedItems = new List<string>();
        while (selectedItems.Count < num && prefixGroups.Count > 0)
        {
            var randomGroup = prefixGroups[random.Next(prefixGroups.Count)];
            var randomItem = randomGroup.ElementAt(random.Next(randomGroup.Count()));
            selectedItems.Add(randomItem);
            prefixGroups.Remove(randomGroup);
        }

        return selectedItems.ToArray();

        string GetPrefix(string item)
        {
            var parts = item.Split('_');
            return parts[0];
        }
    }
}
