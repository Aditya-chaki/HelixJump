using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallBounce : MonoBehaviour
{

    Rigidbody rb;
    private float _bounceHeight = 5;
    [SerializeField] private GameObject _cylinder1, _cylinder2;

    // [SerializeField]  AudioSource jumpsource;
    // [SerializeField]  AudioClip  jumpclip;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }


    public void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Platform"))
        {
            rb.linearVelocity = new Vector3(0, _bounceHeight, 0);
            // jumpsource.PlayOneShot(jumpclip);

        }
    }
    


}

