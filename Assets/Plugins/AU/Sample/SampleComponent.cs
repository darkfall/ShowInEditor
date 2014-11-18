using UnityEngine;
using System.Collections;

[AU.ShowInEditor]
public class SampleComponent : MonoBehaviour {

    [AU.ShowInEditor(CompressArrayLayout = true)]
    public bool[] measures = new bool[8];


}
