using UnityEngine;
namespace PirateGame
{
    public enum GamePhase { Dock, RoundSelect, Battle, Result }
    public sealed class GameFlowController : MonoBehaviour
    {
        public BattleController battle;
        public GameUI ui;
        public RoundDefinition[] rounds;
        public PlayerProgress Progress { get; private set; }
        public DockService Dock { get; private set; }
        public GamePhase Phase { get; private set; }
        public BattleResult LastResult { get; private set; }
        void Awake() { Progress = new PlayerProgress(); Dock = new DockService(Progress); }
        void Start() { battle.BattleEnded += OnBattleEnded; ui.Initialize(this); ReturnToDock(); }
        public void ReturnToDock() { Time.timeScale = 1; battle.Clear(); Phase = GamePhase.Dock; ui.Show(Phase); }
        public void SelectRound() { if (Phase != GamePhase.Dock) return; Phase = GamePhase.RoundSelect; ui.Show(Phase); }
        public void StartRound(int index)
        {
            if (Phase != GamePhase.RoundSelect || index < 0 || index >= rounds.Length || Progress.Hull <= 0) return;
            Time.timeScale = 1; Phase = GamePhase.Battle; battle.Begin(rounds[index], Progress); ui.Show(Phase);
        }
        void OnBattleEnded(BattleResult result)
        {
            if (Phase != GamePhase.Battle) return;
            Time.timeScale = 1; LastResult = result; Progress.AddGold(result.Reward);
            Progress.SetHull(result.Victory ? result.Hull : PlayerProgress.MaxHull * 0.5f);
            Phase = GamePhase.Result; ui.Show(Phase);
        }
        public void Repair() { if (Phase == GamePhase.Dock) { Dock.Repair(); ui.Refresh(); } }
        public void UpgradeDamage() { if (Phase == GamePhase.Dock) { Dock.UpgradeDamage(); ui.Refresh(); } }
        public void UpgradeReload() { if (Phase == GamePhase.Dock) { Dock.UpgradeReload(); ui.Refresh(); } }
        void OnDestroy() { if (battle) battle.BattleEnded -= OnBattleEnded; Time.timeScale = 1; }
    }
}
