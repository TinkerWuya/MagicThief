using UnityEngine;
using System.Collections;

public delegate bool FingerEvent(object sender);

public class Finger
{
    static public string FINGER_STATE_NONE = "None";
    static public string FINGER_STATE_DOWN = "FD";
    static public string FINGER_STATE_HOLDING = "H";
    static public string FINGER_STATE_UP = "FU";

    static public string FINGER_STATE_FAST_MOVING = "FM";
    static public string FINGER_STATE_SLOW_MOVING = "SM";

    static public string FINGER_STATE_HOVERING = "HOVER";


    static public string GESTURE_NONE = "None";
    static public string GESTURE_CLICK = "C";
    static public string GESTURE_DOUBLE_CLICK = "DC";
    static public string GESTURE_R = "R";
    static public string GESTURE_L = "L";
    static public string GESTURE_D = "D";
    static public string GESTURE_U = "U";
    static public string GESTURE_TWO_FIGURE_CLICK = "2C";

    static public float HALF_PI = (float)Mathf.PI / 2.0f;
    static public float UP_GESTURE_RADIAN = 60.0f / 90.0f;


    public Finger(int fingerId, InputMgr mgr)
    {
        inputMgr = mgr;
        touchBegin = touchStaying = touchMoving = touchEnd = false;
        id = fingerId;
    }
    public InputMgr inputMgr;
    public bool enabled;
    private bool touchBegin;
    private bool touchStaying;
    private bool touchMoving;
    private bool touchEnd;
    private bool hovering;

    public Vector2 beginPosition;
    public Vector2 nowPosition;
    public Vector2 lastPosition;
    public Vector2 holdingPosition;

    public int id;


    // ����ָ���¿�ʼ�ۼƵ�ʱ��
    public float timeSinceTouchBegin;

    // ���ƶ�ʱ��ʼ�ۼƵ�ʱ��
    public float timeSinceMoving;

    // ���ƶ�ʱ��ʼ�ۼƵ�����
    private float movingLength;

    public string currentState = FINGER_STATE_NONE;
    public string currentGesture = Finger.GESTURE_NONE;

    public float timeLastClick = UnityEngine.Mathf.NegativeInfinity;

    public FingerEvent Evt_Down;
    public FingerEvent Evt_Staying;
    public FingerEvent Evt_Moving;
    public FingerEvent Evt_Up;
    public FingerEvent Evt_Hovering;
    public FingerEvent Evt_FastMoving;
    public FingerEvent Evt_SlowMoving;    

    public void Down(Vector2 pos)
    {
        if (IsPositionOnCanvas(pos))
        {
            return;
        }

        enabled = true;
        touchBegin = touchStaying = touchMoving = touchEnd = hovering = false;

        touchBegin = true;

        holdingPosition = beginPosition = nowPosition = lastPosition = pos;

        timeSinceTouchBegin = 0.0f;
        timeSinceMoving = UnityEngine.Mathf.Infinity;

        if (Evt_Down != null)
        {
            Evt_Down(this);
        }
    }

    public void Staying(Vector2 pos)
    {
        touchBegin = touchStaying = touchMoving = touchEnd = hovering = false;

        touchStaying = true;

        holdingPosition = lastPosition = nowPosition;

        timeSinceTouchBegin += Time.deltaTime;
        if (enabled)
        {
            if (Evt_Staying != null)
            {
                Evt_Staying(this);
            }
        }
    }

    public bool IsPositionOnCanvas(Vector3 position)
    {
        UnityEngine.Canvas[] canvases = GameObject.FindObjectsOfType<UnityEngine.Canvas>();
        foreach (UnityEngine.Canvas canvas in canvases)
        {
            UnityEngine.EventSystems.PointerEventData event_data = new UnityEngine.EventSystems.PointerEventData(GameObject.FindObjectOfType<UnityEngine.Canvas>().GetComponent<UnityEngine.EventSystems.EventSystem>());
            event_data.position = position;
            System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult> results = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
            canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>().Raycast(event_data, results);
            foreach(UnityEngine.EventSystems.RaycastResult result in results)
            {
                // 17 : UIWontBloackFinger
                UnityEngine.UI.Selectable selectable = result.gameObject.GetComponent<UnityEngine.UI.Selectable>();
                if (result.gameObject.layer != 17 && selectable != null)
                {
                    return true;
                }
            }            
        }
        return false;
    }

