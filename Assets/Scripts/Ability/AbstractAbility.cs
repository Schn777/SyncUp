using System;
using UnityEngine;
using UnityEngine.UI;

public abstract class Ability : MonoBehaviour
{

    [Range(0f, 15f)]
    public float CooldownInSeconds = 3f;
    public float CooldownCountdown = 0f;
    public bool CooldownEnabled = true;
    public bool OnCooldown = false;
    public float Duration = 0.8f;

    public string Name; 
    public Image Icon; 
    public bool IsLive = false;
    public int CreditsNeeded = 0; 
    public KeyCode TriggerKey;
    protected PlayerController PlayerController;

    public event EventHandler OnCooldownEnter;
    public event EventHandler OnCooldownLeave;

    public void GoOnCooldown()
    {
        OnCooldownEnter?.Invoke(this, EventArgs.Empty);
        OnCooldown = true; //true every frame?
        StartCoroutine(TimingController.Time(TimeType.SCALEDTIME, CooldownInSeconds, ExitOnCooldown));
    }

    public void ExitOnCooldown()
    {
        OnCooldownLeave?.Invoke(this, EventArgs.Empty);
        OnCooldown = false;
    }


}
