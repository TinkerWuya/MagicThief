//#define ASTARDEBUG
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;

/** AI for following paths.
 * This AI is the default movement script which comes with the A* Pathfinding Project.
 * It is in no way required by the rest of the system, so feel free to write your own. But I hope this script will make it easier
 * to set up movement for the characters in your game. This script is not written for high performance, so I do not recommend using it for large groups of units.
 * \n
 * \n
 * This script will try to follow a target transform, in regular intervals, the path to that target will be recalculated.
 * It will on FixedUpdate try to move towards the next point in the path.
 * However it will only move in the forward direction, but it will rotate around it's Y-axis
 * to make it reach the target.
 * 
 * \section variables Quick overview of the variables
 * In the inspector in Unity, you will see a bunch of variables. You can view detailed information further down, but here's a quick overview.\n
 * The #repathRate determines how often it will search for new paths, if you have fast moving targets, you might want to set it to a lower value.\n
 * The #target variable is where the AI will try to move, it can be a point on the ground where the player has clicked in an RTS for example.
 * Or it can be the player object in a zombie game.\n
 * The speed is self-explanatory, so is turningSpeed, however #slowdownDistance might require some explanation.
 * It is the approximate distance from the target where the AI will start to slow down. Note that this doesn't only affect the end point of the path
 * but also any intermediate points, so be sure to set #forwardLook and #pickNextWaypointDist to a higher value than this.\n
 * #pickNextWaypointDist is simply determines within what range it will switch to target the next waypoint in the path.\n
 * #forwardLook will try to calculate an interpolated target point on the current segment in the path so that it has a distance of #forwardLook from the AI\n
 * Below is an image illustrating several variables as well as some internal ones, but which are relevant for understanding how it works.
 * Note that the #forwardLook range will not match up exactly with the target point practically, even though that's the goal.
 * \shadowimage{aipath_variables.png}
 * This script has many movement fallbacks.
 * If it finds a NavmeshController, it will use that, otherwise it will look for a character controller, then for a rigidbody and if it hasn't been able to find any
 * it will use Transform.Translate which is guaranteed to always work.
 */
[RequireComponent(typeof(Seeker))]
public class AIPath : MonoBehaviour {
	
	/** Determines how often it will search for new paths. 
	 * If you have fast moving targets or AIs, you might want to set it to a lower value.
	 * The value is in seconds between path requests.
	 */
	int repathRate = 30;
	
	/** Target to move towards.
	 * The AI will try to follow/move towards this target.
	 * It can be a point on the ground where the player has clicked in an RTS for example, or it can be the player object in a zombie game.
	 */
    public UnityEngine.Vector3 targetOffset;
	public Transform target;
	
	/** Enables or disables searching for paths.
	 * Setting this to false does not stop any active path requests from being calculated or stop it from continuing to follow the current path.
	 * \see #canMove
	 */
	public bool canSearch = true;
	
	/** Enables or disables movement.
	  * \see #canSearch */
	public bool canMove = true;
	
	/** Maximum velocity.
	 * This is the maximum speed in world units per second.
	 */
	public double speed = 3;
	
	/** Rotation speed.
	 * Rotation is calculated using Quaternion.SLerp. This variable represents the damping, the higher, the faster it will be able to rotate.
	 */
    [UnityEngine.HideInInspector]
	public double turningSpeed = 5;
	
	/** Distance from the target point where the AI will start to slow down.
	 * Note that this doesn't only affect the end point of the path
 	 * but also any intermediate points, so be sure to set #forwardLook and #pickNextWaypointDist to a higher value than this
 	 */
    [UnityEngine.HideInInspector]
	public double slowdownDistance = 0.6F;
	
	/** Determines within what range it will switch to target the next waypoint in the path */
	public double pickNextWaypointDist = 2;
	
	/** Target point is Interpolated on the current segment in the path so that it has a distance of #forwardLook from the AI.
	  * See the detailed description of AIPath for an illustrative image */
	public double forwardLook = 1;
	
	/** Distance to the end point to consider the end of path to be reached.
	 * When this has been reached, the AI will not move anymore until the target changes and OnTargetReached will be called.
	 */
	public double endReachedDistance = 0.2F;
	
	/** Do a closest point on path check when receiving path callback.
	 * Usually the AI has moved a bit between requesting the path, and getting it back, and there is usually a small gap between the AI
	 * and the closest node.
	 * If this option is enabled, it will simulate, when the path callback is received, movement between the closest node and the current
	 * AI position. This helps to reduce the moments when the AI just get a new path back, and thinks it ought to move backwards to the start of the new path
	 * even though it really should just proceed forward.
	 */
	public bool closestOnPathCheck = true;
	
