using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformController : MonoBehaviour
{
    public List<Material> colors;
    public int colorIndex = 0;

    public int colorChangeInterval = 10;

    // Start is called before the first frame update
    void Start()
    {
        if (colors.Count > 0)
        {
            InvokeRepeating("ChangeColor", 0, colorChangeInterval);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ChangeColor()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).GetComponent<MeshRenderer>().material = colors[colorIndex];
        }
        colorIndex = (colorIndex + 1) % colors.Count;
    }
}
