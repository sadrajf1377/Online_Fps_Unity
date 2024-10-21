using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
public class ScoreBoard : MonoBehaviour
{
    Dictionary<string, int> users_scores;
    [SerializeField] Transform scoreboard_rows,row_prefab;
    List<Transform> rows;
    List<int> scores_sorted;
    void Start()
    {
        users_scores = new Dictionary<string, int>();
        rows = new List<Transform>();
        this.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        scores_sorted = users_scores.Values.ToList();
        scores_sorted.Sort((x, y) => x.CompareTo(y));
        for(int i=0;i<rows.Count;i++)
        {
            var score = rows[i].GetChild(0).GetComponent<TextMeshProUGUI>();
            score.text = scores_sorted[i].ToString();
            
        }
    }
    public void Add_To_ScoreBoard(string username)
    {
        if(users_scores.ContainsKey(username))
        {
            return;
        }
        Transform row = Instantiate(row_prefab, scoreboard_rows);
        users_scores.Add(username, 0);
        rows.Add(row);
    }
    public void Update_User_Score(string username,int score)
    {
        users_scores[username] = score;
    }
}
