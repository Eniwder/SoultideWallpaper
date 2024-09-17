#if UNITY_STANDALONE_WIN
using System;
using System.Windows.Forms;
using System.Drawing;
using UnityEngine;
#endif
public class TaskTrayController : MonoBehaviour
{
#if UNITY_STANDALONE_WIN
    private NotifyIcon trayIcon;

    void Start()
    {
        if(!ConfigLoader.GetConfig().trayicon) return;
        // タスクトレイにアイコンを追加
        trayIcon = new NotifyIcon();
        trayIcon.Text = "SoultideWallpaper";
         // Resourcesからアイコンを取得
        Texture2D iconTexture = Resources.Load<Texture2D>("icon");

        // Texture2DをBitmapに変換
        Bitmap bitmap = Texture2DToBitmap(iconTexture);

        // BitmapをIconに変換
        trayIcon.Icon = Icon.FromHandle(bitmap.GetHicon());

        // タスクトレイのコンテキストメニューを作成
        trayIcon.ContextMenu = new System.Windows.Forms.ContextMenu(new MenuItem[]
        {
            new MenuItem("Exit", OnExit)
        });

        // タスクトレイにアイコンを表示
        trayIcon.Visible = true;
    }

    // アプリが終了する際にトレイアイコンも削除
    void OnApplicationQuit()
    {
        if(!ConfigLoader.GetConfig().trayicon) return;
        trayIcon.Visible = false;
    }

    // メニューの「Exit」選択時に呼ばれる
    private void OnExit(object sender, EventArgs e)
    {
        UnityEngine.Application.Quit();
    }

    private Bitmap Texture2DToBitmap(Texture2D texture)
    {
        // Texture2DからBitmapを作成
        Color32[] colors = texture.GetPixels32();
        Bitmap bitmap = new Bitmap(texture.width, texture.height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                Color32 color = colors[y * texture.width + x];
                bitmap.SetPixel(x, y, System.Drawing.Color.FromArgb(color.a, color.r, color.g, color.b));
            }
        }

        return bitmap;
    }
#endif

}
