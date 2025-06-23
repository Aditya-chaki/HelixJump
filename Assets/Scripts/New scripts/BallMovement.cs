using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class BallMovement : NetworkBehaviour
{
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private float _jumpForce;
    [SerializeField] private GameObject _cylinder1, _cylinder2;
    [SerializeField] private GameObject _speedEffectObject; // GameObject to activate/deactivate
    [SerializeField] private float _speedThreshold = 10f; // Speed threshold for activation

    [Networked] private float _cylinderPositionY { get; set; } = -24f;
    [SerializeField] private AudioSource _audioSource;

    private bool shouldJump = false;
    private Collider _cylinder1Collider;
    private Collider _cylinder2Collider;

    [Networked] private float Cylinder1DisableUntil { get; set; } = 0f;
    [Networked] private float Cylinder2DisableUntil { get; set; } = 0f;

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
            Debug.LogError("[BallMovement] Cylinder1 or Cylinder2 not found!");
        }
        else
        {
            _cylinder1Collider = _cylinder1.GetComponent<Collider>();
            _cylinder2Collider = _cylinder2.GetComponent<Collider>();
        }
        if (_audioSource == null)
        {
            _audioSource = GetComponent<AudioSource>();
        }
        if (_audioSource == null)
        {
            Debug.LogError("[BallMovement] AudioSource not found!");
        }
        if (_speedEffectObject == null)
        {
            Debug.LogWarning("[BallMovement] SpeedEffectObject not assigned!");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            if (Object.HasStateAuthority)
            {
                shouldJump = true;
            }
            if (Object.InputAuthority == Runner.LocalPlayer && _audioSource != null)
            {
                _audioSource.Play();
            }
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (shouldJump)
        {
            _rigidbody.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
            shouldJump = false;
        }

        // Check ball speed and toggle speed effect GameObject
        if (_speedEffectObject != null && Object.HasStateAuthority)
        {
            float currentSpeed = _rigidbody.linearVelocity.magnitude;
            bool shouldBeActive = currentSpeed > _speedThreshold;
            if (_speedEffectObject.activeSelf != shouldBeActive)
            {
                _speedEffectObject.SetActive(shouldBeActive);
            }
        }

        // Manage cylinder colliders
        if (_cylinder1Collider != null)
        {
            _cylinder1Collider.enabled = Runner.SimulationTime >= Cylinder1DisableUntil;
        }
        if (_cylinder2Collider != null)
        {
            _cylinder2Collider.enabled = Runner.SimulationTime >= Cylinder2DisableUntil;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only the state authority should initiate position changes and collider disabling
        if (Object.HasStateAuthority)
        {
            if (other.name == "Cylinder1" && _cylinder2 != null)
            {
                Cylinder1DisableUntil = Runner.SimulationTime + 2f;
                Vector3 currentPos = _cylinder2.transform.position;
                Vector3 newPos = new Vector3(currentPos.x, _cylinderPositionY, currentPos.z);
                Debug.Log($"[BallMovement] Player {transform.parent.tag} - Changing Cylinder2 position to {newPos}");
                RPC_SetCylinder2Position(newPos);
                _cylinderPositionY -= 24f;
                Debug.Log("Cylinder1 pos changed");
            }
            else if (other.name == "Cylinder2" && _cylinder1 != null)
            {
                Cylinder2DisableUntil = Runner.SimulationTime + 2f;
                Vector3 currentPos = _cylinder1.transform.position;
                Vector3 newPos = new Vector3(currentPos.x, _cylinderPositionY, currentPos.z);
                Debug.Log($"[BallMovement] Player {transform.parent.tag} - Changing Cylinder1 position to {newPos}");
                RPC_SetCylinder1Position(newPos);
                _cylinderPositionY -= 24f;
                Debug.Log("Cylinder2 pos changed");
            }
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SetCylinder1Position(Vector3 position)
    {
        if (_cylinder1 != null)
        {
            _cylinder1.transform.position = position;
            Debug.Log($"[BallMovement] Player {transform.parent.tag} - Applied position change: Cylinder1 set to {position}");
        }
        else
        {
            Debug.LogError("[BallMovement] Cylinder1 is null during RPC_SetCylinder1Position!");
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SetCylinder2Position(Vector3 position)
    {
        if (_cylinder2 != null)
        {
            _cylinder2.transform.position = position;
            Debug.Log($"[BallMovement] Player {transform.parent.tag} - Applied position change: Cylinder2 set to {position}");
        }
        else
        {
            Debug.LogError("[BallMovement] Cylinder2 is null during RPC_SetCylinder2Position!");
        }
    }
}