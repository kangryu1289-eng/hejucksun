using UnityEngine;
namespace PirateGame
{
    public sealed class GameUI : MonoBehaviour
    {
        public DockPanel dock;
        public RoundSelectPanel roundSelect;
        public BattleHUD battleHUD;
        public ResultPanel result;
        GameFlowController flow;
        public void Initialize(GameFlowController value)
        { flow = value; dock.Bind(flow); roundSelect.Bind(flow); result.Bind(flow); battleHUD.flow = flow; }
        public void Show(GamePhase phase)
        {
            dock.gameObject.SetActive(phase == GamePhase.Dock);
            roundSelect.gameObject.SetActive(phase == GamePhase.RoundSelect);
            battleHUD.gameObject.SetActive(phase == GamePhase.Battle);
            result.gameObject.SetActive(phase == GamePhase.Result);
            Refresh();
        }
        public void Refresh()
        { if (dock.gameObject.activeSelf) dock.Refresh(); if (result.gameObject.activeSelf) result.Refresh(); }
    }
}
