using UnityEngine;
using System;
using System.Runtime.InteropServices;

public class WindowPositionManager : MonoBehaviour
{
    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOZORDER = 0x0004;
    private static readonly IntPtr HWND_TOP = IntPtr.Zero;

    void Start()
    {
        if (!Screen.fullScreen)
        {
            CenterWindow();
        }
    }

    private void CenterWindow()
    {
        IntPtr hWnd = FindWindow(null, Application.productName);

        if (hWnd != IntPtr.Zero)
        {
            int screenX = Display.main.systemWidth;
            int screenY = Display.main.systemHeight;
            int windowX = Screen.width;
            int windowY = Screen.height;

            int xPos = (screenX - windowX) / 2;
            int yPos = (screenY - windowY) / 2;

            SetWindowPos(hWnd, HWND_TOP, xPos, yPos, 0, 0, SWP_NOSIZE | SWP_NOZORDER);
        }
        else
        {
            Debug.LogError("Window handle not found. Ensure the game window title matches Application.productName.");
        }
    }
}
