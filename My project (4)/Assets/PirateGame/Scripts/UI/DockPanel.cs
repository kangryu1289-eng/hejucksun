using UnityEngine;
using TMPro;
using UnityEngine.UI;
namespace PirateGame
{
    public sealed class DockPanel : MonoBehaviour
    {
        public TMP_Text stats;
        public TMP_Text repairLabel, damageLabel, reloadLabel;
        public Button repairButton, damageButton, reloadButton, sailButton;
        GameFlowController flow;
        public void Bind(GameFlowController value)
        { flow = value; repairButton.onClick.AddListener(flow.Repair); damageButton.onClick.AddListener(flow.UpgradeDamage); reloadButton.onClick.AddListener(flow.UpgradeReload); sailButton.onClick.AddListener(flow.SelectRound); }
        public void Refresh()
        {
            var p = flow.Progress; var d = flow.Dock;
            stats.text = $"GOLD  {p.Gold}\nHULL  {p.Hull:0} / 100\nCANNON  {p.Damage:0} damage   |   RELOAD  {p.ReloadSeconds:0.00}s";
            repairLabel.text = $"REPAIR HULL   {d.RepairCost} G";
            damageLabel.text = p.DamageLevel >= DockService.MaxLevel ? "CANNON  MAX" : $"CANNON +5   {d.DamageCost} G";
            reloadLabel.text = p.ReloadLevel >= DockService.MaxLevel ? "RELOAD  MAX" : $"RELOAD -10%   {d.ReloadCost} G";
            repairButton.interactable = d.RepairCost > 0 && p.Gold >= d.RepairCost;
            damageButton.interactable = p.DamageLevel < DockService.MaxLevel && p.Gold >= d.DamageCost;
            reloadButton.interactable = p.ReloadLevel < DockService.MaxLevel && p.Gold >= d.ReloadCost;
        }
    }
}
