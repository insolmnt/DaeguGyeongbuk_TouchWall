using UnityEngine;
using System.Collections;
using System;
using System.IO;

namespace StartPage
{
    public class Config
    {
        // !! 현재 프로그램 버전
        public const string AppVersion = "1.0.2";
        /// <summary>
        /// 1.0.2
        /// 2023. 03. 13
        /// 중복 검사 카운트 추가
        /// 
        /// 1.0.1
        /// 2022. 12. 13
        /// 화면 설정 키스톤 화면 해상도 대응
        /// 커브설정 처음과 끝이라고 값 0, 1 고정 제거
        /// 커브 마스크 페이드 기능 추가
        /// 스크린 페이드 Top 오류 수정
        /// 마스크 커브 설정 중  키프레임에 빨간점 표시 (키패드 +/-로 크기 조절)
        /// 커브설정 좌우 간격 넓힘
        /// 커브설정 아이콘 사이즈 줄임
        /// 설정파일 저장 시 백업폴더에도 함께 저장
        /// 
        /// 1.0.0
        /// 2022. 12. 02
        /// 마스크 기능 추가
        /// 스크린 관련 버그 수정
        /// 커브 설정 키보드 사용 추가
        /// 
        /// 2022. 08. 22
        /// 초기버전
        /// </summary>

        // !! 업데이트 확인을 위한 프로그램 이름
        public const string AppType = "AppUpdateName";

        // !! 업데이트후 자동실행을 위한 실행파일 명
        public const string ExeFileName = "AppName.exe";


        public const int AppTypeCode = 101;                     //프로그램 타입 ID. 일반프로그램은 101 사용                http://api.aiaiplay.com/admin/content/app/   
        static public readonly int[] AppContentsCodes = { 10000 };      //해당 프로그램의 콘텐츠 ID 목록;           http://api.aiaiplay.com/admin/content/content/


        static readonly public string ScanPath = @"C:\Scan\";
        static public string StandardPath;
        static public string BackupPath;
        static readonly public string ColoringGroup = "Etc";



        // !! 실제 라이센스 받아오는 주소로 수정 필요
        public const string KEY_CHECK_URL = @"http://api.aiaiplay.com/registration/licenses/{0}/?unity_key={1}";
        public const string SUB_KEY_CHECK_URL = "http://earthzoo.biz/license/license.php?";


        public const string VERSION_CHECK_URL = "http://earthzoo.biz/aqua/Info.php?";
        public const string PROGRAM_SETUP_URL = "http://earthzoo.biz/Program/";


        static public void Init()
        {
            BackupPath = ScanPath + ColoringGroup + @"\" + DateTime.Now.ToString("yyyy-MM") + @"\";
            StandardPath = @"Standard\" + ColoringGroup + @"\";
            if (Directory.Exists(BackupPath) == false)
            {
                Directory.CreateDirectory(BackupPath);
            }
            if (Directory.Exists(StandardPath) == false)
            {
                Directory.CreateDirectory(StandardPath);
            }
        }
    }





    public class LicenseDetailResponse
    {
        public LicensesState State = LicensesState.비활성화;
        public string detail;
        public string expire_time;
        public int AvailableDay;

        public int local_max_count;
        public string institution_name;
        public bool is_active = false;
        public int[] available_apps;
        public int[] available_contents;

        public bool is_unlimit = false;

        public override string ToString()
        {
            string str = "";
            //foreach(var app in available_apps)
            //{
            //    str += app.name + "(" + app.id + "), ";
            //}
            return string.Format("Active : {0}, detail : {1}", is_active, detail);
        }

    }
    [System.Serializable]
    public class SubLicenseResponseData
    {
        public bool is_unlimit = false;
        public string expire_time;
        public int local_max_count;
        public string institution_name;
        public bool is_active = false;
        public string available_apps;
        public string available_contents;
        public string unity_key;
    }
}
