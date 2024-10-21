using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Net.WebSockets;
using Newtonsoft.Json;
using System.Text;
using System.Threading;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class Ui_Functions : MonoBehaviour
{
    //Headers Section
    [SerializeField] Animator Header_DropDowns;
    [SerializeField] TMP_InputField room_name;
    
    public void Open_Or_Close_Options(string To_do)
    {
        Header_DropDowns.SetTrigger($"options_{To_do}");
    }
    public void Open_Or_Close_play_Options(string To_do)
    {
        Header_DropDowns.SetTrigger($"play_options_{To_do}");
    }
    public void Open_Or_Close_Friends_List(string To_do)
    {
        Header_DropDowns.SetTrigger($"friends_list_{To_do}");
    }
    public void Join_Room_With_Name(string rmname)
    {
        StartCoroutine(Join_Room_With_Name_enumrator(rmname));
    }
    IEnumerator Join_Room_With_Name_enumrator(string rm_name = "none")
    {
        string address = "ws://"
        + "127.0.0.1:8000"
        + "/ws/room/" +(rm_name=="none"?room_name.text:rm_name) + "/";
        print(address);
        var uri = new Uri(address);
       On_Room_Loaded.  room_socket = new ClientWebSocket();
        print(room_name.text);
        yield return On_Room_Loaded. room_socket.ConnectAsync(uri, default);
        yield return new WaitUntil(() =>On_Room_Loaded.room_socket.State == WebSocketState.Open);
        if (rm_name == "none")
        {
            var data = new
            {
                func_type = "add_squad_to_room",
                room_name = room_name.text
            };
            string json_str = JsonConvert.SerializeObject(data);
            byte[] buffer = Encoding.UTF8.GetBytes(json_str);
            var segment = new ArraySegment<byte>(buffer);
            print("invited squad to the group");

            yield return send_credintials.pv_socket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
        }
       
        
        SceneManager.LoadScene(1);

        
    }
    public void Load_shop()
    {
        SceneManager.LoadScene(2);
        
    }
    public void Hide_Display_Notifs(string to_do)
    {
        Header_DropDowns.SetTrigger($"{to_do}_notifs");
    }
    public void show_hide_squad(string to_do)
    {
        Header_DropDowns.SetTrigger($"{to_do}_squad");
    }
    

}

