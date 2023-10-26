using UnityEngine;
using System.Collections;
using StartPage;
using UnityEngine.UI;

namespace InsolDefaultProject
{
    public class SettingManager : MonoBehaviour
    {
        static public SettingManager Instance;

        public DOShowHide SettingPanel;
        public Setting[] SettingList;


        [Header("Data")]
        private int CurrentIndex;
        public bool IsShowSetting = false;


        void Awake()
        {
            if(Instance != null)
            {
                Destroy(Instance.gameObject);
            }
            Instance = this;
        }


        void Start()
        {
            DataClass.Load();

            SettingPanel.gameObject.SetActive(false);
            CurrentIndex = 0;
            for (int i = 0; i < SettingList.Length; i++)
            {
                SettingList[i].ViewPanel.SetActive(i == CurrentIndex);
                SettingList[i].Init();
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if(CurveSetting.Instance != null && CurveSetting.Instance.gameObject.activeSelf)
                {
                    CurveSetting.Instance.OnCloseButtonClick();
                    return;
                }

                if(ColorSetting.IsShowSetting)
                {
                    ColorSetting.Instance.HideSetting();
                    return;
                }

                ShowSettingPanel(!IsShowSetting);
            }
        }

        public void ShowSettingPanel(bool isShow)
        {
            Debug.Log("세팅패널 : " + isShow);

            if(IsShowSetting != isShow)
            {
                UIManager.Instance.ShowCursor(isShow);
            }

            IsShowSetting = isShow;
            if (IsShowSetting)
            {
                SettingPanel.Show();
                OnSettingButtonClick(CurrentIndex);
            }
            else
            {
                foreach (var setting in SettingList)
                {
                    setting.Show(false);
                }

                SettingPanel.Hide();

                Save();
            }
        }

        public void OnSettingButtonClick(int i)
        {
            SettingList[CurrentIndex].SettingButton.Select(false);
            SettingList[CurrentIndex].ViewPanel.SetActive(false);
            SettingList[CurrentIndex].Show(false);
            CurrentIndex = i;
            SettingList[CurrentIndex].ViewPanel.SetActive(true);
            SettingList[CurrentIndex].Show(true);
            SettingList[CurrentIndex].SettingButton.Select(true);
        }

        public void OnSaveButtonClick()
        {
            ShowSettingPanel(false);
            //MsgBox.Show("저장 완료").OnResult((result) => {
            //    ShowSettingPanel(false);
            //});
        }
        public void OnCancelButtonClick()
        {
            for (int i = 0; i < SettingList.Length; i++)
            {
                SettingList[i].Load();
            }
            ShowSettingPanel(false);
            //MsgBox.Show(0, "로드 완료", (id, result) => { MsgBox.Close();});
        }

        public void OnQuitButtonClick()
        {

            MsgBox.Show("프로그램을 종료하시겠습니까?")
                .SetButtonType(MsgBoxButtons.YES_NO)
                .SetStyle(MsgBoxStyle.Custom)
                .OnResult((result) =>
                {
                    if (result.Equals(DialogResult.YES_OK))
                    {
                        UIManager.Instance.Quit();
                    }
                });
        }


        public void Save()
        {
            for (int i = 0; i < SettingList.Length; i++)
            {
                SettingList[i].Save();
            }
        }
    }
}