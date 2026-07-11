using System.Collections;
using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private int clipSize = 10;
    [SerializeField] private int startingReserveAmmo = 40;
    [SerializeField] private float damage = 34f;
    [SerializeField] private float range = 80f;
    [SerializeField] private float fireCooldown = 0.18f;
    [SerializeField] private float reloadTime = 1.2f;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip shootClip;
    [SerializeField] private AudioClip reloadClip;

    private float nextFireTime;

    public int CurrentAmmo { get; private set; }
    public int ReserveAmmo { get; private set; }
    public int TotalAmmoSpent { get; private set; }
    public int LastReloadAmount { get; private set; }
    public int ClipSize => clipSize;
    public bool IsReloading { get; private set; }

    private void Awake()
    {
        CurrentAmmo = clipSize;
        ReserveAmmo = startingReserveAmmo;
    }

    private void Update()
    {
        if (Time.timeScale <= 0f) return;

        if (Input.GetKeyDown(KeyCode.R))
        {
            StartReload();
        }

        if (Input.GetMouseButton(0))
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        if (IsReloading || Time.time < nextFireTime) return;

        if (CurrentAmmo <= 0)
        {
            StartReload();
            return;
        }

        nextFireTime = Time.time + fireCooldown;
        CurrentAmmo--;
        TotalAmmoSpent++;
        if (audioSource != null && shootClip != null) audioSource.PlayOneShot(shootClip);

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            if (hit.collider.TryGetComponent(out EnemyHealth enemy))
            {
                enemy.TakeDamage(damage);
            }
        }
    }

    private void StartReload()
    {
        if (!IsReloading && CurrentAmmo < clipSize && ReserveAmmo > 0)
        {
            StartCoroutine(Reload());
        }
    }

    private IEnumerator Reload()
    {
        IsReloading = true;
        if (audioSource != null && reloadClip != null) audioSource.PlayOneShot(reloadClip);
        yield return new WaitForSeconds(reloadTime);

        int neededAmmo = clipSize - CurrentAmmo;
        int ammoToLoad = Mathf.Min(neededAmmo, ReserveAmmo);
        CurrentAmmo += ammoToLoad;
        ReserveAmmo -= ammoToLoad;
        LastReloadAmount = ammoToLoad;
        IsReloading = false;
    }
}
