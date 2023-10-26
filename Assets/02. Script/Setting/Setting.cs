using UnityEngine;
using System.Collections;

namespace InsolDefaultProject
{
    public abstract class Setting : MonoBehaviour
    {
        public CustomButton SettingButton;
        public GameObject ViewPanel;

        public abstract void Init();
        public abstract void Save();
        public abstract void Load();
        public abstract void Show(bool isShow);
    }
}