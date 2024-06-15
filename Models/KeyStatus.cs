// ReSharper disable InconsistentNaming
namespace PvZA11y.Models;

public enum KeyStatus
{
    WM_KEYDOWN = 0x0100,
    WM_KEYUP = 0x0101,
    WM_CHAR = 0x0102,
    VK_TAB = 0x09,
    VK_ENTER = 0x0D,
    VK_UP = 0x26,
    VK_DOWN = 0x28,
    VK_RIGHT = 0x27,

    WM_LBUTTONDOWN = 0x0201,
    WM_LBUTTONUP = 0x0202,

    WM_RBUTTONDOWN = 0x0204,
    WM_RBUTTONUP = 0x0205,
}