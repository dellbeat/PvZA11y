using System.Runtime.InteropServices;
using Memory;
using PvZA11y.Models;
using PvZA11y.Utility;

namespace PvZA11y.Games;

internal class Game
{
    public Mem mem;
    public PointerInfo ptr;
    private ulong _gameVersion;
    private nint _processId;

    //首先适配1051版内部逻辑，做到尽可能多的实现SafeMain内关心的参数

    public Game(string appName, ulong gameVersion, Mem mem, nint processId = -1)
    {
        this.mem = mem;
        _gameVersion = gameVersion;
        _processId = processId;
        switch (gameVersion)
        {
            case 1201073:
                ptr = Pointers._1_2_0_1073(appName);
                break;
            case 1201096:
                ptr = Pointers._1_2_0_1096(appName);
                break;
            case 1001051:
                ptr = Pointers._1_0_0_1051(appName);
                break;
            default:
                Console.WriteLine("Unsupported game version!");
                Console.WriteLine("Press enter to quit!");
                Console.ReadLine();
                Environment.Exit(1);
                break;
        }

        ApplyKeyboardPatches();
    }

    public GameScene GetGameScene()
    {
        GameScene scene = GameScene.UnInit;

        switch (_gameVersion) //Maybe need change hard version code to enums 
        {
            case 1001051:
                if (mem.ReadInt(ptr.lawnAppPtr + ptr.offsetDic["loadingScreen"]) > 10000) //How To Change
                {
                    scene = GameScene.Loading;
                }
                else if (mem.ReadInt(ptr.lawnAppPtr + ptr.offsetDic["mainMenu"]) > 10000)
                {
                    scene = GameScene.MainMenu;
                }
                else if (mem.ReadInt(ptr.lawnAppPtr + ptr.offsetDic["gameSelector"]) > 10000)
                {
                    scene = GameScene.MinigameSelector;
                }
                else if (mem.ReadInt(ptr.lawnAppPtr + ptr.offsetDic["seedPicker"]) > 10000)
                {
                    scene = GameScene.SeedPicker; //If exist the ptr of seedPicker,the ptr of board is exist too.
                }
                else if (mem.ReadInt(ptr.lawnAppPtr + ptr.offsetDic["board"]) > 10000)
                {
                    scene = GameScene.Board; //场景句柄包含常规的游戏场景（0-4）和禅静花园（6-9），需要返回后综合判断
                }
                else if (mem.ReadInt(ptr.lawnAppPtr + ptr.offsetDic["awardScreen"]) > 10000)
                {
                    scene = GameScene.AwardScreen;
                }
                else if (mem.ReadInt(ptr.lawnAppPtr + ptr.offsetDic["credits"]) > 10000)
                {
                    scene = GameScene.Credits;
                }

                break;
            case 1201073:
            case 1201096:
                scene = (GameScene)mem.ReadInt(ptr.gameSceneChain);
                break;
        }

        return scene;
    }

    /// <summary>
    /// Get the game's load status
    /// </summary>
    /// <returns>If load complete,return true;otherwise return false.</returns>
    public bool CheckLoadingComplete()
    {
        bool status = false;
        
        switch (_gameVersion)
        {
            case 1001051:
                status = mem.ReadByte(ptr.offsetDic["loaded"]) == 1;
                break;
            case 1201073:
            case 1201096:
                status = mem.ReadByte(ptr.lawnAppPtr + ",86c,b9") > 0;
                break;
        }

        return status;
    }

