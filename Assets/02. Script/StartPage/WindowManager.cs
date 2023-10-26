using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;

namespace StartPage
{
    public class WindowManager
    {
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        static extern long GetWindowLong(IntPtr hWnd, Int32 nIndex);
        [DllImport("user32.dll")]
        static extern long SetWindowLong(IntPtr hWnd, Int32 nIndex, long win);
        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        [DllImport("user32.dll")]
        static extern int GetSystemMetrics(int nIndex);
        [DllImport("user32.dll")]
        static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumDelegate lpfnEnum, IntPtr dwData);
        delegate bool MonitorEnumDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData);

        [StructLayout(LayoutKind.Sequential)]
        public struct WinRect
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }
        public struct MonitorInfo
        {
            public int Size;
            public WinRect Monitor;
            public WinRect WorkArea;
            public uint Flags;
            public void Init()
            {
                this.Size = 40;
            }

            public int GetWidth()
            {
                return Monitor.right - Monitor.left;
            }
            public int GetHeight()
            {
                return Monitor.bottom - Monitor.top;
            }
        }
        [DllImport("user32.dll")]
        static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfo lpmi);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern System.IntPtr FindWindow(String lpClassName, String lpWindowName);

        private static List<MonitorInfo> MonitorList;
        private static bool mIsStart = false;

        private static IntPtr w;

        public static MonitorInfo MainMonitor;

        public static void Init()
        {
            if (mIsStart)
            {
                return;
            }
            mIsStart = true;

            w = FindWindow((string)null, Application.productName);
            if ((int)w == 0)
            {
                w = GetForegroundWindow();
                Debug.Log("w : " + w);
            }
            MonitorList = new List<MonitorInfo>();
            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero,
                delegate (IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData)
                {
                    MonitorInfo mi = new MonitorInfo();

                    mi.Size = Marshal.SizeOf(mi);
                    bool success = GetMonitorInfo(hMonitor, ref mi);
                    if (success)
                    {
                        if (mi.Monitor.left == 0)
                        {
                            MainMonitor = mi;
                        }

                        MonitorList.Add(mi);
                        MonitorList.Sort((left, right) => { return left.Monitor.left - right.Monitor.left; });
                    }
                    return true;
                }, IntPtr.Zero);
        }

        public static List<MonitorInfo> GetMonitorList()
        {
            return MonitorList;
        }
        public static int GetMonitorCount()
        {
            if (MonitorList == null)
            {
                return 1;
            }
            return MonitorList.Count;
        }



        public static void RemoveFrame(int x, int y, int width, int height)
        {
#if UNITY_EDITOR
#else
        //SetWindowPos(w, -1, x, y, width, height, 1); 

        //프레임 제거 작업
        long window = GetWindowLong(w, -16); //GWL_STYLE
        //window &= ~(0x00000000 | 0x00C00000L | 0x00080000 | 0x00020000 | 0x00010000);
        window &= ~(0x00000000 | 0x00C00000L | 0x00080000 | 0x00040000L);
        SetWindowLong(w, -16, window);
#endif
        }


        public static void ResizeWindow(int x, int y, int width, int height)
        {
#if UNITY_EDITOR
#else
        SetWindowPos(w, -1, x, y, width, height, 0);
#endif
        }
        public static void TopWindow()
        {
#if UNITY_EDITOR
#else
        SetWindowPos(w, -1, 0, 0, 0, 0, 1);
#endif
        }


        // HWND_NOTOPMOST  -2  최상위 Window의 바로 다음 위치로 이동  
        //HWND_TOP  0  바로 다음 상위로 이동  
        //HWND_BOTTOM  1  최상위로 이동  
        //HWND_TOPMOST  -1  최상위로 이동, Focus를 잃더라도 level을 유지

        //SWP_HIDEWINDOW  128  Window 숨김  
        //SWP_NOACTIVATE  10  Window 비활성화  
        //SWP_NOMOVE  2  x,y인수를 무시하고 현재위치 고수  
        //SWP_NOREDRAW  8  다시그리지 않음  
        //SWP_NOSIZE  1  c2, cy인수를 무시하고 현재크기 고수

        /*
        SWP_ASYNCWINDOWPOS  0x4000  이 함수를 부른 스레드와 윈도우를 소유한 스레드가 다른 입력 큐를 사용할 경우 시스템은 윈도우를 소유한 스레드에게 요구를 포스팅하기만 한다. 이는 호출 스레드가 다른 스레드가 요구를 처리하는 동안 블럭되는 것을 방지한다.
        SWP_DEFERERASE  0x2000  WM_SYNCPAINT 메시지 발생을 금지한다.
        SWP_DRAWFRAME   0x0020  윈도우 주변에 프레임을 그린다.
        SWP_FRAMECHANGED    0x0020  SetWindowLong으로 경계선 스타일을 변경했을 경우 새 스타일을 적용한다.이 플래그가 지정되면 크기가 변경되지 않아도 WM_NCCALCSIZE 메시지가 전달된다.
        SWP_HIDEWINDOW  0x0080  윈도우를 숨긴다.이 경우 이동과 크기 변경은 무시된다.
        SWP_NOACTIVATE  0x0010  크기 변경 후 윈도우를 활성화시키지 않는다.
        SWP_NOCOPYBITS  0x0100  이 플래그가 지정되지 않으면 작업영역의 내용이 저장되었다가 크기나 위치변경 후 다시 작업영역으로 복사된다. 이 플래그가 지정되면 이런 저장을 하지 않는다.
        SWP_NOMOVE  0x0002  위치는 이동하지 않고 크기만 변경한다. X,Y인수가 무시된다.
        SWP_NOOWNERZORDER   0x0200  소유자의 Z순서를 변경하지 않는다.
        SWP_NOREDRAW    0x0008  크기, 위치를 바꾼 후 그리기를 하지 않는다.해당 윈도우는 물론이고 이 윈도우에 의해 다시 드러나는 윈도우도 다시 그리기를 하지 않는다.이 플래그를 주었을 경우 프로그램은 필요한 부분을 즉시 무효화시켜 다시 그리도록 해 주어야 한다.
        SWP_NOREPOSITION    0x0200  = SWP_NOOWNERZORDER
        SWP_NOSENDCHANGING  0x0400  윈도우에게 WM_WINDOWPOSCHANGING 메시지를 보내지 않는다.
        SWP_NOSIZE  0x0001  크기는 변경하지 않고 위치만 이동한다. cx, cy 인수가 무시된다.
        SWP_NOZORDER    0x0004  현재의 Z순서를 그대로 유지한다.hWndInsertAfter 인수를 무시한다.
        SWP_SHOWWINDOW  0x0040  윈도우를 보인다.이 경우 이동과 크기 변경은 무시된다.*/
    }
}
