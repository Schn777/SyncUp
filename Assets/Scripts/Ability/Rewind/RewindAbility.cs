using System.Collections;
using System;
using UnityEngine;
using System.Linq;


/// <summary>
/// # OPTIMIZATIONS
/// - EVENTATTRIBUTE OBJECT?
/// - should use .AddForce() while inverting Vector3s?
/// - GOONCOOLDOWN...
/// - STOP AUDIOMANAGER CAPTURE (LASTLOUDESTSAMPLES SNAPSHOT)
/// - DURING REWIND, EXPAN FIELD OF VIEW
/// - HIGH CONTRAST ON REWIND
/// - BLACK TUNNEL VISION
/// - WIND TUNNEL
/// - CREATE TIMESNAPSHOT CLASS
/// 
/// - Will search for objects implementing IRewind
/// to then use UpdateRewindElements() every x seconds,
/// thereby adding previous elements (Vector3, Color, ...).
/// - WIll use Rewind of every object implementing IRewind
/// to use there custom implementation
/// 
/// </summary>
public class RewindAbility : Ability
{
    private IRewind[] rewindableObjects;
    private Rigidbody[] rewindableRigidbodies;
    [SerializeField]
    private int rewindDurationInFrames;
    private TimingController addElementsTimingController;
    [Range(0f, 2f)]
    public float TargetRewindIterationDelay = 0.5f;
    [Range(100, 360)]
    public float TargetRewindIterationFOV = 120f;
    private Vector2[] iterationDelays;
    private Vector2[] iterationFOVs;
    private Camera mainCamera;
    private SpotifyController spotifyController;

    public event EventHandler OnRewindStart;
    public event EventHandler OnRewindIteration;
    public event EventHandler OnRewindStop;
    public event EventHandler OnRewindElementsAddStart;
    public event EventHandler OnRewindElementsAddStop;

    public void Start()
    {
        spotifyController = SpotifyController.Instance;

        SetRewindDurationInFrames();
        PlayerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>(); //TEMP <===
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        OnRewindStart += BeforeRewind;
        OnRewindStop += AfterRewind;
        //OnRewindElementsAddStart += (object sender, EventArgs e) => lastTimeSnapshot = globalStates.ScaledTime;
        //OnRewindElementsAddStop += (object sender, EventArgs e) => { Debug.Log("Stop");  };
        addElementsTimingController = GetComponent<TimingController>();
        addElementsTimingController.OnTime += UpdateRewindElements;
    }

    public int GetRewindDurationInFrames()
    {
        return this.rewindDurationInFrames;
    }

    private void SetRewindDurationInFrames()
    {
        rewindDurationInFrames = (int)(Duration * 60f); //fpslock
    }

    private void BeforeRewind(object sender, EventArgs e)
    {
        SetRewindDurationInFrames();
        rewindableRigidbodies = FindObjectsOfType<Rigidbody>().ToArray();
        iterationDelays = ParabolicArray.GetArray(TargetRewindIterationDelay, rewindDurationInFrames);
        iterationFOVs = ParabolicArray.GetArray(TargetRewindIterationFOV, rewindDurationInFrames, mainCamera.fieldOfView);
        PrepareRigidBodies();
        PlayerController.PlayerCanMove = false; //enemy can move?
        IsLive = true;
    }

    private void AfterRewind(object sender, EventArgs e)
    {
        PlayerController.PlayerCanMove = true;
        IsLive = false;
        UnPrepareRigidBodies();
        GoOnCooldown();
    }

    /// <summary>
    /// FIND REWINDABLE OBJECTS AND ADD THEM TO THE LIST
    /// - SHOULD BE ONLY WHEN A NEW IREWIND OBJECT IS CREATED
    /// </summary>
    private void UpdateRewindableObjects()
    {
        if (!IsLive)
            rewindableObjects = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<IRewind>().ToArray();
    }


    /// <summary>
    /// USE THE CALLBACK DECLARED IN THE OBJECTS IMPLEMENTING IREWIND
    /// </summary>
    public void UpdateRewindElements(object sender, EventArgs e)
    {
        if (!IsLive)
        {
            OnRewindElementsAddStart?.Invoke(this, EventArgs.Empty);
            foreach (IRewind obj in rewindableObjects) obj?.UpdateRewindElements();
            OnRewindElementsAddStop?.Invoke(this, EventArgs.Empty);
        }
    }

    private void PrepareRigidBodies()
    {
        foreach (Rigidbody body in rewindableRigidbodies)
        {
            body.useGravity = false;
            body.velocity = Vector3.zero;
        }
    }

    private void UnPrepareRigidBodies()
    {
        foreach (Rigidbody body in rewindableRigidbodies)
            body.useGravity = true;
    }

    /// <summary>
    /// START REWINDING
    /// - AFTER IMAGE
    /// - SLOW - FAST - SLOW (SecondsBetweenRewindIteration)
    /// </summary>
    private IEnumerator Rewind()
    {
        OnRewindStart?.Invoke(this, EventArgs.Empty);
        int iterationIndex = 0;
        RewindResponse res;
        while (iterationIndex < rewindDurationInFrames)
        {
            OnRewindIteration?.Invoke(this, EventArgs.Empty);
            mainCamera.fieldOfView = iterationFOVs[iterationIndex].y;
            for (int elem = 0; elem < rewindableObjects.Length; elem++)
            {
                res = rewindableObjects[elem].Rewind();
                if (res.HasToStop && res.RewindingObject.CompareTag("Player"))
                {
                    iterationIndex = rewindDurationInFrames;
                    break;
                } 
            }
            if (iterationIndex < rewindDurationInFrames)
                yield return new WaitForSeconds(iterationDelays[iterationIndex++].y * Time.deltaTime); //* (1 * Time.deltaTime)
        }
        OnRewindStop?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// START REWINDING
    /// USE ALL OBJECTS IMPLEMENTING IREWIND
    /// </summary>
    private async void Update()
    {
        UpdateRewindableObjects();
        if (!IsLive && Input.GetKeyDown(TriggerKey) && !OnCooldown)
        {
            StartCoroutine(Rewind());


            if (spotifyController != null)
            {
                await spotifyController.Rewind(8);
            }
        }   
    }
}
