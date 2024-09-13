using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;

public class MazeManager : MonoBehaviour
{
    private static System.Random random = new System.Random();
    public static MazeManager Instance;
    public bool[,] characterGrid; // キャラがいるかどうか
    public bool[,] walkableGrid; // 通れるかどうか
    public bool[,] exploredGrid; // 踏破されたかどうか

    private GameObject[,] tiles;

    // 描画用
    public GameObject tilePrefab; // Tileのプレハブ（Canvasの子として配置するImage）
    public Sprite tileActive; // Tileのスプライト
    public Sprite tileDeactive; // Tileのスプライト
    public RectTransform canvasRectTransform; // CanvasのRectTransform

    private int col;
    private int row;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            col = ConfigLoader.GetConfig().maze.col;
            row = ConfigLoader.GetConfig().maze.row;
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 初期化
    public void Initialize()
    {
        characterGrid = new bool[row, col];
        walkableGrid = new bool[row, col];
        exploredGrid = new bool[row, col];
        tiles = new GameObject[row, col];

        InitMaze(col, row);

        GenerateTiles();
        // タイルを結ぶRouteの作成
        // exploredGridに応じてタイルの状態を変更
        // exploredGridに応じて？ルートの状態を変更
        Image image = tiles[3, 3].GetComponent<Image>();
        image.sprite = tileActive;
    }

    private void InitMaze(int col, int row)
    {
        DigStart(col, row);
        DebugShowMaze();
    }

    private void DebugShowMaze()
    {
        string buff = "";
        for (int y = 0; y < row; y++)
        {
            for (int x = 0; x < col; x++)
            {
                buff += walkableGrid[y, x] ? "□" : "■";
            }
            buff += "\n";
        }
        Debug.Log(buff);
    }

    private bool IsInside(int x, int y)
    {
        return 0 <= x && x < col && 0 <= y && y < row;
    }

    private void DigStart(int col, int row)
    {
        int startX = random.Next(0, col - 2);
        int startY = random.Next(0, row - 2);
        if (startY % 2 == 0)
            startY++;
        if (startX % 2 == 0)
            startX++;
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

            int breakNum = (int)(walls.Count() * (ConfigLoader.GetConfig().maze.droprate / 100.0f));
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
                        rest.Append(xy);
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
        return walkableGrid[y, x] && !characterGrid[y, x];
    }

    private void GenerateTiles()
    {
        // float tileWidth = tileActive.rect.width * 0.5f;
        // float tileHeight = tileActive.rect.height * 0.5f;

        RectTransform rectTransform = tilePrefab.GetComponent<RectTransform>();
        float tileWidth = rectTransform.rect.width;
        float tileHeight = rectTransform.rect.height;

        float canvasWidth = canvasRectTransform.rect.width;
        float canvasHeight = canvasRectTransform.rect.height;

        // タイルのサイズをピクセル単位で計算
        float tileWidthUnits = tileWidth / canvasWidth;
        float tileHeightUnits = tileHeight / canvasHeight;

        // 中央寄せのためのオフセットを計算
        float gridWidth = (walkableGrid.GetLength(0) + walkableGrid.GetLength(1)) * tileWidthUnits;
        float gridHeight =
            (walkableGrid.GetLength(0) + walkableGrid.GetLength(1)) * tileHeightUnits;

        float offsetX = (1 - gridWidth) / 2 + tileWidthUnits / 2;
        float offsetY = 0.5f;
        int diff = Mathf.Abs(walkableGrid.GetLength(0) - walkableGrid.GetLength(1));
        offsetY +=
            (
                (walkableGrid.GetLength(0) > walkableGrid.GetLength(1) ? diff : -diff)
                * tileHeightUnits
            ) / 2;

        for (int y = 0; y < walkableGrid.GetLength(0); y++)
        {
            for (int x = 0; x < walkableGrid.GetLength(1); x++)
            {
                if (walkableGrid[y, x])
                {
                    CreateTile(x, y, tileWidthUnits, tileHeightUnits, offsetX, offsetY);
                }
            }
        }

        void CreateTile(
            int x,
            int y,
            float tileWidthUnits,
            float tileHeightUnits,
            float offsetX,
            float offsetY
        )
        {
            GameObject tile = Instantiate(tilePrefab, canvasRectTransform);
            tiles[y, x] = tile;
            Image image = tile.GetComponent<Image>();
            image.sprite = tileDeactive;

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
}
