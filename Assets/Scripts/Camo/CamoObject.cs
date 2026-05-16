using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamoObject
{

    

    string materialName;
    /*
    private Dictionary<string, float> materialOffset = new Dictionary<string, float>(camoCount)
    {
        {"Grass", 0f},
        {"Mud", 0f},
        {"Urban", 0f},
        {"Brick", 0f},
        {"Snow", 0f},
        //{"Untagged", 0f}
    };*/
    
    private static int camoCount = 5;
    MatProperty grass = new MatProperty("Grass", 0f);
    MatProperty mud = new MatProperty("Mud", 0f);
    MatProperty urban = new MatProperty("Urban", 0f);
    MatProperty brick = new MatProperty("Brick", 0f);
    MatProperty snow = new MatProperty("Snow", 0f);

    MatProperty[] matProperties = new MatProperty[camoCount];

    public CamoObject(string materialName, float grassVal, float mudVal, float urbanVal, float brickVal, float snowVal/*, float untaggedVal*/)
    {
        this.materialName = materialName;

        // This is REALLY stupid but it beats a million  switch-case statements
        // I should've just comitted to a 2D array, this is basically the same.
        grass.setOffset(grassVal);
        mud.setOffset(mudVal);
        urban.setOffset(urbanVal);
        brick.setOffset(brickVal);
        snow.setOffset(snowVal);

        matProperties[0] = grass;
        matProperties[1] = mud;
        matProperties[2] = urban;
        matProperties[3] = brick;
        matProperties[4] = snow;


        //materialOffset["Untagged"] = untaggedVal; // Potentially unnecessary if TryGetValue handles this?
    }

    public float returnMaterialOffset(string surfaceTag)
    {
        foreach (MatProperty matProperty in matProperties)
        {
            if (matProperty.getTagName() == surfaceTag)
            {
                return matProperty.getOffset();
            }
        }
        return 0f; // Return a default value if the tag is not found
    }
    // Start is called before the first frame update
    /*void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }*/
}
