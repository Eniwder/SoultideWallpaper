using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;

public class ResetButton : MonoBehaviour
{

    [SerializeField] Button testBtn;

    void Start()
    {
        testBtn.onClick.AddListener(OnClickEvent);
    }


    public void OnClickEvent()
    {
        Debug.Log("Click");
        GameManager.Instance.ResetGame();
    }
}
