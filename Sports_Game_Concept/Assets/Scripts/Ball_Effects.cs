using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball_Effects : MonoBehaviour {

    public ParticleSystem Catch_Kick_Effect;

    public TrailRenderer trail;

    private Rigidbody rb;

    
    private void Start()
    {
        Deactivate_Trail();
        rb = GetComponent<Rigidbody>();
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


    


}
