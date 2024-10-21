using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Http;
using UnityEngine.Networking;
using TMPro;
using Newtonsoft.Json;
using System.Net.WebSockets;
using System;
using System.Text;
using System.Threading;
using UnityEngine.UI;
using System.Threading.Tasks;

public class send_credintials : MonoBehaviour
{
    [Header("Assign the Drops Downs and inputs of the form in here")]
    [SerializeField] List<TMP_InputField> Sign_Up_Fields,Login_Fields;
    [SerializeField] List<TMP_Dropdown> Sign_Up_Drop_Downs;
    [SerializeField]string current_token = "";
    [SerializeField] List<GameObject> tabs;
    GameObject previous_tab;

    [Header("Users Info Would Be Displayed in these textmeshpro items")]
    
    [SerializeField] TextMeshProUGUI user_name,user_rank,user_k_d,user_balance;

    [SerializeField] GameObject Player_Status,Players_Friends_List_Parent,Notifs_Parent,Notif_Prefab,Squad_Parent,Squad_Prefab;
    public static ClientWebSocket pv_socket;
    List<string> current_squad_users;
    [SerializeField] Ui_Functions funcs;
    public static string session_id = "";
    void Start()
    {
        current_squad_users = new List<string>();
        PlayerPrefs.SetString("session_id", "");
        StartCoroutine(Ask_For_User_Info());
    }
   
