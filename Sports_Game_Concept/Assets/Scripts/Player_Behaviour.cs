using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class Player_Behaviour : MonoBehaviour {


    public float speed, gravity, sprint_speed_Mod, rot_Mod;
    public float ball_Force;
    public int Player_ID, Team_ID;

    public Transform Ball_Held_Pos;
    public bool player_Controlled;
    public Vector3 vel;
    public TrailRenderer running_Trail;

    [Header("Ground ray variables")]
    [SerializeField]private float ground_Ray_Rad;
    [SerializeField]private float ground_Ray_Dist;

    [Header("Passing Variables")]
    public float reg_Pass_H_Offset;
    public float lob_Pass_H_Offset;

    [Header("Player Variables")]
    public float tackle_Dur_Max;
    public float slide_Tackle_Dur_Max, attack_Speed_Cooldown_Max, pass_Range, Input_Speed, slide_Tackle_Speed_Mod,
        tackle_Speed_Mod, damage_Cooldown_Max, tackle_Damage_Cooldown = 0.5f, slide_Tackle_Damage_Cooldown = 1.5f;

    public List<Player_Behaviour> accept_Teammates, passable_Teammates;


    private Rigidbody rb;
    private float m_speed_Modifier = 1;
    private Vector3 rayDir;
    private float m_Input_X, m_Input_Y, m_Horizontal_Comp, m_Vertical_Comp, m_anti_Bump_Factor = 0.75f;
    private float m_Ball_Throw_Cooldown = 0.5f, m_Orig_Cooldown, m_Tackle_Duration, m_Slide_Tackle_Duration
        , m_original_Speed, m_Attack_Speed_Cooldown = 1f, m_Time_To_Reach, m_Damage_Cooldown, m_DC_Max_Original, m_Electric_Damage_Cooldown;
    [HideInInspector] public GameObject m_Owned_Ball;
    private Player m_Player;
    private bool m_can_Catch_Ball = true, m_Is_Holding_Lob = false, m_Is_Tackling = false, m_Is_Slide_Tackling = false
        , m_Read_Player_Inputs = true, m_Has_Attacked = false, m_Taking_Damage = false;
    private ParticleSystem impact_PS;

    [SerializeField] private bool m_Is_Moving, m_Is_Being_Passed_To = false;
    private Vector3 m_Ball_End_Position, damage_Dir;


    private void Awake()
    {
        foreach (Player_Behaviour g in Resources.FindObjectsOfTypeAll(typeof(Player_Behaviour)))
        {
            if (g.GetComponent<Player_Behaviour>().Team_ID == this.Team_ID && g.GetInstanceID() != this.GetInstanceID())
            {
                accept_Teammates.Add(g);
            }
        }

        impact_PS = transform.Find("Hit_Effect").GetComponent<ParticleSystem>();

    }

    // Use this for initialization
    void Start () {
        rb = GetComponent<Rigidbody>();
        m_Player = ReInput.players.GetPlayer(Player_ID - 1);
        m_Orig_Cooldown = m_Ball_Throw_Cooldown;
        m_original_Speed = speed;
        m_Attack_Speed_Cooldown = attack_Speed_Cooldown_Max;
        m_DC_Max_Original = damage_Cooldown_Max;
        m_Electric_Damage_Cooldown = damage_Cooldown_Max + 1.5f;
    }

    private void FixedUpdate()
    {
        if (m_Read_Player_Inputs) {
            Movement();
        }else
        {
            if (m_Is_Slide_Tackling || m_Is_Tackling) {
                Do_Dash(transform.forward);
            }else if (m_Taking_Damage)
            {
                Do_Dash(damage_Dir);
            }
        }
    }

    // Update is called once per frame
    void Update () {
        if (player_Controlled) {
            m_Horizontal_Comp = m_Player.GetAxisRaw("Horizontal");
            m_Vertical_Comp = m_Player.GetAxisRaw("Vertical");
        }else
        {
            m_Horizontal_Comp = 0;
            m_Vertical_Comp = 0;
        }

        Check_Inputs();
        Check_Cooldowns();

    }

    


    void Movement()
    {

        m_Input_X = Mathf.Lerp(m_Input_X, m_Horizontal_Comp, Time.deltaTime * Input_Speed);
        m_Input_Y = Mathf.Lerp(m_Input_Y, m_Vertical_Comp, Time.deltaTime * Input_Speed);

        
        vel.x = m_Input_X * speed;
        vel.z = m_Input_Y * speed;
        

        if (!m_Is_Grounded())
        {
            vel.y -= gravity * Time.deltaTime;
        }else
        {
            vel.y = 0;
        }


        m_speed_Modifier = (m_Player.GetButton("Sprint") && m_Owned_Ball !=null) ? sprint_speed_Mod : 1;
        if (running_Trail != null) {
            running_Trail.enabled = (m_Player.GetButton("Sprint") && m_Owned_Ball != null) ? true : false;
        }


        ///change forward direction
        Vector3 tempDir = new Vector3(m_Input_X, 0, m_Input_Y);


        if (tempDir.magnitude > 0.1f)
        {
            rayDir = tempDir.normalized;
        }
        if (m_Is_Grounded())
        {
            transform.forward = Vector3.Slerp(transform.forward, rayDir, Time.deltaTime * rot_Mod);
        }

        if (player_Controlled)
        {
            Find_Players_In_Range(rayDir);
        }

        rb.MovePosition(rb.position + new Vector3(Mathf.Clamp(vel.x, -speed, speed), 
            vel.y, Mathf.Clamp(vel.z, -speed, speed)) * m_speed_Modifier * Time.deltaTime);

        
    }

    void Do_Dash(Vector3 _dash_Dir)
    {
        vel.x = _dash_Dir.x * speed;
        vel.z = _dash_Dir.z * speed;
        

        rb.MovePosition(rb.position + new Vector3(Mathf.Clamp(vel.x, -speed, speed),
            vel.y, Mathf.Clamp(vel.z, -speed, speed)) * m_speed_Modifier * Time.deltaTime);
    }

    void Check_Inputs()
    {
        //press w/o ball to pass, press w/ ball to swap players
        if (m_Player.GetButtonDown("S_Pass") && m_Owned_Ball != null)
        {
            //throw ball at closer teammate
            Throw_Ball();
        }
        else if (m_Player.GetButtonDown("S_Pass") && m_Owned_Ball == null && player_Controlled)
        {
            //Swap players
            Swap_Players();
        }

        // press to steal
        if (m_Player.GetButtonDown("D_Tackle") && m_Owned_Ball == null && player_Controlled && !m_Taking_Damage)
        {
            //attempt to steal
            Tackle();
        }

        //hold to preform a lob pass
        m_Is_Holding_Lob = (m_Player.GetButton("S_Lob") && m_Owned_Ball != null) ? true : false;

        //press without ball to preform a MAUL
        if (m_Player.GetButtonDown("S_Lob") && m_Owned_Ball == null && player_Controlled && !m_Taking_Damage)
        {
            //Maul
            Slide_Tackle();
        }

        //press to use primary ability
        if (m_Player.GetButtonDown("Ability_1") && !m_Taking_Damage)
        {
            //Do ability
            //Start Cooldown
        }

        //press to use secondary ability
        if (m_Player.GetButtonDown("Ability_2") && !m_Taking_Damage)
        {
            //Do ability
            //Start Cooldown
        }

        //check whether or not player is moving
        if (Mathf.Abs(m_Horizontal_Comp) > 0.1f || Mathf.Abs(m_Vertical_Comp) > 0.1f)
        {
            m_Is_Moving = true;
        }else if (m_Horizontal_Comp == 0 && m_Vertical_Comp == 0)
        {
            m_Is_Moving = false;
        }

        
    }

    void Check_Cooldowns()
    {
        if (!m_can_Catch_Ball && m_Ball_Throw_Cooldown > 0)
        {
            m_Ball_Throw_Cooldown -= Time.deltaTime;
        }

        if (m_Ball_Throw_Cooldown <= 0 && !m_can_Catch_Ball)
        {
            m_can_Catch_Ball = true;
            m_Ball_Throw_Cooldown = m_Orig_Cooldown;
        }

        if (m_Slide_Tackle_Duration <= slide_Tackle_Dur_Max && m_Is_Slide_Tackling)
        {
            m_Slide_Tackle_Duration += Time.deltaTime;
        }

        if ((m_Slide_Tackle_Duration >= slide_Tackle_Dur_Max && m_Is_Slide_Tackling) || (m_Wall_In_Front() && m_Is_Slide_Tackling))
        {
            m_Slide_Tackle_Duration = 0;
            m_Is_Slide_Tackling = false;
            m_Read_Player_Inputs = true;
            Slow_Speed();
           
        }

        if (m_Tackle_Duration <= tackle_Dur_Max && m_Is_Tackling)
        {
            m_Tackle_Duration += Time.deltaTime;
        }

        if ((m_Tackle_Duration >= tackle_Dur_Max && m_Is_Tackling) || (m_Wall_In_Front() && m_Is_Tackling))
        {
            m_Tackle_Duration = 0;
            m_Is_Tackling = false;
            m_Read_Player_Inputs = true;
            Slow_Speed();
        }

        if (m_Attack_Speed_Cooldown >= 0 && m_Has_Attacked)
        {
            m_Attack_Speed_Cooldown -= Time.deltaTime;
        }

        if (m_Attack_Speed_Cooldown <= 0 && m_Has_Attacked)
        {
            Reset_Speed();
        }

        if (!player_Controlled && m_Is_Being_Passed_To && m_Time_To_Reach > 0)
        {
            m_Time_To_Reach -= Time.deltaTime;
            Vector3 dir_To_Ball = m_Ball_End_Position - transform.position;
            rayDir.x = dir_To_Ball.normalized.x;
            rayDir.z = dir_To_Ball.normalized.z;
            Debug.Log("Moving Dir: " +rayDir);
        }

        if (m_Time_To_Reach <= 0 && !player_Controlled && m_Is_Being_Passed_To)
        {
            player_Controlled = true;
            m_Is_Being_Passed_To = false;
        }

        if (m_Damage_Cooldown <= damage_Cooldown_Max && m_Taking_Damage)
        {
            m_Damage_Cooldown += Time.deltaTime;
            if (!m_Wall_In_Damage_Dir()) {
                float prc = m_Damage_Cooldown / damage_Cooldown_Max;
                speed = Mathf.Lerp(speed, 0, prc);

            }else
            {
                speed = 0;
                damage_Cooldown_Max = m_Electric_Damage_Cooldown;
            }
        }

        if (m_Damage_Cooldown >= damage_Cooldown_Max && m_Taking_Damage)
        {
            m_Damage_Cooldown = 0;
            if (damage_Cooldown_Max != m_DC_Max_Original) damage_Cooldown_Max = m_DC_Max_Original;
            m_Taking_Damage = false;
            m_Read_Player_Inputs = true;
            Slow_Speed();
        }
       
    }

    public void Slow_Speed()
    {
        m_Has_Attacked = true;

        float _Slowed_Speed = m_original_Speed/3f;
        speed = _Slowed_Speed;

        m_Attack_Speed_Cooldown = attack_Speed_Cooldown_Max;
    }

    public void Reset_Speed()
    {
        speed = m_original_Speed;
        m_Has_Attacked = false;
    }

    void Tackle()
    {
        m_Is_Tackling = true;
        m_Read_Player_Inputs = false;
        float _tackle_Speed = speed * tackle_Speed_Mod;
        speed = _tackle_Speed;
    }

    void Slide_Tackle()
    {
        m_Is_Slide_Tackling = true;
        m_Read_Player_Inputs = false;
        float _tackle_Speed = speed * slide_Tackle_Speed_Mod;
        speed = _tackle_Speed;
    }

    public void Swap_Possessor(GameObject _new_Owner)
    {
        m_Owned_Ball.transform.parent = null;
        _new_Owner.GetComponent<Player_Behaviour>().m_Owned_Ball = this.m_Owned_Ball;
        m_Owned_Ball.transform.parent = _new_Owner.GetComponent<Player_Behaviour>().Ball_Held_Pos;
        m_Owned_Ball.transform.position = _new_Owner.GetComponent<Player_Behaviour>().Ball_Held_Pos.position;
        m_Owned_Ball = null;

        Camera_Behaviour.cam_Inst.Update_Target(_new_Owner.transform);
    }

    Vector3 Find_Players_In_Range(Vector3 _dir)
    {
        for (int i = 0; i < accept_Teammates.Count; i++)
        {
            Vector3 dir_To_Team = accept_Teammates[i].transform.position - transform.position;
            float angle = Vector3.Angle(_dir, dir_To_Team);
            
            if (angle < 20f)
            {
                Debug.Log("Found Match");
                Debug.DrawRay(transform.position, dir_To_Team, Color.red);
                if (!passable_Teammates.Contains(accept_Teammates[i]))
                {
                    passable_Teammates.Add(accept_Teammates[i]);
                    return passable_Teammates[0].transform.position;
                }
                
            }else if (angle > 20f && passable_Teammates.Contains(accept_Teammates[i]))
            {
                passable_Teammates.Remove(accept_Teammates[i]);
            }
        }

        return transform.forward;
    }

    void Swap_Players()
    {

        if (passable_Teammates.Count >= 2)
        {
            float angle1 = Vector3.Angle(rayDir, passable_Teammates[0].transform.position - transform.position);
            float angle2 = Vector3.Angle(rayDir, passable_Teammates[1].transform.position - transform.position);
            if (angle2 < angle1)
            {
                passable_Teammates[0] = passable_Teammates[1];
            }
        }

        if (passable_Teammates.Count > 0) {
            player_Controlled = false;
            passable_Teammates[0].player_Controlled = true;
        }

        if (passable_Teammates.Count > 0)
        {
            for (int i = 0; i < passable_Teammates.Count; i++)
            {
                passable_Teammates.Remove(passable_Teammates[i]);
            }
        }
    }

    /// <summary>
    /// apply force to the ball
    /// </summary>
    void Throw_Ball()
    {
        m_can_Catch_Ball = false;
        m_Owned_Ball.transform.parent = null;
        if (!m_Taking_Damage) {
            m_Owned_Ball.GetComponent<Ball_Effects>().Play_Catch_Kick();
        }
        m_Owned_Ball.GetComponent<Rigidbody>().isKinematic = false;
        float _mag = 0;
        
        if (passable_Teammates.Count > 0) {
            if (passable_Teammates.Count >= 2)
            {
                float dist1 = Vector3.Magnitude(passable_Teammates[0].transform.position - transform.position);
                float dist2 = Vector3.Magnitude(passable_Teammates[1].transform.position - transform.position);
                if (dist2 < dist1)
                {
                    passable_Teammates[0] = passable_Teammates[1];
                }
            }
            _mag = Vector3.Magnitude(passable_Teammates[0].transform.position - transform.position);
        }
        Debug.Log("Dist: " + _mag );

        if ((passable_Teammates.Count <= 0 || _mag > pass_Range) && !m_Taking_Damage) {
            m_Owned_Ball.GetComponent<Rigidbody>().AddForce(transform.forward * ball_Force * m_speed_Modifier);
        }else if(passable_Teammates.Count > 0 && _mag < pass_Range && !m_Taking_Damage)
        {
            Physics.gravity = Vector3.up * -gravity;
            m_Owned_Ball.GetComponent<Rigidbody>().AddForce(Calc_Vel(), ForceMode.VelocityChange);
           
            player_Controlled = false;
            passable_Teammates[0].player_Controlled = true;
        }else if (m_Taking_Damage)
        {
            Vector3 random_Dir = Vector3.zero;
            if (m_Wall_In_Damage_Dir()) {
                damage_Dir *= -1;
                random_Dir = new Vector3(damage_Dir.x + Random.Range(-0.4f, 0.4f), Random.Range(0f, 1f), damage_Dir.z + Random.Range(-0.4f, 0.4f));
            }else
            {
                random_Dir = new Vector3(damage_Dir.x + Random.Range(-0.4f, 0.4f), Random.Range(0f, 2f), damage_Dir.z + Random.Range(-0.4f, 0.4f));
            }
            float random_Force = Random.Range(ball_Force/2, ball_Force);
            m_Owned_Ball.GetComponent<Rigidbody>().AddForce((random_Dir) * random_Force);
        }
        m_Owned_Ball.GetComponent<Collider>().enabled = true;
        m_Owned_Ball.GetComponent<Ball_Effects>().Activate_Trail();
        m_Owned_Ball.GetComponent<Rigidbody>().useGravity = true;
        m_Owned_Ball.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        
        m_Owned_Ball = null;

        if (passable_Teammates.Count > 0) {
            for (int i = 0; i < passable_Teammates.Count; i++)
            {
                passable_Teammates.Remove(passable_Teammates[i]);
            }
        }
        Camera_Behaviour.cam_Inst.Reset_Target();
    }

    public void Passed_To(Vector3 _end_Pos, float _time)
    {
        m_Is_Being_Passed_To = true;
        m_Ball_End_Position = _end_Pos;
        m_Time_To_Reach = _time;
    }

    public void Take_Damage(Vector3 _attacker_Pos, float _damage_Cooldown, bool _Is_Stealing)
    {
        damage_Dir = (_attacker_Pos - transform.position) * -1f;
        
        float _damage_Speed = speed * tackle_Speed_Mod * 2f;
        speed = _damage_Speed;
        m_Taking_Damage = true;
        m_Read_Player_Inputs = false;
        damage_Cooldown_Max = _damage_Cooldown;
        
        if (m_Owned_Ball != null) {
            if (!_Is_Stealing) {
                Throw_Ball();
            }
            Camera_Behaviour.cam_Inst.Do_Camera_Shake(0.3f, 0.6f);
        }
        else
        {
            Camera_Behaviour.cam_Inst.Do_Camera_Shake(0.1f, 0.2f);
        }
        impact_PS.Play();
    }

    

    //this calculation was made with the help of this youtube video https://www.youtube.com/watch?v=IvT8hjy6q4o
    Vector3 Calc_Vel()
    {
        float h = 0f;
        if (m_Is_Holding_Lob) {
            h = passable_Teammates[0].transform.position.y + lob_Pass_H_Offset;
        }else
        {
            h = passable_Teammates[0].transform.position.y + reg_Pass_H_Offset;
        }
        float displacementY = passable_Teammates[0].transform.position.y - m_Owned_Ball.transform.position.y;
        
        Vector3 displacementXZ = new Vector3(passable_Teammates[0].transform.position.x - m_Owned_Ball.transform.position.x, 0,
        passable_Teammates[0].transform.position.z - m_Owned_Ball.transform.position.z) 
        + vel;
        
        float _time = Mathf.Sqrt(-2 * h / -gravity) + Mathf.Sqrt(2 * (displacementY - h) / -gravity);
        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * -gravity * h);
        Vector3 velocityXZ = displacementXZ / _time;


        return velocityXZ + velocityY;
    }


    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Ball" && other.gameObject.transform.parent == null && m_can_Catch_Ball)
        {
            Pick_Up_Ball(other.gameObject);
            
        }


        if (other.gameObject.tag == "Player" && (m_Is_Tackling || m_Is_Slide_Tackling))
        {
            if (m_Is_Tackling) {
                other.gameObject.GetComponent<Player_Behaviour>().Take_Damage(transform.position, tackle_Damage_Cooldown, true);
                if (other.gameObject.GetComponent<Player_Behaviour>().m_Owned_Ball != null) {
                    other.gameObject.GetComponent<Player_Behaviour>().Swap_Possessor(this.gameObject);
                }
            }else if (m_Is_Slide_Tackling)
            {
                other.gameObject.GetComponent<Player_Behaviour>().Take_Damage(transform.position, slide_Tackle_Damage_Cooldown, false);
                m_can_Catch_Ball = false;
            }

            
        }

        rb.velocity = Vector3.zero;
    }



    private void OnCollisionExit(Collision collision)
    {
        rb.velocity = Vector3.zero;
    }


    void Pick_Up_Ball(GameObject other)
    {
        if (!player_Controlled)
        {
            player_Controlled = true;
        }

        for (int i = 0; i < accept_Teammates.Count; i++)
        {
            if (accept_Teammates[i].player_Controlled)
            {
                accept_Teammates[i].player_Controlled = false;
            }
        }
        m_Owned_Ball = other.gameObject;
        m_Owned_Ball.GetComponent<Collider>().enabled = false;
        m_Owned_Ball.GetComponent<Rigidbody>().useGravity = false;
        m_Owned_Ball.transform.parent = Ball_Held_Pos;
        m_Owned_Ball.transform.position = Ball_Held_Pos.position;
        m_Owned_Ball.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
        m_Owned_Ball.GetComponent<Rigidbody>().isKinematic = true;
        
        m_Owned_Ball.GetComponent<Ball_Effects>().Deactivate_Trail();
        m_Owned_Ball.GetComponent<Ball_Effects>().Play_Catch_Kick();
        Camera_Behaviour.cam_Inst.Update_Target(this.transform);
    }



    bool m_Is_Grounded()
    {
        RaycastHit hit;
        if (Physics.SphereCast(transform.position, ground_Ray_Rad, Vector3.down, out hit, ground_Ray_Dist))
        {
            if (hit.collider.tag == "Ground")
            {
                return true;
            }
        }
        return false;

        
    }

    bool m_Wall_In_Front()
    {
        RaycastHit hit;
        if (Physics.SphereCast(transform.position, ground_Ray_Rad, transform.forward, out hit, ground_Ray_Dist))
        {
            if (hit.collider.tag == "Wall")
            {
                return true;
            }
        }
        return false;
    }

    bool m_Wall_In_Damage_Dir()
    {
        RaycastHit hit;
        if (Physics.SphereCast(transform.position, ground_Ray_Rad, damage_Dir, out hit, ground_Ray_Dist))
        {
            if (hit.collider.tag == "Wall")
            {
                return true;
            }
        }
        return false;
    }

    ///////////////////////////////// GIZMO DRAWS
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + Vector3.down * ground_Ray_Dist, ground_Ray_Rad);
        Gizmos.DrawRay(transform.position, damage_Dir);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position + transform.forward * ground_Ray_Dist, ground_Ray_Rad);
    }
}
