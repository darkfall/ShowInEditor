__Brief__

A simple editor inspector enhancement for Unity3D.

Adding support for showing public & non-public properties, fields, methods and static members in the editor inspector.

Support nested objects.

License: MIT

![](https://github.com/darkfall/ShowInEditor/blob/master/demo.jpg)

__How to__

* Add [ShowInEditor] attribute to the members you want to show in the inspector. For example:

        [ShowInEditor(Comment = "This is a private field")]
        int privateField = 42;

* Create a custom editor for the type and inherit from AU.AUEditor<T>

        [CustomEditor(typeof(AType))]
        class AEditor : AU.AUEditor<AType>
        {

        }
        
* Done!

__Attribute Options__

* Comment (string)
    
    Comment will be shown above the field
    
* CommentType (ShowInEditorMessagetype)

    Message type of the comment helpbox 
    
* Name (string)

    Override the name of the field
    
* CommentColor (ShowInEditorColor)

    Color of the comment file
    
* FieldColor (ShowInEditorColor)

    Color of the field

* CanModifyArrayLength (bool)

    When the member is an array, can the user modify the length of the array in the inspector or not. Default is __false__
    
* CompressArrayLayout (bool)

    When the member is an array, compress the array layout into a horizontal group to save space. Only supports primitive types.
    
 
* IsButton
    
    When the member is a method, making a button in the inspector instead of showing the result of the method call
    
* ButtonGroup

    When the member is a method and its a button, group the button horizontally by the group index. Default is -1 means no group at all.
 
__Nested Object__

If you want custom structs/classes/components also showing in the inspector, the type of the object must also be decorated by the [ShowInEditor] attribute. For example:

        [ShowInEditor]
        class SimpleClass
        {
            [ShowInEditor(Comment = "By default, only public members are modifiable")]
            public int i = 42;

            [ShowInEditor(Comment = "But you can modify it with flags = ShowInEditorFlags.ReadWrite", Flags = ShowInEditorFlags.ReadWrite)]
            string xixi = "XD";
        }