    // Update is called once per frame
    void Update()
    {
        
    }
    void OnApplicationQuit()
    {
        if(pv_socket.State==WebSocketState.Open)
        {
            pv_socket.CloseAsync(WebSocketCloseStatus.EndpointUnavailable,"app closed",CancellationToken.None);
        }
    }
    IEnumerator Request_Token()
    {
       
        using (UnityWebRequest www = UnityWebRequest.Get("http://127.0.0.1:8000/get_token"))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                var tok = www.downloadHandler.text;
                tok=tok.Remove(0, 1);
                tok=tok.Remove(tok.Length - 1, 1);
                current_token = tok;
                print("current token is:"+tok);
            }
        }
        

    }
    IEnumerator Req_Sing_Up(string url)
    {
        yield return null;
        WWWForm form = new WWWForm();
        foreach(var text in Sign_Up_Fields)
        {
            
            form.AddField(text.transform.parent.name, text.text);
        }
        foreach (var text in Sign_Up_Drop_Downs)
        {

            form.AddField(text.transform.parent.name,text.options[text.value].text);
        }
        using (UnityWebRequest req = UnityWebRequest.Post(url, formData:form))
        {
            StartCoroutine(Request_Token());
            yield return new WaitUntil(() => current_token != "");
            req.SetRequestHeader("X-Csrftoken",current_token);
            
            yield return req.SendWebRequest();
            if(req.isNetworkError || req.isHttpError)
            {
                Debug.Log(req.error);
            }
            else
            {
                var tex = req.downloadHandler.text;
                print(tex);
                var js = JsonConvert.DeserializeObject<Dictionary<string,List<string>>>(tex);
                string stat = js["status"][0];
                js.Remove("status");
                if (stat == "succeed")
                {
                    previous_tab = tabs.Find(x => x.activeSelf);
                    Change_Tab("Email_Sent_Tab");
                }
                else
                {
                   
                    foreach (var key in js.Keys)
                    {
                        GameObject parent_obj;

                        try
                        {
                            parent_obj = Sign_Up_Fields.Find(x => x.transform.parent.name == key).transform.parent.gameObject;
                        }
                        catch
                        {
                            parent_obj = Sign_Up_Drop_Downs.Find(x => x.transform.parent.name == key).transform.parent.gameObject;
                        }
                        var tmpro = parent_obj.transform.Find("errors").GetComponent<TextMeshProUGUI>();
                        tmpro.text = "";
                        foreach (var error in js[key])
                        {
                            print(error);
                            
                            tmpro.text += error +" ";
                            
                        }
                    }
                } 
            }
        }
    }
    public void Request_Sign_Up()
    {
       StartCoroutine(Req_Sing_Up("http://127.0.0.1:8000/sign_up/"));
    }
   public void Change_Tab(string tab_name)
    {
        var targ_tab = tabs.Find(x => x.name == tab_name);
        if (previous_tab != null) { previous_tab.SetActive(false); }
        targ_tab.SetActive(true); previous_tab = targ_tab;

    }
   IEnumerator Ask_For_User_Info()
    {
        string url = "http://127.0.0.1:8000/Get_User_Info/";
     
        using(UnityWebRequest req=UnityWebRequest.Get(url))
        {
            try { 
                req.SetRequestHeader("cookie","session_id="+session_id);
                
                
            } catch { }
            yield return req.SendWebRequest();
            
            if (req.isNetworkError || req.isHttpError)
            {
                Debug.Log(req.error);
            }
            else
            {
                var txt = req.downloadHandler.text;
                print(txt);
                if (txt.Contains("auth_status"))
                {
                    var tab = tabs.Find(x => x.name == "Log_in_form");
                    previous_tab = tab;
                    tab.SetActive(true);
                }
                else
                {
                    var tab = tabs.Find(x => x.name == "User_Info");
                    previous_tab = tab;
                    tab.SetActive(true);
                    var dict =JsonConvert.DeserializeObject<Dictionary<string,string>>(txt);
                    user_name.text = user_name.text.Insert(user_name.text.Length, dict["username"]);
                    On_Room_Loaded.My_username = dict["username"];
                    user_rank.text = dict["user_ranking"]; user_k_d.text = dict["kill_per_death"];
                    user_balance.text += dict["account_balance"] + "$";
                    var friends_list = JsonConvert.DeserializeObject<Dictionary<string, string>>(dict["get_friends_list"]);
                    foreach(string username in friends_list.Keys)
                    {
                        GameObject user = Instantiate(Player_Status, Vector3.zero, Quaternion.identity, Players_Friends_List_Parent.transform);
                        user.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = username;
                        user.transform.GetChild(1).GetComponent<RawImage>().color = friends_list[username] == "True" ? Color.green : Color.grey;
                        user.transform.GetChild(2).gameObject.SetActive(friends_list[username] == "True");
                        user.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(delegate () { Send_Invite(username); } );
                    }
                    pv_socket = new ClientWebSocket();
                    var uri = new Uri(
           "ws://"
        + "127.0.0.1:8000"
        + "/ws/private_socekt/"+ dict["username"]+"/"
        );
                    yield return pv_socket.ConnectAsync(uri, default);
                   
                    yield return new WaitUntil(() => pv_socket.State == WebSocketState.Open);
                    var jsonObj = new
                    {
                        session_id =session_id
                        ,func_type= "resolve_username"
                    };
                    string json = JsonConvert.SerializeObject(jsonObj);
                    // Convert the JSON string to a byte array
                    byte[] buffer = Encoding.UTF8.GetBytes(json);
                    var segment = new ArraySegment<byte>(buffer);
                    yield return pv_socket.SendAsync(segment,WebSocketMessageType.Text,true, CancellationToken.None);
                    yield return new WaitUntil(() => pv_socket.State == WebSocketState.Open || pv_socket.State == WebSocketState.Closed);
                    if(pv_socket.State == WebSocketState.Open)
                    {
                        Recieve_Socket_Messages();
                    }
                    else
                    {
                        Debug.LogError("websocket couldnt connect");
                    }

                    
                   



                }
            }
        }
    }
    async void Send_Invite(string username)
    {
        var json_data = new { func_type = "invite", target = username };
        string json_str = JsonConvert.SerializeObject(json_data);
        byte[] buffer = Encoding.UTF8.GetBytes(json_str);
        var segment = new ArraySegment<byte>(buffer);
        await pv_socket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
    }
    async void Recieve_Socket_Messages()
    {
        try
        {
            ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[4000]);
            await pv_socket.ReceiveAsync(buffer, CancellationToken.None);
            var mes = Encoding.UTF8.GetString(buffer.Array, 0, 4000);
            Dictionary<string, string> message = JsonConvert.DeserializeObject<Dictionary<string, string>>(mes);
            print(mes);

            string function = message["function"];


            switch (function)
            {
                case "receive_invite":
                    {
                        GameObject g = Instantiate(Notif_Prefab, Vector3.zero, Quaternion.identity, Notifs_Parent.transform);
                        g.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = message["message"];
                        g.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(delegate () { Join_Squad(message["squad_name"]); });
                    }
                    break;
                case "update_squad":
                    {
                        print(message["users"]);
                        List<string> users = JsonConvert.DeserializeObject<List<string>>(message["users"]);
                        users.ForEach(delegate (string username)
                        {
                            if (current_squad_users.Contains(username)) { return; }
                            GameObject g = Instantiate(Squad_Prefab, Vector3.zero, Quaternion.identity, Squad_Parent.transform);
                            g.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = username;
                            current_squad_users.Add(username);
                        });


                    }
                    break;
                case "join_room":
                    {

                        funcs.Join_Room_With_Name(message["room_name"]);
                    }
                    break;


                default: { await Task.Delay(1); } break;
            }
        }
        catch { await Task.Delay(1); }
        Recieve_Socket_Messages();
        

    }
    async void Join_Squad(string squad_name1)
    {
        var json_data = new { func_type = "join_squad", squad_name=squad_name1 };
        string json_str = JsonConvert.SerializeObject(json_data);
        byte[] buffer = Encoding.UTF8.GetBytes(json_str);
        var segment = new ArraySegment<byte>(buffer);
        await pv_socket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
    }
    IEnumerator Send_Login_Creds()
    {
        var url = "http://127.0.0.1:8000/Log_User_In";
        print(current_token);
        StartCoroutine(Request_Token());
        yield return new WaitUntil(() => current_token != string.Empty || current_token != "");
        var form = new WWWForm();
        form.AddField("username", Login_Fields.Find(x => x.transform.parent.name == "username").text);
        form.AddField("password", Login_Fields.Find(x => x.transform.parent.name == "password").text);
        using (UnityWebRequest req=UnityWebRequest.Post(url,formData:form))
        {
            req.SetRequestHeader("X-Csrftoken", current_token);
            yield return req.SendWebRequest();
            print(req.downloadHandler.text);
            if( req.isHttpError)
            {
                print(req.error);
            }
            else
            {
                var text = req.downloadHandler.text;
                
                var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(text);
                if(dict["status"] == "succeed")
                {
                    session_id = dict["session_id"];
                    PlayerPrefs.SetString("session_id", session_id);
                    previous_tab.SetActive(false);
                    previous_tab = tabs.Find(x => x.name == "User_Info");
                    previous_tab.SetActive(true);
                    StartCoroutine(Ask_For_User_Info());
                }
                else
                {
                    var txt_error = Login_Fields.Find(x => x.transform.parent.name == "password").transform.parent.Find("errors");
                    txt_error.GetComponent<TextMeshProUGUI>().text = dict["message"];
                }
            }
           

        }
    }
    public void Ask_For_Login()
    {
        StartCoroutine(Send_Login_Creds());
    }
   IEnumerator Log_Me_out_ienumrator()
    {
        var url = "http://127.0.0.1:8000/Log_User_Out_Unity/";
        using(UnityWebRequest req=UnityWebRequest.Get(uri:url))
        {
            StartCoroutine(Request_Token());
            yield return new WaitUntil(() => current_token != "");
            //req.SetRequestHeader("cookie", "session_id=" + PlayerPrefs.GetString("session_id"));
            req.SetRequestHeader("cookie","session_id="+PlayerPrefs.GetString("session_id"));
            yield return req.SendWebRequest();
            if( req.isHttpError || req.isNetworkError)
            {
                print(req.error);
            }
            else
            {
                var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(req.downloadHandler.text);
                if(data["status"]== "Logged Out Successfully")
                {
                    PlayerPrefs.SetString("session_id", ""); previous_tab = tabs.Find(x => x.name == "User_Info");
                    previous_tab.SetActive(false); previous_tab= tabs.Find(x => x.name == "Log_in_form");
                    previous_tab.SetActive(true);

                }
                else
                {
                    print(data["status"]);
                }
            }
                 
        }
       
    }
    public void Log_me_out()
    {
        StartCoroutine(Log_Me_out_ienumrator());
    }
    
}
