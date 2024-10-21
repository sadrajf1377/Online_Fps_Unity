using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;
public class Parent_Child_count
{
    public Transform transform;
    public int child_count;
    
    public static Parent_Child_count instantiate(Transform transfprm,int child_count)
    {
        var obj = new Parent_Child_count();
        obj.transform = transfprm; obj.child_count = child_count;
        
        return obj;
    }
}
public class shop_functions : MonoBehaviour
{
    
    [SerializeField] Transform Vertical_Parent, Horizontal_parent;
    int Hrizontal_Parent_Counter = 0;
    List<Parent_Child_count> H_Parents_Child_Count=new List<Parent_Child_count>();
    [SerializeField] GameObject Detail_Instance;
    [SerializeField] Transform Basket_transform;
    List<Basket_Detail> detail_product_names = new List<Basket_Detail>();
    Texture current_texture;

    [SerializeField] Texture temp_texture;
    public static List<Basket_Detail> details = new List<Basket_Detail>();
    string current_token = "";

    private void Start()
    {
        StartCoroutine(Load_Items(0));
    }
    IEnumerator DownloadImage(string MediaUrl,Action optional=null)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture("http://127.0.0.1:8000" + MediaUrl);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
            Debug.Log(request.error);
        else
            
            current_texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
        if(optional != null)
        {
            optional();
        }
    }
    
    void Add_Product(string image_url,string product_name,int count,int price,string id)
    {
        if(Hrizontal_Parent_Counter ==0)
        {
            var new_obj = Instantiate(Horizontal_parent, Vertical_Parent);
            new_obj.name ="H_Parent"+Hrizontal_Parent_Counter.ToString();
            Hrizontal_Parent_Counter++;
            var new_raw_image = new GameObject();
            
            new_raw_image.transform.parent = new_obj.transform;
            new_raw_image.AddComponent<RawImage>();
            H_Parents_Child_Count.Add(Parent_Child_count.instantiate(new_obj,1));
            
            var button = new_obj.gameObject.AddComponent<EventTrigger>();
            print($"button {button} added to {new_obj.name}");
            var entry = new EventTrigger.Entry();
            
            button.triggers.Add(entry);
            
            StartCoroutine(DownloadImage(image_url
                ,optional:delegate {
                    new_raw_image.GetComponent<RawImage>().texture = current_texture;
                    entry.eventID = EventTriggerType.PointerClick; entry.callback.AddListener(delegate { add_to_basket(new_raw_image.GetComponent<RawImage>().texture, count, product_name, price,id); });
                }));
            

        }
        else
        {
            
            var last_parent = H_Parents_Child_Count[H_Parents_Child_Count.Count-1];

            if(last_parent.child_count >=3)
            {
                var new_obj = Instantiate(Horizontal_parent, Vertical_Parent);
                new_obj.name = "H_Parent" + Hrizontal_Parent_Counter.ToString();
                Hrizontal_Parent_Counter++;
                var new_raw_image = new GameObject();
                new_raw_image.transform.parent = new_obj.transform;
                new_raw_image.AddComponent<RawImage>();
                H_Parents_Child_Count.Add(Parent_Child_count.instantiate(new_obj, 1));
                var button = new_raw_image.gameObject.AddComponent<EventTrigger>();
                print($"button {button} added to {new_raw_image.name}");
                var entry = new EventTrigger.Entry();
                
                button.triggers.Add(entry);
                StartCoroutine(DownloadImage(image_url
                    ,optional:delegate {
                        new_raw_image.GetComponent<RawImage>().texture = current_texture;
                        entry.eventID = EventTriggerType.PointerClick; entry.callback.AddListener(delegate { add_to_basket(new_raw_image.GetComponent<RawImage>().texture, count, product_name, price,id); });
                    }));
                
            }
            else
            {
                var parent = H_Parents_Child_Count[H_Parents_Child_Count.Count-1];
                var new_raw_image = new GameObject();
                new_raw_image.transform.parent =parent.transform ;
                new_raw_image.AddComponent<RawImage>();
                parent.child_count += 1;
                var button = new_raw_image.gameObject.AddComponent<EventTrigger>();
                
                var entry = new EventTrigger.Entry();
                print($"button {button} added to {parent.transform.name}");
                
                button.triggers.Add(entry);
                StartCoroutine(DownloadImage(image_url
                    ,optional:delegate
                    {

                        new_raw_image.GetComponent<RawImage>().texture = current_texture;

                        entry.eventID = EventTriggerType.PointerClick; entry.callback.AddListener(delegate { add_to_basket(new_raw_image.GetComponent<RawImage>().texture, count, product_name, price,id); });
                    }
                    ));
                
            }
        }
    }
    void Remove_Product(GameObject go)
    {
        Destroy(go);
    }
    IEnumerator Load_Items(int start_index)
    {
        string url = $"http://127.0.0.1:8000/product_module/show_products/{start_index}";
        using (UnityWebRequest req = UnityWebRequest.Get(url))
        {
            var form = new WWWForm();
            form.AddField("start", 0);
            yield return req.SendWebRequest();
            if (req.isHttpError)
            {
                print(req.error);
            }
            else
            {
                
                var text = req.downloadHandler.text;
                var data = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(text);
                
                foreach (var item in data)
                {
                    decimal price =decimal.Parse(item["price"].Replace(".","/"));
                    int count = 1; string pr_name = item["product_name"];
                    string id = item["id"];
                    Add_Product(item["get_thumbnail"],pr_name,count,(int)price,id);
                }
                print(text);
            }
        }
    }
    public void add_to_basket(Texture txr,int count,string pr_name,int price,string id)
    {
        if (detail_product_names.Exists(x => x.name == pr_name))
            {
            var dtail_class = detail_product_names.Find(x => x.name == pr_name);
            dtail_class.count += 1;
            dtail_class.Update_Detail_Info();
        }
        else {
            GameObject g = Instantiate(Detail_Instance, Basket_transform);
            Basket_Detail detail = g.GetComponent<Basket_Detail>(); detail.price = price.ToString() + "$";
            detail.id = id;
            detail.Pr_image = txr; detail.count = count; detail.name = pr_name;
            detail_product_names.Add(detail);
            details.Add(detail);
            detail.Update_Detail_Info();
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
                tok = tok.Remove(0, 1);
                tok = tok.Remove(tok.Length - 1, 1);
                current_token = tok;
                print("current token is:" + tok);
            }
        }


    }
    IEnumerator finalize_orders()
    {
        string items = "";
        var last_index = details.Count - 1;
        int counter = 0;
        foreach(Basket_Detail dtail in details)
        {
            var to_add = $"{dtail.id},{dtail.count}";
            if(!(counter ==0 || counter == last_index))
            {
                to_add = "," + to_add + ",";
            }
            items += to_add;
            counter++;
        }
        var ses_id = PlayerPrefs.GetString("session_id");
        var url = "http://127.0.0.1:8000/product_module/confirm_order";
        var form = new WWWForm();
        form.AddField("ids_and_count", items);
        form.AddField("session_id", ses_id);
        var my_data = "{ \"name\" : \"sadra\" }";
        form.AddField("my_data", my_data.ToString());
        var req = UnityWebRequest.Post(url, formData: form);
        using(var request=req)
        {
            StartCoroutine(Request_Token());
            yield return new WaitUntil(() => current_token != "");
            request.SetRequestHeader("X-csrftoken", current_token);
            yield return req.SendWebRequest();
            if(req.isHttpError || req.isNetworkError)
            {
                print("error happend" + req.error);
            }
            else
            {
                var data = req.downloadHandler.text;
                print(data);
            }
        }
    }
    public void confirm_order()
    {
        StartCoroutine(finalize_orders());
    }
}
