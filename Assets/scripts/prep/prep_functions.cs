using Newtonsoft.Json;
using System;

using System.Text;
using System.Threading;
using UnityEngine;

public class prep_functions : MonoBehaviour
{
    
    
    private void Start()
    {
       
        
        announce_joining();
        
    }
    
    async void announce_joining()
    {
        var data = new
        {
            to_do= "announce_joining",session_id=send_credintials.session_id
        };
        string json_str = JsonConvert.SerializeObject(data);
        byte[] buffer = Encoding.UTF8.GetBytes(json_str);
        var segment = new ArraySegment<byte>(buffer);
        await On_Room_Loaded.room_socket.SendAsync(segment, System.Net.WebSockets.WebSocketMessageType.Text, true, CancellationToken.None);
        
    }
}
