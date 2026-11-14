using UnityEngine;
namespace PirateGame
{
    [CreateAssetMenu(menuName = "Pirate Game/Round")]
    public sealed class RoundDefinition : ScriptableObject
    {
        public string title = "Coastal Patrol";
        [Range(1, 8)] public int enemyCount = 2;
        public float enemyHull = 40;
        public float enemyDamage = 8;
        public int victoryGold = 80;
        public Vector2 arenaHalfSize = new Vector2(12, 6.5f);
    }
}