	protected double minMoveScale = 0.05F;
	
	/** Cached Seeker component */
	protected Seeker seeker;
	
	/** Cached Transform component */
	protected Transform tr;
	
	/** Time when the last path request was sent */
	private int lastRepath = -9999;
	
	/** Current path which is followed */
	protected Path path;
	
	/** Cached CharacterController component */
	protected CharacterController controller;
	
	/** Cached Rigidbody component */
	protected Rigidbody rigid;
	
	/** Current index in the path which is current target */
	protected int waypointIndex = 0;
	
	/** Holds if the end-of-path is reached
	 * \see TargetReached */
	protected bool targetReached = false;
	
	/** Only when the previous path has been returned should be search for a new path */
	protected bool canSearchAgain = true;
	
	/** Returns if the end-of-path has been reached
	 * \see targetReached */
	public bool TargetReached {
		get {
			return targetReached;
		}
	}
	
	/** Holds if the Start function has been run.
	 * Used to test if coroutines should be started in OnEnable to prevent calculating paths
	 * in the awake stage (or rather before start on frame 0).
	 */
	private bool startHasRun = false;
	
	/** Initializes reference variables.
	 * If you override this function you should in most cases call base.Awake () at the start of it.
	  * */
	protected virtual void Awake () {
		seeker = GetComponent<Seeker>();
		
		//This is a simple optimization, cache the transform component lookup
		tr = transform;
		
		//Make sure we receive callbacks when paths complete
		seeker.pathCallback += OnPathComplete;
		
		//Cache some other components (not all are necessarily there)
		controller = GetComponent<CharacterController>();
		rigid = GetComponent<Rigidbody>();
	}
	
	/** Starts searching for paths.
	 * If you override this function you should in most cases call base.Start () at the start of it.
	 * \see OnEnable
	 * \see RepeatTrySearchPath
	 */
	protected virtual void Start () {
		startHasRun = true;
		OnEnable ();
	}
	
	/** Run at start and when reenabled.
	 * Starts RepeatTrySearchPath.
	 * 
	 * \see Start
	 */
	protected virtual void OnEnable () {
		if (startHasRun) StartCoroutine (RepeatTrySearchPath ());
	}
	
	/** Tries to search for a path every #repathRate seconds.
	  * \see TrySearchPath
	  */
	public IEnumerator RepeatTrySearchPath () {
		while (true) {
			TrySearchPath ();
            int counter = repathRate;
            while (counter != 0)
            {
                counter--;                
                yield return 0;
            }			
		}
	}
	
	/** Tries to search for a path.
	 * Will search for a new path if there was a sufficient time since the last repath and both
	 * #canSearchAgain and #canSearch are true.
	 * Otherwise will start WaitForPath function.
	 */
	public void TrySearchPath () {
		if (Time.frameCount - lastRepath >= repathRate && canSearchAgain && canSearch) {
			SearchPath ();
		} else {
			StartCoroutine (WaitForRepath ());
		}
	}
	
	/** Is WaitForRepath running */
	private bool waitingForRepath = false;
	
	/** Wait a short time til Time.time-lastRepath >= repathRate.
	  * Then call TrySearchPath
	  * 
	  * \see TrySearchPath
	  */
	protected IEnumerator WaitForRepath () {
		if (waitingForRepath) yield break; //A coroutine is already running
		
		waitingForRepath = true;
		//Wait until it is predicted that the AI should search for a path again
        if ((Time.frameCount - lastRepath) < repathRate)
        {
            yield return 0;
        }
        else
        {
            yield return 0;
        }
				
		waitingForRepath = false;
		//Try to search for a path again
		TrySearchPath ();
	}
	
	/** Requests a path to the target */
	public virtual void SearchPath (OnPathDelegate callback = null) {
		if (target == null) { Debug.LogWarning ("Target is null, aborting all search"); canSearch = false; return; }
		
		lastRepath = Time.frameCount;
		//This is where we should search to
        Vector3 targetPosition = target.position + targetOffset;
		
		canSearchAgain = false;
		
		//Alternative way of requesting the path
		//Path p = PathPool<Path>.GetPath().Setup(GetFeetPosition(),targetPoint,null);
		//seeker.StartPath (p);
		
		//We should search from the current position
        seeker.StartPath(GetFeetPosition(), targetPosition, callback);
	}
	
	public virtual void OnTargetReached () {
		//End of path has been reached
		//If you want custom logic for when the AI has reached it's destination
		//add it here
		//You can also create a new script which inherits from this one
		//and override the function in that script
	}
	