    public void Moving(Vector2 pos)
    {
        if (IsPositionOnCanvas(pos))
        {
            if (enabled)
            {
                Up(pos);
            }
            return;
        }
        else
        {
            if (!enabled)
            {
                Down(pos);
            }
        }

        touchBegin = touchStaying = touchMoving = touchEnd = hovering = false;

        touchMoving = true;

        lastPosition = nowPosition;
        nowPosition = pos;

        timeSinceMoving += Time.deltaTime;
        movingLength += (nowPosition - lastPosition).magnitude;
        if (enabled)
        {
            if (Evt_Moving != null)
            {
                Evt_Moving(this);
            }
        }
    }

    public Vector2 MovmentDelta()
    {
        return nowPosition - lastPosition;
    }

    public void Up(Vector2 pos)
    {

        touchBegin = touchStaying = touchMoving = touchEnd = hovering = false;

        touchEnd = true;

        lastPosition = nowPosition;
        nowPosition = pos;
        if (enabled)
        {
            if (Evt_Up != null)
            {
                Evt_Up(this);
            }
        }


        enabled = false;
    }

    public void Hovering()
    {
        touchBegin = touchStaying = touchMoving = touchEnd = hovering = false;

        hovering = true;

        if (Evt_Hovering != null)
        {
            Evt_Hovering(this);
        }
    }

    void _SetCurrentState(string state)
    {
        currentState = state;
    }

    public string ResolveState()
    {
        if (touchBegin)
        {
            _SetCurrentState(FINGER_STATE_DOWN);
        }
        else if (touchEnd)
        {
            _SetCurrentState(FINGER_STATE_UP);
        }
        else if (touchStaying)
        {
            _SetCurrentState(FINGER_STATE_HOLDING);
        }
        else if (touchMoving)
        {
            if (currentState == FINGER_STATE_HOLDING || currentState == FINGER_STATE_DOWN)
            {
                timeSinceMoving = UnityEngine.Mathf.Epsilon;
                movingLength = (nowPosition - lastPosition).magnitude;
            }
            float speed = movingLength / timeSinceMoving;
            if (speed > inputMgr.movingSpeedThreshold)
            {
                _SetCurrentState(FINGER_STATE_FAST_MOVING);

                if (enabled && Evt_FastMoving != null)
                {
                    Evt_FastMoving(this);
                }
            }
            else if (speed < inputMgr.movingSpeedThreshold)
            {
                _SetCurrentState(FINGER_STATE_SLOW_MOVING);

                if (enabled && Evt_SlowMoving != null)
                {
                    Evt_SlowMoving(this);
                }
            }
        }
        else if (hovering)
        {
            _SetCurrentState(FINGER_STATE_HOVERING);
        }        

        return currentState;
    }

    public bool IsHovering()
    {
        return currentState == FINGER_STATE_HOVERING;
    }

    public bool IsDown()
    {
        return currentState == FINGER_STATE_DOWN;
    }

    public bool IsUp()
    {
        return currentState == FINGER_STATE_UP;
    }

    public bool IsFastMoving()
    {
        return currentState == FINGER_STATE_FAST_MOVING;
    }

    public bool IsSlowMoving()
    {
        return currentState == FINGER_STATE_SLOW_MOVING;
    }

    public bool IsMoving()
    {
        return IsSlowMoving() || IsFastMoving();
    }

