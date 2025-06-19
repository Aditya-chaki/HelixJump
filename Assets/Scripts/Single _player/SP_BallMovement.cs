using UnityEngine;

public class SP_BallMovement : MonoBehaviour
{
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private float _jumpForce;
    [SerializeField] private GameObject _cylinder1, _cylinder2;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private GameObject _speedEffectObject; // GameObject to activate/deactivate based on speed
    [SerializeField] private float _speedThreshold = 10f; // Speed threshold for toggling the GameObject

    private float _cylinderPositionY = -24f;
    private bool _isCollidingWithRingPiece = false; // Tracks collision with ring pieces
    public bool IsCollidingWithRingPiece => _isCollidingWithRingPiece; // Public accessor
    public bool IsAI;

    void Start()
    {
        if (_cylinder1 == null)
        {
            _cylinder1 = transform.parent?.Find("Cylinder1")?.gameObject;
        }
        if (_cylinder2 == null)
        {
            _cylinder2 = transform.parent?.Find("Cylinder2")?.gameObject;
        }

        if (_cylinder1 == null || _cylinder2 == null)
        {
            Debug.LogError($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [SP_BallMovement] Cylinder1 or Cylinder2 not found!");
        }

        // Ensure the speed effect object is initially deactivated
        if (_speedEffectObject != null)
        {
            _speedEffectObject.SetActive(false);
        }
    }

    void Update()
    {
        // Check the ball's speed and toggle the speed effect GameObject
        if (_rigidbody != null && _speedEffectObject != null)
        {
            float speed = _rigidbody.linearVelocity.magnitude;
            bool shouldBeActive = speed > _speedThreshold;

            if (_speedEffectObject.activeSelf != shouldBeActive)
            {
                _speedEffectObject.SetActive(shouldBeActive);
                Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [SP_BallMovement] SpeedEffectObject {(shouldBeActive ? "activated" : "deactivated")} at speed {speed:F2}");
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            _rigidbody.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);

            if (!IsAI)
            {
                _audioSource.Play();
            }
            Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [SP_BallMovement] Applying jump force for {transform.parent.tag}");

            // Check if the ground object is a ring piece
            if (collision.gameObject.GetComponentInParent<SP_PiecePositioning>() != null)
            {
                _isCollidingWithRingPiece = true;
                Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [SP_BallMovement] Ball collided with ring piece (tagged Ground) on {collision.gameObject.name}");
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        // Clear collision state when exiting ring piece collision
        if (collision.gameObject.CompareTag("Ground") && collision.gameObject.GetComponentInParent<SP_PiecePositioning>() != null)
        {
            _isCollidingWithRingPiece = false;
            Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [SP_BallMovement] Ball stopped colliding with ring piece (tagged Ground) on {collision.gameObject.name}");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "Cylinder1" && _cylinder2 != null)
        {
            Vector3 currentPos = _cylinder2.transform.position;
            Vector3 newPos = new Vector3(currentPos.x, _cylinderPositionY, currentPos.z);
            Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [SP_BallMovement] Player {transform.parent.tag} - Changing Cylinder2 position to {newPos}");
            SetCylinder2Position(newPos);
            _cylinderPositionY -= 24f;
            Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [SP_BallMovement] Cylinder1 pos changed");
        }
        else if (other.name == "Cylinder2" && _cylinder1 != null)
        {
            Vector3 currentPos = _cylinder1.transform.position;
            Vector3 newPos = new Vector3(currentPos.x, _cylinderPositionY, currentPos.z);
            Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [SP_BallMovement] Player {transform.parent.tag} - Changing Cylinder1 position to {newPos}");
            SetCylinder1Position(newPos);
            _cylinderPositionY -= 24f;
            Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [SP_BallMovement] Cylinder2 pos changed");
        }
    }

    public void SetCylinder1Position(Vector3 position)
    {
        if (_cylinder1 != null)
        {
            _cylinder1.transform.position = position;
            Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [SP_BallMovement] Player {transform.parent.tag} - Applied position change: Cylinder1 set to {position}");
        }
        else
        {
            Debug.LogError($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [SP_BallMovement] Cylinder1 is null during SetCylinder1Position!");
        }
    }

    public void SetCylinder2Position(Vector3 position)
    {
        if (_cylinder2 != null)
        {
            _cylinder2.transform.position = position;
            Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [SP_BallMovement] Player {transform.parent.tag} - Applied position change: Cylinder2 set to {position}");
        }
        else
        {
            Debug.LogError($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [SP_BallMovement] Cylinder2 is null during SetCylinder2Position!");
        }
    }
}