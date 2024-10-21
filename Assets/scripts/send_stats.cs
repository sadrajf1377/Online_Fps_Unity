using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;
using TMPro;
using System.Threading.Tasks;

public class send_stats : MonoBehaviour
{
    // Start is called before the first frame update
    public bool is_mine = false;
    [SerializeField] Color clr;
    Animator anm;
  
    public bool break_loop = false;
    movements mv;
    public teams my_team;
    void Start()
    {
        if (is_mine)
        {


            mv = GetComponent<movements>();
            anm = GetComponent<Animator>();
            this.GetComponent<movements>()._Camera = Camera.main.transform;
            this.GetComponent<movements>().enabled = true;
            
            send_my_stats();
        }
        
    }
    
    async void send_my_stats()
    {
       
         
        var json_data = new { username = On_Room_Loaded.My_username, to_do = "send_stats", position_x = transform.position.x, position_y = transform.position.y
                , position_z = transform.position.z,
                rot_x = transform.eulerAngles.x, rot_y = transform.eulerAngles.y, rot_z = transform.eulerAngles.z,
                axis_x=(float) anm.GetFloat("x"),axis_y=(float)anm.GetFloat("y"),aiming= (float)anm.GetFloat("Aiming"),tilt= (float)anm.GetFloat("Tilt"),
                shooting=anm.GetBool("shooting")
                ,crouching= anm.GetBool("crouching")
                ,player_health=mv.health
                ,dead=anm.GetBool("die"),
                
                score=mv.score
               
            };
            
            string json_str = JsonConvert.SerializeObject(json_data);
            byte[] buffer = Encoding.UTF8.GetBytes(json_str);
            var segment = new ArraySegment<byte>(buffer);
        await On_Room_Loaded.room_socket.SendAsync(segment, System.Net.WebSockets.WebSocketMessageType.Text, true, CancellationToken.None);

      
            if (!break_loop)
                send_my_stats();
        
        
        
        
    }
    public enum teams
    {
        blue, red
    }

}
