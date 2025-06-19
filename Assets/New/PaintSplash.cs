using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintSplash : MonoBehaviour
{
    [SerializeField] private GameObject _PaintSplashPrefab;
    [SerializeField] private float _destroyTime = 0.5f;
    [SerializeField] private float _raycastDistance = 1.0f; // Adjustable in Inspector
    [SerializeField] private LayerMask _surfaceLayer; // Set in Inspector

    void OnCollisionEnter(Collision other)
    {
        Vector3 direction = transform.TransformDirection(Vector3.down);
        Debug.DrawRay(transform.position, direction * _raycastDistance, Color.red, 2.0f); // Debug ray
        if (Physics.Raycast(transform.position, direction, out RaycastHit hitInfo, _raycastDistance, _surfaceLayer))
        {
            GameObject obj = Instantiate(_PaintSplashPrefab, hitInfo.point, _PaintSplashPrefab.transform.rotation);
            obj.transform.SetParent(other.gameObject.transform);
            obj.transform.position += obj.transform.TransformDirection(Vector3.back) / 1000;
            Destroy(obj, _destroyTime);
        }
    }
}