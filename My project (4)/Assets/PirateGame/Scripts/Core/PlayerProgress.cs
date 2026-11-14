using UnityEngine;
namespace PirateGame
{
    public sealed class PlayerProgress
    {
        public const float MaxHull = 100f;
        public int Gold { get; private set; } = 100;
        public int DamageLevel { get; private set; }
        public int ReloadLevel { get; private set; }
        public float Hull { get; private set; } = MaxHull;
        public float Damage => 20f + DamageLevel * 5f;
        public float ReloadSeconds => 0.8f * Mathf.Pow(0.9f, ReloadLevel);
        public bool Spend(int amount) { if (amount < 0 || Gold < amount) return false; Gold -= amount; return true; }
        public void AddGold(int amount) => Gold += Mathf.Max(0, amount);
        public void SetHull(float value) => Hull = Mathf.Clamp(value, 0, MaxHull);
        public void UpgradeDamage() => DamageLevel++;
        public void UpgradeReload() => ReloadLevel++;
    }
}
