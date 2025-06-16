using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{

	public LayerMask ground;

	private const float jumpForce = 10;

	private Rigidbody rb;

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
	}



	private void OnTriggerEnter(Collider other)
	{
		Debug.Log($"[Ball] TriggerEnter with {other.gameObject.name}, layer={LayerMask.LayerToName(other.gameObject.layer)}");
		if (((1 << other.gameObject.layer) & ground) == 0)
		{
			return;
		}
		rb.linearVelocity = Vector3.zero;
		rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
		
	}

	public void GameOver()
	{
		rb.linearVelocity = Vector3.zero;
		rb.useGravity = false;
	}

}
