using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class explode : MonoBehaviour
{
    [SerializeField] float explosion_time = 2;
    [SerializeField] ParticleSystem pr;
    List<ParticleCollisionEvent> events;

    void Start()
    {
        events = new List<ParticleCollisionEvent>();
        StartCoroutine(Explode());
    }

    IEnumerator Explode()
    {
        yield return new WaitForSeconds(explosion_time);
        pr.Play();
    }
    private void OnParticleCollision(GameObject other)
    {
        int count = pr.GetCollisionEvents(other, events);
        float distance = Vector3.Distance(transform.position, other.transform.position);
        for (int c = 0; c < count; c++)
        {
            
            print($"damage that object {other.name} recieved is {1 / distance}");
        }
    }
}
