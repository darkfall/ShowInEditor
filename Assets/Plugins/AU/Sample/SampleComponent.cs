using UnityEngine;
using System.Collections;

[AU.ShowInEditor]
public class SampleComponent : MonoBehaviour {

    [AU.ShowInEditor(CompactArrayLayout = true)]
    public bool[] measures = new bool[8];


}
