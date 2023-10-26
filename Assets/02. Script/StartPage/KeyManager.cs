using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace StartPage
{
    public class KeyManager : MonoBehaviour
    {

        static public string Key;

        public GameObject KeyInputPanel;
        public InputField KeyInputField;



        [Header("Data")]
        public bool IsShowKeyInputPanel = false;


        public bool IsApiOnlineCheckEnd = false;
        public bool IsSubOnlineCheckEnd = false;
        public bool IsLocalCheckEnd = false;
        public bool IsLoginSuccess = false;


        /// <summary>
        /// 
        /// 결과 데이터, 팝업창 출력 여부
        /// </summary>
        public Action<LicenseDetailResponse, bool> OnKeyCheckEnd;


        void Start()
        {
            Key = SecurityPlayerPrefs.GetString("Login Information key", "");

            //if (string.IsNullOrEmpty(Key))
            //{
            //    KeyPanelActive(true);
            //    return;
            //}
            //else
            //{
            //    KeyPanelActive(false);
            //}

            KeyInputField.text = Key;

            //KeyCheck(Key, false);
        }


        void OnEnable()
        {
            OnKeyCheckEnd += KeyCheckEnd;
        }

        void OnDisable()
        {
            OnKeyCheckEnd -= KeyCheckEnd;
        }

        private bool change = false;
        public void OnKeyInputFieldChange()
        {
            if (change)
            {
                return;
            }
            change = true;
            KeyInputField.text = KeyInputField.text.ToUpper();
            change = false;
        }


        public void OnKeyButtonClick()
        {
            KeyPanelActive(!IsShowKeyInputPanel);
        }

        private void KeyPanelActive(bool isActive)
        {
            IsShowKeyInputPanel = isActive;
            KeyInputPanel.SetActive(IsShowKeyInputPanel);
        }

        void KeyCheckEnd(LicenseDetailResponse data, bool isPopup)
        {
            //if (isPopup)
            //{
            //    MsgBox.Show(0, "" + data.detail, (id, result) => { MsgBox.Close(); });
            //}

            switch (data.State)
            {
                case LicensesState.활성화:
                case LicensesState.활성화_새로운:
                    KeyPanelActive(false);
                    break;

                case LicensesState.실패:
                case LicensesState.비활성화:
                case LicensesState.유니크키다름:
                case LicensesState.시간초과:
                case LicensesState.시간에러:
                    if (String.IsNullOrEmpty(Key))
                        KeyPanelActive(true);
                    break;
            }

        }

        public void OnKeyInputEnterButtonClick()
        {
            if (string.IsNullOrEmpty(KeyInputField.text))
            {
                MsgBox.Show("라이센스 키를 입력해 주세요");
                return;
            }

            MsgBox.Show("최초 인증 후 해당 시리얼 코드로 다른 컴퓨터에서\n사용 할 수 없게 됩니다.\n인증 하시겠습니까?")
                .SetButtonType(MsgBoxButtons.YES_NO)
                .SetStyle(MsgBoxStyle.Warning)
                .OnResult((result) =>
                {
                    if (result.Equals(DialogResult.YES_OK))
                    {
                        SecurityPlayerPrefs.SetString("Login Information key", KeyInputField.text);
                        Key = KeyInputField.text;
                        KeyCheck(true);
                    }
                });

        }

        public void KeyCheck(bool isPopup)
        {
            if (string.IsNullOrEmpty(Key))
            {
                KeyPanelActive(true);
                return;
            }

            DataClass.LicenseData = new LicenseDetailResponse();
            IsApiOnlineCheckEnd = false;
            IsSubOnlineCheckEnd = false;
            IsLocalCheckEnd = false;
            IsLoginSuccess = false;


            StartCoroutine(KeyCkehckLocal(Key, isPopup));
            StartCoroutine(KeyChecoOnlineSub(Key, isPopup));
        }

        IEnumerator KeyCkehckLocal(string key, bool isPopup)
        {
            float keyCheckTime = 0;
            while (keyCheckTime <= DataClass.Timeout && (IsSubOnlineCheckEnd == false || IsApiOnlineCheckEnd == false))
            {
                keyCheckTime += Time.deltaTime;
                yield return null;
            }
            if (IsLoginSuccess)
            {
                yield break;
            }

            if (IsLocalCheckEnd)
            {
                yield break;
            }

            bool isStart = SecurityPlayerPrefs.GetBool(key + " Start", false);
            if (isStart == false)
            {
                yield break;
            }

            var data = DataClass.LicenseData;

            //해당 키로 시작한 적이 있는지 확인
            //if (SecurityPlayerPrefs.GetBool(key + " Start", false) == false)
            //{
            //    Debug.LogWarning("해당 PC에서 시작된 적 없음 : " + key);

            //    data.State = LicensesState.비활성화;
            //    data.detail = "인터넷 연결 후 시도해주세요.";
            //    OnKeyCheckEnd(data, true);
            //    yield break;
            //}


            string save_time_str = SecurityPlayerPrefs.GetString(key + " End Date", "");

            string readStr = SecurityPlayerPrefs.GetString(key + " data", "");
            if (string.IsNullOrEmpty(readStr) == false)
            {
                data = JsonUtility.FromJson<LicenseDetailResponse>(readStr);
            }


            DateTime save_time = DateTime.Parse(save_time_str);
            DateTime last_time = DateTime.Parse(SecurityPlayerPrefs.GetString("Last Date", "" + System.DateTime.Now));
            Debug.Log("[Load] Time : " + save_time);
            Debug.Log("[Load] Last Time : " + last_time);

            if (data.is_unlimit == false && last_time > System.DateTime.Now)  //시간 이상
            {
                data.State = LicensesState.시간에러;
                data.detail = "PC의 시간 설정을 확인해 주세요.";
                OnKeyCheckEnd(data, true);
            }
            //else if ("Start".Equals(start) == false)  //이 컴퓨터에서 활성화 한적 없음
            //{
            //    Debug.Log("[Local] MAC");
            //    data.State = LicensesState.유니크키다름;
            //    data.detail = "다른곳에서 사용된 라이렌스 키 입니다.";
            //    OnKeyCheckEnd(data, true);
            //}
            else if (data.is_unlimit == false && save_time < System.DateTime.Now)  //시간 초과
            {
                Debug.Log("[Local] Time");
                data.State = LicensesState.시간초과;
                data.expire_time = save_time_str;
                data.detail = "기간이 만료됐습니다. \n종료 날짜 : " + DateTime.Parse(data.expire_time).ToString("yyyy년 M월 d일"); ;
                OnKeyCheckEnd(data, true);
            }
            else
            {
                Debug.Log("[Local] OK");
                int count = SecurityPlayerPrefs.GetInt(key + " Certification Count", 0);
                int max_count = SecurityPlayerPrefs.GetInt(key + " Certification Max Count", 0);
                Debug.Log("[Load] Count : " + count + " / " + max_count);

                //count++;
                string str = "활성화.\n실행 횟수 : " + count + " / " + max_count;

                if (max_count >= 100 || max_count == 0)
                {
                    str = "활성화";
                }
                data.AvailableDay = (save_time - DateTime.Now).Days + 1;
                if (count > max_count && max_count > 0)  //실행 횟수 초과
                {
                    data.State = LicensesState.로컬카운트초과;
                    data.detail = "오프라인 실행 가능 횟수가 초과되었습니다.\n인터넷 연결 후 새로 실행 해주세요.";
                    OnKeyCheckEnd(data, true);
                }
                else
                {
                    if (max_count - count < 5 && max_count > 0)
                    {
                        str += "\n (오프라인 실행 가능 횟수가 얼마 안남았습니다. 인터넷 연결 후 새로 실행 해주세요.)";
                    }
                    SecurityPlayerPrefs.SetString("Last Date", "" + DateTime.Now);
                    Debug.Log("[Save] Last Time :" + DateTime.Now);


                    data.State = LicensesState.활성화;
                    data.detail = str;



                    bool isActive = false;
                    foreach (var app in data.available_apps)
                    {
                        if (app == Config.AppTypeCode) //일반 프로그램
                        {
                            foreach (var con in data.available_contents)
                            {
                                foreach (var id in Config.AppContentsCodes)
                                {
                                    if (con == id) // 일치하는 ID가 하나라도 있으면 사용 가능
                                    {
                                        isActive = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    if (isActive == false)
                    {
                        data.State = LicensesState.비활성화;
                        data.detail = "해당 프로그램을 사용할 수 없는 라이센스 입니다.";
                    }
                    OnKeyCheckEnd(data, false);
                }
            }
        }

        IEnumerator KeyCheckOnline(string key, bool isPopup)
        {
            bool isStart = SecurityPlayerPrefs.GetBool(key + " Start", false);
            string url = string.Format(Config.KEY_CHECK_URL, key, CustomUtils.GetMacAddress());// SystemInfo.deviceUniqueIdentifier);

            Debug.Log("Send : " + url);
            WWW www = new WWW(url);
            yield return www;

            LicenseDetailResponse data = new LicenseDetailResponse();
            if (www.error != null)
            {
                Debug.Log("KeyCheckOnline err : " + www.error);
                if (www.error.Contains("404")) //라이센스 없거나 맥주소 다름
                {
                    data.State = LicensesState.실패;
                    data.detail = "라이센스 인증에 실패했습니다.\n다른곳에서 사용된 라이센스일 수 있습니다.";//www.error;
                }
                else //인터넷 연결 안되어있음
                {
                    data.State = LicensesState.실패;
                    data.detail = "라이센스 인증에 실패했습니다.\n인터넷 연결 후 시도해 주세요.";//www.error;

                }

                IsApiOnlineCheckEnd = true;
                if (isStart == false)
                {
                    IsLocalCheckEnd = true;
                    //Debug.LogWarning("해당 PC에서 시작된 적 없음 : " + key);
                    OnKeyCheckEnd(data, true);
                }

                yield break;
            }


            try
            {
                string readStr = www.text.Trim();
                Debug.Log("Read : " + readStr);
                data = JsonUtility.FromJson<LicenseDetailResponse>(readStr);
                Debug.Log("result : " + data.ToString());
                SecurityPlayerPrefs.SetString(key + " data", readStr);
            }
            catch (Exception e)
            {
                Debug.LogError("Err :  " + e);

                IsApiOnlineCheckEnd = true;

                if (isStart == false)
                {
                    IsLocalCheckEnd = true;
                    data.State = LicensesState.실패;
                    data.detail = "" + e.Message;//www.error;
                                                 //Debug.LogWarning("해당 PC에서 시작된 적 없음 : " + key);
                    OnKeyCheckEnd(data, true);
                }
                yield break;
            }


            //if(data.is_active == false)
            //{
            //    if(string.IsNullOrEmpty(data.detail) == false)
            //        SecurityPlayerPrefs.DeleteKey(key + " Start");
            //    data.State = LicensesState.실패;
            //    data.detail = "";
            //    OnKeyCheckEnd(data, true);
            //    yield break;
            //}

            if (data.expire_time != null && data.expire_time.Length > 5)
            {
                SecurityPlayerPrefs.SetString(key + " End Date", data.expire_time);
            }


            SecurityPlayerPrefs.SetString(key + " End Date", data.expire_time);

            int count = SecurityPlayerPrefs.GetInt(key + " Certification Count", 0);
            SecurityPlayerPrefs.SetInt(key + " Certification Save Count", count);
            SecurityPlayerPrefs.SetInt(key + " Certification Count", 0);
            SecurityPlayerPrefs.SetInt(key + " Certification Max Count", data.local_max_count);

            data.AvailableDay = (DateTime.Parse(data.expire_time) - System.DateTime.Now).Days + 1;
            data.State = LicensesState.활성화;
            SecurityPlayerPrefs.SetBool(key + " Start", true);

            data.detail = "활성화";

            if (data.is_unlimit == false)
            {
                data.detail = "활성화\n종료 날짜 : " + DateTime.Parse(data.expire_time).ToString("yyyy년 M월 d일");
            }
            if (data.AvailableDay >= 365)
            {
                data.detail = "활성화";
            }

            if (data.is_unlimit == false && (DateTime.Parse(data.expire_time) - System.DateTime.Now).Ticks < 0)
            {
                data.State = LicensesState.시간초과;
                data.detail = "사용 기간이 만료된 라이센스 입니다.";

            }


            bool isActive = false;
            foreach (var app in data.available_apps)
            {
                if (app == Config.AppTypeCode) //일반 프로그램
                {
                    foreach (var con in data.available_contents)
                    {
                        foreach (var id in Config.AppContentsCodes)
                        {
                            if (con == id) // 일치하는 ID가 하나라도 있으면 사용 가능
                            {
                                isActive = true;
                                break;
                            }
                        }
                    }
                }
            }
            if (isActive == false)
            {
                data.State = LicensesState.비활성화;
                data.detail = "해당 프로그램을 사용할 수 없는 라이센스 입니다.";
            }

            IsApiOnlineCheckEnd = true;
            IsLocalCheckEnd = true;
            IsLoginSuccess = data.State == LicensesState.활성화;
            OnKeyCheckEnd(data, isPopup);
        }





        IEnumerator KeyChecoOnlineSub(string key, bool isPopup)
        {
            var data = DataClass.LicenseData;

            string url = Config.SUB_KEY_CHECK_URL + "CheckLicense=" + key;
            Debug.Log("서브 라이센스 체크 : " + url);
            var mac = CustomUtils.GetMacAddress();
            bool isStart = SecurityPlayerPrefs.GetBool(key + " Start", false);

            WWWForm form = new WWWForm();
            form.AddField("Mac", mac);
            UnityWebRequest www = UnityWebRequest.Post(url, form);

            yield return www.SendWebRequest();

            if (www.error != null)
            {
                Debug.LogError("에러 " + www.error);

                //if (isStart == false)  //Debug.LogWarning("해당 PC에서 시작된 적 없음 : " + key);
                //{
                //    data.State = LicensesState.실패;
                //    data.detail = "" + www.error;
                //                               
                //    OnKeyCheckEnd(data, true);
                //}

                IsSubOnlineCheckEnd = true;
                StartCoroutine(KeyCheckOnline(key, isPopup));
                yield break;
            }

            try
            {
                var read = www.downloadHandler.text;
                Debug.Log("Read : " + read);
                if (read == "noData")
                {
                    if (isStart == false)  //Debug.LogWarning("해당 PC에서 시작된 적 없음 : " + key);
                    {
                        data.State = LicensesState.실패;
                        //data.detail = "" + e.Message;//www.error;

                        //OnKeyCheckEnd(data, true);
                    }
                    IsSubOnlineCheckEnd = true;
                    StartCoroutine(KeyCheckOnline(key, isPopup));
                    yield break;
                }

                var subData = JsonUtility.FromJson<SubLicenseResponseData>(www.downloadHandler.text);

                if (string.IsNullOrEmpty(subData.unity_key) == false && subData.unity_key != mac)
                {
                    if (isStart == false)  //Debug.LogWarning("해당 PC에서 시작된 적 없음 : " + key);
                    {
                        data.State = LicensesState.실패;
                        //data.detail = "" + e.Message;//www.error;

                        //OnKeyCheckEnd(data, true);

                        IsSubOnlineCheckEnd = true;
                        StartCoroutine(KeyCheckOnline(key, isPopup));
                        yield break;
                    }
                }



                data.expire_time = subData.expire_time;
                data.institution_name = subData.institution_name;
                data.is_active = subData.is_active;
                data.is_unlimit = subData.is_unlimit;
                data.local_max_count = subData.local_max_count;

                data.available_apps = GetRangeStringToIntList(subData.available_apps).ToArray();
                data.available_contents = GetRangeStringToIntList(subData.available_contents).ToArray();



                if (data.is_unlimit && string.IsNullOrEmpty(data.expire_time))
                {
                    SecurityPlayerPrefs.SetString(key + " End Date", DateTime.Now.AddYears(50).ToString());
                }
                else if (string.IsNullOrEmpty(data.expire_time))
                {
                    data.State = LicensesState.비활성화;
                    data.detail = "라이센스 정보를 확인해주세요.";
                    //OnKeyCheckEnd(data, isPopup);


                    IsSubOnlineCheckEnd = true;
                    StartCoroutine(KeyCheckOnline(key, isPopup));
                    yield break;
                }
                else
                {
                    SecurityPlayerPrefs.SetString(key + " End Date", data.expire_time);
                }


                int count = SecurityPlayerPrefs.GetInt(key + " Certification Count", 0);

                SecurityPlayerPrefs.SetInt(key + " Certification Save Count", count);
                SecurityPlayerPrefs.SetInt(key + " Certification Count", 0);
                SecurityPlayerPrefs.SetInt(key + " Certification Max Count", data.local_max_count);

                if (data.is_unlimit == false)
                    data.AvailableDay = (DateTime.Parse(data.expire_time) - System.DateTime.Now).Days + 1;
                data.State = LicensesState.활성화;
                SecurityPlayerPrefs.SetBool(key + " Start", true);
                data.detail = "활성화";

                if (data.is_unlimit == false && string.IsNullOrEmpty(data.expire_time) == false)
                {
                    data.detail = "활성화" + "\n" + DateTime.Parse(data.expire_time).ToString("yyyy. MM. dd.");
                }
                if (data.is_unlimit || data.AvailableDay >= 365)
                {
                    data.detail = "활성화";
                }

                if (data.is_unlimit == false && data.AvailableDay < 0)
                {
                    data.State = LicensesState.시간초과;
                    data.detail = "사용 기간이 만료된 라이센스 입니다.";
                }

                bool isActive = false;
                foreach (var app in data.available_apps)
                {
                    if (app == Config.AppTypeCode) //일반 프로그램
                    {
                        foreach (var con in data.available_contents)
                        {
                            foreach (var id in Config.AppContentsCodes)
                            {
                                if (con == id) // 일치하는 ID가 하나라도 있으면 사용 가능
                                {
                                    isActive = true;
                                    break;
                                }
                            }
                        }
                    }
                }
                if (isActive == false)
                {
                    data.State = LicensesState.비활성화;
                    data.detail = "해당 프로그램을 사용할 수 없는 라이센스 입니다.";
                }


                IsLocalCheckEnd = true;
                IsSubOnlineCheckEnd = true;
                IsLoginSuccess = data.State == LicensesState.활성화;
                SecurityPlayerPrefs.SetString(key + " data", JsonUtility.ToJson(data));
                OnKeyCheckEnd(data, isPopup);

            }
            catch (Exception e)
            {
                Debug.LogError("KeyChecoOnlineSub Err : " + e);

                if (isStart == false)  //Debug.LogWarning("해당 PC에서 시작된 적 없음 : " + key);
                {
                    data.State = LicensesState.실패;
                    data.detail = "" + e.Message;//www.error;

                    //OnKeyCheckEnd(data, true);

                    IsSubOnlineCheckEnd = true;
                    StartCoroutine(KeyCheckOnline(key, isPopup));
                }
            }
        }


        public IEnumerator Run(int runCount)
        {
            Debug.Log("Run Count : " + runCount);

            string url = Config.SUB_KEY_CHECK_URL;

            WWWForm form = new WWWForm();
            form.AddField("RunLicense", Key);
            form.AddField("run_count", runCount);
            UnityWebRequest www = UnityWebRequest.Post(url, form);
            yield return www.SendWebRequest();

            if (www.error != null || www.downloadHandler.text != "ok")
            {
                Debug.LogError("Run upload Err : " + www.error);
                yield break;
            }
            else
            {
                Debug.Log("Run Upload 결과 : " + www.downloadHandler.text);
            }
        }


        private List<int> GetRangeStringToIntList(string rangeStr)
        {
            try
            {
                List<int> intList = new List<int>();
                var list = rangeStr.Split(',');
                foreach (var str in list)
                {
                    if (string.IsNullOrEmpty(str))
                    {
                        continue;
                    }
                    if (str.Contains("-"))
                    {
                        var range = str.Split('-');
                        int s, e;
                        if (range.Length == 2)
                        {
                            if (int.TryParse(range[0], out s) && int.TryParse(range[1], out e))
                            {
                                for (int i = s; i <= e; i++)
                                {
                                    intList.Add(i);
                                }
                            }
                        }
                    }
                    else
                    {
                        int con = 0;
                        if (int.TryParse(str, out con))
                        {
                            intList.Add(con);
                        }
                    }
                }


                return intList;
            }
            catch (Exception e)
            {
                Debug.LogError("GetRangeStringToInArray Err " + e);
                return null;
            }
        }
    }


    public enum LicensesState
    {
        활성화, 활성화_새로운, 실패, 비활성화, 유니크키다름, 시간초과, 시간에러, 로컬카운트초과
    }
}