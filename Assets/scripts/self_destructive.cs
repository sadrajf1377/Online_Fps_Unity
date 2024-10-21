using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class self_destructive : MonoBehaviour
{
    public float Wait_Time = 0;
    void Start()
    {
        Destroy(this.gameObject, Wait_Time);
    }

    
}
