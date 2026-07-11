using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    [SerializeField] private float healAmount = 35f;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerHealth health = other.GetComponent<PlayerHealth>();
        if (health == null || health.CurrentHealth >= health.MaxHealth) return;

        health.Heal(healAmount);
        Destroy(gameObject);
    }
}
