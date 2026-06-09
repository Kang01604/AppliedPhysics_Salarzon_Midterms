using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 pivotOffset = new Vector3(0f, 1.55f, 0f);

    [Header("Over-Shoulder Framing")]
    [SerializeField] private float shoulderSide = 0.65f;
    [SerializeField] private float distance = 3.5f;

    [Header("Look Controls")]
    [SerializeField] private float sensitivityX = 0.18f;
    [SerializeField] private float sensitivityY = 0.14f;
    [SerializeField] private float minPitch = -25f;
    [SerializeField] private float maxPitch = 58f;

    [Header("Smoothing")]
    [SerializeField] private float positionSmoothTime = 0.06f;
    [SerializeField] private float collisionPullInTime = 0.04f;
    [SerializeField] private float collisionPullOutTime = 0.12f;

    [Header("Collision")]
    [SerializeField] private float collisionRadius = 0.15f;
    [SerializeField] private LayerMask collisionMask = ~0;

    private float   _yaw;                  
    private float   _pitch;                 
    private float   _currentDistance;       
    private float   _distanceVelocity;      
    private Vector3 _positionVelocity;      

    private void Awake()
    {
        Vector3 startAngles = transform.eulerAngles;
        _yaw             = startAngles.y;
        _pitch           = startAngles.x;
        _currentDistance = distance;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        ReadMouseInput();
        PlaceCamera();
    }

    private void ReadMouseInput()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null) return;

        Vector2 delta = mouse.delta.ReadValue();

        _yaw   += delta.x * sensitivityX;
        _pitch -= delta.y * sensitivityY;
        _pitch  = Mathf.Clamp(_pitch, minPitch, maxPitch);
    }

    private void PlaceCamera()
    {
        Quaternion orbitalRot = Quaternion.Euler(_pitch, _yaw, 0f);
        Vector3 pivot = target.position + pivotOffset;

        Vector3 worldRight   = orbitalRot * Vector3.right;
        Vector3 shiftedPivot = pivot + worldRight * shoulderSide;

        Vector3 backDir = orbitalRot * Vector3.back;

        float desiredDistance = GetSafeDistance(shiftedPivot, backDir);

        float smoothTime = desiredDistance < _currentDistance
                           ? collisionPullInTime
                           : collisionPullOutTime;

        _currentDistance = Mathf.SmoothDamp(_currentDistance, desiredDistance,
                                              ref _distanceVelocity, smoothTime);

        Vector3 desiredPos = shiftedPivot + backDir * _currentDistance;

        transform.position = Vector3.SmoothDamp(transform.position, desiredPos,
                                                  ref _positionVelocity, positionSmoothTime);

        Vector3 lookDir = pivot - transform.position;
        if (lookDir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(lookDir);
    }

    private float GetSafeDistance(Vector3 from, Vector3 direction)
    {
        if (Physics.SphereCast(from, collisionRadius, direction,
                               out RaycastHit hit, distance,
                               collisionMask, QueryTriggerInteraction.Ignore))
        {
            return Mathf.Max(hit.distance - collisionRadius, 0.1f);
        }

        return distance;
    }

    private void OnDrawGizmosSelected()
    {
        if (target == null) return;

        Vector3 pivot        = target.position + pivotOffset;
        Quaternion orbitalRot = Quaternion.Euler(_pitch, _yaw, 0f);
        Vector3 shiftedPivot = pivot + orbitalRot * Vector3.right * shoulderSide;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(pivot, 0.07f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(shiftedPivot, 0.07f);

        Gizmos.color = Color.white;
        Gizmos.DrawLine(shiftedPivot, transform.position);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, collisionRadius);
    }
}