// ─────────────────────────────────────────────
//  FinishPoint.cs
//  Triggers the win condition when the player
//  enters the trigger box.
// ─────────────────────────────────────────────
using UnityEngine;
using UnityEngine.Events;

public class FinishPoint : MonoBehaviour
{
    [Header("Trigger Box")]
    [SerializeField] private Vector3 center = Vector3.zero;
    [SerializeField] private Vector3 size   = new Vector3(3f, 3f, 3f);

    [Header("Detection")]
    [SerializeField] private LayerMask playerLayer;

    [Header("Win Event")]
    [Tooltip("Hook up your scene transition, UI reveal, score tally, etc.")]
    [SerializeField] private UnityEvent onGameWon;

    private bool _triggered;

    private void Update()
    {
        if (_triggered) return;

        Collider[] hits = Physics.OverlapBox(
            transform.TransformPoint(center),
            size * 0.5f,
            transform.rotation,
            playerLayer,
            QueryTriggerInteraction.Ignore);

        for (int i = 0; i < hits.Length; i++)
        {
            PlayerController player = hits[i].GetComponent<PlayerController>()
                                   ?? hits[i].GetComponentInParent<PlayerController>();
            if (player == null) continue;

            TriggerWin(player);
            return;
        }
    }

    private void TriggerWin(PlayerController player)
    {
        _triggered = true;
        Debug.Log($"[FinishPoint] Game Won! Player: {player.name}");
        onGameWon?.Invoke();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.matrix = Matrix4x4.TRS(
            transform.TransformPoint(center),
            transform.rotation,
            Vector3.one);

        bool won = Application.isPlaying && _triggered;
        Gizmos.color = won
            ? new Color(0.4f, 1f, 0.4f, 0.35f)
            : new Color(0f, 1f, 0.2f, 0.25f);
        Gizmos.DrawCube(Vector3.zero, size);

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(Vector3.zero, size);
    }
}
