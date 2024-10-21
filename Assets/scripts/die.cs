using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class die : MonoBehaviour
{
    // Start is called before the first frame update
    public string who_killed_me = "";
   public void kill_me()
    {
       
        this.name = "dead";
        bool call_socket = GetComponent<send_stats>().is_mine;
        this.GetComponent<send_stats>().break_loop = true;
        this.transform.tag = "Untagged";
        this.GetComponent<Animator>().applyRootMotion = true;
        this.GetComponent<CharacterController>().enabled = false;
       
        this.GetComponent<movements>().enabled = false;
        
        this.GetComponent<send_stats>().enabled=false;


        if (call_socket)
        {
            StartCoroutine(create_new_player());
        }
        

       
    }
    IEnumerator create_new_player()
    {
        
        var json_data1 = new { session_id = PlayerPrefs.GetString("session_id"), to_do = "die" ,username=who_killed_me};
        string json_str1 = JsonConvert.SerializeObject(json_data1);
        byte[] buffer1 = Encoding.UTF8.GetBytes(json_str1);
        var segment1 = new ArraySegment<byte>(buffer1);

        yield return On_Room_Loaded.room_socket.SendAsync(segment1, WebSocketMessageType.Text, true, CancellationToken.None);
        var json_data = new { session_id = PlayerPrefs.GetString("session_id"), to_do = "announce_joining" };
        string json_str = JsonConvert.SerializeObject(json_data);
        byte[] buffer = Encoding.UTF8.GetBytes(json_str);
        var segment = new ArraySegment<byte>(buffer);

        yield return On_Room_Loaded.room_socket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
    }
}
