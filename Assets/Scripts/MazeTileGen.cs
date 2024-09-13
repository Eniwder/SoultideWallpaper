using UnityEngine;
using UnityEngine.UI;

public class MazeTileGen : MonoBehaviour
{
    public GameObject tilePrefab; // Tileのプレハブ（Canvasの子として配置するImage）
    public Sprite tileSprite; // Tileのスプライト
    public bool[,] grid; // bool[,]のグリッドマップ
    public RectTransform canvasRectTransform; // CanvasのRectTransform

    void Start()
    {
        grid = MazeManager.Instance.walkableGrid;
        // GenerateTiles();
    }
}




// rt.anchorMin = new Vector2((x+y)/2 * tileWidthUnits, (y-x)/2 * tileHeightUnits);
// rt.anchorMax = new Vector2(((x+y)/2 + 1) * tileWidthUnits, ((y-x)/2 + 1) * tileHeightUnits);
