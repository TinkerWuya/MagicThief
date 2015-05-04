﻿public class GuardAttack : GuardAction 
{    
    bool isAtkCDing = false;
    Cocos2dAction checkFunc;
    Magician targetMage = null;
    public override void Awake()
    {
        base.Awake();
//         if (!Globals.AvatarAnimationEventNameCache.Contains(guard.name + "-A"))
//         {
//             UnityEngine.AnimationEvent evt = new UnityEngine.AnimationEvent();
//             evt.functionName = "AtkEnd";
//             evt.time = guard.anim["A"].length;
//             guard.anim["A"].clip.AddEvent(evt);
//             Globals.AvatarAnimationEventNameCache.Add(guard.name + "-A");
//         }
//         Globals.Assert(guard.atkCd > guard.anim["A"].length / guard.attackSpeed);


        actor.spriteSheet.CreateAnimationByName("Atk", 0.5f);
        actor.spriteSheet.AddAnimationEvent("Atk", -1, () => AtkEnd());
        actor.spriteSheet.AddAnimationEvent("Atk", 3, () => checkIfHitTarget());
    }

    public override void Excute()
    {
        base.Excute();
        targetMage = guard.spot.target.GetComponent<Magician>();
        Attack();
    }

    public override void Stop()
    {
        UnityEngine.Debug.Log("guard stop attack");
        base.Stop();        
        guard.RemoveAction(ref checkFunc);        
        guard.spriteSheet.transform.localEulerAngles = new UnityEngine.Vector3(0, 0, 0);
    }

    void Attack()
    {
        guard.RemoveAction(ref checkFunc);
        if (!isAtkCDing)
        {
            UnityEngine.Debug.Log("Guard Attacking");
            guard.FaceTarget(guard.spot.target);

            if (!checkIfTargetPressDown())
            {
                guard.spriteSheet.transform.localEulerAngles = new UnityEngine.Vector3(0, 180, 0);
                guard.spriteSheet.Play("Atk");
            }
            else
            {
                guard.spriteSheet.Play("idle");
            }
            
            isAtkCDing = true;
            guard.SleepThenCallFunction(guard.data.atkCd, () => AtkCDOver());


            System.String content = gameObject.name;
            content += " Attack";
            Globals.record("testReplay", content);
        }
        else
        {
            UnityEngine.Debug.Log("Guard Attacking Gap");
            guard.spriteSheet.Play("idle");
        }
    }

    bool checkIfTargetPressDown()
    {
        Actor targetMage = guard.spot.target.GetComponent<Actor>();
        Magician mage = targetMage as Magician;
        if (mage == null)
        {
            return false;
        }
        if (mage.gameObject.layer == 11 && mage.currentAction == mage.beenPressDown)
        {
            return true;
        }
        return false;
    }

    public void checkIfHitTarget()
    {
        UnityEngine.Vector3 magicianDir = guard.spot.target.position - guard.transform.position;
        Actor targetActor = guard.spot.target.GetComponent<Actor>();
        if (!checkIfTargetPressDown() && !targetActor.IsLifeOver())
        {
            if (magicianDir.magnitude < guard.data.atkShortestDistance + 0.3f)
            {
                UnityEngine.Vector3 faceDir = UnityEngine.Vector3.left;
                if (transform.localEulerAngles.y > 179)
                {
                    faceDir = UnityEngine.Vector3.right;
                }
                faceDir.z = 0;
                double angle = UnityEngine.Vector3.Angle(magicianDir, faceDir);
                if (angle < 90 && angle > -90)
                {
                    targetActor.ChangeLife(-guard.data.attackValue);
                    if (!targetActor.IsLifeOver())
                    {
                        targetActor.hitted.Excute();
                    }                  
                    targetActor.FaceDir(magicianDir);
                }
            }
        }        
    }

    public void AtkEnd()
    {
        UnityEngine.Debug.Log("atk end");
        guard.spriteSheet.transform.localEulerAngles = new UnityEngine.Vector3(0, 0, 0);
        if (guard.currentAction == this)
        {
            guard.spriteSheet.Play("idle");
            checkFunc = guard.RepeatingCallFunction(5, () => DuringAtkIdle());
        }                
    }

    void DuringAtkIdle()
    {
        // 因为check的时候可能会切换动作
        if(guard.currentAction == this)
        {
            checkTargetStillAlive();
        }
        if (guard.currentAction == this)
        {
            checkTargetStillClose();
        }        
    }

    void AtkCDOver()
    {
        System.String content = gameObject.name;
        content += " AtkCDOver";
        Globals.record("testReplay", content);

        isAtkCDing = false;
        if (guard.currentAction == this)
        {
            if (checkTargetStillClose() && checkTargetStillAlive())
            {
                Attack();
            }
        }
    }

    bool checkTargetStillAlive()
    {
        if (guard.spot.target.GetComponent<Actor>().IsLifeOver())
        {
            guard.wandering.Excute();
            return false;
        }
        return true;
    }

    bool checkTargetStillClose()
    {
        System.Diagnostics.Debug.Assert(guard.spot.target != null);
        if (UnityEngine.Vector3.Distance(guard.spot.target.position, guard.transform.position) > guard.data.atkShortestDistance)
        {
            guard.chase.Excute();
            return false;
        }
        return true;
    }    
}