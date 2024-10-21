using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using Newtonsoft.Json;
using System.Text;
using System.Threading;

[RequireComponent(typeof(Animator),typeof(CharacterController))]

public class movements : MonoBehaviour
{
    [Header("Make Sure to add character controller,animator to your humanoid character")]
    CharacterController Char_Cont;
    Animator Anm;

    [SerializeField,Header("how fast or slow you want the game to be"),Range(0.5f,2)] float Game_Speed = 1;
    [SerializeField,Header("Place this transform at the feet of your character")] Transform Feet_Check,camera_dest,camera_dest_aiming;
    public Transform _Camera;
    [SerializeField] Vector3 gravity;
    [SerializeField,Header("Layer masks that detect player's feet collision")] LayerMask Feet_Check_Mask;
    float aimming = 0,tilt=0;
    [SerializeField] Vector3 camera_pos_offset;
    float cam_speed;
    Dictionary<string, Dictionary<string,float>> char_control_values;
    int my_int_1 = 0;
   
    public int health=100;
    [SerializeField] ParticleSystem bullets;
    [Header("this part is for grenades")]
    [SerializeField] Transform gr_instantiate_pos, gr_prefab,current_nade;

    public int score = 0;
    
    
    void Start()
    {
        
        Char_Cont = GetComponent<CharacterController>(); Anm = GetComponent<Animator>();
        Cursor.lockState = CursorLockMode.Locked;
        char_control_values = new Dictionary<string, Dictionary<string, float>> {
            { "standing", new Dictionary<string, float> 
            { 
                { "radius", .5f },{"height",1.86f},{"centerY",0.95f}
            } 
            
            }
            ,{"crouching",new Dictionary<string, float>
            { { "radius", .5f },{"height",1.36f},{"centerY",1.18f}}
            }
        
        
        };
        

    }
    
    // Update is called once per frame
    void Update()
    {
        Anm.SetBool("die", health <= 0);
        //make sure accelration toward the ground doesnt happent,if player is grounded
        if(Physics.CheckSphere(Feet_Check.position,0.25f,Feet_Check_Mask)) { gravity.y = -2; } else { gravity.y -= 9.8f * Time.deltaTime * Game_Speed; }
        //gets input from w,s,a,d and arrow keys,also gets the mouse inputs
        float Mouse_y = Input.GetAxis("Mouse Y"); float Mouse_x = Input.GetAxis("Mouse X"); float Vertical_v = Input.GetAxis("Vertical");float Horizontal_v= Input.GetAxis("Horizontal");
        
        
        transform.Rotate(0, Mouse_x, 0);
        
        Char_Cont.Move((transform.forward * Vertical_v + transform.right * Horizontal_v + gravity) * Time.deltaTime *2 * Game_Speed);
        //changes animator state,based on inputs from keyboard and mouse buttons
        Anm.SetFloat("x", Horizontal_v); Anm.SetFloat("y", Vertical_v);
        aimming = Mathf.Lerp(aimming, (float)Input.GetMouseButton(1).GetHashCode(), Time.deltaTime * 10);
        //aims if right click is being pressed
        Anm.SetFloat("Aiming",aimming);
        if(tilt+ Mouse_y>=-30 && tilt + Mouse_y <= 30)
        { tilt += Mouse_y*Time.deltaTime*20; Anm.SetFloat("Tilt", tilt); }
        // Vector3 _camera_dest = Gun_Front.position + (Gun_Front.forward * camera_pos_offset.z + Gun_Front.right * camera_pos_offset.x + Gun_Front.up * camera_pos_offset.y);
        // _Camera.position = Vector3.Lerp(_Camera.position, _camera_dest, Time.deltaTime * 30);
        cam_speed = Input.GetMouseButton(1) ? 30: Time.deltaTime * 25;
        _Camera.position = Vector3.Lerp(_Camera.position, Input.GetMouseButton(1) ? camera_dest_aiming.position:camera_dest.position, cam_speed);
        _Camera.rotation = Quaternion.Lerp(_Camera.rotation, Input.GetMouseButton(1) ? camera_dest_aiming.rotation:camera_dest.rotation, cam_speed);
        bool shooting = Input.GetMouseButton(0);
        
        if(Input.GetKeyDown(KeyCode.LeftControl)) { 
            Anm.SetBool("crouching", !Anm.GetBool("crouching"));
            if(Anm.GetBool("crouching")) { Char_Cont.height = char_control_values["crouching"]["height"];
                Char_Cont.center =new Vector3(0, char_control_values["crouching"]["centerY"],0);
            }
            else
            {
                Char_Cont.height = char_control_values["standing"]["height"];
                Char_Cont.center = new Vector3(0, char_control_values["standing"]["centerY"], 0);
            }
        
        }
       
        Anm.SetBool("shooting", shooting);
        if(Input.GetKeyDown(KeyCode.G))
        {
            hold_grenade();
        }
        if (Input.GetKeyUp(KeyCode.G))
        {
            Anm.SetBool("holding_nade", false);
        }



    }
    void shoot()
    {
        bullets.Play();
       
        
    }
    void hold_grenade()
    {
        current_nade = Instantiate(gr_prefab, gr_instantiate_pos.position, gr_instantiate_pos.rotation);
        current_nade.parent =gr_instantiate_pos;
        Anm.SetBool("holding_nade", true);

    }
    void create_object(float x,float y,float z,string username,string hit_by)
    {
     
          var json_data = new
            {
                to_do = "call_instantiate",
                inst_x = x,
                inst_y = y,
                inst_z = z,
               target_username=username,hit_by=hit_by
          };
      
            
    }
    public void throw_grenade()
    {
        
        current_nade.parent = null;
       
        Rigidbody rg = current_nade.GetComponent<Rigidbody>();
        rg.isKinematic = false;
        rg.AddForce(transform.forward *400);
        
    }
   
}
