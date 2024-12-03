using System;
using UnityEngine;

public class GunController : MonoBehaviour
{

    ////should be gun transform
    //public Camera mainCamera; //priavte
    //public KeyCode TriggerKey;
    //public int MaxAmmunitionCount = 5;
    //public int CurrentAmmunitionCount;
    //public float ReloadDurationInSeconds = 3f;
    //public float ShootingCooldownInSeconds = 3f;
    //public event EventHandler BeforeFiring;
    //public event EventHandler AfterFiring; 
    //public bool CanFire = true;
    //public bool GunWasFired = false;

    //private Ray trajectoryRay;
    //public GameObject BaseBulletModelPrefab;
    //public GameObject MultiBulletsPrefab;

    //void Start()
    //{
    //    if (TriggerKey == KeyCode.None) TriggerKey = KeyCode.Mouse0;  
    //    mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    //    CurrentAmmunitionCount = MaxAmmunitionCount;
    //    BeforeFiring += (object sender, EventArgs e) =>
    //    {
    //        GunWasFired = true;
    //    };

    //    AfterFiring += (object sender, EventArgs e) => 
    //    {
    //        GunWasFired = false;
    //    };
    //}

    //private void UpdateBulletDirection()
    //{
    //    trajectoryRay = mainCamera.ScreenPointToRay(Input.mousePosition);
    //}

    //private void Fire()
    //{
    //    BeforeFiring?.Invoke(this, EventArgs.Empty);
    //    InstantiateSoundBullet();
    //    AfterFiring?.Invoke(this, EventArgs.Empty);
    //}

    //private void InstantiateSoundBullet()
    //{
    //    GameObject _gameObject = Instantiate(MultiBulletsPrefab, mainCamera.transform.position, Quaternion.Euler(Vector3.zero));
    //    SoundBullet soundBullet = _gameObject.GetComponent<SoundBullet>();
    //    soundBullet.BaseBulletModel = BaseBulletModelPrefab;
    //    soundBullet.Ray = trajectoryRay;
    //    soundBullet.InstantiateElements();
    //    soundBullet.StartTravelling();
    //}

    //public void UpdateShootingState()
    //{
    //    UpdateBulletDirection();
    //    if (Input.GetKeyUp(TriggerKey) && CanFire)
    //        Fire();
    //}
}
