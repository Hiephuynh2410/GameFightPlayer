using UnityEngine;
using System.Collections;

public class BotsController : MonoBehaviour
{
    [Header("Movement & AI Settings")]
    public float moveSpeed = 3f;
    public float stoppingDistance = 2f;
    public float evadeDistance = 1.0f;
    public float attackFrequency = 0.5f;

    private Rigidbody rb;
    private Transform playerTransform;
    private Animator animator;
    private HealthCharacter health;

    private Coroutine comboCoroutine;
    private bool inAttackRange = false;

    [Header("Ring Settings")]
    public Vector3 ringCenter = Vector3.zero;
    public float ringRadius = 5f;

    [Header("Hit Settings")]
    public Collider hitCollider;

    [Header("AI Level Settings")]
    public int aiLevel = 0;

    [Header("Damage Settings")]
    public int punch1Damage = 10;
    public int hit1Damage = 10;
    public int uppercutDamage = 30;

    private string currentAttack = "";

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        health = GetComponent<HealthCharacter>();

        if (rb == null)
        {
            Debug.LogWarning("No Rigidbody component found on " + gameObject.name);
        }
        else
        {
            rb.freezeRotation = true;
            rb.isKinematic = true;
        }

        if (animator == null)
        {
            Debug.LogWarning("No Animator component found on " + gameObject.name);
        }

        if (hitCollider != null)
        {
            hitCollider.enabled = false;
            hitCollider.isTrigger = true;
        }
        else
        {
            Debug.LogWarning("HitCollider is not assigned on " + gameObject.name);
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
        else
            Debug.LogWarning("Player not found! Please add tag 'Player' to player object.");
    }

