using UnityEngine;
using System.Collections;

public class PlayersController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Collider hitCollider;
    public float recoveryTime = 1.0f;
    private Rigidbody rb;
    private Animator animator;
    private HealthCharacter health;
    private Coroutine comboCoroutine;
    
    private string currentAttack = "";
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        health = GetComponent<HealthCharacter>();

        if (rb == null)
            Debug.LogWarning("No Rigidbody component found on " + gameObject.name);
        else
            rb.freezeRotation = true;

        if (animator == null)
            Debug.LogWarning("No Animator component found on " + gameObject.name);

        if (hitCollider != null)
        {
            hitCollider.enabled = false;
            hitCollider.isTrigger = true;
        }

        StartCoroutine(AutoComboAttack());
    }

    

    private void FixedUpdate()
    {
        if (rb == null || (health != null && health.isDead)) return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(h, 0, v).normalized * moveSpeed;
        Vector3 newPosition = rb.position + movement * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);

        Vector3 lookDir = new Vector3(h, 0, v);
        if (lookDir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDir);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, 10f * Time.fixedDeltaTime));
        }
    }

    private IEnumerator AutoComboAttack()
    {
        while (true)
        {
            if (health != null && health.isDead) yield break;

            animator.SetTrigger("Punch1");
            yield return new WaitForSeconds(0.8f);

            if (health != null && health.isDead) yield break;

            animator.SetTrigger("Hit1");
            yield return new WaitForSeconds(0.8f);

            if (health != null && health.isDead) yield break;

            animator.SetTrigger("Punch1");
            yield return new WaitForSeconds(1.2f);
        }
    }

    public void RestartAutoCombo()
    {
        if (comboCoroutine != null)
        {
            StopCoroutine(comboCoroutine);
        }

        comboCoroutine = StartCoroutine(AutoComboAttack());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hitCollider == null || !hitCollider.enabled) return;
        if (!other.CompareTag("Bot")) return;

        HealthCharacter targetHealth = other.GetComponent<HealthCharacter>();
        if (targetHealth == null) return;

        int damage = GetDamageByAttack(currentAttack);
        targetHealth.TakeDamage(damage);

        Debug.Log($"Player dealt {damage} damage to {other.name} with {currentAttack}");
    }

    private int GetDamageByAttack(string attackName)
    {
        switch (attackName)
        {
            case "Punch1": return 10;
            case "Hit1": return 10;
            default: return 10;
        }
    }

    public void EnableHit()
    {
        if (hitCollider != null)
            hitCollider.enabled = true;
    }

    public void DisableHit()
    {
        if (hitCollider != null)
            hitCollider.enabled = false;
    }
}
