using System;
using System.Collections;
using UnityEngine;

public class HealthCharacter : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    private Animator animator;
    public float deathAnimationDuration = 2f;
    public Animation animationComponent;
    public string knockOutAnimationName = "Knocked Out";

    public bool isDead = false;

    private void Start()
    {
        currentHealth = maxHealth;

        animator = GetComponent<Animator>();
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (animator == null)
            Debug.LogWarning("Animator not found on " + gameObject.name);
        else
            animator.SetBool("canTakeHit", true);
    }

    public void TakeDamage(int damage)
    {
        if (isDead || GameManager.isGameOver) return;

        currentHealth -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage. Current HP: {currentHealth}");

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            isDead = true;

            if (animator != null)
                animator.SetBool("canTakeHit", false);

            Debug.Log($"{gameObject.name} is DEAD!");

            GameManager.EndGame();

            if (animationComponent != null && animationComponent.GetClip(knockOutAnimationName) != null)
            {
                animationComponent.Play(knockOutAnimationName);
                Debug.Log("Playing Legacy KnockOut animation on " + gameObject.name);
                StartCoroutine(WaitForDeathAnimation(animationComponent[knockOutAnimationName].length));
            }
            else if (animator != null)
            {
                animator.SetTrigger("KnockOut");
                Debug.Log("Triggering KnockOut Animator on " + gameObject.name);
                StartCoroutine(WaitForDeathAnimation(deathAnimationDuration));
            }
            else
            {
                Debug.LogWarning("No animator or animation component found on " + gameObject.name);
                StopGame();
            }
        }
        else
        {
            // Nếu còn sống thì cho phép bị đánh
            if (animator != null)
            {
                animator.SetTrigger("StomachHit"); 
                animator.SetBool("canTakeHit", true); 
            }
        }
    }

    private IEnumerator WaitForDeathAnimation(float duration)
    {
        yield return new WaitForSeconds(duration);
        StopGame();
    }

    private void StopGame()
    {
        Debug.Log("Game stopped or object disabled.");
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        isDead = false;

        if (animator != null)
        {
            animator.SetBool("canTakeHit", true);
        }
    }
}
