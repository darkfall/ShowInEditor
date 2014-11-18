using UnityEngine;
using System.Collections;
using AU;

[ShowInEditor]
class SimpleStruct
{
    [ShowInEditor(Comment = "By default, only public members are modifiable")]
    public int i = 42;

    [ShowInEditor(Comment = "But you can modify it with flags = ShowInEditorFlags.ReadWrite", Flags = ShowInEditorFlags.ReadWrite)]
    string xixi = "XD";
}

[ExecuteInEditMode]
public class ShowInEditorTest : MonoBehaviour {

    [ShowInEditor(Comment = "Readonly private field (default)", CommentColor = ShowInEditorColor.Green)]
    int privateField = 42;

    [ShowInEditor(Comment = "ReadWrite private field", CommentColor = ShowInEditorColor.Green, Flags = ShowInEditorFlags.ReadWrite)]
    int rwPrivateField = 42;

    [ShowInEditor(Comment = "Struct refrence", CommentColor = ShowInEditorColor.Red, Flags = ShowInEditorFlags.ReadWrite)]
    SimpleStruct structReference = new SimpleStruct();

    [ShowInEditor(Comment = "Component refrence array (Execute in editor mode)", CommentColor = ShowInEditorColor.Red, Flags = ShowInEditorFlags.ReadWrite)]
    SampleComponent[] componentReferenceArray;


    [ShowInEditor(CanModifyArrayLength = true, Flags = ShowInEditorFlags.ReadWrite, Comment = "Modifiable array (and serializable)", CommentColor = ShowInEditorColor.Yellow, Name = "Array of GameObjects")]
    [SerializeField]
    GameObject[] array;

    #region Properties

    string _someProperty = "Hello World";
    float _rwProperty = 333;

    [ShowInEditor(Comment = "Readonly property", CommentColor = ShowInEditorColor.Cyan)]
    public string SomeProperty
    {
        get { return _someProperty; }
    }


    [ShowInEditor(Comment = "RW property", CommentColor = ShowInEditorColor.Cyan)]
    public float RWProperty
    {
        get { return _rwProperty; }
        set { _rwProperty = value; }
    }

    #endregion
    
    [ShowInEditor(Comment = "This is a method, and it will be readonly", CommentColor = ShowInEditorColor.Magenta)]
    public override string ToString()
    {
        return base.ToString();
    }

    [ShowInEditor(Comment = "this is method call button", IsButton = true)]
    public void TestButton()
    {
        Debug.Log("Hello World");
    }

    [ShowInEditor(Comment = "this is a button group", IsButton = true, ButtonGroup = 1)]
    public void TestButton1()
    {
        Debug.Log("Hello World 1");
    }

    [ShowInEditor(IsButton = true, ButtonGroup = 1)]
    public void TestButton2()
    {
        Debug.Log("Hello World 2");
    }

    [ShowInEditor(IsButton = true, ButtonGroup = 1)]
    public void TestButton3()
    {
        Debug.Log("Hello World 3");
    }

    public void Start()
    {
        componentReferenceArray = GetComponentsInChildren<SampleComponent>();

    }

}