	public void OnDestroy () {
		if (path != null) path.Release (this);
	}
	
	/** Called when a requested path has finished calculation.
	  * A path is first requested by #SearchPath, it is then calculated, probably in the same or the next frame.
	  * Finally it is returned to the seeker which forwards it to this function.\n
	  */
	public virtual void OnPathComplete (Path _p) {
		ABPath p = _p as ABPath;
		if (p == null) throw new System.Exception ("This function only handles ABPaths, do not use special path types");
		
		//Release the previous path
		//if (path != null) path.Release (this);
		
		//Claim the new path
		p.Claim (this);
		
		//Replace the old path
		path = p;
		
		//Reset some variables
		waypointIndex = 0;
		targetReached = false;
		canSearchAgain = true;
		
		//The next row can be used to find out if the path could be found or not
		//If it couldn't (error == true), then a message has probably been logged to the console
		//however it can also be got using p.errorLog
		//if (p.error)
		
		if (closestOnPathCheck) {
			Vector3 p1 = p.startPoint;
			Vector3 p2 = GetFeetPosition ();
			double magn = Vector3.Distance (p1,p2);
			Vector3 dir = p2-p1;
			dir /= (float)magn;
			int steps = (int)(magn/pickNextWaypointDist);
			for (int i=0;i<steps;i++) {
				CalculateVelocity (p1,speed);
				p1 += dir;
			}
#if ASTARDEBUG
			Debug.DrawLine (p1,p2,Color.red,1);
#endif
		}
	}
	
	public virtual Vector3 GetFeetPosition () {		
		if (controller != null) {
			return tr.position - Vector3.up*controller.height*0.5F;
		}

		return tr.position;
	}
	
	public virtual void Update () {
		
		if (!canMove) { return; }
		
		Vector3 dir = CalculateVelocity (GetFeetPosition(),speed);
		
		//Rotate towards targetDirection (filled in by CalculateVelocity)
		if (targetDirection != Vector3.zero) {
			RotateTowards (targetDirection);
		}
	
		if (controller != null) {
			controller.SimpleMove (dir);
		} else if (rigid != null) {
			rigid.AddForce (dir);
		} else {
			transform.Translate (dir*Time.deltaTime, Space.World);
		}
	}
	
	/** Point to where the AI is heading.
	  * Filled in by #CalculateVelocity */
	protected Vector3 targetPoint;
	/** Relative direction to where the AI is heading.
	 * Filled in by #CalculateVelocity */
	protected Vector3 targetDirection;
	
	protected double XYSqrMagnitude (Vector3 a, Vector3 b) {
		double dx = b.x-a.x;
		double dy = b.y-a.y;
		return dx*dx + dy*dy;
	}
	
