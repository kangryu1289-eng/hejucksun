using UnityEngine;
namespace PirateGame
{
    public sealed class DockService
    {
        public const int MaxLevel = 5;
        readonly PlayerProgress progress;
        public DockService(PlayerProgress progress) { this.progress = progress; }
        public int RepairCost => Mathf.CeilToInt(PlayerProgress.MaxHull - progress.Hull);
        public int DamageCost => 50 * (progress.DamageLevel + 1);
        public int ReloadCost => 60 * (progress.ReloadLevel + 1);
        public bool Repair() { int cost = RepairCost; if (cost <= 0 || !progress.Spend(cost)) return false; progress.SetHull(PlayerProgress.MaxHull); return true; }
        public bool UpgradeDamage() { if (progress.DamageLevel >= MaxLevel || !progress.Spend(DamageCost)) return false; progress.UpgradeDamage(); return true; }
        public bool UpgradeReload() { if (progress.ReloadLevel >= MaxLevel || !progress.Spend(ReloadCost)) return false; progress.UpgradeReload(); return true; }
    }
}
