
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

    [SerializeField]
    public Material currentCamo;

    [SerializeField]
    private GameObject leftHand;

    [SerializeField]
    private TextMeshPro indexVal;
    private string targetSurface;
    private float materialOffset = 10f;
    private float speedOffset = 0f;
    private float heightOffset = 0f;

    public Material[] camos = new Material[5];


    // Player Camoflauges
    // I've kind of omitted the "none" camo.. 
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
        // After careful examination of this with a friend of mine, it appears this one only obtains *information* and not the object itself
        // Which is why I was having trouble changing stuff around.
        currentCamo = leftHand.transform.GetChild(0).GetChild(1).GetComponent<MeshRenderer>().material;
        player = GetComponent<CharacterController>();
        
        
        playerVelocity = player.velocity;
    }

    // Update is called once per frame
    void Update()
    { 
        //print(playerVelocity);
        materialOffset = compareSurface()[0]; // These two can melt performance if it was used seriously.
        heightOffset = compareSurface()[1];
        camoIndex = (int)Math.Round(Mathf.Clamp((materialOffset + speedOffset + heightOffset), 0f, 100f));
        indexVal.text = camoIndex.ToString() + "%";
    }

    // for the record I'm so sorry, this need to be turned into an array.
    // Edit: It is not an array, but it looks a LOT better. That being said, still room for improvement.
    //          Also I think it's unecessarily convoluted, but I had an idea here...
    private float getCamoVal(string surfaceTag)
    {
        //print(currentCamo.mainTexture.name + " | " + surfaceTag);
        switch(currentCamo.mainTexture.name)
        {
            
            case "forest_camo":
                materialOffset = forestCamo.returnMaterialOffset(surfaceTag);
                break;
            case "mud_camo":
                materialOffset = mudCamo.returnMaterialOffset(surfaceTag);
                break;
            case "urban_camo":
                materialOffset = urbanCamo.returnMaterialOffset(surfaceTag);
                break;
            case "snow_camo":
                materialOffset = snowCamo.returnMaterialOffset(surfaceTag);
                break;
            case "brick_camo":
                materialOffset = brickCamo.returnMaterialOffset(surfaceTag);
                
                break;
            case "no_camo":
                switch(surfaceTag)
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

    public void changeCamo(Material camoName)
    {
        foreach (Material mat in camos)
        {
            if (mat == camoName)
            {

                leftHand.transform.GetChild(0).GetChild(1).GetComponent<MeshRenderer>().material = mat; // This one changes the actual material of the watch.
                currentCamo = mat; // This one returns the information related to that material.
            }
        }
        
    }

}
