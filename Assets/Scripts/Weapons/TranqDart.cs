using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TranqDart : MonoBehaviour
{
    public GameObject dart;

    private int lifetime = 10;
    // Start is called before the first frame update
    void Start()
    {
        Destroy(dart, lifetime);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
