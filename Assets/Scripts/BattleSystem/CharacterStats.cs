using UnityEngine;
using UnityEngine.UI;

public class CharacterStats : MonoBehaviour
{
    public Animator animator;
    [Header("Stats")]
    public int maxHealth = 3;
    public int currentHealth;

    [Header("Status Effects")]
    public bool willMissNextTurn = false;
    public bool bigPinchDisabled = false; // <-- ADD THIS LINE

    [Header("UI Connection (Optional)")]
    public Image[] hearts;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;
        AudioManager.instance.PlaySFX("Hit");
        if (animator != null)
        {
            animator.SetTrigger("TakeDamage"); // <-- NEW
        }

        UpdateHealthUI();
    }

    public void UpdateHealthUI()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            if (i < currentHealth)
            {
                hearts[i].enabled = true;
            }
            else
            {
                hearts[i].enabled = false;
            }
        }
    }
}