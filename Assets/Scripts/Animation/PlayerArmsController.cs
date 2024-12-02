using UnityEngine;

public class PlayerArmsController : MonoBehaviour
{
    private SprintController sprintController;
    private GunController gunController;
    private JumpController jumpController;
    private Animator animator;

    void Start()
    {
        animator = GameObject.FindGameObjectWithTag("Arms").GetComponent<Animator>();
        sprintController  = GetComponent<SprintController>();
        jumpController = GetComponent<JumpController>();
        gunController = GetComponent<GunController>();
    }

    private void Actuate(bool actionState, string trigger)
    {
        if (actionState) animator.SetTrigger(trigger);
        else
        {
            animator.ResetTrigger(trigger);
            animator.SetTrigger("idle");
        }
    }

    void Update()
    {
        Actuate(sprintController.IsWalking, "walk");
        Actuate(gunController.GunWasFired, "shoot");
        Actuate(jumpController.HasJumped, "jump");
        Actuate(sprintController.IsSprinting, "jog");
        //Actuate Reloading
        //Actuate EmptyGun
        //Actuate ...
    }

}
