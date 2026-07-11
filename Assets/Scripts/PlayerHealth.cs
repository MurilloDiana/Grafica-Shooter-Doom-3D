using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private Image damageFlash;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hurtClip;
    [SerializeField] private AudioClip healClip;

    private Coroutine flashRoutine;

    public float CurrentHealth { get; private set; }
    public float MaxHealth => maxHealth;

    private void Awake()
    {
        CurrentHealth = maxHealth;
        if (damageFlash != null)
        {
            damageFlash.color = Color.clear;
        }
    }

    public void TakeDamage(float amount)
    {
        if (CurrentHealth <= 0f) return;

        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
        if (audioSource != null && hurtClip != null) audioSource.PlayOneShot(hurtClip);

        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashDamage());

        if (CurrentHealth <= 0f)
        {
            GameManager.Instance.PlayerDied();
        }
    }

    public void Heal(float amount)
    {
        if (CurrentHealth <= 0f) return;

        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
        if (audioSource != null && healClip != null) audioSource.PlayOneShot(healClip);
    }

    private IEnumerator FlashDamage()
    {
        if (damageFlash == null) yield break;

        const int pulses = 2;
        const float fadeIn = 0.06f;
        const float fadeOut = 0.16f;
        Color flashColor = new Color(1f, 0f, 0f, 0.24f);

        for (int pulse = 0; pulse < pulses; pulse++)
        {
            float timer = 0f;
            while (timer < fadeIn)
            {
                timer += Time.unscaledDeltaTime;
                damageFlash.color = Color.Lerp(Color.clear, flashColor, timer / fadeIn);
                yield return null;
            }

            timer = 0f;
            while (timer < fadeOut)
            {
                timer += Time.unscaledDeltaTime;
                damageFlash.color = Color.Lerp(flashColor, Color.clear, timer / fadeOut);
                yield return null;
            }
        }

        damageFlash.color = Color.clear;
    }
}
