using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spectate : MonoBehaviour
{
    public send_stats.teams my_team;
    List<GameObject> cameras;
    bool collected_camers=false;
    int index = 0;
    public void Start_Spectating()
    {
        foreach(var go in GameObject.FindGameObjectsWithTag("Player"))
        {
            var team = go.GetComponent<send_stats>().my_team;
            if(team != my_team)
            {
                continue;
            }
            cameras.Add(go.GetComponent<movements>()._Camera.gameObject);
        }
        collected_camers = true;
        change_active_camera();
    }
    private void Update()
    {
        if(collected_camers)
        {
            if (Input.GetKeyDown(KeyCode.Q) && index >0) { index -= 1; change_active_camera(); }
            if (Input.GetKeyDown(KeyCode.E) && index < cameras.Count-1) { index += 1; change_active_camera(); }
        }
    }
    void change_active_camera()
    {
        cameras.ForEach(delegate (GameObject g) { g.SetActive(cameras.IndexOf(g) == index); });
    }
}
