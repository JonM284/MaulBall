using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability")]
public class Ability : ScriptableObject {

    public enum ability_Type
    {
        PROJECTILE,
        STATUS,
        AREA,
        SPAWNING_OBJECT,
        REPEATER,
        DASH
    }

    public string ability_Name;
    [TextArea(3, 5)]
    public string  ability_Description;
    public ability_Type a_type;
    [Tooltip("Projectile to be fired, if this ability calls for it. proj behaviour is on projectile.")]
    public GameObject projectile;
    [Tooltip("Duration of this ability.")]
    public float duration;
    [Tooltip("Cooldown time for this ability.")]
    public float cooldown;
    [Tooltip("Interval time for repeatable abilities.")]
    public float repeat_Time;
    [Tooltip("Speed of the ability, if it calls for it.")]
    public float ability_Speed;
    [HideInInspector]
    public Player_Behaviour my_player;
    [Tooltip("Causes the user to become invulnerable for the duration of the ability")]
    public bool causes_Invul;


    public void SetUp_Ability(Player_Behaviour _character, int ID_Number) {
        my_player = _character;

        my_player.ability_Cooldown[ID_Number] = cooldown;
        my_player.ability_Duration[ID_Number] = duration;
        my_player.ability_Repeater_Time[ID_Number] = repeat_Time;
        switch (a_type)
        {
            case ability_Type.DASH:
                my_player.ability_Type_ID[ID_Number] = (int)ability_Type.DASH;
                break;
            case ability_Type.REPEATER:
                my_player.ability_Type_ID[ID_Number] = (int)ability_Type.REPEATER;
                break;
            case ability_Type.SPAWNING_OBJECT:
                my_player.ability_Type_ID[ID_Number] = (int)ability_Type.SPAWNING_OBJECT;
                break;
            case ability_Type.AREA:
                my_player.ability_Type_ID[ID_Number] = (int)ability_Type.AREA;
                break;
            case ability_Type.STATUS:
                my_player.ability_Type_ID[ID_Number] = (int)ability_Type.STATUS;
                break;
            case ability_Type.PROJECTILE:
                my_player.ability_Type_ID[ID_Number] = (int)ability_Type.PROJECTILE;
                break;
        }

        Debug.Log("Fully added ability");
    }

    public void Use_Ability() {
        switch (a_type)
        {
            case ability_Type.DASH:
                my_player.Initiate_Dash_Type(false, false, true, duration, ability_Speed);
                if (causes_Invul) {
                    my_player.Initiate_Invulnerability(true, duration);
                }
                break;
            case ability_Type.REPEATER:
                
                break;
            case ability_Type.SPAWNING_OBJECT:
                
                break;
            case ability_Type.AREA:
                
                break;
            case ability_Type.STATUS:
                if (causes_Invul) {
                    my_player.Initiate_Invulnerability(true, duration);
                }
                break;
            case ability_Type.PROJECTILE:

                break;
        }

        Debug.Log(ability_Name);
    }
	
}
