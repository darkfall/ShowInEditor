__Brief__

A simple editor inspector enhancement for Unity3D.

Adding support for showing public & non-public properties, fields, methods and static members in the editor inspector.

Support nested objects.

__How to__

* Add [ShowInEditor] attribute to the members you want to show in the inspector

* Create a custom editor for the type and inherit from AU.AUEditor<T>

        [CustomEditor(typeof(AType))]
        class AEditor : AU.AUEditor<AType>
        {

        }
        
* Done!


__Nested Object__

If you want nested object (such as a custom struct) also showing in the inspector, the type of the object must also be decorated by the [ShowInEditor] attribute. For example:

        [ShowInEditor]
        class SimpleStruct
        {
            [ShowInEditor(Comment = "By default, only public members are modifiable")]
            public int i = 42;

            [ShowInEditor(Comment = "But you can modify it with flags = ShowInEditorFlags.ReadWrite", Flags = ShowInEditorFlags.ReadWrite)]
            string xixi = "XD";
        }

