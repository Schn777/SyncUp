using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Rigidbody _rigidBody;
    public bool PlayerCanMove = true;
    public bool PlayerIsDead = false;

    private PlayerCameraController playerCameraController;
    private JumpController jumpController;
    private CrouchController crouchController;
    private SprintController sprintController;
    private HeadBobController headBobController;
    private DashController dashController;
    private GunController gunController;
    private OverlayController overlayController;

    public event EventHandler OnDeath;

    private void Awake()
    {
        jumpController = GetComponent<JumpController>();
        crouchController = GetComponent<CrouchController>();
        sprintController = GetComponent<SprintController>();
        headBobController = GetComponent<HeadBobController>();
        playerCameraController = GetComponent<PlayerCameraController>();
        dashController = GetComponent<DashController>();
        gunController = GetComponent<GunController>();
        _rigidBody = GetComponent<Rigidbody>();
        overlayController = ComponentUtils.Find<OverlayController>("Overlays");
    }

    public void Die()
    {
        if (!PlayerIsDead)
        {
            OnDeath?.Invoke(this, EventArgs.Empty);
            Time.timeScale = 0f;
            PlayerCanMove = false;
            Cursor.lockState = CursorLockMode.None;
            PlayerIsDead = true;
            overlayController?.ChangeOverlay(OverlayType.DEATH);
        }
    }

    private void Update()
    {
        if (PlayerCanMove)
        {
            jumpController.UpdateIsGrounded();
            playerCameraController.UpdatePlayerCameraState();
            playerCameraController.UpdateZoomState();
            sprintController.UpdateSprintStates();
            jumpController.UpdateJumpState();
            crouchController.UpdateCrouchState();
            headBobController.UpdateHeadBobState();
            dashController.UpdateDashState();
            //gunController?.UpdateShootingState();
        }
    }

    private void FixedUpdate()
    {
        if (PlayerCanMove)
        {
            sprintController.UpdateSprintMovementState();
        }
    }

}