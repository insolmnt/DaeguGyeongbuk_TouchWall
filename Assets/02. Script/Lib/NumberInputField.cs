using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NumberInputField : MonoBehaviour
{
    public InputField InputField;


    public string OutputFormat = "F0";
    public bool IsPercent = false;
    public bool IsUseMinMax;
    public float Min;
    public float Max;

    public float Val
    {
        get
        {
            return _Val;
        }
        set
        {
            mIsChange = true;
            _Val = value;
            if (IsUseMinMax)
            {
                _Val = Mathf.Clamp(_Val, Min, Max);
            }
            InputField.text = _Val.ToString(OutputFormat);
            mIsChange = false;
        }
    }
    private float _Val = 0;

    private bool mIsChange = false;
    public void OnInputFieldChangeEnd()
    {
        if (mIsChange)
        {
            return;
        }

        mIsChange = true;
        float val = 0;
        if(float.TryParse(InputField.text, out val))
        {
            if (IsPercent)
            {
                Val = val * 0.01f;
            }
            else
            {
                Val = val;
            }
        }
        else
        {
            Val = _Val;
        }

        mIsChange = false;
    }
}
