using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;
using System.Linq;

public class DollBehaviour : MonoBehaviour
{
    public SkeletonGraphic skel;
    public Vector2Int currentPos;
    private static System.Random random = new System.Random();
    private string[] _normalAnims = { "duration", "dig", "eat", "gather", "hack", "homeInsert", "idle", "sit", "sleep", "mine" };
    private string[] normalAnims;
    public bool escape = false;
    // private Dictionary<string, float> animDuration = new Dictionary<string, float>(){
    //   {"gather", 0.5f },
    //   {"dig", 0.5f},
    //   {"hack", 0.5f},
    //   {"mine", 0.5f},
    //   {"eat", 0.5f},
    //   {"homeInsert", 0.5f},
    //   {"idle", 0.5f},
    //   {"sit", 0.5f},
    //   {"sleep", 0.5f},
    //   {"walk", 0.5f }
    // };

    private Dictionary<string, float> animDuration = new Dictionary<string, float>(){
      {"gather", 3f },
      {"dig", 4f},
      {"hack", 5f},
      {"mine", 6f},
      {"eat", 12f},
      {"homeInsert", 12f},
      {"idle", 2f},
      {"sit", 8f},
      {"sleep", 18f},
      {"walk", 2f }
    };
    void Start()
    {
        skel = GetComponent<SkeletonGraphic>();
        MazeManager.Instance.dollGrid[currentPos.y, currentPos.x] = true;
        skel.AnimationState.Complete += OnInit;
        skel.Skeleton.ScaleX = random.NextDouble() > 0.5 ? 1 : -1;

        normalAnims = skel.SkeletonData.Animations.Items
                            .Where(item => _normalAnims.Any(anim => item.ToString().Contains(anim)))
                            .Select(animation => animation.ToString()).ToArray();
    }

    private void Idling()
    {
        StartCoroutine(PlayAnimationForDuration("idle", animDuration["idle"], false));
    }

    private void RandomMotion()
    {
        if (random.NextDouble() <= ConfigLoader.GetConfig().doll.walkrate)
        {
            var walkables = walkableCand();
            if (walkables.Length > 0)
            {
                StartCoroutine(Move(walkables[random.Next(walkables.Length)]));
                return;
            }
        }
        var anim = normalAnims[random.Next(normalAnims.Length)];
        AddScore(anim);
        StartCoroutine(PlayAnimationForDuration(anim, animDuration.GetValueOrDefault(anim, animDuration["homeInsert"]), true));
    }

    private void AddScore(string anim)
    {
        if (anim == "gather")
        {
            ScoreManager.Instance.AddScore(1);
        }
        else if (anim == "dig")
        {
            ScoreManager.Instance.AddScore(2);

        }
        else if (anim == "hack")
        {
            ScoreManager.Instance.AddScore(3);

        }
        else if (anim == "mine")
        {
            ScoreManager.Instance.AddScore(4);

        }
    }

    private IEnumerator PlayAnimationForDuration(string anim, float time, bool nextIdle)
    {
        skel.AnimationState.SetAnimation(0, anim, true);
        yield return new WaitForSeconds(time);
        skel.AnimationState.SetEmptyAnimation(0, 0);
        if (nextIdle)
        {
            Idling();
        }
        else
        {
            if (escape)
            {
                FallReverse();
            }
            else
            {
                RandomMotion();
            }

        }
    }

    private void OnInit(Spine.TrackEntry trackEntry)
    {
        MazeManager.Instance.walked(currentPos.x, currentPos.y);
        Idling();
        skel.AnimationState.Complete -= OnInit;
        return;
    }

    private Vector2Int[] walkableCand()
    {
        var ret = new List<Vector2Int>();
        int[] dxs = { -1, 1, 0, 0 };
        int[] dys = { 0, 0, -1, 1 };
        for (int i = 0; i < 4; i++)
        {
            int dx = currentPos.x + dxs[i];
            int dy = currentPos.y + dys[i];
            if (MazeManager.Instance.IsInside(dx, dy) &&
             MazeManager.Instance.walkableGrid[dy, dx] && !MazeManager.Instance.dollGrid[dy, dx])
            {
                // 未踏破のタイルは2倍の候補数にして進む増やす
                if (!MazeManager.Instance.exploredGrid[dy, dx])
                {
                    ret.Add(new Vector2Int(dx, dy));
                }
                ret.Add(new Vector2Int(dx, dy));
            }
        }
        return ret.ToArray();
    }

    private void FallReverse()
    {
        var track = skel.AnimationState.SetAnimation(0, "fall", false);
        track.TimeScale = 0f;
        StartCoroutine(Helper());

        IEnumerator Helper()
        {
            for (float i = 0f; i < track.AnimationEnd; i += 0.016f)
            {
                yield return track.TrackTime = (track.AnimationEnd - i);
            }
            track.TrackTime = 0f;
            Destroy(gameObject);
        }
    }


    private IEnumerator Move(Vector2Int nextPos)
    {
        skel.AnimationState.SetAnimation(0, "walk", true);
        skel.Skeleton.ScaleX = (currentPos.x + currentPos.y) < (nextPos.x + nextPos.y) ? -1 : 1;

        RectTransform crt = MazeManager.Instance.tiles[currentPos.y, currentPos.x].GetComponent<RectTransform>();
        RectTransform drt = MazeManager.Instance.tiles[nextPos.y, nextPos.x].GetComponent<RectTransform>();
        // 歩き終わる前に存在判定を更新しておくことで、他のキャラと交差できるようになる
        MazeManager.Instance.dollGrid[currentPos.y, currentPos.x] = false;
        currentPos = nextPos;
        MazeManager.Instance.dollGrid[currentPos.y, currentPos.x] = true;
        yield return StartCoroutine(Helper(crt.position, drt.position, animDuration["walk"]));
        MazeManager.Instance.walked(currentPos.x, currentPos.y);
        Idling();

        IEnumerator Helper(Vector2 cpos, Vector2 dpos, float duration)
        {
            Vector2 diff = dpos - cpos;
            Vector2 step = diff / duration;
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                gameObject.GetComponent<RectTransform>().position = cpos + (elapsedTime * step);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            gameObject.GetComponent<RectTransform>().position = dpos;
        }
    }


}
