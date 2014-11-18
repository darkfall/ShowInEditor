//
// [ShowInEditor]
// Developed as part of Project Ambiguous Utopia
// By Darkfall (http://darkfall.me)
// Licemse: MIT
//

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace AU
{

    public enum ShowInEditorFlags
    {
        Default,
        ReadOnly,
        ReadWrite,
    }

    public enum ShowInEditorMessageType
    {
        None = 0,
        Info = 1,
        Warning = 2,
        Error = 3,
    }

    public enum ShowInEditorColor
    {
        Default = 0,
        Green,
        Red,
        Blue,
        Magenta,
        White,
        Cyan,
        Yellow,
    }

    /**
     * 
     */

    [System.AttributeUsage(System.AttributeTargets.Property |
                           System.AttributeTargets.Field |
                           System.AttributeTargets.Method |
                           System.AttributeTargets.Class |
                           System.AttributeTargets.Struct)]
    public class ShowInEditor : System.Attribute
    {
        public ShowInEditorFlags Flags;

        public string Comment = "";
        public string Name = "";
        public string Prefix = "";

        /**
         * 
         * Can modify the length of the array or not
         *
         * When enabled, users can modify the length of the array in the inspector (similar to how default inspector works)
         */
        public bool CanModifyArrayLength = false;

        /**
         *
         * Compress array layout to a horizontal group
         * Only support primitive types (bool, int, float)
         * 
         * When enabled, the layout of an array will be
         *    NAME 
         *      ELEMENT1 ELEMENT2 ELEMENT3 ...
         * Instead of 
         *    
         *    NAME
         *      [0] ...
         *      [1] ...
         *      [2] ...
         * 
         */
        public bool CompressArrayLayout = false;

        /**
         * When decorating a method, optionally making a button in the inspector
         * and the method will be invoked when the button is clicked
         */
        public bool isButton = false;

        public ShowInEditorMessageType CommentType = ShowInEditorMessageType.None;

        public static Color[] EditorColors = {
            GUI.color,
            Color.green,
            Color.red,
            Color.blue,
            Color.magenta,
            Color.white,
            Color.cyan,
            Color.yellow
        };

        public ShowInEditorColor CommentColor = ShowInEditorColor.Default;
        public ShowInEditorColor FieldColor = ShowInEditorColor.Default;

        public int IndentLevel = 0;

        public Color UnityColor
        {
            get
            {
                return EditorColors[(int)FieldColor];
            }
        }
        public Color UnityCommentColor
        {
            get
            {
                return EditorColors[(int)CommentColor];
            }
        }

        public ShowInEditor(ShowInEditorFlags f = ShowInEditorFlags.Default)
        {
            Flags = f;
        }


    }

    public static class EditorHelper
    {
        public abstract class MemberField
        {
            protected System.Object _obj;
            protected ShowInEditor _attribute;
            protected SerializedPropertyType _type;

            public string NamePrefix = "";

            public SerializedPropertyType Type
            {
                get { return _type; }
            }

            public ShowInEditorFlags Flags
            {
                get { return _attribute.Flags; }
            }

            public string Comment
            {
                get { return _attribute.Comment; }
            }

            public bool CanModifyArrayLength
            {
                get { return _attribute.CanModifyArrayLength; }
            }

            public bool CompressArrayLayout
            {
                get { return _attribute.CompressArrayLayout; }
            }

            public Color Color
            {
                get { return _attribute.UnityColor; }
            }

            public Color CommentColor
            {
                get { return _attribute.UnityCommentColor; }
            }

            public int IndentLevel
            {
                get { return _attribute.IndentLevel; }
                set { _attribute.IndentLevel = value; }
            }

            public bool IsDefaultColor
            {
                get { return _attribute.FieldColor == ShowInEditorColor.Default; }
            }

            public bool IsButton
            {
                get { return _attribute.isButton;  }
            }

            public bool IsDefaultCommentColor
            {
                get { return _attribute.CommentColor == ShowInEditorColor.Default; }
            }

            public UnityEditor.MessageType CommentMessageType
            {
                get { return (UnityEditor.MessageType)_attribute.CommentType; }
            }

            public void Rebind(System.Object obj)
            {
                _obj = obj;
            }

            public MemberField(System.Object obj, ShowInEditor attr, SerializedPropertyType type)
            {
                _obj = obj;
                _type = type;
                _attribute = attr;
            }

            public abstract System.Object GetValue();

            public abstract void SetValue(System.Object value);

            public abstract string GetName();

            public abstract System.Type GetFieldType();

            public abstract bool IsReadOnly();

            public string GetName(MemberInfo info)
            {
                return _attribute.Prefix + NamePrefix + (_attribute.Name.Length > 0 ? _attribute.Name : ObjectNames.NicifyVariableName(info.Name));
            }

            protected static
                Dictionary<System.Type, SerializedPropertyType> _typemap = new Dictionary<System.Type, SerializedPropertyType>
                {
                   { typeof(int),           SerializedPropertyType.Integer },
                   { typeof(float),         SerializedPropertyType.Float },
                   { typeof(bool),          SerializedPropertyType.Boolean },
                   { typeof(double),        SerializedPropertyType.Float },
                   { typeof(string),        SerializedPropertyType.String },
                   { typeof(Vector2),       SerializedPropertyType.Vector2 },
                   { typeof(Vector3),       SerializedPropertyType.Vector3 },
                   { typeof(Vector4),       SerializedPropertyType.Vector4 },
                   { typeof(Quaternion),    SerializedPropertyType.Quaternion },
                   { typeof(Rect),          SerializedPropertyType.Rect },
                   { typeof(Bounds),        SerializedPropertyType.Bounds },
                   { typeof(Color),         SerializedPropertyType.Color },
                   { typeof(AnimationCurve), SerializedPropertyType.AnimationCurve },
                   { typeof(Gradient),      SerializedPropertyType.Gradient },
                };

            public static bool GetSerializedTypeFromType(System.Type type, out SerializedPropertyType serializedType)
            {
                serializedType = SerializedPropertyType.Generic;
                if (type.IsEnum)
                {
                    serializedType = SerializedPropertyType.Enum;
                    return true;
                }
                if (type.IsSubclassOf(typeof(UnityEngine.Object)))
                {
                    serializedType = SerializedPropertyType.ObjectReference;
                    return true;
                }
                if (type.IsArray)
                {
                    return GetSerializedTypeFromType(type.GetElementType(), out serializedType);
                }
                return _typemap.TryGetValue(type, out serializedType);
            }

        }

        public class PropertyField : MemberField
        {
            PropertyInfo _info;

            MethodInfo _getter;
            MethodInfo _setter;

            public PropertyInfo PropertyInfo
            {
                get { return _info; }
            }

            public PropertyField(System.Object instance, ShowInEditor attribute, SerializedPropertyType type, PropertyInfo info)
                : base(instance, attribute, type)
            {
                _info = info;

                _getter = _info.GetGetMethod();
                _setter = _info.GetSetMethod();
            }

            public override System.Object GetValue()
            {
                return _getter.Invoke(_obj, null);
            }

            public override void SetValue(System.Object value)
            {
                _setter.Invoke(_obj, new System.Object[] { value });
            }

            public override string GetName()
            {
                return GetName(_info);
            }

            public override System.Type GetFieldType()
            {
                return _info.PropertyType;
            }

            public static bool IsReadOnly(ShowInEditor attribute, PropertyInfo info)
            {
                return attribute.Flags == ShowInEditorFlags.ReadOnly || !info.CanWrite;
            }

            public override bool IsReadOnly()
            {
                return IsReadOnly(_attribute, _info);
            }

            public static bool GetSerializedType(PropertyInfo info, out SerializedPropertyType propertyType)
            {
                return GetSerializedTypeFromType(info.PropertyType, out propertyType);
            }
        }

        public class FieldField : MemberField
        {
            protected FieldInfo _info;

            public FieldInfo FieldInfo
            {
                get { return _info; }
            }

            public FieldField(System.Object instance, ShowInEditor attribute, SerializedPropertyType type, FieldInfo info)
                : base(instance, attribute, type)
            {
                _info = info;
            }

            public override System.Object GetValue()
            {
                return _info.GetValue(_obj);
            }

            public override void SetValue(System.Object value)
            {
                _info.SetValue(_obj, value);
            }

            public override System.Type GetFieldType()
            {
                return _info.FieldType;
            }

            public override string GetName()
            {
                return GetName(_info);
            }

            public static bool IsReadOnly(ShowInEditor attribute, FieldInfo info)
            {
                if (attribute.Flags == ShowInEditorFlags.Default)
                    return !info.IsPublic;
                return attribute.Flags == ShowInEditorFlags.ReadOnly;
            }

            public override bool IsReadOnly()
            {
                return IsReadOnly(_attribute, _info);
            }

            public static bool GetSerializedType(FieldInfo info, out SerializedPropertyType propertyType)
            {
                return GetSerializedTypeFromType(info.FieldType, out propertyType);
            }
        }

        public class ArrayFieldField : FieldField
        {
            public bool editor_showContent = true;


            public int Length
            {
                get
                {
                    IList arr = (IList)this.GetValue();
                    if (arr != null)
                        return arr.Count;
                    return 0;
                }
                set
                {
                    IList oldArr = (IList)this.GetValue();
                    if (value != oldArr.Count)
                    {
                        IList newArr = (IList)System.Activator.CreateInstance(_info.FieldType, value);
                        for (int i = 0; i < (int)Mathf.Min(oldArr.Count, newArr.Count); ++i)
                            newArr[i] = oldArr[i];
                        this.SetValue(newArr);
                    }
                }
            }

            public ArrayFieldField(System.Object instance, ShowInEditor attribute, SerializedPropertyType type, FieldInfo info)
                : base(instance, attribute, type, info)
            {

            }

            public override System.Type GetFieldType()
            {
                return _info.FieldType.GetElementType();
            }

            public new static bool GetSerializedType(FieldInfo info, out SerializedPropertyType propertyType)
            {
                return GetSerializedTypeFromType(info.FieldType.GetElementType(), out propertyType);
            }
        }

        public class MethodField : MemberField
        {
            protected MethodInfo _info;

            public MethodInfo FieldInfo
            {
                get { return _info; }
            }

            public MethodField(System.Object instance, ShowInEditor attribute, SerializedPropertyType type, MethodInfo info)
                : base(instance, attribute, type)
            {
                _info = info;
            }

            public void Invoke()
            {
                _info.Invoke(_obj, new object[0] { });
            }

            public override System.Object GetValue()
            {
                return _info.Invoke(_obj, new object[0] { });
            }

            public override void SetValue(System.Object value)
            {
                //
            }

            public override System.Type GetFieldType()
            {
                return _info.ReturnType;
            }

            public override string GetName()
            {
                return GetName(_info);
            }

            public static bool IsReadOnly(ShowInEditor attribute, MethodInfo info)
            {
                return true;
            }

            public override bool IsReadOnly()
            {
                return true;
            }

            public static bool GetSerializedType(MethodInfo info, out SerializedPropertyType propertyType)
            {
                return GetSerializedTypeFromType(info.ReturnType, out propertyType);
            }
        }

        public class ObjectReferenceField : FieldField
        {
            protected MemberField[] _members;

            public bool editor_showContent = true;

            public MemberField[] Members
            {
                get { return _members; }
            }

            public ObjectReferenceField(System.Object instance, ShowInEditor attribute, SerializedPropertyType type, FieldInfo info, MemberField[] members)
                : base(instance, attribute, type, info)
            {
                _members = members;
            }

            public void BindRefernecedObject(System.Object obj)
            {
                foreach (MemberField mf in _members)
                {
                    mf.Rebind(obj);
                }
            }
        }

        public class ArrayObjectReferenceField : ArrayFieldField
        {
            protected MemberField[] _members;
            public MemberField[] Members
            {
                get { return _members; }
            }

            public ArrayObjectReferenceField(System.Object instance, ShowInEditor attribute, SerializedPropertyType type, FieldInfo info, MemberField[] members)
                : base(instance, attribute, type, info)
            {
                _members = members;
            }

            public void BindRefernecedObject(System.Object obj)
            {
                foreach (MemberField mf in _members)
                {
                    mf.Rebind(obj);
                }
            }
        }

        public static object DrawFieldElement(SerializedPropertyType type, string name, object value, System.Type sysType)
        {
            switch (type)
            {
                case SerializedPropertyType.Integer:
                    return EditorGUILayout.IntField(name, (int)value);

                case SerializedPropertyType.Float:
                    {
                        if (sysType != typeof(double))
                        {
                            return EditorGUILayout.FloatField(name, (float)value);

                        }
                        else
                        {
                            // aaaa
                            double v = (double)value;
                            float fv = (float)v;
                            float rv = EditorGUILayout.FloatField(name, fv);
                            return (double)rv;
                        }
                    }

                case SerializedPropertyType.Boolean:
                    return EditorGUILayout.Toggle(name, (bool)value);

                case SerializedPropertyType.String:
                    return EditorGUILayout.TextField(name, (string)value);

                case SerializedPropertyType.Vector2:
                    return EditorGUILayout.Vector2Field(name, (Vector2)value);

                case SerializedPropertyType.Vector3:
                    return EditorGUILayout.Vector3Field(name, (Vector3)value);

                case SerializedPropertyType.Vector4:
                    return EditorGUILayout.Vector4Field(name, (Vector4)value);

                case SerializedPropertyType.Quaternion:
                    {
                        Vector4 v = EditorGUILayout.Vector4Field(name, (Vector4)value);
                        Quaternion q = new Quaternion(v.x, v.y, v.z, v.w);
                        return q;
                    }

                case SerializedPropertyType.Enum:
                    return EditorGUILayout.EnumPopup(name, (System.Enum)value);

                case SerializedPropertyType.Color:
                    return EditorGUILayout.ColorField(name, (Color)value);

                case SerializedPropertyType.Rect:
                    return EditorGUILayout.RectField(name, (Rect)value);

                case SerializedPropertyType.Bounds:
                    return EditorGUILayout.BoundsField(name, (Bounds)value);

                case SerializedPropertyType.Gradient:
                    Gradient g = (Gradient)value;
                    EditorGUILayout.LabelField(name, g.ToString());
                    break;

                case SerializedPropertyType.AnimationCurve:
                    return EditorGUILayout.CurveField(name, (AnimationCurve)value);

                case SerializedPropertyType.ObjectReference:
                    {
                        if (typeof(UnityEngine.Object).IsAssignableFrom(sysType))
                        {
                            return EditorGUILayout.ObjectField(name, (UnityEngine.Object)value, sysType, true);
                        }
                    }
                    break;

                default:
                    break;
            }

            return null;
        }

        public static object DrawFieldElementCompressed(SerializedPropertyType type, object value, System.Type sysType)
        {
            switch (type)
            {
                case SerializedPropertyType.Integer:
                    return EditorGUILayout.IntField((int)value);

                case SerializedPropertyType.Float:
                    {
                        if (sysType != typeof(double))
                        {
                            return EditorGUILayout.FloatField((float)value);

                        }
                        else
                        {
                            // aaaa
                            double v = (double)value;
                            float fv = (float)v;
                            float rv = EditorGUILayout.FloatField(fv);
                            return (double)rv;
                        }
                    }

                case SerializedPropertyType.Boolean:
                    // for some reason, editor gui layout doesn't work here
                    return GUILayout.Toggle((bool)value, new GUIContent(""), new GUILayoutOption[] { GUILayout.Width(24) });

                case SerializedPropertyType.String:
                    return EditorGUILayout.TextField((string)value);

                case SerializedPropertyType.Enum:
                    return EditorGUILayout.EnumPopup((System.Enum)value);

                default:
                    break;
            }

            return null;
        }

        public static void DrawArrayField(ArrayFieldField field)
        {
            IList arr = (IList)field.GetValue();

            field.editor_showContent = EditorGUILayout.Foldout(field.editor_showContent, field.GetName() + System.String.Format(": ({0} Elements)", arr != null ? arr.Count : 0));

            if (field.editor_showContent)
            {
                EditorGUI.indentLevel = 1 + field.IndentLevel;

                if (arr != null)
                {
                    if (field.IsReadOnly())
                    {
                        if (arr.Count == 0)
                            EditorGUILayout.LabelField("The array is empty");

                        for (int i = 0; i < arr.Count; ++i)
                        {
                            System.Object v = arr[i];
                            if(v != null)
                                EditorGUILayout.LabelField(string.Format("[{0}]", i), arr[i].ToString());
                            else
                                EditorGUILayout.LabelField(string.Format("[{0}]", i), "(null)");
                        }
                    }
                    else
                    {
                        if (field.CanModifyArrayLength)
                            field.Length = EditorGUILayout.IntField("Length", field.Length);

                        System.Type type = field.GetFieldType();
                        if (field.CompressArrayLayout && type.IsPrimitive)
                        {
                            EditorGUILayout.BeginHorizontal();

                            if (field.Type == SerializedPropertyType.Boolean)
                            {
                                // for some reason, editor gui layout doesn't work here
                                GUILayout.Space(EditorGUI.indentLevel * 24);
                            }
                            for (int i = 0; i < arr.Count; ++i)
                            {
                                arr[i] = DrawFieldElementCompressed(field.Type, arr[i], type);
                            }

                            EditorGUILayout.EndHorizontal();
                        }
                        else
                        {
                            for (int i = 0; i < arr.Count; ++i)
                            {
                                arr[i] = DrawFieldElement(field.Type, string.Format("[{0}]", i), arr[i], type);
                            }
                        }
                    }
                }
            }
        }

        public static void DrawMethodField(MethodField field)
        {
            if (!field.IsButton)
            {
                object v = field.GetValue();

                if (v != null)
                {
                    if (v.GetType().IsArray)
                    {
                        IList arr = (IList)v;

                        EditorGUILayout.LabelField(field.GetName() + System.String.Format(": ({0} Elements)", arr.Count));

                        EditorGUI.indentLevel = 1 + field.IndentLevel;
                        for (int i = 0; i < arr.Count; ++i)
                        {
                            EditorGUILayout.LabelField(string.Format("[{0}]", i), arr[i].ToString());
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField(field.GetName(), v.ToString());
                    }
                }
                else
                {
                    EditorGUILayout.LabelField(field.GetName(), "null");
                }
            }
            else
            {
                if (GUILayout.Button(field.GetName()))
                {
                    field.Invoke();
                }
            }
        }

        public static void DrawGenericField(MemberField field)
        {
            EditorGUILayout.BeginHorizontal();

            if (field.IsReadOnly())
            {
                System.Object v = field.GetValue();
                if (v != null)
                    EditorGUILayout.LabelField(field.GetName(), field.GetValue().ToString());
                else
                    EditorGUILayout.LabelField(field.GetName(), "(null)");

                EditorGUILayout.EndHorizontal();
                return;
            }
            field.SetValue(DrawFieldElement(field.Type, field.GetName(), field.GetValue(), field.GetFieldType()));

            EditorGUILayout.EndHorizontal();
        }

        public static void DrawObjectReference(System.Object obj, MemberField[] fields, Color defaultColor)
        {
            if (obj != null)
            {
                string namePrefix = "";
                if (obj is UnityEngine.Component)
                    namePrefix = (obj as UnityEngine.Component).gameObject.name + ".";
                else if (obj is UnityEngine.GameObject)
                    namePrefix = (obj as UnityEngine.GameObject).name + ".";

                foreach (MemberField memberField in fields)
                {
                    memberField.NamePrefix = namePrefix;
                    DrawField(memberField, defaultColor);
                }
            }
            else
            {
                EditorGUILayout.LabelField("(null)");
            }
        }

        public static void DrawObjectReferenceField(ObjectReferenceField field, Color defaultColor)
        {
            System.Object obj = field.GetValue();

            field.editor_showContent = EditorGUILayout.Foldout(field.editor_showContent,
                field.GetName());

            if (field.editor_showContent)
            {
                field.BindRefernecedObject(obj);
                DrawObjectReference(obj, field.Members, defaultColor);
            }
        }

        public static void DrawArrayObjectReferenceField(ArrayObjectReferenceField field, Color defaultColor)
        {
            object v = field.GetValue();
            IList arr = (IList)v;

            field.editor_showContent = EditorGUILayout.Foldout(field.editor_showContent,
                field.GetName() + System.String.Format(": ({0} Elements)", arr != null ? arr.Count : 0));

            if (field.editor_showContent)
            {
                EditorGUI.indentLevel = 1 + field.IndentLevel;
                if (v != null)
                {
                    for (int i = 0; i < arr.Count; ++i)
                    {
                        field.BindRefernecedObject(arr[i]);
                        DrawObjectReference(arr[i], field.Members, defaultColor);
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("The array is empty");
                }

            }

        }

        public static void DrawField(MemberField field, Color defaultColor)
        {
            EditorGUI.indentLevel = field.IndentLevel;

            if (field.Comment.Length > 0)
            {
                if (field.IsDefaultCommentColor)
                    GUI.color = defaultColor;
                else
                    GUI.color = field.CommentColor;
                EditorGUILayout.HelpBox(field.Comment, field.CommentMessageType);
            }

            if (field.IsDefaultColor)
                GUI.color = defaultColor;
            else
                GUI.color = field.Color;

            if (field is ArrayObjectReferenceField)
            {
                DrawArrayObjectReferenceField((ArrayObjectReferenceField)field, defaultColor);
            }
            else if (field is ArrayFieldField)
            {
                DrawArrayField((ArrayFieldField)field);
            }
            else if (field is MethodField)
            {
                DrawMethodField((MethodField)field);
            }
            else if (field is ObjectReferenceField)
            {
                DrawObjectReferenceField((ObjectReferenceField)field, defaultColor);
            }
            else
            {
                DrawGenericField(field);
            }
        }

        public static void DrawFields(MemberField[] fields)
        {
            Color defaultColor = GUI.color;

            EditorGUILayout.BeginVertical();

            foreach (MemberField field in fields)
            {
                DrawField(field, defaultColor);
            }

            EditorGUILayout.EndVertical();

            GUI.color = defaultColor;
        }

        public static MemberField[] GetFieldsFromType(System.Type type, System.Object obj, int level = 0)
        {
            List<MemberField> fields = new List<MemberField>();

            BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;

            foreach (MemberInfo member in type.GetMembers(bindingFlags))
            {
                ShowInEditor attribute = member.GetCustomAttributes(false).OfType<ShowInEditor>().FirstOrDefault();

                if (attribute != null)
                {
                    if (member is PropertyInfo)
                    {
                        PropertyInfo property = member as PropertyInfo;
                        if (!property.CanRead)
                            continue;

                        // skip array property, to do (or is it neccessary?)
                        if (property.PropertyType.IsArray)
                            continue;

                        if (attribute != null)
                        {
                            SerializedPropertyType serializedType = SerializedPropertyType.Integer;
                            bool isKnownType = PropertyField.GetSerializedType(property, out serializedType);

                            if (!isKnownType)
                                attribute.Flags = ShowInEditorFlags.ReadOnly;

                            fields.Add(new PropertyField(obj, attribute, serializedType, property));
                        }
                    }
                    else if (member is FieldInfo)
                    {
                        FieldInfo field = member as FieldInfo;

                        System.Type realType = field.FieldType;
                        if (realType.IsArray)
                            realType = field.FieldType.GetElementType();

                        bool isObjectRef = !FieldField.IsReadOnly(attribute, field);
                        
                        isObjectRef &= (realType.IsClass ||
                                        (realType.IsValueType && !realType.IsEnum && !realType.IsPrimitive));

                        if (isObjectRef)
                        {
                            ShowInEditor showInEditor = realType.GetCustomAttributes(false).OfType<ShowInEditor>().FirstOrDefault();

                            isObjectRef &= (showInEditor != null);
                        }

                        if (isObjectRef)
                        {
                            MemberField[] memberFields = GetFieldsFromType(realType, null, level + 1);

                            if (!field.FieldType.IsArray)
                            {
                                fields.Add(new ObjectReferenceField(obj, attribute, SerializedPropertyType.ObjectReference, field, memberFields) );
                            }
                            else
                            {
                                fields.Add(new ArrayObjectReferenceField(obj, attribute, SerializedPropertyType.ObjectReference, field, memberFields));
                            }
                        }
                        else
                        {
                            SerializedPropertyType serializedType = SerializedPropertyType.Integer;
                            bool isKnownType = FieldField.GetSerializedType(field, out serializedType);

                            if (!isKnownType)
                                attribute.Flags = ShowInEditorFlags.ReadOnly;

                            MemberField mf;
                            if (!field.FieldType.IsArray)
                                mf = new FieldField(obj, attribute, serializedType, field);
                            else
                                mf = new ArrayFieldField(obj, attribute, serializedType, field);

                            if (mf.IndentLevel == 0)
                                mf.IndentLevel = level;

                            fields.Add(mf);
                        }
                    }
                    else if (member is MethodInfo)
                    {
                        MethodInfo method = member as MethodInfo;

                        if (method.GetParameters().Length == 0)
                        {
                            if (method.ReturnType != typeof(void) || attribute.isButton)
                            {
                                fields.Add(new MethodField(obj, attribute, SerializedPropertyType.Generic, method));
                            }
                            else
                                Debug.LogError("[ShowInEditor] Method that returns nothing cannot be shown in the editor");
                        }
                        else
                        {
                            Debug.LogError("[ShowInEditor] Method with arguments cannot be shown in the editor");
                        }
                    }
                }
            }

            return fields.ToArray();
        }

        public static MemberField[] GetFields(System.Object obj, int level = 0)
        {
            return GetFieldsFromType(obj.GetType(), obj, level);
        }
    }

    public abstract class AUEditor<T> : Editor
    {
        public bool forceRepaint = false;
        public bool drawDefaultFields = true;

        EditorHelper.MemberField[] _properties = null;

        void OnEnable()
        {
            _properties = EditorHelper.GetFields(this.target);
        }

        public override void OnInspectorGUI()
        {
            if (this.target == null)
                return;

            if (_properties == null)
            {
                _properties = EditorHelper.GetFields(this.target);
            }

            if (drawDefaultFields)
            {
                this.DrawDefaultInspector();

                EditorGUILayout.HelpBox("ShowInEditor Fields: ", MessageType.None);
            }

            if (_properties == null)
                return;

            EditorHelper.DrawFields(_properties);

            if (forceRepaint && Application.isPlaying)
            {
                Repaint();
            }
        }

    }

}
