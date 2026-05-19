using UnityEngine;

namespace ToySiege.Combat
{
    public interface IDamageable
    {
        void TakeDamage(float damage, Vector3 hitPoint, Vector3 hitDirection);
        bool IsDead { get; }
    }
}