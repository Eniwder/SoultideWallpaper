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
    private static Dictionary<string, WaitForSeconds> AnimDuration = new Dictionary<string, WaitForSeconds>(){
      {"gather", new WaitForSeconds(3f) },
      {"dig", new WaitForSeconds(4f)},
      {"hack", new WaitForSeconds(5f)},
      {"mine", new WaitForSeconds(6f)},
      {"eat", new WaitForSeconds(12f)},
      {"homeInsert", new WaitForSeconds(12f)},
      {"idle", new WaitForSeconds(2f)},
      {"sit", new WaitForSeconds(8f)},
      {"sleep", new WaitForSeconds(18f)}
     };
    private float walkTime = 2f;
    void Start()
    {
        skel = GetComponent<SkeletonGraphic>();
        MazeManager.Instance.dollGrid[currentPos.y, currentPos.x] = true;
        skel.AnimationState.Complete += OnInit;
        skel.Skeleton.ScaleX = random.NextDouble() > 0.5 ? 1 : -1;

        normalAnims = skel.SkeletonData.Animations.Items
                            .Where(item =>
                                !(gameObject.name.Contains("Netsuki") && (item.ToString() == "homeInsert02")) &&
                                !(gameObject.name.Contains("Hagakure") && (item.ToString() == "homeInsert04")) &&
                                _normalAnims.Any(anim => item.ToString().Contains(anim)))
                            .Select(animation => animation.ToString()).ToArray();
        foreach (var anim in normalAnims)
        {
            Debug.Log(anim);
        }
    }

    private void Idling()
    {
        StartCoroutine(PlayAnimationForDuration("idle", AnimDuration["idle"], false));
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
        StartCoroutine(PlayAnimationForDuration(anim, AnimDuration.GetValueOrDefault(anim, AnimDuration["homeInsert"]), true));
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

    private IEnumerator PlayAnimationForDuration(string anim, WaitForSeconds wait, bool nextIdle)
    {
        skel.AnimationState.SetAnimation(0, anim, true);
        yield return wait;
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
                track.TrackTime = (track.AnimationEnd - i);
                yield return null;
            }
            track.TrackTime = 0f;
            Destroy(gameObject);
        }
    }


    private IEnumerator Move(Vector2Int nextPos)
    {
        string animName = "walk";
        animName = (gameObject.name.Contains("Netsuki") && random.NextDouble() > 0.5) ? "homeInsert02" :
        (gameObject.name.Contains("Hagakure") && random.NextDouble() > 0.5) ? "homeInsert04" : animName;
        skel.AnimationState.SetAnimation(0, animName, true);
        skel.Skeleton.ScaleX = (currentPos.x + currentPos.y) < (nextPos.x + nextPos.y) ? -1 : 1;

        RectTransform crt = MazeManager.Instance.tiles[currentPos.y, currentPos.x].GetComponent<RectTransform>();
        RectTransform drt = MazeManager.Instance.tiles[nextPos.y, nextPos.x].GetComponent<RectTransform>();
        // 歩き終わる前に存在判定を更新しておくことで、他のキャラと交差できるようになる
        MazeManager.Instance.dollGrid[currentPos.y, currentPos.x] = false;
        currentPos = nextPos;
        MazeManager.Instance.dollGrid[currentPos.y, currentPos.x] = true;
        if (gameObject.name.Contains("Hagakure") && animName == "homeInsert04")
        {
            yield return StartCoroutine(RuriHelper(crt.position, drt.position, 2f));
        }
        else
        {
            yield return StartCoroutine(Helper(crt.position, drt.position, walkTime));
        }
        DollManager.Instance.SortDollsByPosition();
        MazeManager.Instance.walked(currentPos.x, currentPos.y);
        Idling();

        IEnumerator Helper(Vector2 cpos, Vector2 dpos, float duration)
        {
            Vector2 diff = dpos - cpos;
            Vector2 step = diff / duration;
            RectTransform crt = gameObject.GetComponent<RectTransform>();
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                crt.position = cpos + (elapsedTime * step);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            crt.position = dpos;
        }

        IEnumerator RuriHelper(Vector2 cpos, Vector2 dpos, float duration)
        {
            Vector2 diff = dpos - cpos;
            Vector2 step = diff / duration;
            RectTransform crt = gameObject.GetComponent<RectTransform>();
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            crt.position = dpos;
        }
    }


}
