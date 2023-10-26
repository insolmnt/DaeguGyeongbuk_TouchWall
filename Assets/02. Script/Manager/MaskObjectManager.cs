using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaskObjectManager : MonoBehaviour
{
    [Header("곡선 마스크")]
    public CurveMaskImage MainMask;
    public CurveMaskImage MiddleMask;

    [Header("Warp Image 마스크")]
    public WarpMaskImage WarpMaskImagePrefab;
    public RectTransform[] WarpMaskImageParent;
    public List<WarpMaskImage> WarpMaskImageList = new List<WarpMaskImage>();

    [Header("Data")]
    public MaskObjectData Data;

    private void Start()
    {
        WarpMaskImagePrefab.gameObject.SetActive(false);
    }

    public void Save()
    {
        DataManager.SetData("MaskObject", Data);
    }
    public void Load()
    {
        Data = DataManager.GetData<MaskObjectData>("MaskObject");
        if(Data == null)
        {
            Data = new MaskObjectData();
        }

        MainMask.SetData(Data.MainMaskImage);
        MiddleMask.SetData(Data.MiddleMaskImage);
        SetWarpMaskData();
    }
    public void SetWarpMaskData()
    {
        foreach(var image in WarpMaskImageList)
        {
            image.gameObject.SetActive(false);
        }

        foreach(var data in Data.WarpMaskImage)
        {
            var image = GetIdleWarpMaskImage();
            image.gameObject.SetActive(true);
            image.SetData(data);
        }
    }

    internal WarpMaskImage AddWarpImage()
    {
        var data = new WarpMaskImageData();
        Data.WarpMaskImage.Add(data);

        var image = GetIdleWarpMaskImage();
        image.gameObject.SetActive(true);
        image.SetData(data);

        return image;
    }

    private WarpMaskImage GetIdleWarpMaskImage()
    {
        foreach(var image in WarpMaskImageList)
        {
            if(image.gameObject.activeSelf == false)
            {
                return image;
            }
        }

        var item = Instantiate(WarpMaskImagePrefab, WarpMaskImagePrefab.transform.parent);
        //item.WarpImage = Instantiate(item.WarpImage, WarpMaskImageParent[0]);
        WarpMaskImageList.Add(item);
        return item;
    }
}

[System.Serializable]
public class MaskObjectData
{
    public CurveMaskImageData MainMaskImage = new CurveMaskImageData();
    public CurveMaskImageData MiddleMaskImage = new CurveMaskImageData();

    public List<WarpMaskImageData> WarpMaskImage = new List<WarpMaskImageData>();
}