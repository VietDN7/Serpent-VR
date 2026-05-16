using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TranqDart : MonoBehaviour
{
    public GameObject dart;

    private int lifetime = 10;
    private int damage = 5;


    // Start is called before the first frame update
    void Start()
    {
        Destroy(dart, lifetime);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            if (collision.gameObject.TryGetComponent<EnemyStats>(out EnemyStats enemyStats))
            {
                enemyStats.takeDamage(damage);
            }
        }
    }

    public int getDamage()
    {
        return damage;
    }
}
