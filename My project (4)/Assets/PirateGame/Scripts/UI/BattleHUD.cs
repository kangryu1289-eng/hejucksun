using UnityEngine;
using TMPro;
using UnityEngine.UI;
namespace PirateGame
{
    public sealed class BattleHUD : MonoBehaviour
    {
        public GameFlowController flow;
        public TMP_Text status, aimText;
        public Slider hullBar, reloadBar;
        void Update()
        {
            var battle = flow.battle; var player = battle.Player;
            if (!player || !player.IsAlive) return;
            var aim = player.GetComponent<CannonAim>(); var weapon = player.GetComponent<CannonWeapon>();
            hullBar.value = player.Hull / player.MaxHull; reloadBar.value = weapon.Reload01;
            status.text = $"HULL {player.Hull:0}/100    ENEMIES {battle.EnemiesRemaining}";
            aimText.text = $"DISTANCE {aim.Distance:0.0} / {aim.range:0.0} m   |   {(aim.InRange ? "IN RANGE" : "OUT OF RANGE")}\nW/S move   A/D turn   Mouse aim   LMB fire";
        }
    }
}
