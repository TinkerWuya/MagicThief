﻿public class Gem : Actor 
{
    FlyToScreenCashNumber fly;
    public int cashValue = 1000;
    
	public override void Awake () 
    {
        base.Awake();
        fly = GetComponent<FlyToScreenCashNumber>();

        UnityEngine.Vector3 from = UnityEngine.Vector3.zero;
        UnityEngine.Vector3 to = new UnityEngine.Vector3(0.0f, 360.0f, 0.0f);
        if(UnityEngine.Random.Range(0.0f, 1.0f) < 0.5f)
        {
            from = to;
            to = UnityEngine.Vector3.zero;
        }        
        AddAction(new RotateTo(from, to, UnityEngine.Random.Range(8.0f, 15.0f), true));
	}

    void OnTriggerEnter(UnityEngine.Collider other)
    {
        float floatDuration = 0.8f;
        transform.parent = null;
        ClearAllActions();
        AddAction(new MoveTo(transform, transform.localPosition + new UnityEngine.Vector3(0.0f, 2.0f, 0.0f), floatDuration));
        fly.cashDelta = cashValue;
        fly.Invoke("FloatUp", floatDuration + 0.1f);
    }    
}