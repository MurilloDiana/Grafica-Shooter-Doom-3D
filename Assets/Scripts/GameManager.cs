using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerWeapon playerWeapon;
    [SerializeField] private Text ammoText;
    [SerializeField] private Text enemiesText;
    [SerializeField] private Text healthText;
    [SerializeField] private Image healthFill;
    [SerializeField] private Text statusText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject victoryPanel;

    private readonly HashSet<EnemyController> enemies = new HashSet<EnemyController>();
    private bool exitReached;
    private bool matchEnded;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        UpdateHud();
    }

    private void Update()
    {
        UpdateHud();
    }

    public void RegisterEnemy(EnemyController enemy)
    {
        enemies.Add(enemy);
        UpdateHud();
    }

    public void EnemyDefeated(EnemyController enemy)
    {
        enemies.Remove(enemy);
        UpdateHud();
        TryWin();
    }

    public void ReachExit()
    {
        exitReached = true;
        if (enemies.Count > 0 && statusText != null)
        {
            statusText.text = $"Elimina a todos los enemigos para ganar. Restan: {enemies.Count}";
        }

        TryWin();
    }

    public void PlayerDied()
    {
        if (matchEnded) return;

        matchEnded = true;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (statusText != null) statusText.text = "Game Over";
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.buildIndex >= 0)
        {
            SceneManager.LoadScene(activeScene.buildIndex);
        }
        else
        {
            SceneManager.LoadScene(activeScene.name);
        }
    }

    private void TryWin()
    {
        if (matchEnded || !exitReached || enemies.Count > 0) return;

        matchEnded = true;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (victoryPanel != null) victoryPanel.SetActive(true);
        if (statusText != null) statusText.text = "Victoria";
    }

    private void UpdateHud()
    {
        if (playerWeapon != null && ammoText != null)
        {
            ammoText.text = playerWeapon.IsReloading
                ? $"Municion: recargando... | Reserva: {playerWeapon.ReserveAmmo} | Gastadas: {playerWeapon.TotalAmmoSpent}"
                : $"Cargador: {playerWeapon.CurrentAmmo}/{playerWeapon.ClipSize} | Reserva: {playerWeapon.ReserveAmmo} | Gastadas: {playerWeapon.TotalAmmoSpent} | Ultima recarga: +{playerWeapon.LastReloadAmount}";
        }

        if (enemiesText != null)
        {
            enemiesText.text = $"Enemigos: {enemies.Count}";
        }

        if (playerHealth != null && healthText != null)
        {
            healthText.text = $"Vida: {Mathf.CeilToInt(playerHealth.CurrentHealth)}/{Mathf.CeilToInt(playerHealth.MaxHealth)}";
        }

        if (playerHealth != null && healthFill != null)
        {
            float normalizedHealth = Mathf.Clamp01(playerHealth.CurrentHealth / playerHealth.MaxHealth);
            healthFill.fillAmount = normalizedHealth;
            healthFill.color = Color.Lerp(new Color(0.85f, 0.08f, 0.05f), new Color(0.1f, 0.85f, 0.35f), normalizedHealth);
        }
    }
}
