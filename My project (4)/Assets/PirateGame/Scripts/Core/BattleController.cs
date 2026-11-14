using System;
using System.Collections.Generic;
using UnityEngine;
namespace PirateGame
{
    public sealed class BattleController : MonoBehaviour
    {
        public ShipHealth playerPrefab, enemyPrefab;
        public Transform actorsRoot, projectileRoot;
        public GameObject arenaRoot;
        public Camera worldCamera;
        public CombatFeedbackEvents feedback;
        public ShipHealth Player { get; private set; }
        public bool Running { get; private set; }
        public int EnemiesRemaining { get { int n = 0; foreach (var e in enemies) if (e && e.IsAlive) n++; return n; } }
        public event Action<BattleResult> BattleEnded;
        readonly List<ShipHealth> enemies = new List<ShipHealth>();
        RoundDefinition round; float startedAt; int sunk;
        public void Begin(RoundDefinition definition, PlayerProgress progress)
        {
            Clear();
            round = definition; sunk = 0; arenaRoot.SetActive(true);
            Player = Instantiate(playerPrefab, new Vector3(-7, 0, 0), Quaternion.Euler(0, 0, -90), actorsRoot);
            Player.Initialize(true, PlayerProgress.MaxHull, progress.Hull, feedback);
            Configure(Player, progress.Damage, progress.ReloadSeconds);
            var aim = Player.GetComponent<CannonAim>(); aim.worldCamera = worldCamera;
            for (int i = 0; i < round.enemyCount; i++)
            {
                Vector3 point = new Vector3(4f + (i % 2) * 3f, -3.6f + i * (7.2f / Mathf.Max(1, round.enemyCount - 1)), 0);
                var enemy = Instantiate(enemyPrefab, point, Quaternion.Euler(0, 0, 90), actorsRoot);
                enemy.Initialize(false, round.enemyHull, round.enemyHull, feedback);
                Configure(enemy, round.enemyDamage, 1.8f + i * 0.15f);
                enemy.GetComponent<EnemyShipAI>().target = Player;
                enemies.Add(enemy);
            }
            feedback.Sunk += OnSunk;
            startedAt = Time.time; Running = true;
        }
        void Configure(ShipHealth ship, float damage, float reload)
        {
            var weapon = ship.GetComponent<CannonWeapon>();
            weapon.damage = damage; weapon.reloadSeconds = reload; weapon.projectileRoot = projectileRoot; weapon.feedback = feedback;
            ship.GetComponent<ShipMotor>().arenaHalfSize = round.arenaHalfSize;
        }
        void OnSunk(SinkFeedback value) { if (!value.IsPlayer) sunk++; }
        void LateUpdate()
        {
            if (!Running) return;
            if (!Player || !Player.IsAlive) Finish(false);
            else if (EnemiesRemaining == 0) Finish(true);
        }
        void Finish(bool victory)
        {
            if (!Running) return;
            Running = false; feedback.Sunk -= OnSunk;
            // Freeze surviving controls/projectiles while keeping the final battlefield visible.
            foreach (var motor in actorsRoot.GetComponentsInChildren<ShipMotor>()) motor.enabled = false;
            foreach (var input in actorsRoot.GetComponentsInChildren<PlayerShipInput>()) input.enabled = false;
            foreach (var ai in actorsRoot.GetComponentsInChildren<EnemyShipAI>()) ai.enabled = false;
            foreach (var aim in actorsRoot.GetComponentsInChildren<CannonAim>()) aim.enabled = false;
            foreach (var ball in projectileRoot.GetComponentsInChildren<Cannonball>()) ball.enabled = false;
            int reward = victory ? round.victoryGold : 20 + sunk * 10;
            BattleEnded?.Invoke(new BattleResult(victory, Player ? Player.Hull : 0, Time.time - startedAt, sunk, reward));
        }
        public void Clear()
        {
            Running = false;
            if (feedback) feedback.Sunk -= OnSunk;
            ClearChildren(actorsRoot); ClearChildren(projectileRoot);
            enemies.Clear(); Player = null;
            if (arenaRoot) arenaRoot.SetActive(false);
        }
        static void ClearChildren(Transform root)
        {
            if (!root) return;
            for (int i = root.childCount - 1; i >= 0; i--)
            { GameObject child = root.GetChild(i).gameObject; child.SetActive(false); Destroy(child); }
        }
        void OnDestroy() { if (feedback) feedback.Sunk -= OnSunk; }
    }
}