    public string ResolveSingleFingerGesture()
    {
        currentGesture = Finger.GESTURE_NONE;
        if (currentState == Finger.FINGER_STATE_FAST_MOVING)
        {
            Vector2 line = holdingPosition - nowPosition;
            line.Normalize();
            float rad = Mathf.Abs(line.y / line.x);
            if (rad > Finger.UP_GESTURE_RADIAN)
            {
                if (line.y > 0)
                {
                    currentGesture = Finger.GESTURE_D;

                }
                else
                {
                    currentGesture = Finger.GESTURE_U;
                }
            }
            else
            {
                if (line.x < 0)
                {
                    currentGesture = Finger.GESTURE_R;
                }
                else
                {
                    currentGesture = Finger.GESTURE_L;
                }
            }
        }

        return currentGesture;
    }
}

public delegate bool KeyEvent(string key);
public delegate bool MouseEvent(UnityEngine.Vector2 pos);

public class InputMgr : MonoBehaviour
{
    public ArrayList fingers = new ArrayList();

    bool bBlock = false;

    // ��ֵ
    public float movingSpeedThreshold = 600.0f;

    public KeyEvent Evt_WASD;
    public KeyEvent Evt_WASD_Up;
    public KeyEvent Evt_SpaceDown;
    
    public KeyEvent Evt_KeyQ;
    public KeyEvent Evt_KeyW;
    public KeyEvent Evt_KeyE;
    public KeyEvent Evt_KeyDownR;
    public KeyEvent Evt_KeyUpR;
    public MouseEvent Evt_MouseLeftDown;
    public MouseEvent Evt_MouseRightDown;
    public MouseEvent Evt_MouseLeftUp;
    public MouseEvent Evt_MouseRightUp;

    float cameraPinchZoomingSpeed = 0.002f;

    void Awake()
    {        
        DontDestroyOnLoad(this);
        Globals.input = this;
        UnityEngine.Application.targetFrameRate = 30;
        for (int idx = 0; idx < 3; ++idx)
        {
            Finger finger = new Finger(idx, this);
            fingers.Add(finger);
        }
        UnityEngine.Screen.SetResolution(960, 600, false, 30);        
    }    

    void OnLevelWasLoaded(int scene_id)
    {
//         foreach (Finger f in fingers)
//         {
//             f.Evt_Down = null;
//             f.Evt_FastMoving = null;
//             f.Evt_Hovering = null;
//             f.Evt_SlowMoving = null;
//             f.Evt_Moving = null;
//             f.Evt_Staying = null;
//             f.Evt_Up = null;
//         }
    }

    bool GetTouchByID(int id)
    {
        for (int idx = 0; idx < Input.touches.Length; ++idx)
        {
            if (Input.touches[idx].fingerId == id)
            {
                //m_touch = Input.touches[idx];
                return true;
            }
        }

        return false;
    }

    public Finger GetFingerByID(int idx)
    {
        return (Finger)fingers[idx];
    }

    public void Block()
    {
        bBlock = true;
    }

    public void Rewind()
    {
        bBlock = false;
    }

