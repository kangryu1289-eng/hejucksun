using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
namespace PirateGame
{
    [RequireComponent(typeof(ShipMotor), typeof(CannonAim), typeof(CannonWeapon))]
    public sealed class PlayerShipInput : MonoBehaviour
    {
        ShipMotor motor; CannonAim aim; CannonWeapon weapon; ShipHealth health;
        void Awake() { motor = GetComponent<ShipMotor>(); aim = GetComponent<CannonAim>(); weapon = GetComponent<CannonWeapon>(); health = GetComponent<ShipHealth>(); }
        void Update()
        {
            if (!health.IsAlive) { motor.SetInput(0, 0); return; }
            var key = Keyboard.current;
            bool blocked = EventSystem.current && EventSystem.current.IsPointerOverGameObject();
            float forward = key == null ? 0 : (key.wKey.isPressed ? 1 : 0) - (key.sKey.isPressed ? 1 : 0);
            float turn = key == null ? 0 : (key.dKey.isPressed ? 1 : 0) - (key.aKey.isPressed ? 1 : 0);
            motor.SetInput(forward, turn);
            if (!blocked && Mouse.current != null && Mouse.current.leftButton.isPressed && aim.InRange) weapon.TryFire(aim.Target);
        }
    }
}