    private void FixedUpdate()
    {
        if (rb == null || playerTransform == null || (health != null && health.isDead))
            return;

        Vector3 directionToPlayer = playerTransform.position - rb.position;
        directionToPlayer.y = 0; 
        float distance = directionToPlayer.magnitude;

        if (distance < 0.5f)
        {
            StopComboIfRunning();
            inAttackRange = false;
            return;
        }

        Vector3 moveDir = directionToPlayer.normalized;
        Vector3 newPosition = rb.position;

        if (distance <= evadeDistance)
        {
            Vector3 evadeDir = Vector3.Cross(moveDir, Vector3.up).normalized;
            evadeDir = (Random.value > 0.5f) ? evadeDir : -evadeDir;

            newPosition = rb.position + evadeDir * moveSpeed * Time.fixedDeltaTime;

            StopComboIfRunning();
            inAttackRange = false;
        }
        else if (distance <= stoppingDistance)
        {
            if (!inAttackRange)
            {
                inAttackRange = true;
                if (comboCoroutine == null)
                    comboCoroutine = StartCoroutine(AutoComboAttack());
            }
        }
        else
        {
            newPosition = rb.position + moveDir * moveSpeed * Time.fixedDeltaTime;

            StopComboIfRunning();
            inAttackRange = false;
        }
        Vector3 offsetFromCenter = newPosition - ringCenter;
        if (offsetFromCenter.magnitude > ringRadius)
        {
            Vector3 toCenterDir = (ringCenter - rb.position).normalized;
            newPosition = rb.position + toCenterDir * moveSpeed * Time.fixedDeltaTime;
        }

        rb.MovePosition(newPosition);

        if (moveDir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, 5f * Time.fixedDeltaTime));
        }
    }

    private void StopComboIfRunning()
    {
        if (comboCoroutine != null)
        {
            StopCoroutine(comboCoroutine);
            comboCoroutine = null;
        }
    }

    private IEnumerator AutoComboAttack()
    {
        string[] attackTriggers = { "Punch1", "Hit1" };
        HealthCharacter playerHealth = playerTransform?.GetComponent<HealthCharacter>();

        float initialDelay = Random.Range(0.5f, 2.0f);
        yield return new WaitForSeconds(initialDelay);

        while (true)
        {
            if (health != null && health.isDead) yield break;
            if (playerHealth == null || playerHealth.isDead) yield break;

            currentAttack = attackTriggers[Random.Range(0, attackTriggers.Length)];
            animator.SetTrigger(currentAttack);

            float nextDelay = Random.Range(attackFrequency * 0.8f, attackFrequency * 1.2f);
            yield return new WaitForSeconds(nextDelay);
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

    private void OnTriggerEnter(Collider other)
    {
        if (hitCollider == null || !hitCollider.enabled) return;
        if (!other.CompareTag("Player")) return;

        HealthCharacter targetHealth = other.GetComponent<HealthCharacter>();
        if (targetHealth == null) return;

        int damage = GetDamageByAttack(currentAttack);
        targetHealth.TakeDamage(damage);

        Debug.Log($"Bot dealt {damage} damage to {other.name} with {currentAttack}");
    }

    private int GetDamageByAttack(string attackName)
    {
        switch (attackName)
        {
            case "Punch1": return punch1Damage;
            case "Hit1": return hit1Damage;
            case "Uppercut": return uppercutDamage;
            default: return 10;
        }
    }

    // Thiết lập AI level và cập nhật thuộc tính tương ứng
    public void SetAILevel(int level)
    {
        aiLevel = level;

        switch (level)
        {
            case 0:
                moveSpeed = 1.0f;
                attackFrequency = 1f;
                punch1Damage = 1;
                hit1Damage = 1;
                evadeDistance = 0.2f;
                break;
            case 1:
                moveSpeed = 2.5f;
                attackFrequency = 1.3f;
                punch1Damage = 8;
                hit1Damage = 8;
                evadeDistance = 0.4f;
                break;
            case 2:
                moveSpeed = 3.0f;
                attackFrequency = 1.1f;
                punch1Damage = 11;
                hit1Damage = 11;
                evadeDistance = 0.6f;
                break;
            case 3:
                moveSpeed = 3.5f;
                attackFrequency = 0.95f;
                punch1Damage = 14;
                hit1Damage = 14;
                evadeDistance = 0.8f;
                break;
            case 4:
                moveSpeed = 4.0f;
                attackFrequency = 0.8f;
                punch1Damage = 17;
                hit1Damage = 17;
                evadeDistance = 1.0f;
                break;
            case 5:
                moveSpeed = 4.5f;
                attackFrequency = 0.7f;
                punch1Damage = 20;
                hit1Damage = 20;
                evadeDistance = 1.1f;
                break;
            case 6:
                moveSpeed = 5.0f;
                attackFrequency = 0.6f;
                punch1Damage = 24;
                hit1Damage = 24;
                evadeDistance = 1.2f;
                break;
            case 7:
                moveSpeed = 5.5f;
                attackFrequency = 0.5f;
                punch1Damage = 28;
                hit1Damage = 28;
                evadeDistance = 1.3f;
                break;
            case 8:
                moveSpeed = 6.0f;
                attackFrequency = 0.4f;
                punch1Damage = 32;
                hit1Damage = 32;
                evadeDistance = 1.4f;
                break;
            case 9:
                moveSpeed = 6.5f;
                attackFrequency = 0.3f;
                punch1Damage = 36;
                hit1Damage = 36;
                evadeDistance = 1.5f;
                break;
            default:
                moveSpeed = 2f;
                attackFrequency = 1f;
                punch1Damage = 10;
                hit1Damage = 10;
                evadeDistance = 0.5f;
                break;
        }

        UpdateHitboxDamage(punch1Damage);

        Debug.Log($"{gameObject.name} AI Level set to {level} | Speed: {moveSpeed}, Attack Frequency: {attackFrequency}, Damage: {punch1Damage}");
    }

    private void UpdateHitboxDamage(int damage)
    {
        HitDetector[] hitDetectors = GetComponentsInChildren<HitDetector>();
        foreach (var hitDetector in hitDetectors)
        {
            hitDetector.damage = damage;
            Debug.Log($"[BotsController] Set HitDetector damage to {damage} on {hitDetector.gameObject.name}");
        }
    }
}
