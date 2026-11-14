using UnityEngine;
using TMPro;
using UnityEngine.UI;
namespace PirateGame
{
    public sealed class ResultPanel : MonoBehaviour
    {
        public TMP_Text heading, summary;
        public Button dockButton;
        GameFlowController flow;
        public void Bind(GameFlowController value) { flow = value; dockButton.onClick.AddListener(flow.ReturnToDock); }
        public void Refresh()
        {
            var r = flow.LastResult;
            heading.text = r.Victory ? "VICTORY" : "SHIP LOST";
            summary.text = $"SHIPS SUNK  {r.Sunk}\nREWARD  +{r.Reward} G\nBATTLE TIME  {r.Seconds:0.0}s\nHULL AT DOCK  {flow.Progress.Hull:0}/100" + (r.Victory ? "" : "\nEmergency repairs restored 50 hull.");
        }
    }
}
