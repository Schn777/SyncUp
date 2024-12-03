using System;
using System.Collections;
using UnityEngine;

public class DashController : MonoBehaviour
{

    //public float DashSpeed = 5f;
    //public int DashDurationInFrames = 30;
    //public float DashCooldown = 1f;
    //public bool CurrentlyDashing = false;
    //public bool ShowDebugDashDirectionLine = false;
    //public event EventHandler OnDashIteration;
    //public event EventHandler OnDashStop;

    public enum DashTypes
    {
        RESTRICTED,
        OMNI_DIRECTIONAL
    }

    public float DashForce = 15f;
    public bool CanDash = true;
    public bool OnCooldown = false;
    public event EventHandler OnDashStart;
    public KeyCode TriggerKey;
    public DashTypes DashType;
    public ParticleSystem dashForward;
    public ParticleSystem dashBackward;
    public ParticleSystem dashRightSide;
    public ParticleSystem dashLeftSide;
    private PlayerController firstPersonController;
    private Vector3 playerVelocity;
    private SpotifyController spotifyController;


    private void Start()
    {
        spotifyController = SpotifyController.Instance;

        if (TriggerKey == KeyCode.None) TriggerKey = KeyCode.C;
        firstPersonController = GetComponent<PlayerController>();
        OnDashStart += (object sender, EventArgs e) =>
        {
            playerVelocity = firstPersonController._rigidBody.velocity;
        };
    }

    public async void UpdateDashState()
    {
        if (Input.GetKeyDown(TriggerKey) && CanDash && !OnCooldown)
        {
            OnDashStart?.Invoke(this, EventArgs.Empty);
            switch (DashType)
            {
                case DashTypes.RESTRICTED:
                    firstPersonController._rigidBody.AddForce(new Vector3(playerVelocity.x, 0, playerVelocity.z * DashForce), ForceMode.VelocityChange);
                    Debug.Log(playerVelocity.z);
                    if(playerVelocity.z !=0)
                        (playerVelocity.z > 0 ? dashForward : dashBackward).Play();
                    break;
                case DashTypes.OMNI_DIRECTIONAL:
                    firstPersonController._rigidBody.AddForce(playerVelocity * DashForce, ForceMode.VelocityChange);
                    break;
            }

            if (spotifyController != null)
            {
                await spotifyController.FastForward(8);
            }
            
        }
    }

    private void Update()
    {
        //Debug.Log(firstPersonController._rigidBody.velocity);
    }

}
