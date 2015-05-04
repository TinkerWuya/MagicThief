﻿public class Escape : Action
{
    int Duration = 100;
    public int GetDuration()
    {
        return Duration;
    }
    public override void Excute()
    {
        UnityEngine.Debug.Log("magician Escape");
        Globals.maze.GuardsTargetVanish(actor.gameObject);
        base.Excute();
        actor.moving.canMove = false;
        actor.spriteSheet.Play("flying");
        transform.position = transform.position - new UnityEngine.Vector3(0,0,0.6f);
        actor.AddAction(
            new Sequence(new MoveTo(transform, transform.position + new UnityEngine.Vector3(0, 15, 0), Duration)
                ,new FunctionCall(()=>EscapeOver())));
        if (actor.moving.GetSeeker().GetCurrentPath() != null)
        {
            actor.moving.GetSeeker().GetCurrentPath().Reset();
        }        
    }

    void EscapeOver()
    {        
        Stop();
        Globals.magician.gameObject.SetActive(false);
    }
}