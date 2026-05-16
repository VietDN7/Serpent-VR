using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    [SerializeField]
    private int health = 5;
    
    // Start is called before the first frame update

    public void takeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            health = 0;
            transform.GetComponent<Rigidbody>().isKinematic = false;
            transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            transform.GetComponent<Rigidbody>().AddForce(0, 0, 1f, ForceMode.Impulse);
            transform.GetComponent<EnemyAI>().enabled = false;
            transform.GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = false;
            //print(health);
            //Destroy(gameObject);
        }
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
