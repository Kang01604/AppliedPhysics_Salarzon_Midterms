// ─────────────────────────────────────────────
//  DeadZone.cs
//  Instantly kills and respawns the player at
//  the last activated Checkpoint.
// ─────────────────────────────────────────────
using UnityEngine;

public class DeadZone : MonoBehaviour
{
    [Header("Trigger Box")]
    [SerializeField] private Vector3 center = Vector3.zero;
    [SerializeField] private Vector3 size   = new Vector3(5f, 2f, 5f);

    [Header("Detection")]
    [SerializeField] private LayerMask playerLayer;

    [Header("Respawn")]
    [Tooltip("Used when no Checkpoint has been activated yet.")]
    [SerializeField] private Transform defaultSpawnPoint;

    [Tooltip("Seconds of immunity after respawn to prevent immediate re-trigger.")]
    [SerializeField] private float respawnCooldown = 1.5f;

    private float _cooldownTimer;

    private void Update()
    {
        if (_cooldownTimer > 0f)
        {
            _cooldownTimer -= Time.deltaTime;
            return;
        }

        Collider[] hits = Physics.OverlapBox(
            transform.TransformPoint(center),
            size * 0.5f,
            transform.rotation,
            playerLayer,
            QueryTriggerInteraction.Ignore);

        for (int i = 0; i < hits.Length; i++)
        {
            // Handles both root CapsuleCollider and ragdoll bone colliders
            PlayerController player = hits[i].GetComponent<PlayerController>()
                                   ?? hits[i].GetComponentInParent<PlayerController>();
            if (player == null) continue;

            RespawnPlayer(player);
            return;
        }
    }

    private void RespawnPlayer(PlayerController player)
    {
        // Priority: last checkpoint → default spawn → world origin
        Vector3 spawnPos = Checkpoint.HasCheckpoint
            ? Checkpoint.LastSpawnPosition
            : (defaultSpawnPoint != null ? defaultSpawnPoint.position : Vector3.zero);

        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity  = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position        = spawnPos;    // Physics-safe teleport via Rigidbody
        }
        else
        {
            player.transform.position = spawnPos;
        }

        _cooldownTimer = respawnCooldown;

        Debug.Log($"[DeadZone] Respawned '{player.name}' at {spawnPos}");
        // TODO: death SFX, screen flash, life counter decrement, etc.
        // NOTE: If you want a ragdoll death before respawn, add a public
        //       Respawn(Vector3) method to PlayerController that re-enables
        //       the animator and collider, then call player.EnableRagdoll()
        //       here and invoke Respawn() via Invoke() after a delay.
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.matrix = Matrix4x4.TRS(
            transform.TransformPoint(center),
            transform.rotation,
            Vector3.one);

        Gizmos.color = new Color(1f, 0.1f, 0.1f, 0.25f);
        Gizmos.DrawCube(Vector3.zero, size);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(Vector3.zero, size);

        // Default spawn indicator (only visible when no checkpoint set)
        if (defaultSpawnPoint != null)
        {
            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.color  = Color.magenta;
            Gizmos.DrawWireSphere(defaultSpawnPoint.position, 0.25f);
        }
    }
}
