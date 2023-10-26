using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

namespace StartPage
{
    public class StartPageManager : MonoBehaviour
    {
        public KeyManager KeyManager;
        public UpdateManager UpdateManager;

        public Button StartButton;
        public Text StateText;

        public GameObject AutoStartPanel;
        public Text AutoStartStateText;

        public Text NameText;

        public GameObject SceneLodingObject;

        void Awake()
        {
            StartButton.interactable = false;

            StateText.text = "";
            NameText.text = "";


            DataClass.Load();
        }

        private void OnEnable()
        {
            KeyManager.OnKeyCheckEnd += OnResultKey;
        }

        private void OnDisable()
        {
            KeyManager.OnKeyCheckEnd -= OnResultKey;
        }

        IEnumerator Start()
        {
            WindowManager.Init();
            yield return new WaitForSeconds(0.5f);
            WindowManager.MonitorInfo monitor = WindowManager.MainMonitor;

            int width = (int)(monitor.GetWidth() * 0.5f);
            int height = (int)(width / 800f * 600);
            Screen.SetResolution(width, height, false);
            yield return new WaitForSeconds(0.5f);
            WindowManager.ResizeWindow((monitor.GetWidth() - width) / 2, (monitor.GetHeight() - height) / 2, width, height);

        }


        private bool IsAutoStart = false;
        IEnumerator AutoStart()
        {
            IsAutoStart = true;
            AutoStartPanel.SetActive(true);
            for (int i = 3; i >= 0; i--)
            {
                AutoStartStateText.text = string.Format("<color=yellow>{0}</color>초 뒤 자동 실행합니다.", i);
                yield return new WaitForSeconds(1f);

                if (IsAutoStart == false)
                {
                    yield break;
                }
            }

            OnStartButtonClick();
        }
        public void OnAutoStartCancleButtonClick()
        {
            IsAutoStart = false;
            AutoStartPanel.SetActive(false);
        }


        public void OnResultKey(LicenseDetailResponse data, bool isPopup)
        {
            DataClass.LicenseData = data;

            StateText.text = data.detail;
            NameText.text = data.institution_name;


            switch (data.State)
            {
                case LicensesState.활성화:
                case LicensesState.활성화_새로운:
                    StartButton.interactable = true;

                    if (isPopup)
                    {
                        MsgBox.Show(data.detail).OnResult((result) => {
                            StartCoroutine(AutoStart());
                        });
                    }
                    else
                    {
                        StartCoroutine(AutoStart());
                    }
                    break;

                case LicensesState.실패:
                case LicensesState.비활성화:
                case LicensesState.유니크키다름:
                case LicensesState.시간초과:
                case LicensesState.시간에러:
                case LicensesState.로컬카운트초과:
                    StartButton.interactable = false;
                    IsAutoStart = false;

                    if (isPopup)
                    {
                        MsgBox.Show(data.detail);
                    }
                    break;
            }
        }


        private bool mIsStartButtonClick = false;
        public void OnStartButtonClick()
        {
            if (mIsStartButtonClick)
            {
                return;
            }

            if (KeyManager.IsLoginSuccess)
            {
                int count = SecurityPlayerPrefs.GetInt(KeyManager.Key + " Certification Count", 0);
                int saveCount = SecurityPlayerPrefs.GetInt(KeyManager.Key + " Certification Save Count", 0);
                count = count + saveCount + 1;

                SecurityPlayerPrefs.SetInt(KeyManager.Key + " Certification Save Count", 0);
                SecurityPlayerPrefs.SetInt(KeyManager.Key + " Certification Count", 0);

                StartCoroutine(KeyManager.Run(count));
            }
            else
            {
                int count = SecurityPlayerPrefs.GetInt(KeyManager.Key + " Certification Count", 0);
                SecurityPlayerPrefs.SetInt(KeyManager.Key + " Certification Count", count + 1);
            }


            mIsStartButtonClick = true;
            StartCoroutine(SceneStart());
        }



        public void OnQuitButtonClick()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
		        Application.Quit();
#endif
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (UpdateManager.DownPanel.activeSelf)
                {
                    UpdateManager.OnCancelButtonClick();
                }
                else if (MsgBox.IsShow)
                {
                    MsgBox.Close();
                }
                else if (IsAutoStart)
                {
                    OnAutoStartCancleButtonClick();
                }
                else if (KeyManager.IsShowKeyInputPanel)
                {
                    KeyManager.OnKeyButtonClick();
                }
                //else if (MonitorManager.MonitorSettingPanel.activeSelf)
                //{
                //    MonitorManager.OnMonitorSettingButtonClick();
                //}
            }

            if (Input.GetKeyDown(KeyCode.Return))
            {
                if (MsgBox.IsShow)
                {
                    MsgBox._lastMsgBox.ButtonClickEvent(0);
                    //MsgBox.Close();
                }
                else if (KeyManager.IsShowKeyInputPanel)
                {
                    KeyManager.OnKeyInputEnterButtonClick();
                }
                else if (StartButton.interactable)
                {
                    OnStartButtonClick();
                }
                //else if (MonitorManager.MonitorSettingPanel.activeSelf && MonitorManager.OKButton.interactable)
                //{
                //    MonitorManager.OnMonitorOkButtonClick();
                //}
            }
        }

        IEnumerator SceneStart()
        {
            SceneLodingObject.SetActive(true);

            yield return null;

            Debug.Log("system : " + Display.main.systemWidth + " , " + Display.main.systemHeight);
            Debug.Log("render : " + Display.main.renderingWidth + " , " + Display.main.renderingHeight);

            Screen.SetResolution(Display.main.systemWidth, Display.main.systemHeight, true);
            yield return new WaitForSeconds(0.5f);

            WindowManager.TopWindow();
            //MonitorManager.Resize();
            SceneManager.LoadScene(1);
        }
    }
}