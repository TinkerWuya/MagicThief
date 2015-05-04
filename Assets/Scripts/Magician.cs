public class Magician : Actor
{
    public bool isMoving;
    public TryEscape tryEscape;
    public Escape escape;
    public Victory victory;
    public Falling falling;
    public Incant incant;    
        
    public Hypnosis hypnosis;
    public ShotLight shot;
    public Disguise disguise;
    public FlyUp flyUp;    

    [UnityEngine.HideInInspector]
    public UnityEngine.GameObject TrickTimerPrefab;    
    
    public override void Awake()
    {
        base.Awake();
        spriteSheet.CreateAnimationByName("idle");
        spriteSheet.CreateAnimationByName("moving");
        DontDestroyOnLoad(this);
        tryEscape = GetComponent<TryEscape>();
        escape = GetComponent<Escape>();
        victory = GetComponent<Victory>();
        falling = GetComponent<Falling>();
        incant = GetComponent<Incant>();        
        
        TrickData trick = new TrickData();        
        trick.nameKey = "hypnosis";
        trick.descriptionKey = "hypnosis_desc";
        trick.duration = 350;
        trick.powerCost = 25;
        trick.unlockRoseCount = 0;
        trick.price = 0;
        trick.bought = true;
        hypnosis = GetComponent<Hypnosis>();
        hypnosis.data = trick;
        Globals.tricks.Add(trick);

        trick = new TrickData();
        trick.nameKey = "dove";
        trick.descriptionKey = "dove_desc";
        trick.duration = 400;
        trick.powerCost = 20;
        trick.unlockRoseCount = 0;
        trick.price = 300;
        Globals.tricks.Add(trick);

        trick = new TrickData();
        trick.nameKey = "disguise";
        trick.descriptionKey = "disguise_desc";
        trick.duration = 500;
        trick.powerCost = 35;
        trick.unlockRoseCount = 0;
        trick.price = 500;
        disguise = GetComponent<Disguise>();
        disguise.data = trick;
        Globals.tricks.Add(trick);

        trick = new TrickData();
        trick.nameKey = "flash_grenade";
        trick.descriptionKey = "flash_grenade_desc";
        trick.duration = 0;
        trick.powerCost = 2;
        trick.unlockRoseCount = 10;
        trick.price = 10000;        
        Globals.tricks.Add(trick);

        trick = new TrickData();
        trick.nameKey = "flyUp";
        trick.descriptionKey = "flyUp_desc";
        trick.duration = 300;
        trick.powerCost = 35;
        trick.unlockRoseCount = 30;
        trick.price = 16000;
        flyUp = GetComponent<FlyUp>();
        flyUp.data = trick;
        Globals.tricks.Add(trick);

        trick = new TrickData();
        trick.nameKey = "shotLight";
        trick.descriptionKey = "shotLight_desc";
        trick.duration = 0;
        trick.powerCost = 10;
        trick.unlockRoseCount = 50;
        trick.price = 31000;
        shot = GetComponent<ShotLight>();
        shot.data = trick;
        Globals.tricks.Add(trick);                
        

        moving.canMove = false;
        gameObject.name = "Magician";              
        Globals.magician = this;
        LifeAmount = 100;
        LifeCurrent = LifeAmount;
        PowerAmount = 100;
        PowerCurrent = PowerAmount;

        GuardData guard_data = new GuardData();
        guard_data.name = "guard";
        guard_data.price = 3500;
        guard_data.roomConsume = 2;
        guard_data.magicianOutVisionTime = 250;
        guard_data.atkCd = 120;
        guard_data.attackValue = 60;
        guard_data.atkShortestDistance = 1.5f;
        guard_data.doveOutVisionTime = 50;
        guard_data.attackSpeed = 1.0f;
        Globals.guardDatas.Add(guard_data);

        guard_data = new GuardData();
        guard_data.name = "dog";
        guard_data.price = 8000;
        guard_data.roomConsume = 1;
        guard_data.magicianOutVisionTime = 500;
        guard_data.attackValue = 40;
        guard_data.atkShortestDistance = 1.0f;
        guard_data.doveOutVisionTime = 500;
        Globals.guardDatas.Add(guard_data);

        guard_data = new GuardData();
        guard_data.name = "armed";
        guard_data.price = 12000;
        guard_data.roomConsume = 3;
        guard_data.magicianOutVisionTime = 450;
        guard_data.atkCd = 60;
        guard_data.attackValue = 101;
        guard_data.atkShortestDistance = 1.5f;
        guard_data.doveOutVisionTime = 50;
        guard_data.attackSpeed = 1.0f;
        Globals.guardDatas.Add(guard_data);

        guard_data = new GuardData();
        guard_data.name = "lamp";
        guard_data.price = 15000;
        guard_data.roomConsume = 1;        
        Globals.guardDatas.Add(guard_data);

        MazeLvData data = new MazeLvData();
        data.lockGuardsName = new System.String[]{};
        data.playerEverClickGuards = true;
        data.playerEverClickSafebox = true;
        Globals.mazeLvDatas.Add(data);

        data = new MazeLvData();
        data.roseRequire = 0;
        data.price = 3000;
        data.roomSupport = 8;
        data.lockGuardsName = new System.String[] { "guard"};
        data.safeBoxCount = 3;
        Globals.mazeLvDatas.Add(data);

        data = new MazeLvData();
        data.roseRequire = 20;
        data.price = 12000;
        data.roomSupport = 15;
        data.lockGuardsName = new System.String[] { "dog","armed"};
        data.safeBoxCount = 4;
        Globals.mazeLvDatas.Add(data);

        data = new MazeLvData();
        data.roseRequire = 50;
        data.price = 25000;
        data.roomSupport = 23;
        data.lockGuardsName = new System.String[] { "lamp"};
        data.safeBoxCount = 5;
        Globals.mazeLvDatas.Add(data);

        TrickTimerPrefab = UnityEngine.Resources.Load("UI/FakeGuardTimer") as UnityEngine.GameObject;

        eye.gameObject.SetActive(false);
    }

    public void RegistEvent()
    {
        
    }

    public void UnRegistEvent()
    {
        
    }

    public bool stopMoving(string value)
    {
        isMoving = false;
        return true;
    }	

    void OnTriggerEnter(UnityEngine.Collider other)
    {
        //Debug.Log("magician trigger : 碰撞到的物体的名字是：" + other.gameObject.name);
    }

    public override void InStealing()
    {
        base.InStealing();        
        RegistEvent();
        eye.gameObject.SetActive(true);
        //moving.canMove = true;
        Globals.EnableAllInput(true);
        Globals.cameraFollowMagician.SetDragSpeed(0.02f);
        Globals.canvasForMagician.gameObject.SetActive(true);
    }

    public override void OutStealing()
    {
        base.OutStealing();
        UnRegistEvent();
        moving.canMove = false;  
//         Globals.cameraFollowMagician.MoveToPoint(
//             transform.position + new UnityEngine.Vector3(0.0f, 0.5f, 0.0f),
//             new UnityEngine.Vector3(Globals.cameraFollowMagician.disOffset.x * 0.7f,
//                 Globals.cameraFollowMagician.disOffset.y * 0.1f,
//                 Globals.cameraFollowMagician.disOffset.z * 0.5f), 0.7f);
        isMoving = false;
        eye.gameObject.SetActive(false);
        Globals.cameraFollowMagician.CloseMinimap();
        Globals.canvasForMagician.gameObject.SetActive(false);
        (Globals.LevelController as TutorialLevelController).canvasForStealing.gameObject.SetActive(false);
    }

    public void CoverInMoonlight()
    {
        if(currentAction != null)
        {
            currentAction.Stop();
        }
        spriteSheet.Play("idle");
//         for (int idx = 0; idx < skinnedMeshRenderers.Length; ++idx)
//         {
//             skinnedMeshRenderers[idx].material.shader = UnityEngine.Shader.Find("Diffuse");            
//         }
    }

    public void CastMagic(TrickData data)
    {
        Globals.replaySystem.RecordMagicCast(data);
        TutorialLevelController level = Globals.LevelController as TutorialLevelController;
        if (level != null && data.nameKey == "flash_grenade")
        {
            if (!Stealing)
            {                
                if (data.IsInUse() || Globals.playingReplay != null)
                {
                    UnityEngine.GameObject landingMark = level.landingMark;
                    if (landingMark.activeSelf)
                    {
                        if(ChangePower(-data.powerCost))
                        {
                            UnityEngine.GameObject flashPrefab = UnityEngine.Resources.Load("Avatar/Flash") as UnityEngine.GameObject;
                            UnityEngine.GameObject flash = UnityEngine.GameObject.Instantiate(flashPrefab) as UnityEngine.GameObject;
                            flash.transform.position = landingMark.transform.position;
                        }                        
                    }
                    else
                    {
                        Globals.tipDisplay.Msg("no_landmark_yet");
                    }
                }                
            }
            else
            {
                Globals.tipDisplay.Msg("stealing_cant_use_flash");
            }
        }
        else if (Stealing && ChangePower(-data.powerCost))
        {
            if (data.nameKey == "disguise")
            {
                disguise.Excute();
            }
            if (data.nameKey == "flyUp")
            {
                Globals.maze.GuardsTargetVanish(gameObject);
                flyUp.Excute();
            }
            if (data.nameKey == "dove")
            {
                UnityEngine.GameObject dovePrefab = UnityEngine.Resources.Load("Avatar/Dove") as UnityEngine.GameObject;
                // 取最近的两个守卫
                System.Collections.Generic.List<Guard> guards = new System.Collections.Generic.List<Guard>(Globals.maze.guards.ToArray());
                guards.Sort();
                int dove_idx = 0;
                foreach (Guard guard in guards)
                {
                    if (guard.moving == null)
                    {
                        continue;
                    }
                    Dove dove = (UnityEngine.GameObject.Instantiate(dovePrefab) as UnityEngine.GameObject).GetComponent<Dove>();
                    dove.transform.position = transform.position;
                    dove.StartOut(guard.transform.position, data);
                    ++dove_idx;
                    if (dove_idx == 2)
                    {
                        break;
                    }
                }
            }
            if (data.nameKey == "hypnosis" && Globals.magician.currentAction != Globals.magician.hypnosis)
            {
                Guard guard = (Globals.LevelController as TutorialLevelController).m_lastNearest;
                if (guard != null)
                {
                    hypnosis.Cast(guard);
                }
                else
                {
                    ChangePower(data.powerCost);
                    Globals.tipDisplay.Msg("no_hypnosis_target_nearby");
                }                
            }
            if (data.nameKey == "shotLight")
            {
                UnityEngine.GameObject soundPrefab = UnityEngine.Resources.Load("Misc/GunSound") as UnityEngine.GameObject;
                GuardAlertSound sound = (UnityEngine.GameObject.Instantiate(soundPrefab) as UnityEngine.GameObject).GetComponent<GuardAlertSound>();
                sound.transform.position = transform.position;
                sound.StartAlert();                
            }
        }        
    }

    public override bool GoTo(UnityEngine.Vector3 pos, OnPathDelegate callback = null)
    {
        System.String content = "Mage go to:";
        content += pos.ToString("F3");
        Globals.record("testReplay", content);

        if(currentAction == flyUp)
        {
            flyUp.destination = pos;
        }
        else
        {
            moving.canSearch = false;
            moving.GetSeeker().StartPath(moving.GetFeetPosition(), pos, OnPathComplete);
        }        
        return true;
    }

    public void ShotLight(UnityEngine.GameObject bulb)
    {
        if (Globals.magician.shot.data.IsInUse()&& 
            ChangePower(-shot.data.powerCost))
        {
            shot.Shot(bulb);
        }
    }

    public void FingerDown(Finger finger)
    {
        return;
        if (currentAction == null && incant.dove == null)
        {
            incant.FingerDown(finger);
        }
    }

    public void FingerMoving(Finger finger)
    {
        if (currentAction == incant)
        {
            incant.FingerMoving(finger);
        }        
    }

    public void FingerUp(Finger finger)
    {
        if (currentAction == incant)
        {
            incant.FingerUp(finger);
        }       
    }

    public bool ChangePower(int delta)
    {
        int powerTemp = PowerCurrent;
        powerTemp += delta;
        if (powerTemp < 0)
        {
            Globals.tipDisplay.Msg("not_enough_power");
            return false;
        }
        else
        {            
            PowerCurrent = powerTemp;
            Globals.canvasForMagician.PowerNumber.UpdateCurrentLife(PowerCurrent, PowerAmount + Globals.thiefPlayer.GetPowerDelta());
            return true;
        }
    }


    public override void ChangeLife(int delta)
    {
        base.ChangeLife(delta);
        Globals.canvasForMagician.lifeNumber.UpdateCurrentLife(LifeCurrent, LifeAmount);
    }

    public void ResetLifeAndPower(PlayerInfo player)
    {
        LifeCurrent = LifeAmount;        
        Globals.canvasForMagician.lifeNumber.UpdateText(LifeCurrent, LifeAmount);
        ResetPower(player);
    }

    public void ResetPower(PlayerInfo player)
    {
        PowerCurrent = PowerAmount + player.GetPowerDelta();
        Globals.canvasForMagician.PowerNumber.UpdateText(PowerCurrent, PowerAmount + player.GetPowerDelta());
    }

    public void OnDestroy()
    {
        UnityEngine.Debug.Log("Magician Destroyed");
    }
}
