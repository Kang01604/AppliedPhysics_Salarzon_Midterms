/*using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TrapObstacle : MonoBehaviour
{
    [SerializeField] private float hitCooldown = 1f;
    private float    _lastHitTime;
    private Collider _col;

    private void Awake() => _col = GetComponent<Collider>();

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.TryGetComponent(out PlayerController player)) return;
        Hit(player);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.TryGetComponent(out PlayerController player)) return;
        Hit(player);
    }

    private void Hit(PlayerController player)
    {
        if (Time.time < _lastHitTime + hitCooldown) return;
        player.ActivateRagdoll(_col);
        _lastHitTime = Time.time;
    }
}*/