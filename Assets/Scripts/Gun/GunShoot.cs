using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunShoot : MonoBehaviour
{
    public KeyCode TriggerKey;
    public bool CanFire = true;
    public Camera playerCamera;
    public GameObject impactEffect; 
    public bool isShooting = false;
    public float shootDamage = 10f;
    public float range = 100f; 
    public Color rayColor = Color.red; 

    void Start()
    {
        if (TriggerKey == KeyCode.None) TriggerKey = KeyCode.Mouse0; 
    }

    void Update()
    {
        if (Input.GetKeyUp(TriggerKey) && CanFire)
            Fire(); // Effectuer le tir
    }

    void Fire()
    {
        isShooting = true;
        RaycastHit hit;
        Vector3 startPosition = playerCamera.transform.position;
        Vector3 direction = playerCamera.transform.forward;

        // Dessiner le rayon de tir pour le débogage
        Debug.DrawRay(startPosition, direction * range, rayColor, 2f);

        if (Physics.Raycast(startPosition, direction, out hit, range))
        {
            if (hit.transform.TryGetComponent<EnemyDamage>(out var enemyDamage))
            {
                enemyDamage.Takedamage(shootDamage); 
            }

            // Vérifier si l'effet d'impact est assigné
            if (impactEffect != null)
            {
              Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
            }
            else
            {
                Debug.LogWarning("impactEffect n'est pas assigné !");
            }
        }
    }
}
