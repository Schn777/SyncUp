using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    public float MaxHealth = 50f;
    public float CurrentHealth;
    private Animator _enemyAnimator;
    public float deathAnimationDuration = 2f;

    private void Start()
    {
        CurrentHealth = MaxHealth;

        _enemyAnimator = GetComponentInChildren<Animator>();
        if (_enemyAnimator == null)
        {
            Debug.LogError($"Animator non trouvé pour l'objet {gameObject.name}");
        }
    }

    public void Takedamage(float damage)
    {
        CurrentHealth -= damage;
        if (CurrentHealth < 1)
        {
            Die();
        }
    }

    private void Die()
    {
        if (_enemyAnimator != null)
        {
            _enemyAnimator.SetTrigger("isDying");
        }
        
        Destroy(gameObject, deathAnimationDuration);
    }
}
