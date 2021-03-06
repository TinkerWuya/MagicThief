﻿using System.Collections;

public class Guard : Actor 
{
    public Cell birthCell;
    public GuardMoving moving;
    public Patrol patrol;
    public Chase chase;
    public Spot spot;
    public GuardAttack atk;
    public WanderingLostTarget wandering;
    public BackToBirthCell backing;
    public UnityEngine.Canvas canvasForCommandBtns;
    public BeginPatrolBtn beginPatrolBtn;
    public TakeGuardBack takeGuardBackBtn;
    public override void Awake()
    {
        moving = GetComponent<GuardMoving>();
        patrol = GetComponent<Patrol>();
        chase = GetComponent<Chase>();
        spot = GetComponent<Spot>();
        atk = GetComponent<GuardAttack>();
        wandering = GetComponent<WanderingLostTarget>();
        backing = GetComponent<BackToBirthCell>();

        UnityEngine.GameObject prefab = UnityEngine.Resources.Load("Avatar/CanvasOnGuard") as UnityEngine.GameObject;
        UnityEngine.GameObject obj = UnityEngine.GameObject.Instantiate(prefab) as UnityEngine.GameObject;
        canvasForCommandBtns = obj.GetComponent<UnityEngine.Canvas>();
        canvasForCommandBtns.worldCamera = Globals.cameraForDefender.camera;
        beginPatrolBtn = obj.GetComponentInChildren<BeginPatrolBtn>();
        beginPatrolBtn.guard = this;
        beginPatrolBtn.patrol = patrol;

        // 最开始不允许收回，因为是直接由SelectGuardUI替换的
        takeGuardBackBtn = obj.GetComponentInChildren<TakeGuardBack>();
        takeGuardBackBtn.gameObject.SetActive(false);
        takeGuardBackBtn.guard = this;
        base.Awake();
    }

    public void Choosen()
    {
        UnityEngine.Debug.Log("Choosen");
        ShowBtns();
        Tint();
        patrol.SetRouteNodesVisible(true);
        if (currentAction != null)
        {
            currentAction.Stop();
        }        
        Globals.choosenGuard = this;
    }

    public void Unchoose()
    {
        UnityEngine.Debug.Log("unchoose");
        StopTint();
        HideBtns();
        patrol.RouteConfirmed();
        patrol.SetRouteNodesVisible(false);
        patrol.Excute();
        Globals.choosenGuard = null;
    }

    public override void Start()
    {        
        base.Start();
    }

    public bool isShownBtns = true;
    public void ShowBtns()
    {
        if (!isShownBtns)
        {
            StartCoroutine(_scaleCanvasOut());
            isShownBtns = true;
        }        
    }

    public void HideBtns()
    {
        StopCoroutine(_scaleCanvasOut());
        isShownBtns = false;
        canvasForCommandBtns.transform.localScale = UnityEngine.Vector3.zero;
    }

    float currentScaleTime = 1.0f;
    float scaleCanvasForCommandTime = 0.2f;
    IEnumerator _scaleCanvasOut()
    {
        float scale = 0.0f;
        currentScaleTime = 0.0f;
        while (scale < 1.0f)
        {
            currentScaleTime = currentScaleTime + UnityEngine.Time.deltaTime;
            scale = currentScaleTime / scaleCanvasForCommandTime;
            canvasForCommandBtns.transform.localScale = new UnityEngine.Vector3(scale, scale, scale);

            yield return null;
        }
        yield return null;
    }
//     IEnumerator _scaleCanvasToZero()
//     {
//         float scale = 1.0f;
//         currentScaleTime = scaleCanvasForCommandTime;
//         while (scale > 0.0f)
//         {
//             currentScaleTime = currentScaleTime - UnityEngine.Time.deltaTime;
//             scale = currentScaleTime / scaleCanvasForCommandTime;
//             canvasForCommandBtns.transform.localScale = new UnityEngine.Vector3(scale, scale, scale);
//          
//             yield return null;
//         }        
//         yield return null;
//     }

    

    public void OnTargetReached()
    {
        if (currentAction == patrol)
        {
            patrol.NextPatrol();
        }
        else if (currentAction == backing)
        {
            patrol.Excute();
        }
        else if(currentAction == chase)
        {
            atk.Excute();
        }      
    }

    public void FaceTarget(UnityEngine.Transform target)
    {
        UnityEngine.Vector3 horDir = GetDirToTarget(target);
        transform.forward = horDir;
    }

    public UnityEngine.Vector3 GetDirToTarget(UnityEngine.Transform target)
    {
        UnityEngine.Vector3 horDir = target.transform.position - transform.position;
        horDir.y = 0;
        return horDir;
    }

    bool tinting;
    public void Tint()
    {
        tinting = true;
        StartCoroutine(_tint());
    }

    public void StopTint()
    {
        tinting = false;        
    }

    float tintCurrentTime = 1.0f;
    float tintFadeTime = 0.3f;
    IEnumerator _tint()
    {
        UnityEngine.Color color = UnityEngine.Color.white;
        tintCurrentTime = tintFadeTime;
        while (color.r > 0.3f)
        {
            tintCurrentTime = tintCurrentTime - UnityEngine.Time.deltaTime;
            color.r = tintCurrentTime / tintFadeTime;
            color.g = tintCurrentTime / tintFadeTime;
            color.b = tintCurrentTime / tintFadeTime;
            SetColor(color);
            yield return null;
        }

        if (tinting)
        {
            yield return StartCoroutine(_tintBack());
        }
        else
        {
            SetColor(UnityEngine.Color.white);
            yield return null;
        }        
    }


    IEnumerator _tintBack()
    {
        UnityEngine.Color color = UnityEngine.Color.black;
        tintCurrentTime = color.r;
        while (color.r < 1.0f)
        {
            tintCurrentTime = tintCurrentTime + UnityEngine.Time.deltaTime;
            color.r = tintCurrentTime / tintFadeTime;
            color.g = tintCurrentTime / tintFadeTime;
            color.b = tintCurrentTime / tintFadeTime;
            SetColor(color);
            yield return null;
        }

        if (tinting)
        {
            yield return StartCoroutine(_tint());
        }
        else
        {
            SetColor(UnityEngine.Color.white);
            yield return null;
        }        
    }

    void SetColor(UnityEngine.Color color)
    {
        for (int idx = 0; idx < meshRenderers.Length; ++idx)
        {
            meshRenderers[idx].material.SetColor("_Color", color);
        }

        for (int idx = 0; idx < skinnedMeshRenderers.Length; ++idx)
        {
            skinnedMeshRenderers[idx].material.SetColor("_Color", color);
        }
    }


	// Update is called once per frame
	void Update () {
	    canvasForCommandBtns.transform.position = transform.position + new UnityEngine.Vector3(0.0f, 1.0f, 0.0f);
	}
}
