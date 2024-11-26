using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameRate : MonoBehaviour
{
    [Range(15, 200)]
    public int _FrameRate = 30;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        Application.targetFrameRate = _FrameRate;
    }
}
