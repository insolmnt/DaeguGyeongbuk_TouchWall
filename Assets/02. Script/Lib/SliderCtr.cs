using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using UnityEngine.Events;

public class SliderCtr : MonoBehaviour {
    public Slider mSlider;
    public Text ValText;
    public InputField mInputField;
    public string ViewStr = "F3";


    public UnityEvent OnValChange;
    public int Index;

    public float Val
    {
        set
        {
            if(value > mSlider.maxValue)
            {
                mSlider.maxValue = value;
            }
            if (value < mSlider.minValue)
            {
                mSlider.minValue = value;
            }
            mSlider.value = value;
            OnSliderValChange();
        }
        get
        {
            return mSlider.value;
        }
    }

    public bool interactable
    {
        set
        {
            mSlider.interactable = value;
            if(mInputField != null)
            {
                mInputField.interactable = value;
            }
        }
        get
        {
            return mSlider.interactable;
        }

    }

    public void OnPlusMinusButtonClick(float val)
    {
        Val += val;
    }

    private float PlusMinus = 0;
    private float CurrentTime = 0;
    private bool DownDelay = false;
    public void OnPlusMinusButtonPointDown(float val)
    {
        //Val += val;
        DownDelay = false;
        CurrentTime = 0;
        PlusMinus = val;
    }
    private void Update()
    {
        if (PlusMinus != 0)
        {
            CurrentTime += Time.deltaTime;

            if (DownDelay)
            {
                if (CurrentTime > 0.05f)
                {
                    Val += PlusMinus;
                    CurrentTime = 0;
                }
            }
            else //누르기 시작
            {
                if (CurrentTime > 0.5f)
                {
                    DownDelay = true;
                    CurrentTime = 0;
                }
            }

            if (Input.GetMouseButton(0) == false)
            {
                PlusMinus = 0;
            }
        }
    }

    [ContextMenu("SetData")]
    private void SetData()
    {
        mSlider = GetComponent<Slider>();
        if(mSlider == null)
        {
            mSlider = GetComponentInChildren<Slider>();
        }
        mInputField = GetComponentInChildren<InputField>();
        var list = GetComponentsInChildren<Text>();
        foreach(var te in list)
        {
            if (te.name.ToUpper().Contains("VAL"))
            {
                ValText = te;
            }
        }

        if(mSlider != null)
        {
            if (mSlider.wholeNumbers)
            {
                ViewStr = "F0";
            }
            else if(mSlider.maxValue < 2)
            {
                ViewStr = "F2";
            }
            else
            {
                ViewStr = "F1";
            }
            OnSliderValChange();
        }
    }

    void Awake () {
        //ValText.text = "" + mSlider.value.ToString(ViewStr);
    }
	
    public void OnSliderValChange()
    {
        if(ValText != null)
            ValText.text = "" + mSlider.value.ToString(ViewStr);
        if (mInputField != null)
            mInputField.text = "" + mSlider.value.ToString(ViewStr);

        if (OnValChange != null)
        {
            OnValChange.Invoke();
        }
    }

    public void OnInputFieldValChangeEnd()
    {
        try
        {
            float val = float.Parse(mInputField.text);
            if (val > mSlider.maxValue)
            {
                mSlider.maxValue = val;
            }
            else if (val < mSlider.minValue)
            {
                mSlider.minValue = val;
            }
            Val = val;
        }
        catch
        {

        }
    }
}
