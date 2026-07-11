using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private EnemyController controller;

    private float currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
        if (controller == null) controller = GetComponent<EnemyController>();
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0f)
        {
            if (controller != null) controller.Die();
            else Destroy(gameObject);
        }
    }
}
