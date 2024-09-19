using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.U2D;

public static class Direction
{
    public const int Horizontal = 0;
    public const int Vertical = 1;
}
public class MazeManager : MonoBehaviour
{
    private static System.Random random = new System.Random();
    public static MazeManager Instance;
    public bool[,] dollGrid; // キャラがいるかどうか
    public bool[,] walkableGrid; // 通れるかどうか
    public bool[,] exploredGrid; // 踏破されたかどうか
    private int unexplored;


    private MazePool pool = new MazePool();
    public GameObject[,,] roads;
    public GameObject[,] tiles;

    // 描画用
    public GameObject tilePrefab;
    public SpriteAtlas mazeAtlas;
    private Sprite tileActive;
    private Sprite tileDeactive;

    public GameObject roadPrefab;
    private Sprite roadActive;
    private Sprite roadDeactive;


    public RectTransform canvasRectTransform;

    public int col;
    public int row;
#if UNITY_EDITOR
    private float totalFadeDuration = 2f;
#else
    private float totalFadeDuration = 10f;
#endif

    public void Initialize()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            tileActive = mazeAtlas.GetSprite("Tile_White");
            tileDeactive = mazeAtlas.GetSprite("Tile_Black");
            roadActive = mazeAtlas.GetSprite("Route_White");
            roadDeactive = mazeAtlas.GetSprite("Route_Black");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 初期化
    public IEnumerator CreateMaze()
    {
        col = ConfigLoader.GetConfig().maze.col;
        row = ConfigLoader.GetConfig().maze.row;
        if (col < 1 || row < 1)
        {
            // 1920x1080のとき、(col+row)<35。これを基準とする。
            // screenWidth/1920*35 ≒ screenWidth*0.01823
            int maxNum = (int)(Screen.width * 0.01823f);
            int minNum = (int)Mathf.Sqrt(maxNum); // 最小値は大体このくらいでよさそう？
            col = random.Next(minNum, maxNum - minNum);
            row = maxNum - col;
            // MyUtil.Log(maxNum, minNum, col, row);
        }

        dollGrid = new bool[row, col];
        walkableGrid = new bool[row, col];
        exploredGrid = new bool[row, col];
        tiles = new GameObject[row, col];
        roads = new GameObject[row, col, 2]; // 2は方向の数。ある地点から→と↓だけを持つ。

        InitMaze(col, row);
        InitTiles(col, row);
        InitRoads(col, row);

        yield return StartCoroutine(FadeInMaze(col, row));
    }

    public void walked(int x, int y)
    {
        if (exploredGrid[y, x]) return;
        unexplored--;
        ScoreManager.Instance.AddScore(10);
        ScoreManager.Instance.SaveScore();
        exploredGrid[y, x] = true;
        Image image = tiles[y, x].GetComponent<Image>();
        image.sprite = tileActive;

        // roadsの生成ロジック的にIsInsideは不要だが一応やっておく
        if (IsInside(x + 1, y) && roads[y, x, Direction.Horizontal] != null && exploredGrid[y, x + 1])
        {
            image = roads[y, x, Direction.Horizontal].GetComponent<Image>();
            image.sprite = roadActive;
        }
        if (IsInside(x, y + 1) && roads[y, x, Direction.Vertical] != null && exploredGrid[y + 1, x])
        {
            image = roads[y, x, Direction.Vertical].GetComponent<Image>();
            image.sprite = roadActive;
        }
        if (IsInside(x - 1, y) && roads[y, x - 1, Direction.Horizontal] != null && exploredGrid[y, x - 1])
        {
            image = roads[y, x - 1, Direction.Horizontal].GetComponent<Image>();
            image.sprite = roadActive;
        }
        if (IsInside(x, y - 1) && roads[y - 1, x, Direction.Vertical] != null && exploredGrid[y - 1, x])
        {
            image = roads[y - 1, x, Direction.Vertical].GetComponent<Image>();
            image.sprite = roadActive;
        }
        if (unexplored == 0)
        {
            GameManager.Instance.FinishGame();
        }
    }

    public IEnumerator DeleteMaze()
    {
        yield return StartCoroutine(FadeOutMaze(col, row));
        foreach (var tile in tiles)
        {
            if (tile != null)
            {
                pool.ReturnTile(tile);
            }
        }
        foreach (var road in roads)
        {
            if (road != null)
            {
                pool.ReturnRoad(road);
            }
        }
    }

