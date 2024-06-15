using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using PvZA11y.Models;

namespace PvZA11y.Utility;

public static partial class ExternUtility
{
    /// <summary>
    /// Gets the screen size of a specified window.
    /// </summary>
    /// <param name="ptr">The handle to the window.</param>
    /// <returns>A tuple containing the width and height of the screen.Return the default value if an exception is encountered.</returns>
    public static (int, int) GetScreenSize(nint ptr)
    {
        int width = 0, height = 0;

        try
        {
            GetWindowRect(ptr, out RECT windowRect);
            Screen screen = Screen.FromRectangle(new Rectangle(windowRect.Left, windowRect.Top, windowRect.Right - windowRect.Left,
                windowRect.Bottom - windowRect.Top));
            width = screen.Bounds.Width;
            height = screen.Bounds.Height;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return (width, height);
    }

    /// <summary>
    /// Gets the scaling factor of the primary screen.
    /// </summary>
    /// <returns>The scaling factor as a float value. If an exception is encountered return 1 as default value.</returns>
    public static float GetScalingFactor()
    {
        Screen[] screenList = Screen.AllScreens;
        for (int i = 0; i < screenList.Length; i++)
        {
            DEVMODE dm = new DEVMODE
            {
                dmSize = (short)Marshal.SizeOf(typeof(DEVMODE))
            };
            EnumDisplaySettings(screenList[i].DeviceName, -1, ref dm);

            var scalingFactor = Math.Round(Decimal.Divide(dm.dmPelsWidth, screenList[i].Bounds.Width), 2);
            return (float)scalingFactor;
        }

        return 1;
    }

    /// <summary>
    /// Performs a click task on a specified window.
    /// </summary>
    /// <param name="gameWHnd">The handle to the game window.</param>
    /// <param name="info">The information about the draw size.</param>
    /// <param name="x">The x-coordinate of the click position in relative units.</param>
    /// <param name="y">The y-coordinate of the click position in relative units.</param>
    /// <param name="rightClick">Indicates whether it is a right-click or left-click.</param>
    /// <param name="delayTime">The delay time in milliseconds between mouse down and mouse up events.</param>
    /// <param name="moveMouse">Indicates whether to move the mouse cursor to the click position before clicking.</param>
    public static void ClickTask(nint gameWHnd, DrawSizeInfo info, float x, float y, bool rightClick, int delayTime, bool moveMouse)
    {
        float windowScale = GetScalingFactor();

        int clickX = (int)(x * info.DrawWidth / windowScale + info.DrawStartX / windowScale);
        int clickY = (int)(y * info.DrawHeight / windowScale);

        uint clickDown = (uint)(rightClick ? KeyStatus.WM_RBUTTONDOWN : KeyStatus.WM_LBUTTONDOWN);
        uint clickUp = (uint)(rightClick ? KeyStatus.WM_RBUTTONUP : KeyStatus.WM_LBUTTONUP);

        if (moveMouse && Config.current.MoveMouseCursor)
        {
            GetWindowRect(gameWHnd, out RECT rect);
            //Console.WriteLine("Window Pos: {0},{1}", rect.Left, rect.Top);
            int cursorX = rect.Left + clickX;
            int cursorY = rect.Top + clickY;
            Cursor.Position = new System.Drawing.Point(cursorX, cursorY);

            //Move mouse before processing click
            PostMessage(gameWHnd, 0x0200, 1, MakeLParam(clickX, clickY));

            Task.Delay(delayTime).Wait();
        }

        PostMessage(gameWHnd, clickDown, 1, MakeLParam(clickX, clickY));

        Task.Delay(delayTime).Wait();

        PostMessage(gameWHnd, clickUp, 0, MakeLParam(clickX, clickY));
    }

    private static int MakeLParam(int x, int y) => (y << 16) | (x & 0xFFFF);
}

public static class ScreenExtensions
{
    public static Rectangle[] GetWindows(this Rectangle bounds)
    {
        return new Rectangle[] { bounds };
    }
}