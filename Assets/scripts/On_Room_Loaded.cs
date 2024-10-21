using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using UnityEngine;
using TMPro;
using System.Threading.Tasks;

public class On_Room_Loaded : MonoBehaviour
{
    // Start is called before the first frame update
    public static string My_username;
    public static ClientWebSocket room_socket;
    void Start()
    {
  
        StartCoroutine(WebSocketConnection());
    }

    // Update is called once per frame
    
     IEnumerator WebSocketConnection()
    {
        var json_data = new { session_id = send_credintials.session_id , to_do = "announce_joining" };
        
        
        string json_str = JsonConvert.SerializeObject(json_data);
        byte[] buffer = Encoding.UTF8.GetBytes(json_str);
        var segment = new ArraySegment<byte>(buffer);
  
        yield return room_socket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
        yield return new WaitUntil(() => room_socket.State == WebSocketState.Open);

    }

   

}
