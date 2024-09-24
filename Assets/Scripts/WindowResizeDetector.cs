using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Runtime.InteropServices;

public class WindowResizeDetector : MonoBehaviour
{
    private int lastScreenWidth;
    private int lastScreenHeight;
    private bool isResizing = false;
    private const float aspectRatio = 16.0f / 9.0f;
    public Text waitingText;

    // Windows APIのインポート
    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(System.IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(System.IntPtr hWnd, System.IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    private static extern System.IntPtr GetActiveWindow();

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    private System.IntPtr windowHandle; // ウィンドウのハンドル
    private int originalX;
    private int originalY;

    void Start()
    {
        waitingText.enabled = false;

        if (MyUtil.IsWrapped())
        {
            Destroy(this);
            return;
        }

        // 初期のスクリーンサイズを記録
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;

        // ウィンドウのハンドルを取得
        windowHandle = GetActiveWindow();
    }

    void Update()
    {
        // ウィンドウサイズが変更されているか確認
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            if (!isResizing)
            {
                // リサイズが開始されたとき
                isResizing = true;
                waitingText.enabled = true;
                GameManager.Instance.CleanGame();
            }

            // ウィンドウサイズの変更を記録
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
        }

        // マウスの左クリックが離された場合
        if (isResizing && Input.GetMouseButtonUp(0))
        {
            waitingText.enabled = false;
            OnResizeEnd();
            StartCoroutine(Helper());
        }
    }

    IEnumerator Helper()
    {
        // アスペクト比調整のロジックによるリサイズ検知を防止
        yield return new WaitForSeconds(0.1f);
        isResizing = false;
        GameManager.Instance.StartGame();
    }

    void OnResizeEnd()
    {
        // リサイズが終了したときに行う処理
        RECT rect;
        GetWindowRect(windowHandle, out rect);

        int newWidth = Screen.width;
        int newHeight = Mathf.RoundToInt(newWidth / aspectRatio);

        if (newHeight > Screen.height)
        {
            newHeight = Screen.height;
            newWidth = Mathf.RoundToInt(newHeight * aspectRatio);
        }

        // リサイズ後にウィンドウの位置を復元
        SetWindowPos(windowHandle, System.IntPtr.Zero, rect.left, rect.top, newWidth, newHeight, 0);
    }

}
