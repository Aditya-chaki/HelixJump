using UnityEngine;
using System.Collections; // Required for IEnumerator and coroutines

public class SP_BallMovement : MonoBehaviour
{
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private float _jumpForce;
    [SerializeField] private GameObject _cylinder1, _cylinder2;
    [SerializeField] private GameObject _speedEffectObject; // GameObject to activate/deactivate
    [SerializeField] private float _speedThreshold = 10f; // Speed threshold for activation
    [SerializeField] private AudioSource _audioSource;

    private float _cylinderPositionY = -24f;
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
    }
    void Update()
    {
         // Check ball speed and toggle speed effect GameObject
        if (_speedEffectObject != null)
        {
            float currentSpeed = _rigidbody.linearVelocity.magnitude;
            bool shouldBeActive = currentSpeed > _speedThreshold;
            if (_speedEffectObject.activeSelf != shouldBeActive)
            {
                _speedEffectObject.SetActive(shouldBeActive);
                // Debug.Log($"[BallMovement] SpeedEffectObject {(shouldBeActive ? "activated" : "deactivated")} at speed {currentSpeed}");
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            _rigidbody.linearVelocity= Vector3.up * _jumpForce;

            if (!IsAI)
            {
                _audioSource.Play();
            }
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

            // Disable Cylinder1's collider for 2 seconds
            Collider cylinderCollider = other.GetComponent<Collider>();
            if (cylinderCollider != null)
            {
                cylinderCollider.enabled = false;
                StartCoroutine(ReEnableCollider(cylinderCollider, 2f));
            }
        }
        else if (other.name == "Cylinder2" && _cylinder1 != null)
        {
            Vector3 currentPos = _cylinder1.transform.position;
            Vector3 newPos = new Vector3(currentPos.x, _cylinderPositionY, currentPos.z);
            Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [SP_BallMovement] Player {transform.parent.tag} - Changing Cylinder1 position to {newPos}");
            SetCylinder1Position(newPos);
            _cylinderPositionY -= 24f;
            Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [SP_BallMovement] Cylinder2 pos changed");

            // Disable Cylinder2's collider for 2 seconds
            Collider cylinderCollider = other.GetComponent<Collider>();
            if (cylinderCollider != null)
            {
                cylinderCollider.enabled = false;
                StartCoroutine(ReEnableCollider(cylinderCollider, 2f));
            }
        }
    }

    private IEnumerator ReEnableCollider(Collider collider, float delay)
    {
        yield return new WaitForSeconds(delay);
        collider.enabled = true;
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