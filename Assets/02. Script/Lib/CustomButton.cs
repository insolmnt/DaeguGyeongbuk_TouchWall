using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomButton : MonoBehaviour
{
    public Image ButtonImage;
    public Text ButtonText;

    public Color SelectButtonColor = Color.black;
    public Color UnSelectButtonColor = Color.gray;

    public Color SelectTextColor = Color.yellow;
    public Color UnSelectTextColor = Color.white;

    [Header("Data")]
    public int Index = 0;
    public bool IsSelect = false;



    public void Select(bool isSelect)
    {
        IsSelect = isSelect;

        ButtonImage.color = isSelect ? SelectButtonColor : UnSelectButtonColor;
        ButtonText.color = isSelect ? SelectTextColor : UnSelectTextColor;
    }


    [ContextMenu("Select")]
    private void TestSelect()
    {
        Select(true);
    }
    [ContextMenu("UnSelect")]
    private void TestUnSelect()
    {
        Select(false);
    }
}
