using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
public class prep_recieve : MonoBehaviour
{
    [SerializeField] GameObject red_users, blue_users, username_prefab;
    bool red_is_odd, blue_is_odd;
    [SerializeField] Color odd, even;
    List<string> current_users;
    // Start is called before the first frame update
    private void Start()
    {
        red_is_odd = blue_is_odd = false;
        current_users = new List<string>();
        Recieve();

    }
    async void Recieve()
    {

        ArraySegment<byte> buffer2 = new ArraySegment<byte>(new byte[2000]);
        await On_Room_Loaded.room_socket.ReceiveAsync(buffer2, CancellationToken.None);
        var mes = Encoding.UTF8.GetString(buffer2.Array, 0, 2000);
        if (mes.Contains("groups_users"))
        {
            var data = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(mes);
            print("update users are"+mes);
            var users = data["groups_users"];
            foreach (var key in users.Keys)
            {
                if (current_users.Contains(key))
                {
                    continue;
                }
                var username = Instantiate(username_prefab); username.transform.Find("username").GetComponent<TMPro.TextMeshProUGUI>().text = key;
                if (users[key] == "red")
                {
                    username.transform.SetParent(red_users.transform);
                    red_is_odd = !red_is_odd;
                    username.GetComponent<UnityEngine.UI.RawImage>().color = red_is_odd ? odd : even;
                }
                else
                {
                    username.transform.SetParent(blue_users.transform);
                    blue_is_odd = !blue_is_odd;
                    username.GetComponent<UnityEngine.UI.RawImage>().color = blue_is_odd ? odd : even;
                }
                current_users.Add(key);
               
            }
            Recieve();
        }
        else if(mes.Contains("start_game")) { SceneManager.LoadScene(2);  }
        
        
       
    }
}
