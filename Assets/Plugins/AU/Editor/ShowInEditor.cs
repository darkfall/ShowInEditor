//
// [ShowInEditor v0.1]
// Developed as part of Project Ambiguous Utopia
// By Darkfall (http://darkfall.me)
// License: MIT
//

using UnityEngine;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR

using UnityEditor;

#endif

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

    public enum ShowInEditorStyle
    {
        Default,
        // button only available for methods
        Button,
        // slider only available for int / floats
        Slider,
    }


    /**
     * 
     */
    public class ShowInEditorRange
    {
        public float Min;
        public float Max;

        public ShowInEditorRange(float min, float max)
        {
            Min = min;
            Max = max;
        }
    }

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
        public bool CompactArrayLayout = false;

        /**
         * When decorating a method, optionally making a button in the inspector
         * and the method will be invoked when the button is clicked
         */
        public ShowInEditorStyle Style = ShowInEditorStyle.Default;

        /**
         * When the field is int / float
         * Optionally making clamping the value within range
         * When the style is slider, the GUI control will be a slider
         * **/
        public float RangeMin = 0;
        public float RangeMax = 0;

        public ShowInEditorRange Range;

        /**
         * When its a button, optionally group the button using the grouping index
         * Default is -1 means no group (vertical aligned)
         */
        public int Group = -1;

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

    }

