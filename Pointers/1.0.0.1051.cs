namespace PvZA11y;

partial class Pointers
{
    public static PointerInfo _1_0_0_1051(string appName)
    {
        return new PointerInfo(
            appName: appName,
            lawnAppPtrOffset: "+002A9EC0",
            //lawnAppPtrOffset:                   "+0035799C",  //Pointer for cn version of 1.2.0.1073 (why didn't they change the version number)
            dirtyBoardPtr: ",320,18,0,8",
            boardPtrOffset: ",868",
            boardPausedOffset: ",17c",
            playerInfoOffset: ",94c",
            playerLevelOffset: ",4c",
            playerCoinsOffset: ",50",
            playerAdventureCompletionsOffset: ",54",
            playerPurchaseOffset: 0x1e8,
            playerMinigamesUnlockedOffset: ",348",
            playerPuzzleUnlockedOffset: ",34c",
            playerSurvivalUnlockedOffset: ",360",
            gameSceneOffset: ",91c",
            gameModeOffset: ",918",
            awardScreenOffset: ",878",
            awardTypeOffset: ",b8"
        )
        {
            offsetDic = new Dictionary<string, string>()
            {
                { "loadingScreen", ",76c" },
                { "loaded", "PlantsVsZombies.exe+322B20" },
                //Temp,I just find some address will change value from 0 to 1 after loading complete
                { "mainMenu", ",770" },
                { "gameSelector", ",780" },
                { "seedPicker", ",774,0" },
                { "board", ",768" },
                { "awardScreen", ",778" },
                { "credits", ",77c" },
                { "nonFullScreen", ",343" },
                { "windowWidth", ",C0" },
                { "windowHeight", ",C4" }
            }
        };
    }
}