	/** Calculates desired velocity.
	 * Finds the target path segment and returns the forward direction, scaled with speed.
	 * A whole bunch of restrictions on the velocity is applied to make sure it doesn't overshoot, does not look too far ahead,
	 * and slows down when close to the target.
	 * /see speed
	 * /see endReachedDistance
	 * /see slowdownDistance
	 * /see CalculateTargetPoint
	 * /see targetPoint
	 * /see targetDirection
	 * /see currentWaypointIndex
	 */
	protected Vector3 CalculateVelocity (Vector3 currentPosition, double s) {
//         currentPosition = new UnityEngine.Vector3(
//             (float)System.Math.Round(currentPosition.x, 2),
//             (float)System.Math.Round(currentPosition.y, 2),
//             (float)System.Math.Round(currentPosition.z, 2));
        if (path == null || path.vectorPath == null || path.vectorPath.Count == 0)
        {
//             System.String content = gameObject.name;
//             content += " no path";
//             Globals.record("testReplay", content);
            return Vector3.zero; 
        }
		
		List<Vector3> vPath = path.vectorPath;
		//Vector3 currentPosition = GetFeetPosition();
		
		if (vPath.Count == 1) {
			vPath.Insert (0,currentPosition);
		}
		
		if (waypointIndex >= vPath.Count) 
        { 
            waypointIndex = vPath.Count-1; 
        }
		
		if (waypointIndex <= 1) 
            waypointIndex = 1;
		
		while (true) {
			if (waypointIndex < vPath.Count-1) {
				//There is a "next path segment"
				double dist = XYSqrMagnitude (vPath[waypointIndex], currentPosition);
					//Mathfx.DistancePointSegmentStrict (vPath[currentWaypointIndex+1],vPath[currentWaypointIndex+2],currentPosition);
				if (dist < pickNextWaypointDist*pickNextWaypointDist) {
					waypointIndex++;
				} else {
					break;
				}
			} else {
				break;
			}
		}

		
		Vector3 dir = vPath[waypointIndex] - vPath[waypointIndex-1];
		//Vector3 targetPosition = CalculateTargetPoint (currentPosition,vPath[waypointIndex-1] , vPath[waypointIndex]);
        Vector3 targetPosition = CalculateMovingDir.Calculate(currentPosition, vPath[waypointIndex - 1], vPath[waypointIndex], forwardLook);
//         targetPosition = new UnityEngine.Vector3(
//             (float)System.Math.Round(targetPosition.x, 2),
//             (float)System.Math.Round(targetPosition.y, 2),
//             (float)System.Math.Round(targetPosition.z, 2));
        
        
        dir = targetPosition-currentPosition;
        //dir = new Vector3(Globals.Floor(dir.x), Globals.Floor(dir.y), 0);
//         dir = new UnityEngine.Vector3(
//             (float)System.Math.Round(dir.x, 2),
//             (float)System.Math.Round(dir.y, 2),
//             0);
		dir.z = 0;

//         System.String record_content = gameObject.name;
//         record_content += " vPath " + (waypointIndex - 1).ToString() + ":" + vPath[waypointIndex - 1].ToString("F5");
//         record_content += "vPath " + waypointIndex.ToString() +":" + vPath[waypointIndex].ToString("F5");
//         record_content += " currentPosition:" + currentPosition.ToString("F5");
//         record_content += " targetPosition:" + targetPosition.ToString("F5");
//         record_content += " dir:" + dir.ToString("F5");
//         Globals.record("testReplay", record_content);

		double targetDist = dir.magnitude;
		
		this.targetDirection = dir;
		//this.targetPoint = targetPosition;
		
		if (targetDist <= endReachedDistance) 
        {
			if (!targetReached) 
            { 
                targetReached = true; 
                OnTargetReached (); 
            }

//             System.String content = gameObject.name;
//             content += " endReachedDistance:" + endReachedDistance.ToString("F3");
//             content += " dist:" + targetDist.ToString("F3");
//             content += " currentPosition:" + currentPosition.ToString("F3");
//             
//             Globals.record("testReplay", content);
			
			//Send a move request, this ensures gravity is applied
			return Vector3.zero;
		}        
		
#if ASTARDEBUG
		Debug.DrawLine (vPath[waypointIndex-1] , vPath[waypointIndex],Color.black);
		Debug.DrawLine (GetFeetPosition(),targetPosition,Color.red);
		Debug.DrawRay (targetPosition,Vector3.up, Color.red);
		Debug.DrawRay (GetFeetPosition(),dir,Color.yellow);
		Debug.DrawRay (GetFeetPosition(),forward*sp,Color.cyan);
#endif

        return dir.normalized * (float)s;
	}
	
	/** Rotates in the specified direction.
	 * Rotates around the Y-axis.
	 * \see turningSpeed
	 */
	protected virtual void RotateTowards (Vector3 dir) {
		Quaternion rot = tr.rotation;
        if (!dir.Equals(Vector3.zero))
        {
            Quaternion toTarget = Quaternion.LookRotation(dir);

            rot = Quaternion.Slerp(rot, toTarget, (float)turningSpeed * Time.fixedDeltaTime);
            Vector3 euler = rot.eulerAngles;
            euler.y = 90;
            euler.z = -90;
            rot = Quaternion.Euler(euler);
        }		
		
		tr.rotation = rot;
	}
	
	/** Calculates target point from the current line segment.
	 * \param p Current position
	 * \param a Line segment start
	 * \param b Line segment end
	 * The returned point will lie somewhere on the line segment.
	 * \see #forwardLook
	 * \todo This function uses .magnitude quite a lot, can it be optimized?
	 */    

	protected Vector3 CalculateTargetPoint (Vector3 p, Vector3 a, Vector3 b) {
		a.z = p.z;
		b.z = p.z;
		
		double magn = (a-b).magnitude;
		if (magn == 0) return a;
		
		double closest = Mathfx.Clamp01 (Mathfx.NearestPointFactor (a, b, p));
		Vector3 point = (b-a)*(float)closest + a;
		double distance = (point-p).magnitude;
		
		double lookAhead = Globals.Clamp(forwardLook - distance, 0, forwardLook);
		
		double offset = lookAhead / magn;
        offset = Globals.Clamp(offset + closest, 0, 1);
		return (b-a)*(float)offset + a;
	}

    public void ClearPath()
    {
        path = null;
    }
}
