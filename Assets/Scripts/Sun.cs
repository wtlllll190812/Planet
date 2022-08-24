using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Sun : Planet
{
    public static Sun instance { get; private set; }
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
