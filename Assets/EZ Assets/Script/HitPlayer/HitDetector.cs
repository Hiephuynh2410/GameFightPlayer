
using UnityEngine;

public class HitDetector : MonoBehaviour
{
    public string targetTag;
    public int damage = 10;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            Animator targetAnimator = other.GetComponentInParent<Animator>();
            if (targetAnimator != null)
                targetAnimator.SetTrigger("StomachHit");

            HealthCharacter health = other.GetComponentInParent<HealthCharacter>();
            if (health != null)
                health.TakeDamage(damage);

            Debug.Log($"{gameObject.name} hit {other.name}, triggered StomachHit and dealt {damage} damage.");
        }
    }
}