    /// <summary>
    /// Get the game's size of window
    /// </summary>
    /// <returns>a struct contains size and startX </returns>
    public DrawSizeInfo GetWindowSize()
    {
        int windowHeight = 0;
        int windowWidth = 0;
        DrawSizeInfo info = new DrawSizeInfo();

        switch (_gameVersion)
        {
            case 1001051:
            {
                windowHeight = mem.ReadInt(ptr.lawnAppPtr + ptr.offsetDic["windowHeight"]);
                windowWidth = mem.ReadInt(ptr.lawnAppPtr + ptr.offsetDic["windowWidth"]);
                info.DrawHeight = windowHeight;
                info.DrawWidth = windowWidth;
                bool fullScreen = mem.ReadByte(ptr.lawnAppPtr + ptr.offsetDic["nonFullScreen"]) == 0;
                if (fullScreen)
                {
                    var rect = ExternUtility.GetScreenSize(_processId);
                    info.DrawWidth = rect.Item1;
                    info.DrawHeight = rect.Item2;
                    if (rect.Item1 != 0) //I think nobody's screen is vertical when play pvz.
                    {
                        double minResizeRate = Math.Min(rect.Item1 * 1.0 / windowWidth, rect.Item2 * 1.0 / windowHeight);
                        windowWidth = Convert.ToInt32(windowWidth * minResizeRate);
                        windowHeight = Convert.ToInt32(windowHeight * minResizeRate);
                        info.DrawStartX = (info.DrawWidth - windowWidth) / 2;
                    }
                }
            }
                break;
            case 1201073:
            case 1201096:
                windowWidth = mem.ReadInt(ptr.lawnAppPtr + ",3a0,a0"); //Need change the raw offset string to other way
                windowHeight = mem.ReadInt(ptr.lawnAppPtr + ",3a0,a4");
                info.DrawWidth = mem.ReadInt(ptr.lawnAppPtr + ",3a0,b8");
                info.DrawHeight = mem.ReadInt(ptr.lawnAppPtr + ",3a0,bc");
                info.DrawStartX = (windowWidth - info.DrawWidth) / 2;
                break;
        }

        return info;
    }

    private void ApplyKeyboardPatches()
    {
        string[] disableArray =
            [ptr.keyboardInputDisable1, ptr.keyboardInputDisable2, ptr.keyboardInputDisable3, ptr.musicPausePatch, ptr.keyboardInputDisable4];
        string[] disableNameArray =
        [
            nameof(ptr.keyboardInputDisable1), nameof(ptr.keyboardInputDisable2), nameof(ptr.keyboardInputDisable3),
            nameof(ptr.musicPausePatch), nameof(ptr.keyboardInputDisable4),
        ];
        string[] patchedArray =
        [
            ptr.keyboardInputDisable1Patched, ptr.keyboardInputDisable2Patched, ptr.keyboardInputDisable3, ptr.musicPausePatched,
            ptr.keyboardInputDisable4Patched
        ];

        string[][] patchCodeArray = [["EB"], ["90 90", "90 90", "eb"], ["5f 5e c2 04 00 90 90"], ["90 90"], ["c2 04 00"]];
        int[][] patchOffsetArray = [[7], [12, 5, 5], [0], [0], [0]];

        for (int i = 0; i < disableArray.Length; i++)
        {
            long codeAddress = mem.AoBScan(disableArray[i]).Result.FirstOrDefault();
            if (codeAddress == 0)
            {
                if (mem.AoBScan(patchedArray[i]).Result.FirstOrDefault() != 0)
                {
                    Console.WriteLine($"'{disableNameArray[i]}' already patched!");
                }
                else
                {
                    Console.WriteLine($"Failed to find '{disableNameArray[i]}' code!");
                }
            }

            for (var offset = 0; offset < patchOffsetArray[i].Length; offset++)
            {
                codeAddress += patchOffsetArray[i][offset];
                mem.WriteMemory(codeAddress.ToString("X2"), patchCodeArray[i][offset].Length == 2 ? "byte" : "bytes", patchCodeArray[i][offset]);
            }
        }

        //Overwrite instruction which handles keyboard input for some dialogues (y/n for yes/no dialogues, escape, space)
        //Overwrite instructions which read keyboard input in options menu (space/escape/enter to close dialog)
        //Overwrite instructions which read keyboard input on board (space/escape for pause/options)
        //Replace (cmp dword ptr [ecx+0000091C],02) (check if gameScene is Into/cutscene/seedpicker), with "pop edi; pop esi; ret 0004;" (return)

        Console.WriteLine("Finished patching");
    }
}