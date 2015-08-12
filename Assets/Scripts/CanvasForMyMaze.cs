﻿public class CanvasForMyMaze : Actor
{
    public MyMazeLevelController myMaze;
    
    public UnityEngine.UI.Button FingerImageToDragGuard;
    UnityEngine.Vector2 fingerImagePosCache;

    public EnhanceDefenseUI enhanceDefenseUI;    
    UIMover ExitMazeBtn;
    
    public UnityEngine.UI.Text unclickedCityEventCount;
    public UIMover TipBuyHereAsHome;

    public UnityEngine.GameObject MazeRoomNumerBg;
    public LifeNumber MazeRoomNumber;
    public int roomConsumed;
    public UnityEngine.GameObject room_not_full_used;
    

    UIMover FogBtn;
    public override void Awake()
    {
        base.Awake();
        Globals.canvasForMyMaze = this;
        TipBuyHereAsHome = Globals.getChildGameObject<UIMover>(gameObject, "TipBuyHereAsHome");
        
        enhanceDefenseUI = Globals.getChildGameObject<EnhanceDefenseUI>(gameObject, "EnhanceDefenseUI");

        

        ExitMazeBtn = Globals.getChildGameObject<UIMover>(gameObject, "ExitMazeBtn");
        ExitMazeBtn.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => GoToCity());

        //unclickedCityEventCount = Globals.getChildGameObject<UnityEngine.UI.Text>(ExitMazeBtn.gameObject, "UnclickedCount");
       
        room_not_full_used.gameObject.SetActive(false);        

        FogBtn = Globals.getChildGameObject<UIMover>(gameObject, "FogBtn");
        FogBtn.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => SwitchFog());
    }
    
	// Use this for initialization
	public override void Start () 
    {
        base.Start();
        myMaze = (Globals.LevelController as MyMazeLevelController);

        UnityEngine.UI.Button chestDownBtn = Globals.getChildGameObject<UnityEngine.UI.Button>(TipBuyHereAsHome.gameObject, "ChestDown");
        chestDownBtn.onClick.AddListener(() => myMaze.ChestFallingStart());

        gameObject.SetActive(false);
	}
	
    public void ShowCreateMazeBtn()
    {
        myMaze.currentThief.HideTip();
        Globals.canvasForMyMaze.enhanceDefenseUI.Open();
   }

    public void MazeZeroToOne()
    {        
        myMaze.StartDropPieces();
        foreach (Cell cell in Globals.maze.EveryCells)
        {
            cell.gameObject.SetActive(true);
            if (cell.chest != null)
            {
                cell.ShowEverything();
            }
        }
    }

    public void ShowSelectGuardPanelTip()
    {
        myMaze.currentThief.HideTip();
        Globals.canvasForMyMaze.gameObject.SetActive(true);
        Globals.canvasForMagician.gameObject.SetActive(true);
        Globals.canvasForMyMaze.enhanceDefenseUI.Open();        
    }

    void ShowTutorialEndMsgBox()
    {
        Globals.MessageBox("property_is_safe_now", () => TutorialEnd());
        Globals.self.AdvanceTutorial();
    }

    public void TutorialEnd()
    {
        //Globals.selectGuard.mover.BeginMove(Globals.uiMoveAndScaleDuration);
        //DestroyObject(Globals.canvasForMyMaze.MazeTabPointer);
        Globals.canvasForMagician.gameObject.SetActive(true);
        Globals.canvasForMyMaze.gameObject.SetActive(true);
        Globals.canvasForMyMaze.enhanceDefenseUI.Open();
        ExitMazeBtn.BeginMove(Globals.uiMoveAndScaleDuration);
        FogBtn.BeginMove(Globals.uiMoveAndScaleDuration);
        //进入MyMaze，HomeExit上面的红点根据new targets来更新        
        //Globals.UpdateUnclickedRedPointsText(unclickedCityEventCount);
    }

    public void GoToCity()
    {
        Globals.asyncLoad.ToLoadSceneAsync("City");

        //Globals.selectGuard.mover.Goback(Globals.uiMoveAndScaleDuration);
        Globals.canvasForMyMaze.enhanceDefenseUI.gameObject.SetActive(false);
        ExitMazeBtn.Goback(Globals.uiMoveAndScaleDuration);
        FogBtn.Goback(Globals.uiMoveAndScaleDuration);
        Globals.cameraFollowMagician.CloseMinimap();
        room_not_full_used.gameObject.SetActive(false);
    }

    public bool ChangeMazeRoom(int delta)
    {
        int roomTemp = roomConsumed;
        roomTemp += delta;
        if (roomTemp > Globals.mazeLvDatas[Globals.self.currentMazeLevel].roomSupport)
        {
            Globals.tipDisplay.Msg("not_enough_room");
            return false;
        }
        else
        {
            roomConsumed = roomTemp;
            MazeRoomNumber.UpdateCurrentLife(roomTemp, Globals.mazeLvDatas[Globals.self.currentMazeLevel].roomSupport);            
            return true;
        }
    }

    public void CheckRoomFullUses()
    {
        int roomSupport = Globals.mazeLvDatas[Globals.self.currentMazeLevel].roomSupport;
        if (roomConsumed == roomSupport || roomSupport == 0)
        {
            room_not_full_used.SetActive(false);
        }
        else
        {
            room_not_full_used.SetActive(true);
        }
    }

    public void SwitchFog()
    {
        myMaze.fogPlane.SetActive(!myMaze.fogPlane.activeSelf);

        if (!myMaze.fogPlane.activeSelf)
        {
            Globals.languageTable.SetText(FogBtn.GetComponentInChildren<MultiLanguageUIText>(), "open_fog");
            
        }
        else
        {
            Globals.languageTable.SetText(FogBtn.GetComponentInChildren<MultiLanguageUIText>(), "close_fog");
            myMaze.fogCam.clearFlags = UnityEngine.CameraClearFlags.Color;
            Invoke("ClearFogTexOver",0.1f);
        }
    }

    public void ClearFogTexOver()
    {
        myMaze.fogCam.clearFlags = UnityEngine.CameraClearFlags.Depth;
    }
}
