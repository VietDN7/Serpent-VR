using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Scrapped script to make the holster track a bit better aroudn the player.

public class HolsterParent : MonoBehaviour
{
    [SerializeField]
    public GameObject cameraOffset;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        //transform.position = cameraOffset.transform.position.y - 0.5;
    }
}
