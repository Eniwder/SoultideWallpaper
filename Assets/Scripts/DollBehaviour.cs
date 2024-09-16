using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;

public class DollBehaviour : MonoBehaviour
{
    public SkeletonGraphic skel;
    private int count = 0;
    public Vector2Int currentPos;

    [SpineAnimation]
    public string listOfAllAnimations;
    private static System.Random random = new System.Random();
    // fall,drag,revive,teleport
    void Start()
    {
        skel = GetComponent<SkeletonGraphic>();
        // skel.AnimationState.ClearTrack(0);
        // skel.AnimationState.SetAnimation(0, "teleport", false);
        Debug.Log(skel.SkeletonData.Animations.Items[0]);
    }

    // Update is called once per frame
    void Update()
    {
        count++;
        if (count % 120 == 0)
        {
            skel.AnimationState.SetAnimation(0, skel.SkeletonData.Animations.Items[random.Next(skel.SkeletonData.Animations.Items.Length)], true);
        }
    }

    private void Move()
    {
        Vector2Int nextPos = GetRandomNextPosition();

        // if (GridManager.Instance.IsCellFree(nextPos.x, nextPos.y)) {
        //     GridManager.Instance.SetCellState(currentPos.x, currentPos.y, false);  // 現在位置を空にする
        //     currentPos = nextPos;
        //     GridManager.Instance.SetCellState(currentPos.x, currentPos.y, true);   // 新しい位置を占有
        //     transform.position = new Vector3(currentPos.x, currentPos.y, 0);      // 実際の移動
        // }
    }

    private Vector2Int GetRandomNextPosition()
    {
        // 隣接するマスをランダムに取得するロジック
        return new Vector2Int(currentPos.x + Random.Range(-1, 2), currentPos.y + Random.Range(-1, 2));
    }
}
