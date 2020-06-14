using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Object_Rotator : MonoBehaviour {

    
    public float time_Modifier, timer;
    public bool X_Sin, Y_Sin, Z_Sin;
    public Vector3 modifiers, ranges;
    private Vector3 m_original_rot;

    private void Update()
    {
        timer += Time.deltaTime * time_Modifier;
    }

    private void LateUpdate()
    {
        if (X_Sin)
        {
            modifiers.x = m_original_rot.x + Mathf.Sin(timer) * ranges.x;
        }
        if (Y_Sin)
        {
            modifiers.y = m_original_rot.y + Mathf.Sin(timer) * ranges.y;
        }
        if (Z_Sin)
        {
            modifiers.z = m_original_rot.z + Mathf.Sin(timer) * ranges.z;
        }


        transform.Rotate(modifiers);
    }
}
