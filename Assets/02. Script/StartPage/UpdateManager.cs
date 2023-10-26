using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;

namespace StartPage
{
    public class UpdateManager : MonoBehaviour
    {
        public KeyManager KeyManager;

        public Text VerText;

        public GameObject DownPanel;
        public Text TitleText;
        public Text StateText;
        public Slider DownProgress;

        Coroutine CheckCoroutine = null;
        Coroutine DownCoroutine = null;
        void Start()
        {
            //Setup.bat 파일 만들기
            if (File.Exists("Setup.bat") == false)
            {
                File.WriteAllText("Setup.bat", "Setup.exe /silent\r\n\"" + Config.ExeFileName + "\"");
            }

            VerText.text = "Ver " + Config.AppVersion;

            CheckCoroutine = StartCoroutine(CheckVersion());
        }

        public void OnCancelButtonClick()
        {
            if (CheckCoroutine != null)
            {
                StopCoroutine(CheckCoroutine);
            }
            if (DownCoroutine != null)
            {
                StopCoroutine(DownCoroutine);
            }
            DownPanel.gameObject.SetActive(false);
            KeyManager.KeyCheck(false);
        }

        IEnumerator CheckVersion()
        {
            WWWForm form = new WWWForm();
            form.AddField("GetListVersion", Config.AppType);
            form.AddField("CurrentVer", Config.AppVersion);
            form.AddField("Key", SecurityPlayerPrefs.GetString("Login Information key", ""));
            WWW post_www = new WWW(Config.VERSION_CHECK_URL, form);
            yield return post_www;

            if (post_www.error != null)
            {
                Debug.LogError("CheckVersion Err : " + post_www.error);
                KeyManager.KeyCheck(false);

                yield break;
            }
            Debug.Log("Data : " + post_www.text);
            if (post_www.text.Equals("false"))
            {
                Debug.Log("자료 없음!");
                KeyManager.KeyCheck(false);
                yield break;
            }
            VersionData data = JsonUtility.FromJson<VersionData>(post_www.text);

            if (isLast(data.LastVersion))
            {
                Debug.Log("최신버전!");
                KeyManager.KeyCheck(false);
            }
            else
            {
                Debug.Log("예전버전!");
                DownPanel.SetActive(true);
                TitleText.text = "<color=#f00>" + Config.AppVersion + "</color> => <color=#ff0>" + data.LastVersion + "</color>";

                WWW www = new WWW(Config.PROGRAM_SETUP_URL + data.SetupPath);
                Debug.Log("Down URL : " + Config.PROGRAM_SETUP_URL + data.SetupPath);
                while (www.isDone == false)
                {
                    yield return new WaitForSeconds(0.1f);
                    DownProgress.value = www.progress;
                    StateText.text = "Downloading...    <color=#f80>" + (www.progress * 100).ToString("F2") + "</color>%";
                }

                StateText.text = "Download End. Start Setup.";
                DownCoroutine = StartCoroutine(WriteAllBytes(@"Setup.exe", www.bytes));
                yield return DownCoroutine;
                yield return new WaitForSeconds(1f);
                System.Diagnostics.Process.Start(@"Setup.bat");
                Application.Quit();
            }
        }


        IEnumerator WriteAllBytes(string fileName, byte[] bytes, int chunkSizeDesired = 1024 * 1024)
        {
            var stream = new FileStream(fileName, FileMode.Create);
            var writer = new BinaryWriter(stream);

            var bytesLeft = bytes.Length;
            var bytesWritten = 0;

            while (bytesLeft > 0)
            {
                yield return null;
                var chunkSize = Mathf.Min(chunkSizeDesired, bytesLeft);
                writer.Write(bytes, bytesWritten, chunkSize);
                bytesWritten += chunkSize;
                bytesLeft -= chunkSize;
                Debug.Log("" + bytesWritten + " / " + bytesLeft);


                var val = (float)bytesWritten / bytes.Length;
                DownProgress.value = val;
                StateText.text = "Setup File Copy : " + (val * 100).ToString("F2") + "%";
                Debug.Log(string.Format("Saved {0:P1}", (float)bytesWritten / bytes.Length));
            }
            writer.Close();
            Debug.Log("Done writing " + fileName);
        }

        bool isLast(string lastVer)
        {
            if (DataClass.IsUpdate)
            {
                return false;
            }
            int currentVer = GetVerInt(Config.AppVersion);
            return !(GetVerInt(lastVer) > currentVer);
        }
        int GetVerInt(string ver)
        {
            int data = 0;
            var list = ver.Split('.');

            int count = 0;
            for (int i = list.Length - 1; i >= 0; i--)
            {
                data += int.Parse(list[i]) * (int)Mathf.Pow(100, count);
                count++;
            }
            Debug.Log("버전 : " + ver + " => " + data);
            return data;
        }


        public class VersionData
        {
            public string LastVersion;
            public string SetupPath;
        }
    }
}