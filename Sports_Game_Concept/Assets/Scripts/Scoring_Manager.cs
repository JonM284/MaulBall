using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scoring_Manager : MonoBehaviour {

    public static Scoring_Manager sm_Inst;

    public List<Player_Behaviour> all_Players;
    public List<Vector3> all_Initial_Positions;

    public int[] team_ID = new int[2];

    private void Awake()
    {
        sm_Inst = this;
    }

    private void Start()
    {
        foreach (Player_Behaviour g in Resources.FindObjectsOfTypeAll(typeof(Player_Behaviour)))
        {
            all_Players.Add(g);
        }

        for (int i = 0; i < all_Players.Count; i++)
        {
            all_Initial_Positions.Add(all_Players[i].transform.position);
        }
    }

    public void Reset_All()
    {
        Debug.Log("Resetting Location of players");
        //reset player positions
        for (int i = 0; i < all_Players.Count; i++)
        {
            all_Players[i].transform.position = all_Initial_Positions[i];
        }
        //change location of goal a.k.a this

    }

    public void Add_Score(int _Team_ID, GameObject _accessing_Gameobject)
    {
        team_ID[_Team_ID - 1] += 1;

        Reset_All();
        _accessing_Gameobject.GetComponent<Goal_Behaviour>().Choose_New_Location();
    }

}
