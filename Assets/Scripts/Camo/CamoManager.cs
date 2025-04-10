using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class CamoManager : MonoBehaviour
{
    private float camoIndex;

    private CharacterController player;

    private Vector3 playerVelocity;
    private LayerMask groundMask;

    public Material currentCamo;
    [SerializeField]
    private TextMeshPro indexVal;
    private string targetSurface;
    private float materialOffset = 10f;
    private float speedOffset = 0f;
    private float heightOffset = 0f;
    // Start is called before the first frame update
    void Start()
    {
        groundMask = LayerMask.GetMask("Ground");
        
        currentCamo = GameObject.Find("Wristwatch").GetComponent<Material>();
        player = GetComponent<CharacterController>();
        
    }

    // Update is called once per frame
    void Update()
    { 
        materialOffset = compareSurface();
        camoIndex = Mathf.Clamp(materialOffset + speedOffset + heightOffset, 0f, 100f);
        indexVal.text = camoIndex.ToString() + "%";
    }

    private float getCamoVal(string camoName)
    {
        switch(currentCamo.mainTexture.name)
        {
            case "forest_camo":
                switch(camoName)
                {
                    case "Grass":
                        materialOffset = 25f;
                        break;
                    case "Mud":
                        materialOffset = 10f;
                        break;
                    case "Untagged":
                        materialOffset = 0f;
                        break;
                }
                break;
            case "mud_camo":
                switch(camoName)
                {
                    case "Grass":
                        materialOffset = 10f;
                        break;
                    case "Mud":
                        materialOffset = 30f;
                        break;
                    case "Untagged":
                        materialOffset = 0f;
                        break;
                }
                break;
        }
        return materialOffset;
    }

    private float compareSurface()
    {
        RaycastHit groundCheck;

        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out groundCheck, 10, groundMask))
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.down) * 10, Color.red);
            targetSurface = groundCheck.transform.gameObject.tag;
            return getCamoVal(targetSurface);
        } else {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.down) * 10, Color.red);
            return 0f;
        }
    }
}
