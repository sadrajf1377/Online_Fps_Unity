using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class Basket_Detail : MonoBehaviour
{
    public Texture Pr_image;
    public string pr_name,price;
    public int count;

    [SerializeField] RawImage product_image;
    [SerializeField] TextMeshProUGUI product_name,prodocut_price,product_count;
    string my_id="";
   

    public void Update_Detail_Info()
    {
        product_image.texture = Pr_image; product_name.text = pr_name; prodocut_price.text = price;
        product_count.text ="count :"+count.ToString();
    }
    public void delete_me()
    {
        shop_functions.details.Remove(this);

        Destroy(this.gameObject);
    }
    public string id
    {
        set
        {
            if(my_id=="")
            {
                my_id = value;
            }
        }
        get
        {
            return my_id;
        }
    }
        

}
