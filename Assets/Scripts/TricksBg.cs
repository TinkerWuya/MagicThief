﻿public class TricksBg : CustomEventTrigger
{
    UnityEngine.UI.Button TrickItemsTabBtn;
    public UnityEngine.UI.GridLayoutGroup TrickItemsLayout;
    public System.Collections.Generic.List<TrickItem> trickItemsInPack = new System.Collections.Generic.List<TrickItem>();
    public System.Collections.Generic.List<UnityEngine.GameObject> trickSlots = new System.Collections.Generic.List<UnityEngine.GameObject>();

    public UIMover ClickHypnosisPointer;
    public override void Awake()
    {
        base.Awake();
        TrickItemsTabBtn = Globals.getChildGameObject<UnityEngine.UI.Button>(gameObject, "TrickItemsTabBtn");
        TrickItemsLayout = Globals.getChildGameObject<UnityEngine.UI.GridLayoutGroup>(gameObject, "TrickItemsLayout");        

        ClickHypnosisPointer = Globals.getChildGameObject<UIMover>(gameObject, "ClickHypnosisPointer");
        ClickHypnosisPointer.transform.parent.gameObject.SetActive(false);
    }

    public void CreateTrickItemsInPack()
    {
        UnityEngine.GameObject itemSlotPrefab = UnityEngine.Resources.Load<UnityEngine.GameObject>("UI/TrickItemSlot");
        UnityEngine.GameObject itemPrefab = UnityEngine.Resources.Load<UnityEngine.GameObject>("UI/TrickItem");
        ClickHypnosisPointer.transform.parent.gameObject.SetActive(false);
        foreach (TrickData data in Globals.tricks)
        {
            UnityEngine.GameObject itemSlot = UnityEngine.GameObject.Instantiate(itemSlotPrefab) as UnityEngine.GameObject;
            itemSlot.transform.parent = TrickItemsLayout.transform;
            itemSlot.transform.localScale = UnityEngine.Vector3.one;
                                               
            if (!data.IsInUse())
            {
                TrickItem trickItem = (UnityEngine.GameObject.Instantiate(itemPrefab) as UnityEngine.GameObject).GetComponent<TrickItem>();
                trickItem.trickData = data;
                if (Globals.self.tricksBought.Contains(data.nameKey))
                {
                    trickItem.Buy();
                }
                else
                {
                    trickItem.GetComponent<UnityEngine.UI.Image>().sprite = UnityEngine.Resources.Load<UnityEngine.Sprite>("UI/" + data.nameKey + "_icon_disabled");
                }
                trickItem.rt.parent = itemSlot.transform;
                trickItem.rt.localScale = UnityEngine.Vector3.one;
                trickItem.rt.localPosition = UnityEngine.Vector3.zero;
                trickItem.name = data.nameKey;                
                trickItem.CheckIfUnlock();
                trickItem.slotInPack = itemSlot;
                trickItemsInPack.Add(trickItem);
            }            
            trickSlots.Add(itemSlot);

            // 如果在教程阶段，还没有购买催眠
            if (data == Globals.tricks[0] && Globals.self.TutorialLevelIdx == PlayerInfo.TutorialLevel.FirstTrick && !data.bought)
            {
                ClickHypnosisPointer.transform.parent.gameObject.SetActive(true);
                ClickHypnosisPointer.BlinkForever();
            }            
        }
    }

    public int GetEmptyItemSlotIdx()
    {
        for (int idx = 0; idx < trickSlots.Count; ++idx)
        {
            if (trickSlots[idx].transform.childCount == 0)
            {
                return idx;
            }
        }
        return -1;
    }

    public UnityEngine.GameObject GetEmptyItemSlot()
    {
        int slotIdx = GetEmptyItemSlotIdx();
        return trickSlots[slotIdx];        
    }    

    public override void OnTouchUpOutside(Finger f)
    {
        base.OnTouchUpOutside(f);
        foreach (UnityEngine.GameObject slot in trickSlots)
        {
            DestroyObject(slot);
        }
        trickSlots.Clear();
        trickItemsInPack.Clear();
        gameObject.SetActive(false);
        Globals.canvasForMagician.CheckIfNeedDraggingItemFinger();
        StealingLevelController controller = (Globals.LevelController as StealingLevelController);
        if (controller != null)
        {
            Globals.maze.RegistChallengerEvent();
        }
    }
}