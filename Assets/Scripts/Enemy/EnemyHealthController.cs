using System;
using System.Collections;
using UnityEngine;

public class EnemyHealthController : MonoBehaviour
{
    public float MaxHealth = 500f;
    public float CurrentHealth;

    public event EventHandler OnDamageTaken;
    public event EventHandler OnZero;

    private Animator _enemyAnimator;

    //private GlobalStates globalStates = GlobalStates.Instance;

    private void Start()
    {
        CurrentHealth = MaxHealth;
        _enemyAnimator = GetComponentInChildren<Animator>();
        OnZero += (object sender, EventArgs e) => StartCoroutine(Die());
    }

    private IEnumerator Die()
    {
        _enemyAnimator.SetTrigger("isDying");
        for (int elem = 0; elem < 120; elem++) yield return new WaitForEndOfFrame();
        Destroy(gameObject);
    }

    /// <summary>
    /// Value between 0f and 1f
    /// Current health - (percentage * MaxHealth)
    /// </summary>
    public void AlterateHealthInstantly(float percentage)
    {
        CurrentHealth -= MaxHealth * percentage;
        OnDamageTaken?.Invoke(this, EventArgs.Empty);
        if (CurrentHealth <= 0f) OnZero?.Invoke(this, EventArgs.Empty);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
            AlterateHealthInstantly(0.25f);
    }

}
