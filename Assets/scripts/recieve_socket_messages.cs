using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
public class recieve_socket_messages : MonoBehaviour
{
    [SerializeField]GameObject player_instance;
    [SerializeField] GameObject prefab;
    [SerializeField] List<Transform> blues, reds;
    [SerializeField] ScoreBoard scr;
    void Start()
    {
       Recieve_Socket_Messages();
      
    }

    // Update is called once per frame
   
   async void  Recieve_Socket_Messages()
    {

        for (; ; )
        {
            ArraySegment<byte> buffer2 = new ArraySegment<byte>(new byte[4000]);
            await On_Room_Loaded.room_socket.ReceiveAsync(buffer2, CancellationToken.None);
            var mes = Encoding.UTF8.GetString(buffer2.Array, 0, 4000);
            if (mes.Contains("groups_users"))
            {
                var response = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(mes);

                foreach (var name in response["groups_users"])
                {
                    if (!GameObject.Find(name.Key))
                    {
                        Vector3 pos = Vector3.zero;
                        GameObject go = Instantiate(player_instance, pos, Quaternion.identity);
                        go.name = name.Key;
                        var stat = go.AddComponent<send_stats>();
                        stat.is_mine = name.Key == On_Room_Loaded.My_username;
                        scr.Add_To_ScoreBoard(name.Key);
                        if (stat.is_mine)
                        {
                            int ran = UnityEngine.Random.Range(0, 2);
                            
                            go.transform.position = name.Value == "red" ? reds[ran].position : blues[ran].position;

                        }
                        stat.my_team = name.Value == "red" ? send_stats.teams.red : send_stats.teams.blue;
                    }

                }

            }
            else if (!mes.Contains("groups_users") && mes.Contains("username"))
            {

                try
                {
                    
                    var objs_list = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(mes);
                  
                    foreach (var key in objs_list.Keys)
                    {
                        
                        
                        if (key == On_Room_Loaded.My_username)
                        {
                            print("returned");
                            continue;
                        }
                        var data = objs_list[key];
                        print(data);
                        GameObject target = GameObject.Find(data["username"]);
                        float pos_x = float.Parse(data["position_x"].Replace('.', '/'));
                        float pos_y = float.Parse(data["position_y"].Replace('.', '/')); float pos_z = float.Parse(data["position_z"].Replace('.', '/'));
                        var position = new Vector3(pos_x, pos_y, pos_z);
                        float rot_x = float.Parse(data["rot_x"].Replace('.', '/')); float rot_y = float.Parse(data["rot_y"].Replace('.', '/'));
                        float rot_z = float.Parse(data["rot_z"].Replace('.', '/'));
                        float aiming = float.Parse(data["aiming"].Replace('.', '/')); float y_axis = float.Parse(data["axis_y"].Replace('.', '/'));
                        float x_axis = float.Parse(data["axis_x"].Replace('.', '/')); float tilt = float.Parse(data["tilt"].Replace('.', '/'));
                        bool shooting = bool.Parse(data["shooting"]); bool crouching = bool.Parse(data["crouching"]);
                        bool die = bool.Parse(data["dead"]);
                        var anm = target.GetComponent<Animator>();
                        anm.SetFloat("x", x_axis); anm.SetFloat("y", y_axis); anm.SetFloat("Aiming", aiming);
                        anm.SetFloat("Tilt", tilt); anm.SetBool("shooting", shooting);
                        anm.SetBool("crouching", crouching);
                        anm.SetBool("die", die);
                        target.transform.position = position;
                        target.transform.eulerAngles = new Vector3(rot_x, rot_y, rot_z);
                        target.GetComponent<movements>().health = int.Parse(data["player_health"]);
                        int score= int.Parse(data["score"]);
                        target.GetComponent<movements>().score = score; scr.Update_User_Score(data["username"], score);
                        
                    }
                }
               
                catch (Exception e) {  }

            }
            else if (mes.Contains("increase_my_score"))
            {
                var me = GameObject.Find(On_Room_Loaded.My_username);
                me.GetComponent<movements>().score += 10;

            }

        }
    
    }
    
}
