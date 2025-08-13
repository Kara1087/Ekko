
using UnityEngine;

public interface IDamageable
{
    void TakeDamage(float damageAmount, Transform source = null);
}