using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatProperty// : MonoBehaviour
{
    string tagName;
    float offset;
    public MatProperty(string tagName, float offset)
    {
        this.tagName = tagName;
        this.offset = offset;
    }

    public string getTagName()
    {
        return tagName;
    }
    public float getOffset()
    {
        return offset;
    }

    public void setOffset(float newOffset)
    {
        offset = newOffset;
    }
    // Start is called before the first frame update
    /*
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }*/
}
