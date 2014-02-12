using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class JSONExamples : MonoBehaviour
{

  void Start()
  {
    ExampleBasicJSON();
  }

  void ExampleBasicJSON()
  {
    // Let's serialize a simple address book

    JSONOutStream outStream = new JSONOutStream();
    outStream.Content("a", 1)
             .Content("b", 2)
             .Start("c")
              .Content("d", 3)
             .End();

    string serialized = outStream.Serialize();

    // serialized outputs this JSON structure:
  
  
    // {a:1, b: 2, c: {d: 3}}

    

    // Deserialize it
    JSONInStream inStream = new JSONInStream(serialized);
    int a, b, d;
    inStream.Content("a", out a)
            .Content("b", out b)
            .Start("c")
              .Content("d", out d)
            .End();

    Debug.Log("SERIALIZED JSON STRING: " + serialized);
    Debug.Log("JSON DESERIALIZATION of a:" + a + ", b: " + b + ", d: " + d );
  } 
}
