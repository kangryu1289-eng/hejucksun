using UnityEngine;
using TMPro;
using UnityEngine.UI;
namespace PirateGame
{
    public sealed class RoundSelectPanel : MonoBehaviour
    {
        public Button[] roundButtons;
        public TMP_Text[] roundLabels;
        public Button backButton;
        public void Bind(GameFlowController flow)
        {
            for (int i = 0; i < roundButtons.Length; i++)
            {
                int index = i; bool exists = index < flow.rounds.Length;
                roundButtons[i].gameObject.SetActive(exists);
                if (!exists) continue;
                var round = flow.rounds[i];
                roundLabels[i].text = $"{i + 1}. {round.title}\n{round.enemyCount} enemy ships   |   Reward {round.victoryGold} G";
                roundButtons[i].onClick.AddListener(() => flow.StartRound(index));
            }
            backButton.onClick.AddListener(flow.ReturnToDock);
        }
    }
}
