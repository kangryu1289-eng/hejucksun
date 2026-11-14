using System;
using UnityEngine;
namespace PirateGame
{
    public readonly struct ShotFeedback
    {
        public readonly Transform Shooter;
        public readonly Vector2 Position, Direction;
        public readonly bool IsPlayer;
        public ShotFeedback(Transform shooter, Vector2 position, Vector2 direction, bool isPlayer)
        { Shooter = shooter; Position = position; Direction = direction; IsPlayer = isPlayer; }
    }
    public readonly struct HitFeedback
    {
        public readonly ShipHealth Target;
        public readonly Vector2 Position;
        public readonly float Damage;
        public readonly bool IsPlayer;
        public HitFeedback(ShipHealth target, Vector2 position, float damage)
        { Target = target; Position = position; Damage = damage; IsPlayer = target.isPlayer; }
    }
    public readonly struct SinkFeedback
    {
        public readonly ShipHealth Target;
        public readonly Vector2 Position;
        public readonly bool IsPlayer;
        public SinkFeedback(ShipHealth target)
        { Target = target; Position = target.transform.position; IsPlayer = target.isPlayer; }
    }
    public readonly struct BattleResult
    {
        public readonly bool Victory;
        public readonly float Hull, Seconds;
        public readonly int Sunk, Reward;
        public BattleResult(bool victory, float hull, float seconds, int sunk, int reward)
        { Victory = victory; Hull = hull; Seconds = seconds; Sunk = sunk; Reward = reward; }
    }
    public sealed class CombatFeedbackEvents : MonoBehaviour
    {
        public event Action<ShotFeedback> ShotFired;
        public event Action<HitFeedback> Hit;
        public event Action<SinkFeedback> Sunk;
        public event Action<Vector2> WaterMiss;
        public void PublishShot(ShotFeedback value) => ShotFired?.Invoke(value);
        public void PublishHit(HitFeedback value) => Hit?.Invoke(value);
        public void PublishSunk(SinkFeedback value) => Sunk?.Invoke(value);
        public void PublishWaterMiss(Vector2 position) => WaterMiss?.Invoke(position);
    }
}
