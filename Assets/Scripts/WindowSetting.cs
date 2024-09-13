
using UnityEngine;

public class WindowSetting : MonoBehaviour
{
 
    void Start()
    {
        Application.targetFrameRate = ConfigLoader.GetConfig().fps;
    }

}

