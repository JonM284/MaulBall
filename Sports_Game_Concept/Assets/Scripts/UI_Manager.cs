using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Manager : MonoBehaviour {

    public Queue<UI_Follower> indicator;
    public List<Player_Behaviour> all_Players;
    public List<UI_Follower> All_Indicators;

    // Use this for initialization
    void Start () {

        foreach (UI_Follower u in Resources.FindObjectsOfTypeAll(typeof(UI_Follower)))
        {
            All_Indicators.Add(u);
        }


        foreach (Player_Behaviour g in Resources.FindObjectsOfTypeAll(typeof(Player_Behaviour)))
        {
            all_Players.Add(g);
            if (g.Player_ID > 0 && g.player_Controlled)
            {
                All_Indicators[0].target = g.gameObject;
                All_Indicators[0].player_ID = g.Player_ID;
                All_Indicators[0].Update_Player_To_Use();
                All_Indicators.Remove(All_Indicators[0]);
            }
        }
    }
	
	
}
