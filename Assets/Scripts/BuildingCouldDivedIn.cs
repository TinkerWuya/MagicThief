﻿public class BuildingCouldDivedIn : Building 
{
    public UnityEngine.GameObject DiveInBtn;
    
    public override void Awake()
    {
        DiveInBtn = Globals.getChildGameObject<UnityEngine.RectTransform>(gameObject, "DiveIn").gameObject;
        DiveInBtn.SetActive(false);
        base.Awake();
    }
	public override void Choosen()
    {                
        DiveInBtn.gameObject.SetActive(true);
        DiveInBtn.transform.localScale = UnityEngine.Vector3.zero;
        AddAction(new Sequence(
            new ScaleTo(DiveInBtn.transform, new UnityEngine.Vector3(1.2f, 1.2f, 1.2f), Globals.uiMoveAndScaleDuration / 3),
            new ScaleTo(DiveInBtn.transform, new UnityEngine.Vector3(1.0f, 1.0f, 1.0f), Globals.uiMoveAndScaleDuration / 4)));
        base.Choosen();
    }

    public override void Unchoose()
    {
        AddAction(new ScaleTo(DiveInBtn.transform, UnityEngine.Vector3.zero, Globals.uiMoveAndScaleDuration/3));
        base.Unchoose();
    }

    public virtual void DivedIn()
    {
        Unchoose();
        city.cityEventsOpenBtn.Goback(Globals.uiMoveAndScaleDuration);
        city.eventsWindow.GetComponent<UIMover>().Goback(Globals.uiMoveAndScaleDuration);
    }
}