    public void FrameFunc()
    {
        //UnityEngine.Physics2D.Raycast();

        // �ֱ�����iphone��PC��ѯ����
        UnityEngine.Cursor.lockState = UnityEngine.CursorLockMode.Confined;
        if (!bBlock)
        {
#if UNITY_IPHONE
			for (int idx = 0; idx < 3; ++idx)
			{
				Finger finger = GetFingerByID(idx);

				if (GetTouchByID(idx))
				{
					if (m_touch.phase == TouchPhase.Began)
					{
						finger.Down(m_touch.position);
					}
					else if (m_touch.phase == TouchPhase.Canceled || m_touch.phase == TouchPhase.Ended)
					{
						finger.Up(m_touch.position);
					}
					else
					{
						Vector2 pos = new Vector2(m_touch.position.x, m_touch.position.y);
						if (pos == finger.nowPosition)
						{
							finger.Staying(m_touch.position);
						}
						else
						{
							finger.Moving(m_touch.position);
						}
					}
				}
				else
				{
					finger.Hovering();
				}

				finger.ResolveState();
			}
#else            
            for (int idx = 0; idx < 3; ++idx)
            {
                Finger finger = GetFingerByID(idx);
                Vector2 mousePos = Input.mousePosition;

                string fingerName = "Finger" + idx.ToString();
                if (Input.GetButtonDown(fingerName) || Input.GetButtonUp(fingerName) || Input.GetButton(fingerName))
                {
                    if (Input.GetButtonDown(fingerName))
                    {
                        finger.Down(mousePos);
                    }
                    else if (Input.GetButtonUp(fingerName))
                    {
                        finger.Up(mousePos);
                    }
                    else if (Input.GetButton(fingerName))
                    {
                        Vector2 pos = new Vector2(mousePos.x, mousePos.y);
                        if (pos == finger.nowPosition)
                        {
                            finger.Staying(mousePos);
                        }
                        else
                        {
                            finger.Moving(mousePos);
                        }
                    }
                }
                else
                {
                    finger.Hovering();
                }

                finger.ResolveState();
            }

            if (Input.GetKeyUp(KeyCode.Q))
            {
                if (Evt_KeyQ != null)
                {
                    Evt_KeyQ("Q");
                }
            }

            if (Input.GetKeyUp(KeyCode.W))
            {
                if (Evt_KeyW != null)
                {
                    Evt_KeyW("W");
                }
            }

            if (Input.GetKeyUp(KeyCode.E))
            {
                if (Evt_KeyE != null)
                {
                    Evt_KeyE("E");
                }
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                if (Evt_KeyDownR != null)
                {
                    Evt_KeyDownR("R");
                }
            }

            if (Input.GetKeyUp(KeyCode.R))
            {
                if (Evt_KeyUpR != null)
                {
                    Evt_KeyUpR("R");
                }
            }


            if (Input.GetButtonDown("Horizontal") || Input.GetButton("Horizontal"))
            {
                if (Evt_WASD != null)
                {
                    Evt_WASD("WASD");
                }
            }

            if (Input.GetButtonDown("Vertical") || Input.GetButton("Vertical"))
            {
                if (Evt_WASD != null)
                {
                    Evt_WASD("");
                }
            }

            if (Input.GetButtonUp("Horizontal") || Input.GetButtonUp("Vertical"))
            {
                if (!Input.GetButton("Horizontal") && !Input.GetButton("Vertical"))
                {
                    if (Evt_WASD_Up != null)
                    {
                        Evt_WASD_Up("");
                    }
                }
            }

            // ����
            if (Input.GetMouseButtonDown(0))
            {
                if (Evt_MouseLeftDown != null)
                {
                    Evt_MouseLeftDown(Input.mousePosition);
                }
            }

            if (Input.GetMouseButtonDown(1))
            {
                if (Evt_MouseRightDown != null)
                {
                    Evt_MouseRightDown(Input.mousePosition);
                }

            }
            if (Input.GetMouseButtonUp(0))
            {
                if (Evt_MouseLeftUp != null)
                {
                    Evt_MouseLeftUp(Input.mousePosition);
                }
            }

            if (Input.GetMouseButtonUp(1))
            {
                if (Evt_MouseRightUp != null)
                {
                    Evt_MouseRightUp(Input.mousePosition);
                }
            }
#endif
			Finger finger0 = GetFingerByID(0);
			Finger finger1 = GetFingerByID(1);
			if (finger0.enabled && finger1.enabled && (finger0.IsMoving()||finger1.IsMoving()))
			{
				// Find the magnitude of the vector (the distance) between the touches in each frame.
				float prevTouchDeltaMag = (finger0.lastPosition - finger1.lastPosition).magnitude;
				float touchDeltaMag = (finger0.nowPosition - finger1.nowPosition).magnitude;
				
				// Find the difference in the distances between each frame.
				float deltaMagnitudeDiff = touchDeltaMag - prevTouchDeltaMag;
				Globals.cameraFollowMagician.SetDisScale(-deltaMagnitudeDiff * cameraPinchZoomingSpeed);
			}
		}        
	}	
}

