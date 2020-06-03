﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Goal_Behaviour : MonoBehaviour {


    
    public int team_ID;
    public float time_To_Score;
    public Transform[] spawn_Zones;
    public TMP_Text health_Text;
    

    private bool m_Ball_In_Zone = false;
    private float m_Current_Used_Time;

    

	// Use this for initialization
	void Start () {
        if (health_Text != null)
        {
            health_Text.gameObject.name = "Goal" +team_ID+ "Text";
        }
    }
	
	// Update is called once per frame
	void Update () {
        if (m_Ball_In_Zone && m_Current_Used_Time <= time_To_Score)
        {
            m_Current_Used_Time += Time.deltaTime;
        }

        if (m_Ball_In_Zone && m_Current_Used_Time >= time_To_Score)
        {
            Add_Score();
        }

        if (health_Text != null) {
            int _health_Amount = Mathf.RoundToInt(time_To_Score) - Mathf.CeilToInt(m_Current_Used_Time);
            health_Text.text = _health_Amount.ToString();
        }
	}

   

    private void OnBecameVisible()
    {
        health_Text.gameObject.SetActive(true);
    }

    private void OnBecameInvisible()
    {
        health_Text.gameObject.SetActive(false);
    }

    void Reset_Timer()
    {
        Debug.Log("Resetting Timer");
        m_Current_Used_Time = 0;
    }

    void Add_Score()
    {
        m_Ball_In_Zone = false;
        Reset_Timer();
        try
        {
            Scoring_Manager.sm_Inst.Add_Score(team_ID, this.gameObject);
        }
        catch
        {
            Debug.LogAssertion("Cannot find instance of SCORING MANAGER, please add to scene.");
        }
    }

    public void Choose_New_Location()
    {
        //choose new location
        Debug.Log("Choosing new spot");
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player" && other.gameObject.GetComponent<Player_Behaviour>().m_Owned_Ball != null)
        {
            m_Ball_In_Zone = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if ((other.gameObject.tag == "Player" && other.gameObject.GetComponent<Player_Behaviour>().m_Owned_Ball != null) || other.gameObject.tag == "Ball")
        {
            m_Ball_In_Zone = false;
        }
    }
}