#if UNITY_EDITOR

    public static class EditorHelper
    {
        public abstract class MemberField
        {
            protected System.Object _obj;
            protected ShowInEditor _attribute;
            protected SerializedPropertyType _type;

            public string NamePrefix = "";

            public ShowInEditor Attribute { get { return _attribute; } }

            public SerializedPropertyType Type { get { return _type; } }

            public ShowInEditorFlags Flags { get { return _attribute.Flags; } }

            public ShowInEditorStyle Style { get { return _attribute.Style; } }

            public string Comment { get { return _attribute.Comment; } }

            public bool CanModifyArrayLength { get { return _attribute.CanModifyArrayLength; } }

            public bool CompactArrayLayout { get { return _attribute.CompactArrayLayout; } }

            public Color Color { get { return _attribute.UnityColor; } }

            public Color CommentColor { get { return _attribute.UnityCommentColor; } }

            public bool IsDefaultColor { get { return _attribute.FieldColor == ShowInEditorColor.Default; } }

            public bool IsDefaultCommentColor { get { return _attribute.CommentColor == ShowInEditorColor.Default; } }

            public int Group { get { return _attribute.Group; } }

            public ShowInEditorRange ValueRange
            {
                get
                {
                    if(_attribute.Range == null && _attribute.RangeMin != _attribute.RangeMax)
                        _attribute.Range = new ShowInEditorRange(_attribute.RangeMin, _attribute.RangeMax);
                    return _attribute.Range;
                }
            }

            public UnityEditor.MessageType CommentMessageType { get { return (UnityEditor.MessageType)_attribute.CommentType; } }

            public int IndentLevel
            {
                get { return _attribute.IndentLevel; }
                set { _attribute.IndentLevel = value; }
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

        public class ButtonGroupField : MethodField
        {
            protected MethodField[] _buttons;

            public MethodField[] Buttons
            {
                get { return _buttons; }
            }

            public ButtonGroupField(MethodField[] buttons)
                : base(null, null, SerializedPropertyType.Generic, null)
            {
                _buttons = buttons;
                _attribute = _buttons[0].Attribute;
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

        public static int DrawIntegerField(string name, object value, System.Type sysType, ShowInEditorRange range, ShowInEditorStyle style)
        {
            int v;
            if (style != ShowInEditorStyle.Slider || range == null)
            {
                v = EditorGUILayout.IntField(name, (int)value);
                if (range != null)
                    v = (int)Mathf.Clamp(v, range.Min, range.Max);
                return v;
            }
            else
            {
                int vv = (int)value;
                v = (int)EditorGUILayout.Slider(name, (float)vv, range.Min, range.Max);
                return v;
            }
        }

        public static float DrawFloatField(string name, object value, System.Type sysType, ShowInEditorRange range, ShowInEditorStyle style)
        {
            float fv;
            if (sysType != typeof(double))
            {
                fv = (float)value;
            }
            else
            {
                // aaaa
                double v = (double)value;
                fv = (float)v;
            }

            float rv;
            if (style != ShowInEditorStyle.Slider || range == null)
            {
                rv = EditorGUILayout.FloatField(name, fv);
                if (range != null)
                    rv = Mathf.Clamp(rv, range.Min, range.Max);
            }
            else
            {
                rv = EditorGUILayout.Slider(name, fv, range.Min, range.Max);
            }
            return rv;
        }

        public static object DrawFieldElement(SerializedPropertyType type, string name, object value, System.Type sysType, ShowInEditorRange range, ShowInEditorStyle style)
        {
            switch (type)
            {
                case SerializedPropertyType.Integer:
                    return DrawIntegerField(name, value, sysType, range, style);

                case SerializedPropertyType.Float:
                    return DrawFloatField(name, value, sysType, range, style);
                        
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

        public static object DrawFieldElementCompact(SerializedPropertyType type, object value, System.Type sysType, ShowInEditorRange range, ShowInEditorStyle style)
        {
            switch (type)
            {
                case SerializedPropertyType.Integer:
                    return DrawIntegerField("", value, sysType, range, style);

                case SerializedPropertyType.Float:
                    return DrawFloatField("", value, sysType, range, style);
                        
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
                        if (field.CompactArrayLayout && type.IsPrimitive)
                        {
                            EditorGUILayout.BeginHorizontal();

                            if (field.Type == SerializedPropertyType.Boolean)
                            {
                                // for some reason, editor gui layout doesn't work here
                                GUILayout.Space(EditorGUI.indentLevel * 24);
                            }
                            for (int i = 0; i < arr.Count; ++i)
                            {
                                arr[i] = DrawFieldElementCompact(field.Type, arr[i], type, field.ValueRange, field.Style);
                            }

                            EditorGUILayout.EndHorizontal();
                        }
                        else
                        {
                            for (int i = 0; i < arr.Count; ++i)
                            {
                                arr[i] = DrawFieldElement(field.Type, string.Format("[{0}]", i), arr[i], type, field.ValueRange, field.Style);
                            }
                        }
                    }
                }
            }
        }

        public static void DrawMethodField(MethodField field)
        {
            if (field.Style != ShowInEditorStyle.Button)
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
            field.SetValue(DrawFieldElement(field.Type, field.GetName(), field.GetValue(), field.GetFieldType(), field.ValueRange, field.Style));

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

                if (field.CanModifyArrayLength)
                    field.Length = EditorGUILayout.IntField("Length", field.Length);

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

        public static void DrawButtonGroup(ButtonGroupField field)
        {
            GUILayout.BeginHorizontal();
            foreach (MethodField button in field.Buttons)
            {
                if (GUILayout.Button(button.GetName()))
                {
                    button.Invoke();
                }
            }
            GUILayout.EndHorizontal();
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
            else if(field is ButtonGroupField)
            {
                DrawButtonGroup((ButtonGroupField)field);
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
            Dictionary<int, List<MethodField>> buttonFields = new Dictionary<int, List<MethodField>>();

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

                        MemberField mf;
                        if (isObjectRef)
                        {
                            MemberField[] memberFields = GetFieldsFromType(realType, null, level + 1);

                            if (!field.FieldType.IsArray)
                            {
                                mf = (new ObjectReferenceField(obj, attribute, SerializedPropertyType.ObjectReference, field, memberFields));
                            }
                            else
                            {
                                mf = (new ArrayObjectReferenceField(obj, attribute, SerializedPropertyType.ObjectReference, field, memberFields));
                            }
                        }
                        else
                        {
                            SerializedPropertyType serializedType = SerializedPropertyType.Integer;
                            bool isKnownType = FieldField.GetSerializedType(field, out serializedType);

                            if (!isKnownType)
                                attribute.Flags = ShowInEditorFlags.ReadOnly;

                            if (!field.FieldType.IsArray)
                                mf = new FieldField(obj, attribute, serializedType, field);
                            else
                                mf = new ArrayFieldField(obj, attribute, serializedType, field);
                        }

                        if (mf != null)
                        {
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
                            if (method.ReturnType != typeof(void))
                            {
                                fields.Add(new MethodField(obj, attribute, SerializedPropertyType.Generic, method));
                            }
                            else if (attribute.Style == ShowInEditorStyle.Button)
                            {
                                if (attribute.Group == -1)
                                {
                                    fields.Add(new MethodField(obj, attribute, SerializedPropertyType.Generic, method));
                                }
                                else
                                {
                                    List<MethodField> group;
                                    if (!buttonFields.TryGetValue(attribute.Group, out group))
                                    {
                                        group = new List<MethodField>();
                                        buttonFields.Add(attribute.Group, group);
                                    }
                                    group.Add(new MethodField(obj, attribute, SerializedPropertyType.Generic, method));
                                }
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

            foreach (List<MethodField> buttonGroup in buttonFields.Values)
            {
                fields.Add(new ButtonGroupField(buttonGroup.ToArray()));
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
    
#endif
}

