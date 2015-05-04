﻿public class LevelTip : AlphaFadeUI 
{
    [UnityEngine.HideInInspector]
    float fade = 1.0f;
    [UnityEngine.HideInInspector]
    int wait = 180;
    UnityEngine.UI.Text tip;
    
    public override void Awake()
    {
        base.Awake();                
        UpdateAlpha(0);
    }

    public void Show(System.String text)
    {
        if (tip == null)
        {
            tip = Globals.getChildGameObject<UnityEngine.UI.Text>(gameObject, "Text");
        }
        gameObject.SetActive(true);
        Globals.languageTable.SetText(tip, text);
        AddAction(new Sequence(new FadeUI(this, 0, 1, fade), new SleepFor(wait),
            new FadeUI(this, 1, 0, fade), new FunctionCall(()=>FadeOver())));
    }

    void FadeOver()
    {
        gameObject.SetActive(false);
    }

    public float GetFadeDuration()
    {
        return fade;
    }

    public int GetWaitingDuration()
    {
        return wait;
    }
}
