
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
    private GameObject leftHand;

    [SerializeField]
    private TextMeshPro indexVal;
    private string targetSurface;
    private float materialOffset = 10f;
    private float speedOffset = 0f;
    private float heightOffset = 0f;


    // Player Camoflauges
    CamoObject forestCamo = new CamoObject("forest_camo", 25f, 10f, 5f, 0f, 0f);
    CamoObject mudCamo = new CamoObject("mud_camo", 10f, 30f, 10f, 5f, 0f);
    CamoObject urbanCamo = new CamoObject("urban_camo", 10f, 15f, 30f, 5f, 0f);
    CamoObject snowCamo = new CamoObject("snow_camo", 5f, 5f, 10f, 5f, 40f);
    CamoObject brickCamo = new CamoObject("brick_camo", 5f, 15f, 10f, 40f, 5f);

    
    // Start is called before the first frame update
    void Start()
    {
        groundMask = LayerMask.GetMask("Ground");
        raycastOffset = new Vector3(0, 1, 0);
        currentCamo = leftHand.transform.GetChild(0).GetChild(1).GetComponent<MeshRenderer>().material;//GetComponent<Material>();
        player = GetComponent<CharacterController>();
        
        playerVelocity = player.velocity;
    }

    // Update is called once per frame
    void Update()
    { 
        //print(playerVelocity);
        materialOffset = compareSurface()[0];
        heightOffset = compareSurface()[1];
        camoIndex = (int)Math.Round(Mathf.Clamp((materialOffset + speedOffset + heightOffset), 0f, 100f));
        indexVal.text = camoIndex.ToString() + "%";
    }

    // for the record I'm so sorry, this need to be turned into an array.
    // Edit: It is not an array, but it looks a LOT better. That being said, still room for improvement.
    //          Also I think it's unecessarily convoluted, but I had an idea here...
    private float getCamoVal(string camoName)
    {
        print(currentCamo.mainTexture.name + " | " + camoName);
        switch(currentCamo.mainTexture.name)
        {
            
            case "forest_camo":
                materialOffset = forestCamo.returnMaterialOffset(camoName);
                break;
            case "mud_camo":
                materialOffset = mudCamo.returnMaterialOffset(camoName);
                break;
            case "urban_camo":
                materialOffset = urbanCamo.returnMaterialOffset(camoName);
                break;
            case "snow_camo":
                materialOffset = snowCamo.returnMaterialOffset(camoName);
                break;
            case "brick_camo":
                materialOffset = brickCamo.returnMaterialOffset(camoName);
                
                break;
            case "no_camo":
                switch(camoName)
                {
                    case "Grass":
                    case "Mud":
                    case "Urban":
                    case "Brick":
                    case "Snow":
                    //case "Untagged":
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
