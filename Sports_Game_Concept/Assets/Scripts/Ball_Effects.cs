using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball_Effects : MonoBehaviour {

    public ParticleSystem Catch_Kick_Effect;

    public TrailRenderer trail;

    private Rigidbody rb;

    private Vector3 starting_Pos;

    public float bounce_Force;

    
    private void Start()
    {
        Deactivate_Trail();
        rb = GetComponent<Rigidbody>();
    }

    private void LateUpdate()
    {
        if (transform.position.y < -1f)
        {
            transform.position = starting_Pos;
        }
    }

    public void Activate_Trail()
    {
        trail.enabled = true;
    }

    public void Deactivate_Trail()
    {
        trail.enabled = false;
    }

    

    public void Play_Catch_Kick()
    {
        Catch_Kick_Effect.Play();
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Wall")
        {
            Vector3 _dir = other.contacts[0].point - transform.position;
            Vector3 _opposite_Dir = _dir.normalized * -1;
            rb.AddForce(_opposite_Dir * bounce_Force);
            
        }
    }



}
