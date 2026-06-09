// ─────────────────────────────────────────────
//  Checkpoint.cs
//  Saves the player's respawn position when
//  they walk through the trigger box.
// ─────────────────────────────────────────────
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Trigger Box")]
    [SerializeField] private Vector3 center = Vector3.zero;
    [SerializeField] private Vector3 size   = new Vector3(2f, 3f, 2f);

    [Header("Detection")]
    [SerializeField] private LayerMask playerLayer;

    [Header("Spawn")]
    [Tooltip("Where the player respawns. Falls back to this object's position if left empty.")]
    [SerializeField] private Transform spawnPoint;

    // ── Shared state across all Checkpoint instances ──────────────────────────
    public static Vector3 LastSpawnPosition { get; private set; }
    public static bool    HasCheckpoint     { get; private set; }

    private bool _activated;

    private void Update()
    {
        if (_activated) return;

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

            Activate();
            return;
        }
    }

    private void Activate()
    {
        _activated        = true;
        LastSpawnPosition = spawnPoint != null ? spawnPoint.position : transform.position;
        HasCheckpoint     = true;

        Debug.Log($"[Checkpoint] '{name}' activated — spawn at {LastSpawnPosition}");
        // TODO: VFX / SFX
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.matrix = Matrix4x4.TRS(
            transform.TransformPoint(center),
            transform.rotation,
            Vector3.one);

        bool active = Application.isPlaying && _activated;
        Gizmos.color = active
            ? new Color(0f, 1f, 0.4f, 0.25f)
            : new Color(1f, 0.85f, 0f, 0.25f);
        Gizmos.DrawCube(Vector3.zero, size);

        Gizmos.color = active ? Color.green : Color.yellow;
        Gizmos.DrawWireCube(Vector3.zero, size);

        // Spawn point indicator
        if (spawnPoint != null)
        {
            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.color  = Color.cyan;
            Gizmos.DrawWireSphere(spawnPoint.position, 0.25f);
            Gizmos.DrawLine(transform.TransformPoint(center), spawnPoint.position);
        }
    }
}