    private IEnumerator FadeOutMaze(int col, int row)
    {
        StartCoroutine(FadeRoad(col, row, totalFadeDuration, false));
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(FadeTile(col, row, totalFadeDuration, false));
    }

    private IEnumerator FadeInMaze(int col, int row)
    {
        StartCoroutine(FadeRoad(col, row, totalFadeDuration, true));
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(FadeTile(col, row, totalFadeDuration, true));
    }

    private IEnumerator FadeTile(int col, int row, float totalFadeDuration, bool fadeIn)
    {
        float elapsedTime = 0f;
        float fadeDuration = 1f;
        Image[,] images = new Image[row, col];
        float[,] randomDelays = new float[row, col];
        for (int y = 0; y < row; y++)
        {
            for (int x = 0; x < col; x++)
            {
                if (tiles[y, x])
                {
                    images[y, x] = tiles[y, x].GetComponent<Image>();
                    randomDelays[y, x] = (float)(random.NextDouble() * (totalFadeDuration - fadeDuration));
                }
            }
        }
        float xy = (totalFadeDuration - fadeDuration) / (col + row + 0.01f); // 左から右にFadeInしたい場合
        while (elapsedTime < totalFadeDuration)
        {
            for (int y = 0; y < row; y++)
            {
                for (int x = 0; x < col; x++)
                {
                    if (images[y, x])
                    {
                        Color color = images[y, x].color;
                        float alpha = Mathf.Clamp01(Mathf.Max(elapsedTime - xy * (x + y), 0f) / fadeDuration); // 左から右にFadeInしたい場合
                                                                                                               // float alpha = Mathf.Clamp01(Mathf.Max(elapsedTime - randomDelays[y, x], 0f) / fadeDuration);
                        color.a = fadeIn ? alpha : 1 - alpha;
                        images[y, x].color = color;
                    }
                }
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        foreach (Image image in images)
        {
            if (!image)
                continue;
            image.color = new Color(image.color.r, image.color.g, image.color.b, fadeIn ? 1 : 0);
        }
    }

    private IEnumerator FadeRoad(int col, int row, float totalFadeDuration, bool fadeIn)
    {
        float elapsedTime = 0f;
        float fadeDuration = 1f;
        Image[,,] images = new Image[row, col, 2];
        for (int y = 0; y < row; y++)
        {
            for (int x = 0; x < col; x++)
            {
                if (roads[y, x, Direction.Horizontal])
                {
                    images[y, x, Direction.Horizontal] = roads[y, x, Direction.Horizontal].GetComponent<Image>();
                }
                if (roads[y, x, Direction.Vertical])
                {
                    images[y, x, Direction.Vertical] = roads[y, x, Direction.Vertical].GetComponent<Image>();
                }
            }
        }
        float xy = (totalFadeDuration - fadeDuration) / (col + row + 0.01f); // 左から右にFadeInしたい場合
        while (elapsedTime < totalFadeDuration)
        {
            for (int y = 0; y < row; y++)
            {
                for (int x = 0; x < col; x++)
                {
                    if (roads[y, x, Direction.Horizontal])
                    {
                        Color color = images[y, x, Direction.Horizontal].color;
                        float alpha = Mathf.Clamp01(Mathf.Max(elapsedTime - xy * (x + y), 0f) / fadeDuration); // 左から右にFadeInしたい場合
                        color.a = fadeIn ? alpha : 1 - alpha;
                        images[y, x, Direction.Horizontal].color = color;
                    }
                    if (roads[y, x, Direction.Vertical])
                    {
                        Color color = images[y, x, Direction.Vertical].color;
                        float alpha = Mathf.Clamp01(Mathf.Max(elapsedTime - xy * (x + y), 0f) / fadeDuration); // 左から右にFadeInしたい場合
                        color.a = fadeIn ? alpha : 1 - alpha;
                        images[y, x, Direction.Vertical].color = color;
                    }
                }
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        foreach (Image image in images)
        {
            if (!image)
                continue;
            image.color = new Color(image.color.r, image.color.g, image.color.b, fadeIn ? 1 : 0);
        }
    }

    private void InitMaze(int col, int row)
    {
        DigStart(col, row);
        unexplored = 0;
        for (int y = 0; y < row; y++)
        {
            for (int x = 0; x < col; x++)
            {
                unexplored += walkableGrid[y, x] ? 1 : 0;
            }
        }
        // DebugShowMaze();
    }

    private void InitRoads(int col, int row)
    {
        RectTransform tileRt = tilePrefab.GetComponent<RectTransform>();
        float canvasWidth = canvasRectTransform.rect.width;
        float canvasHeight = canvasRectTransform.rect.height;
        float tileWidth = tileRt.rect.width;
        float tileHeight = tileRt.rect.height;
        float tileWidthUnits = tileWidth / canvasWidth;
        float tileHeightUnits = tileHeight / canvasHeight;

        for (int y = 0; y < row; y++)
        {
            for (int x = 0; x < col; x++)
            {
                if (walkableGrid[y, x] && IsInside(x + 1, y) && walkableGrid[y, x + 1])
                {
                    roads[y, x, Direction.Horizontal] = CreateRoad(x, y, Direction.Horizontal);
                }
                if (walkableGrid[y, x] && IsInside(x, y + 1) && walkableGrid[y + 1, x])
                {
                    roads[y, x, Direction.Vertical] = CreateRoad(x, y, Direction.Vertical);
                }
            }
        }

        GameObject CreateRoad(int x, int y, int direction)
        {
            // GameObject road = Instantiate(roadPrefab, canvasRectTransform);
            GameObject road = pool.GetRoad(roadPrefab, canvasRectTransform);

            Image image = road.GetComponent<Image>();
            image.sprite = roadDeactive;
            image.color = new Color(image.color.r, image.color.g, image.color.b, 0);

            RectTransform trt = tiles[y, x].GetComponent<RectTransform>();
            RectTransform rrt = road.GetComponent<RectTransform>();
            rrt.SetSiblingIndex(1);

            if (direction == Direction.Horizontal)
            {
                rrt.anchorMin = new Vector2(
                    trt.anchorMin.x + tileWidthUnits * 0.65f,
                    trt.anchorMin.y + tileHeightUnits * 0.65f
                );
                rrt.anchorMax = new Vector2(
                    trt.anchorMin.x + tileWidthUnits * 1.35f,
                    trt.anchorMin.y + tileHeightUnits * 1.35f
                );
                rrt.Rotate(0, 0, 30f);
            }
            else
            {
                rrt.anchorMin = new Vector2(
                    trt.anchorMin.x + tileWidthUnits * 0.65f,
                    trt.anchorMin.y - tileHeightUnits * 0.65f
                );
                rrt.anchorMax = new Vector2(
                    trt.anchorMin.x + tileWidthUnits * 1.35f,
                    trt.anchorMin.y + tileHeightUnits * 0.65f
                );
                rrt.Rotate(0, 0, -30f);
            }

            rrt.offsetMin = Vector2.zero;
            rrt.offsetMax = Vector2.zero;
            return road;
        }
    }

    public bool IsInside(int x, int y)
    {
        return 0 <= x && x < col && 0 <= y && y < row;
    }

    private void DigStart(int col, int row)
    {
        int startX = random.Next(0, col - 2);
        int startY = random.Next(0, row - 2);
        Dig(startX, startY);
        randomDig(col, row);

        void Dig(int x, int y)
        {
            walkableGrid[y, x] = true;
            int[] moveX = { 0, 0, -1, 1 };
            int[] moveY = { 1, -1, 0, 0 };
            int[] act = { 0, 1, 2, 3 };
            MyUtil.Shuffle(act);
            foreach (int idx in act)
            {
                int dx = x + moveX[idx] * 2;
                int dy = y + moveY[idx] * 2;
                if (IsInside(dx, dy) && !walkableGrid[dy, dx])
                {
                    walkableGrid[dy - moveY[idx], dx - moveX[idx]] = true;
                    Dig(dx, dy);
                }
            }
        }

        void randomDig(int col, int row)
        {
            var walls = Enumerable
                .Range(0, row)
                .SelectMany(
                    y =>
                        Enumerable
                            .Range(0, col)
                            .Where(x => !walkableGrid[y, x])
                            .Select(x => new Vector2Int(x, y))
                )
                .ToArray();

            if (ConfigLoader.GetConfig().maze.droprate >= 1)
            {
                foreach (var wall in walls)
                {
                    walkableGrid[wall.y, wall.x] = true;
                }
                return;
            }

            int breakNum = (int)(walls.Count() * ConfigLoader.GetConfig().maze.droprate);
            MyUtil.Shuffle(walls);
            var toBreak = walls.Take(breakNum).ToList();
            while (true)
            {
                var rest = new Queue<Vector2Int>();
                foreach (Vector2Int xy in toBreak)
                {
                    if (IsNextToRoute(xy.x, xy.y))
                    {
                        walkableGrid[xy.y, xy.x] = true;
                    }
                    else
                    {
                        rest.Enqueue(xy);
                    }
                }
                if (toBreak.Count() == rest.Count())
                    break;
                toBreak = rest.ToList();
            }
        }
    }

    private bool IsNextToRoute(int x, int y)
    {
        return (IsInside(x + 1, y) && walkableGrid[y, x + 1])
            || (IsInside(x - 1, y) && walkableGrid[y, x - 1])
            || (IsInside(x, y + 1) && walkableGrid[y + 1, x])
            || (IsInside(x, y - 1) && walkableGrid[y - 1, x]);
    }

    // マスが通れるかどうかを判定
    public bool CanMoveTo(int x, int y)
    {
        return walkableGrid[y, x] && !dollGrid[y, x];
    }

    private void InitTiles(int col, int row)
    {
        RectTransform rectTransform = tilePrefab.GetComponent<RectTransform>();
        float tileWidth = rectTransform.rect.width;
        float tileHeight = rectTransform.rect.height;

        float canvasWidth = canvasRectTransform.rect.width;
        float canvasHeight = canvasRectTransform.rect.height;

        // タイルのサイズをピクセル単位で計算
        float tileWidthUnits = tileWidth / canvasWidth;
        float tileHeightUnits = tileHeight / canvasHeight;

        // 中央寄せのためのオフセットを計算
        float gridWidth = (col + row) * tileWidthUnits;
        float gridHeight = (col + row) * tileHeightUnits;

        float offsetX = (1 - gridWidth) / 2 + tileWidthUnits / 2;
        float offsetY = 0.5f;
        int diff = Mathf.Abs(col - row);
        offsetY += ((col > row ? -diff : diff) * tileHeightUnits) / 2;

        for (int y = 0; y < row; y++)
        {
            for (int x = 0; x < col; x++)
            {
                if (walkableGrid[y, x])
                {
                    CreateTile(x, y);
                }
            }
        }

        void CreateTile(int x, int y)
        {
            // GameObject tile = Instantiate(tilePrefab, canvasRectTransform);
            GameObject tile = pool.GetTile(tilePrefab, canvasRectTransform);
            tiles[y, x] = tile;
            Image image = tile.GetComponent<Image>();
            image.sprite = tileDeactive;
            image.color = new Color(image.color.r, image.color.g, image.color.b, 0);

            RectTransform rt = tile.GetComponent<RectTransform>();

            rt.anchorMin = new Vector2(
                (x + y) * tileWidthUnits + offsetX,
                (x - y) * tileHeightUnits + offsetY
            );
            rt.anchorMax = new Vector2(
                ((x + y) + 1) * tileWidthUnits + offsetX,
                ((x - y) + 1) * tileHeightUnits + offsetY
            );

            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
    class MazePool
    {
        private Queue<GameObject> tiles = new Queue<GameObject>();
        private Queue<GameObject> roads = new Queue<GameObject>();

        public GameObject GetTile(GameObject tilePrefab, Transform parent)
        {
            GameObject tile;
            if (tiles.Count > 0)
            {
                tile = tiles.Dequeue();
                tile.SetActive(true);
            }
            else
            {
                tile = GameObject.Instantiate(tilePrefab, parent);
            }
            return tile;
        }

        public void ReturnTile(GameObject tile)
        {
            tile.SetActive(false);
            tiles.Enqueue(tile);
        }
        public GameObject GetRoad(GameObject roadPrefab, Transform parent)
        {
            GameObject road;
            if (roads.Count > 0)
            {
                road = roads.Dequeue();
                road.SetActive(true);
            }
            else
            {
                road = GameObject.Instantiate(roadPrefab, parent);
            }
            return road;
        }

        public void ReturnRoad(GameObject road)
        {
            road.SetActive(false);
            road.GetComponent<RectTransform>().localRotation = Quaternion.identity;
            roads.Enqueue(road);
        }
    }
}
