﻿public class AddSafeBoxUI : UnityEngine.MonoBehaviour 
{
    UnityEngine.UI.Button AddBtn;    
    UnityEngine.UI.Text AddPrice;
    UnityEngine.GameObject Lock;
    UnityEngine.UI.Text LockMsg;
    MultiLanguageUIText capacity;
    public void Awake()
    {                
        AddBtn = Globals.getChildGameObject<UnityEngine.UI.Button>(gameObject, "AddBtn");
        AddBtn.onClick.AddListener(() => ClickToAddSafeBox());
        AddPrice = Globals.getChildGameObject<UnityEngine.UI.Text>(gameObject, "CashNumber");       
        Lock = Globals.getChildGameObject(gameObject, "LockBg");
        LockMsg = Globals.getChildGameObject<UnityEngine.UI.Text>(gameObject, "LockMsg");
        capacity = Globals.getChildGameObject<MultiLanguageUIText>(gameObject, "capacity");
    }

    public void ClickToAddSafeBox()
    {
        UnityEngine.Debug.Log("ClickToAddSafeBox");
        if (Globals.canvasForMagician.ChangeCash(-Globals.buySafeBoxPrice))
        {
            Globals.canvasForMyMaze.enhanceDefenseUI.OnTouchUpOutside(null);
            UnityEngine.Vector3 falling_pos = (Globals.LevelController as MyMazeLevelController).AddSafeBox();
            Globals.cameraFollowMagician.MoveToPoint(falling_pos, 30);
        }
    }

	public void UpdateInfo()
    {
        MazeLvData mazeData = Globals.mazeLvDatas[Globals.self.currentMazeLevel];
        if (Globals.self.currentMazeLevel != 0)
        {
            Globals.Assert(Globals.maze.noOfRoomsToPlace == mazeData.safeBoxCount);
        }
        Globals.languageTable.SetText(capacity,"capacity",
            new System.String[] { Globals.safeBoxLvDatas[0].capacity.ToString() });
        if (Globals.self.safeBoxDatas.Count >= mazeData.safeBoxCount)
        {
            if (Globals.self.currentMazeLevel == Globals.mazeLvDatas.Count - 1)
            {
                AddBtn.gameObject.SetActive(false);
                capacity.gameObject.SetActive(false);
                Lock.gameObject.SetActive(true);                
                LockMsg.gameObject.SetActive(true);
                Globals.languageTable.SetText(LockMsg, "no_more_safebox");
            }
            else
            {
                AddBtn.gameObject.SetActive(false);
                Lock.gameObject.SetActive(true);
                LockMsg.gameObject.SetActive(true);
                Globals.languageTable.SetText(LockMsg,
                    "upgrade_maze_to_add_safe_box", new System.String[] { (Globals.self.currentMazeLevel + 1).ToString() });
            }            
        }
        else if (Globals.self.safeBoxDatas.Count < mazeData.safeBoxCount)
        {            
            AddBtn.gameObject.SetActive(true);
            AddPrice.text = Globals.buySafeBoxPrice.ToString();
            if (Globals.self.cashAmount < Globals.buySafeBoxPrice)
            {
                AddPrice.color = UnityEngine.Color.red;
            }
            else
            {
                AddPrice.color = UnityEngine.Color.white;
            }
            Lock.gameObject.SetActive(false);
        }        
    }
}