using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthController : MonoBehaviour
{
    public enum HealthAlterationTypes
    {
        HEAL,
        DAMAGE,
        REGENERATE
    }

    public float MaxHealth = 500f;
    public float CurrentHealth;

    public bool CanRegainHealth = true;
    public bool IsInvulnerable = false;
    public bool IsRegenerating = false; //for later

    public float HealthAlterationSpeed = 1f;
    public float HealthAlterationRate = 0.5f;

    public Slider HealthSlider;
    public bool UseHealthSlider = true;

    private PlayerController playerController;

    public event EventHandler OnZero;

    void Start()
    {
        playerController = GetComponent<PlayerController>();
        CurrentHealth = MaxHealth;
        if (HealthSlider == null ) UseHealthSlider = false;

        OnZero += (object sender, EventArgs e) =>
        {
                playerController.Die();
        };
    }

    private IEnumerator AlterateHealth(float by, HealthAlterationTypes alterationType)
    {
       for (int elem = 0; elem < by; elem++)
       {
            switch (alterationType)
            {
                case HealthAlterationTypes.HEAL:
                    if (CurrentHealth < MaxHealth) CurrentHealth += HealthAlterationRate;
                    UpdateHealthSlider();
                    break;
                case HealthAlterationTypes.DAMAGE:
                    if (CurrentHealth > 0) CurrentHealth -= HealthAlterationRate;
                    UpdateHealthSlider();
                    break;
            }
            yield return new WaitForSeconds(HealthAlterationSpeed * Time.deltaTime);
       }
        
    }

    public void UpdateHealthSlider()
    {
        if (UseHealthSlider) HealthSlider.value = CurrentHealth / MaxHealth;
    }

    public void ReduceHealth(float by)
    {
        StartCoroutine(AlterateHealth(by, HealthAlterationTypes.DAMAGE));
    }

    public void RegainHealth(float by)
    {
        StartCoroutine(AlterateHealth(by, HealthAlterationTypes.HEAL));
    }

    public void AlterateHealthInstantly(float value)
    {
        CurrentHealth = value;
        UpdateHealthSlider();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
            RegainHealth(100);
        else if (Input.GetKeyDown(KeyCode.J))
            ReduceHealth(100);

        if (CurrentHealth <= 0f) OnZero?.Invoke(this, EventArgs.Empty);
    }

}
