using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bullet_trigger : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] ParticleSystem system;
    
    List<ParticleCollisionEvent> events = new List<ParticleCollisionEvent>();
    [SerializeField] movements bullet_origin;
    private void Start()
    {
        system = GetComponent<ParticleSystem>();
    }
    private void OnParticleCollision(GameObject other)
    {
        
        int number = system.GetCollisionEvents(other, events);
        for(int i=0;i<number;i++)
        {
           if(other.CompareTag("Player")&&other.GetComponent<movements>().enabled)
            {
                other.GetComponent<movements>().health -= 10;
                other.GetComponent<die>().who_killed_me = bullet_origin.name;
            }
           
        }
    }
    
}
