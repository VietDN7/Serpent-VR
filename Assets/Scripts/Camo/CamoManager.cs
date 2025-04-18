
using System;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using Vector3 = UnityEngine.Vector3;

public class CamoManager : MonoBehaviour
{
    private int camoIndex;

    private CharacterController player;

    [SerializeField]
    private GameObject playerHead;
    private Vector3 playerVelocity;
    
    private Vector3 raycastOffset;
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
        raycastOffset = new Vector3(0, 1, 0);
        currentCamo = GameObject.Find("Wristwatch").GetComponent<Material>();
        player = GetComponent<CharacterController>();
        
        playerVelocity = player.velocity;
    }

    // Update is called once per frame
    void Update()
    { 
        print(playerVelocity);
        materialOffset = compareSurface()[0];
        heightOffset = compareSurface()[1];
        camoIndex = (int)Math.Round(Mathf.Clamp((materialOffset + speedOffset + heightOffset), 0f, 100f));
        indexVal.text = camoIndex.ToString() + "%";
    }

    // for the record I'm so sorry, this need to be turned into an array.
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
                    case "Urban":
                        materialOffset = 5f;
                        break;
                    case "Brick":
                        materialOffset = 0f;
                        break;
                    case "Snow":
                        materialOffset = 0f;
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
                    case "Urban":
                        materialOffset = 10f;
                        break;
                    case "Brick":
                        materialOffset = 5f;
                        break;
                    case "Snow":
                        materialOffset = 0f;
                        break;
                    case "Untagged":
                        materialOffset = 0f;
                        break;
                }
                break;
            case "urban_camo":
                switch(camoName)
                {
                    case "Grass":
                        materialOffset = 10f;
                        break;
                    case "Mud":
                        materialOffset = 15f;
                        break;
                    case "Urban":
                        materialOffset = 30f;
                        break;
                    case "Brick":
                        materialOffset = 5f;
                        break;
                    case "Snow":
                        materialOffset = 0f;
                        break;
                    case "Untagged":
                        materialOffset = 0f;
                        break;
                }
                break;
        }
        return materialOffset;
    }

    private float[] compareSurface()
    {
        RaycastHit groundCheck;
        float[] surfaceConditions = new float[2];

        if (Physics.Raycast(playerHead.transform.position, transform.TransformDirection(Vector3.down), out groundCheck, 2, groundMask))
        {
            Debug.DrawRay(playerHead.transform.position, transform.TransformDirection(Vector3.down) * 2, Color.red);
            targetSurface = groundCheck.transform.gameObject.tag;
            surfaceConditions[0] = getCamoVal(targetSurface);

            if(groundCheck.distance/1.8f > 0.5)
            {
                surfaceConditions[1] = 0f;
            }
            else if (groundCheck.distance/1.8f > 0.25)
            {
                surfaceConditions[1] = 25f;
            }
            else
            {
                surfaceConditions[1] = 50f;
            }
            
            return surfaceConditions;
        } else {
            Debug.DrawRay(playerHead.transform.position, transform.TransformDirection(Vector3.down) * 2, Color.red);
            surfaceConditions[0] = 0;
            surfaceConditions[1] = 100;
            return surfaceConditions;
        }
    }

}
