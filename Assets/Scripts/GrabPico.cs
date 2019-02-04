using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabPico : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var mat = transform.GetComponentInParent<Pico>().transform.GetComponent<MeshRenderer>().material;

        transform.GetComponent<MeshRenderer>().material = mat;

    }

    // Update is called once per frame
    void Update()
    {

    }
}
