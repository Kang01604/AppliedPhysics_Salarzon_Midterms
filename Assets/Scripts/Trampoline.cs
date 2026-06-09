using UnityEngine;

public class Trampoline : MonoBehaviour
{
    [SerializeField] float trampolineForce = 2f;

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.rigidbody) return;
        if (collision.contacts[0].normal.y >= 0.9f) return;
        collision.rigidbody.AddForce(Vector3.up * trampolineForce, ForceMode.Impulse);
        Debug.Log(collision.gameObject.name);
    }
}
