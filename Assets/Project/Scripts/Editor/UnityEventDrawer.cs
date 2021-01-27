﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Treevel.Common.Attributes;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace Treevel.Editor
{
    [CustomPropertyDrawer(typeof(UnityEvent), true)]
    [CustomPropertyDrawer(typeof(UnityEvent<bool>), true)]
    [CustomPropertyDrawer(typeof(UnityEvent<Int32>), true)]
    public class UnityEventDrawer : PropertyDrawer
    {
        private Dictionary<string, State> m_States = new Dictionary<string, State>();

        // Find internal methods with reflection
        private static readonly MethodInfo findMethod = typeof(UnityEventBase).GetMethod(
            "FindMethod", BindingFlags.NonPublic | BindingFlags.Instance, null, CallingConventions.Standard,
            new[] { typeof(string), typeof(Type), typeof(PersistentListenerMode), typeof(Type) }, null);

        private static readonly MethodInfo temp = typeof(GUIContent).GetMethod(
            "Temp", BindingFlags.NonPublic | BindingFlags.Static, null, CallingConventions.Standard,
            new[] { typeof(string) }, null);

        private static readonly PropertyInfo mixedValueContent =
            typeof(EditorGUI).GetProperty("mixedValueContent", BindingFlags.NonPublic | BindingFlags.Static);

        private Styles m_Styles;
        private string m_Text;
        private UnityEventBase m_DummyEvent;
        private SerializedProperty m_Prop;
        private SerializedProperty m_ListenersArray;
        private ReorderableList m_ReorderableList;
        private int m_LastSelectedIndex;

        private static string GetEventParams(UnityEventBase evt)
        {
            var method =
                (MethodInfo)findMethod.Invoke(
                    evt, new object[] { "Invoke", evt.GetType(), PersistentListenerMode.EventDefined, null });
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(" (");
            var array = method.GetParameters().Select(x => x.ParameterType).ToArray();
            for (var index = 0; index < array.Length; ++index) {
                stringBuilder.Append(array[index].Name);
                if (index < array.Length - 1) stringBuilder.Append(", ");
            }

            stringBuilder.Append(")");
            return stringBuilder.ToString();
        }

        private State GetState(SerializedProperty prop)
        {
            var propertyPath = prop.propertyPath;
            State state;
            m_States.TryGetValue(propertyPath, out state);
            if (state == null) {
                state = new State();
                var propertyRelative = prop.FindPropertyRelative("m_PersistentCalls.m_Calls");
                state.m_ReorderableList =
                    new ReorderableList(prop.serializedObject, propertyRelative, false, true, true, true);
                state.m_ReorderableList.drawHeaderCallback =
                    new ReorderableList.HeaderCallbackDelegate(DrawEventHeader);
                state.m_ReorderableList.drawElementCallback =
                    new ReorderableList.ElementCallbackDelegate(DrawEventListener);
                state.m_ReorderableList.onSelectCallback =
                    new ReorderableList.SelectCallbackDelegate(SelectEventListener);
                state.m_ReorderableList.onReorderCallback = new ReorderableList.ReorderCallbackDelegate(EndDragChild);
                state.m_ReorderableList.onAddCallback = new ReorderableList.AddCallbackDelegate(AddEventListener);
                state.m_ReorderableList.onRemoveCallback = new ReorderableList.RemoveCallbackDelegate(RemoveButton);
                state.m_ReorderableList.elementHeight = 43f;
                m_States[propertyPath] = state;
            }

            return state;
        }

        private State RestoreState(SerializedProperty property)
        {
            var state = GetState(property);
            m_ListenersArray = state.m_ReorderableList.serializedProperty;
            m_ReorderableList = state.m_ReorderableList;
            m_LastSelectedIndex = state.lastSelectedIndex;
            m_ReorderableList.index = m_LastSelectedIndex;
            return state;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            m_Prop = property;
            m_Text = label.text;
            var state = RestoreState(property);
            OnGUI(position);
            state.lastSelectedIndex = m_LastSelectedIndex;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            RestoreState(property);
            var num = 0.0f;
            if (m_ReorderableList != null) num = m_ReorderableList.GetHeight();
            return num;
        }

        public void OnGUI(Rect position)
        {
            if (m_ListenersArray == null || !m_ListenersArray.isArray) return;
            m_DummyEvent = GetDummyEvent(m_Prop);
            if (m_DummyEvent == null) return;
            if (m_Styles == null) m_Styles = new Styles();
            if (m_ReorderableList == null) return;
            var indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            m_ReorderableList.DoList(position);
            EditorGUI.indentLevel = indentLevel;
        }

        protected virtual void DrawEventHeader(Rect headerRect)
        {
            headerRect.height = 16f;
            var text = (!string.IsNullOrEmpty(m_Text) ? m_Text : "Event") + GetEventParams(m_DummyEvent);
            GUI.Label(headerRect, text);
        }

        private static PersistentListenerMode GetMode(SerializedProperty mode)
        {
            return (PersistentListenerMode)mode.enumValueIndex;
        }

        private void DrawEventListener(Rect rect, int index, bool isactive, bool isfocused)
        {
            var arrayElementAtIndex = m_ListenersArray.GetArrayElementAtIndex(index);
            ++rect.y;
            var rowRects = GetRowRects(rect);
            var position1 = rowRects[0];
            var position2 = rowRects[1];
            var rect1 = rowRects[2];
            var position3 = rowRects[3];
            var propertyRelative1 = arrayElementAtIndex.FindPropertyRelative("m_CallState");
            var propertyRelative2 = arrayElementAtIndex.FindPropertyRelative("m_Mode");
            var propertyRelative3 = arrayElementAtIndex.FindPropertyRelative("m_Arguments");
            var propertyRelative4 = arrayElementAtIndex.FindPropertyRelative("m_Target");
            var propertyRelative5 = arrayElementAtIndex.FindPropertyRelative("m_MethodName");
            var backgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.white;
            EditorGUI.PropertyField(position1, propertyRelative1, GUIContent.none);
            EditorGUI.BeginChangeCheck();
            GUI.Box(position2, GUIContent.none);
            EditorGUI.PropertyField(position2, propertyRelative4, GUIContent.none);
            if (EditorGUI.EndChangeCheck()) propertyRelative5.stringValue = null;
            var persistentListenerMode = GetMode(propertyRelative2);
            if (propertyRelative4.objectReferenceValue == null || string.IsNullOrEmpty(propertyRelative5.stringValue)) persistentListenerMode = PersistentListenerMode.Void;
            SerializedProperty propertyRelative6;
            switch (persistentListenerMode) {
                case PersistentListenerMode.Object:
                    propertyRelative6 = propertyRelative3.FindPropertyRelative("m_ObjectArgument");
                    break;
                case PersistentListenerMode.Int:
                    propertyRelative6 = propertyRelative3.FindPropertyRelative("m_IntArgument");
                    break;
                case PersistentListenerMode.Float:
                    propertyRelative6 = propertyRelative3.FindPropertyRelative("m_FloatArgument");
                    break;
                case PersistentListenerMode.String:
                    propertyRelative6 = propertyRelative3.FindPropertyRelative("m_StringArgument");
                    break;
                case PersistentListenerMode.Bool:
                    propertyRelative6 = propertyRelative3.FindPropertyRelative("m_BoolArgument");
                    break;
                default:
                    propertyRelative6 = propertyRelative3.FindPropertyRelative("m_IntArgument");
                    break;
            }

            var stringValue = propertyRelative3.FindPropertyRelative("m_ObjectArgumentAssemblyTypeName").stringValue;
            var type = typeof(Object);
            if (!string.IsNullOrEmpty(stringValue)) type = Type.GetType(stringValue, false) ?? typeof(Object);
            if (persistentListenerMode == PersistentListenerMode.Object) {
                EditorGUI.BeginChangeCheck();
                var @object = EditorGUI.ObjectField(position3, GUIContent.none, propertyRelative6.objectReferenceValue,
                                                    type, true);
                if (EditorGUI.EndChangeCheck()) propertyRelative6.objectReferenceValue = @object;
            } else if (persistentListenerMode != PersistentListenerMode.Void &&
                       persistentListenerMode != PersistentListenerMode.EventDefined) {
                // Try to find Find the EnumActionAttribute
                var method = GetMethod(m_DummyEvent, propertyRelative5.stringValue,
                                       propertyRelative4.objectReferenceValue, GetMode(propertyRelative2), type);
                object[] attributes = null;
                if (method != null) attributes = method.GetCustomAttributes(typeof(EnumActionAttribute), true);
                if (attributes != null && attributes.Length > 0) {
                    // Make an enum popup
                    var enumType = ((EnumActionAttribute)attributes[0]).enumType;
                    var value = (Enum)Enum.ToObject(enumType, propertyRelative6.intValue);
                    propertyRelative6.intValue = Convert.ToInt32(EditorGUI.EnumPopup(position3, value));
                } else {
                    EditorGUI.PropertyField(position3, propertyRelative6, GUIContent.none);
                }
            }

            EditorGUI.BeginDisabledGroup(propertyRelative4.objectReferenceValue == null);
            EditorGUI.BeginProperty(rect1, GUIContent.none, propertyRelative5);
            GUIContent content;
            if (EditorGUI.showMixedValue) {
                content = (GUIContent)mixedValueContent.GetValue(null, null);
            } else {
                var stringBuilder = new StringBuilder();
                if (propertyRelative4.objectReferenceValue == null ||
                    string.IsNullOrEmpty(propertyRelative5.stringValue))
                    stringBuilder.Append("No Function");
                else if (!IsPersistantListenerValid(m_DummyEvent, propertyRelative5.stringValue,
                                                    propertyRelative4.objectReferenceValue, GetMode(propertyRelative2),
                                                    type)) {
                    var str = "UnknownComponent";
                    var objectReferenceValue = propertyRelative4.objectReferenceValue;
                    if (objectReferenceValue != null) str = objectReferenceValue.GetType().Name;
                    stringBuilder.Append(string.Format("<Missing {0}.{1}>", str, propertyRelative5.stringValue));
                } else {
                    stringBuilder.Append(propertyRelative4.objectReferenceValue.GetType().Name);
                    if (!string.IsNullOrEmpty(propertyRelative5.stringValue)) {
                        stringBuilder.Append(".");
                        if (propertyRelative5.stringValue.StartsWith("set_"))
                            stringBuilder.Append(propertyRelative5.stringValue.Substring(4));
                        else
                            stringBuilder.Append(propertyRelative5.stringValue);
                    }
                }

                content = (GUIContent)temp.Invoke(null, new object[] { stringBuilder.ToString() });
            }

            if (GUI.Button(rect1, content, EditorStyles.popup)) {
                BuildPopupList(propertyRelative4.objectReferenceValue, m_DummyEvent, arrayElementAtIndex)
                    .DropDown(rect1);
            }

            EditorGUI.EndProperty();
            EditorGUI.EndDisabledGroup();
            GUI.backgroundColor = backgroundColor;
        }

        private Rect[] GetRowRects(Rect rect)
        {
            var rectArray = new Rect[4];
            rect.height = 16f;
            rect.y += 2f;
            var rect1 = rect;
            rect1.width *= 0.3f;
            var rect2 = rect1;
            rect2.y += EditorGUIUtility.singleLineHeight + 2f;
            var rect3 = rect;
            rect3.xMin = rect2.xMax + 5f;
            var rect4 = rect3;
            rect4.y += EditorGUIUtility.singleLineHeight + 2f;
            rectArray[0] = rect1;
            rectArray[1] = rect2;
            rectArray[2] = rect3;
            rectArray[3] = rect4;
            return rectArray;
        }

        private void RemoveButton(ReorderableList list)
        {
            ReorderableList.defaultBehaviours.DoRemoveButton(list);
            m_LastSelectedIndex = list.index;
        }

        private void AddEventListener(ReorderableList list)
        {
            if (m_ListenersArray.hasMultipleDifferentValues) {
                foreach (var targetObject in m_ListenersArray.serializedObject.targetObjects) {
                    var serializedObject = new SerializedObject(targetObject);
                    ++serializedObject.FindProperty(m_ListenersArray.propertyPath).arraySize;
                    serializedObject.ApplyModifiedProperties();
                }

                m_ListenersArray.serializedObject.SetIsDifferentCacheDirty();
                m_ListenersArray.serializedObject.Update();
                list.index = list.serializedProperty.arraySize - 1;
            } else
                ReorderableList.defaultBehaviours.DoAddButton(list);

            m_LastSelectedIndex = list.index;
            var arrayElementAtIndex = m_ListenersArray.GetArrayElementAtIndex(list.index);
            var propertyRelative1 = arrayElementAtIndex.FindPropertyRelative("m_CallState");
            var propertyRelative2 = arrayElementAtIndex.FindPropertyRelative("m_Target");
            var propertyRelative3 = arrayElementAtIndex.FindPropertyRelative("m_MethodName");
            var propertyRelative4 = arrayElementAtIndex.FindPropertyRelative("m_Mode");
            var propertyRelative5 = arrayElementAtIndex.FindPropertyRelative("m_Arguments");
            propertyRelative1.enumValueIndex = 2;
            propertyRelative2.objectReferenceValue = null;
            propertyRelative3.stringValue = null;
            propertyRelative4.enumValueIndex = 1;
            propertyRelative5.FindPropertyRelative("m_FloatArgument").floatValue = 0.0f;
            propertyRelative5.FindPropertyRelative("m_IntArgument").intValue = 0;
            propertyRelative5.FindPropertyRelative("m_ObjectArgument").objectReferenceValue = null;
            propertyRelative5.FindPropertyRelative("m_StringArgument").stringValue = null;
            propertyRelative5.FindPropertyRelative("m_ObjectArgumentAssemblyTypeName").stringValue = null;
        }

        private void SelectEventListener(ReorderableList list)
        {
            m_LastSelectedIndex = list.index;
        }

        private void EndDragChild(ReorderableList list)
        {
            m_LastSelectedIndex = list.index;
        }

        private static UnityEventBase GetDummyEvent(SerializedProperty prop)
        {
            var type = Type.GetType(prop.type, false);
            if (type == null) return new UnityEvent();
            return Activator.CreateInstance(type) as UnityEventBase;
        }

        private static IEnumerable<ValidMethodMap> CalculateMethodMap(Object target, Type[] t,
                                                                      bool allowSubclasses)
        {
            var validMethodMapList = new List<ValidMethodMap>();
            if (target == null || t == null) return validMethodMapList;
            var type = target.GetType();
            var list = type.GetMethods().Where(x => !x.IsSpecialName).ToList();
            var source = type.GetProperties().AsEnumerable().Where(x => {
                if (x.GetCustomAttributes(typeof(ObsoleteAttribute), true).Length == 0) return x.GetSetMethod() != null;
                return false;
            });
            list.AddRange(source.Select(x => x.GetSetMethod()));
            using (var enumerator = list.GetEnumerator()) {
                while (enumerator.MoveNext()) {
                    var current = enumerator.Current;
                    var parameters = current.GetParameters();
                    if (parameters.Length == t.Length &&
                        current.GetCustomAttributes(typeof(ObsoleteAttribute), true).Length <= 0 &&
                        current.ReturnType == typeof(void)) {
                        var flag = true;
                        for (var index = 0; index < t.Length; ++index) {
                            if (!parameters[index].ParameterType.IsAssignableFrom(t[index])) flag = false;
                            if (allowSubclasses && t[index].IsAssignableFrom(parameters[index].ParameterType)) flag = true;
                        }

                        if (flag)
                            validMethodMapList.Add(new ValidMethodMap() {
                                target = target,
                                methodInfo = current
                            });
                    }
                }
            }

            return validMethodMapList;
        }

        public static bool IsPersistantListenerValid(UnityEventBase dummyEvent, string methodName,
                                                     Object uObject, PersistentListenerMode modeEnum,
                                                     Type argumentType)
        {
            if (uObject == null || string.IsNullOrEmpty(methodName)) return false;
            return GetMethod(dummyEvent, methodName, uObject, modeEnum, argumentType) != null;
        }

        private static MethodInfo GetMethod(UnityEventBase dummyEvent, string methodName, Object uObject,
                                            PersistentListenerMode modeEnum, Type argumentType)
        {
            return (MethodInfo)findMethod.Invoke(
                dummyEvent, new object[] { methodName, uObject.GetType(), modeEnum, argumentType });
        }

        private static GenericMenu BuildPopupList(Object target, UnityEventBase dummyEvent,
                                                  SerializedProperty listener)
        {
            var target1 = target;
            if (target1 is Component) target1 = (target as Component).gameObject;
            var propertyRelative = listener.FindPropertyRelative("m_MethodName");
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("No Function"), string.IsNullOrEmpty(propertyRelative.stringValue),
                         ClearEventFunction,
                         new UnityEventFunction(listener, null, null, PersistentListenerMode.EventDefined));
            if (target1 == null) return menu;
            menu.AddSeparator(string.Empty);
            var array = dummyEvent.GetType().GetMethod("Invoke").GetParameters()
                .Select(x => x.ParameterType).ToArray();
            GeneratePopUpForType(menu, target1, false, listener, array);
            if (target1 is GameObject) {
                var components = (target1 as GameObject).GetComponents<Component>();
                var list = components.Where(c => c != null).Select(c => c.GetType().Name)
                    .GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
                foreach (var component in components) {
                    if (!(component == null)) GeneratePopUpForType(menu, component, list.Contains(component.GetType().Name), listener, array);
                }
            }

            return menu;
        }

        private static void GeneratePopUpForType(GenericMenu menu, Object target, bool useFullTargetName,
                                                 SerializedProperty listener, Type[] delegateArgumentsTypes)
        {
            var methods = new List<ValidMethodMap>();
            var targetName = !useFullTargetName ? target.GetType().Name : target.GetType().FullName;
            var flag = false;
            if (delegateArgumentsTypes.Length != 0) {
                GetMethodsForTargetAndMode(target, delegateArgumentsTypes, methods,
                                           PersistentListenerMode.EventDefined);
                if (methods.Count > 0) {
                    menu.AddDisabledItem(new GUIContent(targetName + "/Dynamic " +
                                                        string.Join(
                                                            ", ",
                                                            delegateArgumentsTypes
                                                                .Select(e => GetTypeName(e)).ToArray())));
                    AddMethodsToMenu(menu, listener, methods, targetName);
                    flag = true;
                }
            }

            methods.Clear();
            GetMethodsForTargetAndMode(target, new Type[1] { typeof(float) }, methods, PersistentListenerMode.Float);
            GetMethodsForTargetAndMode(target, new Type[1] { typeof(int) }, methods, PersistentListenerMode.Int);
            GetMethodsForTargetAndMode(target, new Type[1] { typeof(string) }, methods, PersistentListenerMode.String);
            GetMethodsForTargetAndMode(target, new Type[1] { typeof(bool) }, methods, PersistentListenerMode.Bool);
            GetMethodsForTargetAndMode(target, new Type[1] { typeof(Object) }, methods,
                                       PersistentListenerMode.Object, true);
            GetMethodsForTargetAndMode(target, new Type[0], methods, PersistentListenerMode.Void);
            if (methods.Count <= 0) return;
            if (flag) menu.AddItem(new GUIContent(targetName + "/ "), false, null);
            if (delegateArgumentsTypes.Length != 0) menu.AddDisabledItem(new GUIContent(targetName + "/Static Parameters"));
            AddMethodsToMenu(menu, listener, methods, targetName);
        }

        private static void AddMethodsToMenu(GenericMenu menu, SerializedProperty listener,
                                             List<ValidMethodMap> methods, string targetName)
        {
            foreach (var method in methods.OrderBy(e => e.methodInfo.Name.StartsWith("set_") ? 0 : 1)
                .ThenBy(e => e.methodInfo.Name)) {
                AddFunctionsForScript(menu, listener, method, targetName);
            }
        }

        private static void GetMethodsForTargetAndMode(Object target, Type[] delegateArgumentsTypes,
                                                       List<ValidMethodMap> methods, PersistentListenerMode mode,
                                                       bool allowSubclasses = false)
        {
            var methodMaps = CalculateMethodMap(target, delegateArgumentsTypes, allowSubclasses).ToArray();
            for (var i = 0; i < methodMaps.Length; i++) {
                methodMaps[i].mode = mode;
                methods.Add(methodMaps[i]);
            }
        }

        private static void AddFunctionsForScript(GenericMenu menu, SerializedProperty listener, ValidMethodMap method,
                                                  string targetName)
        {
            var mode1 = method.mode;
            var objectReferenceValue = listener.FindPropertyRelative("m_Target").objectReferenceValue;
            var stringValue = listener.FindPropertyRelative("m_MethodName").stringValue;
            var mode2 = GetMode(listener.FindPropertyRelative("m_Mode"));
            var propertyRelative = listener.FindPropertyRelative("m_Arguments")
                .FindPropertyRelative("m_ObjectArgumentAssemblyTypeName");
            var stringBuilder = new StringBuilder();
            var length = method.methodInfo.GetParameters().Length;
            for (var index = 0; index < length; ++index) {
                var parameter = method.methodInfo.GetParameters()[index];
                stringBuilder.Append(string.Format("{0}", GetTypeName(parameter.ParameterType)));
                if (index < length - 1) stringBuilder.Append(", ");
            }

            var on = objectReferenceValue == method.target && stringValue == method.methodInfo.Name && mode1 == mode2;
            if (on && mode1 == PersistentListenerMode.Object && method.methodInfo.GetParameters().Length == 1)
                on &= method.methodInfo.GetParameters()[0].ParameterType.AssemblyQualifiedName ==
                      propertyRelative.stringValue;
            var formattedMethodName = GetFormattedMethodName(targetName, method.methodInfo.Name,
                                                             stringBuilder.ToString(),
                                                             mode1 == PersistentListenerMode.EventDefined);
            menu.AddItem(new GUIContent(formattedMethodName), on, SetEventFunction,
                         new UnityEventFunction(listener, method.target, method.methodInfo, mode1));
        }

        private static string GetTypeName(Type t)
        {
            if (t == typeof(int)) return "int";
            if (t == typeof(float)) return "float";
            if (t == typeof(string)) return "string";
            if (t == typeof(bool)) return "bool";
            return t.Name;
        }

        private static string GetFormattedMethodName(string targetName, string methodName, string args, bool dynamic)
        {
            if (dynamic) {
                if (methodName.StartsWith("set_")) return string.Format("{0}/{1}", targetName, methodName.Substring(4));
                return string.Format("{0}/{1}", targetName, methodName);
            }

            if (methodName.StartsWith("set_")) return string.Format("{0}/{2} {1}", targetName, methodName.Substring(4), args);
            return string.Format("{0}/{1} ({2})", targetName, methodName, args);
        }

        private static void SetEventFunction(object source)
        {
            ((UnityEventFunction)source).Assign();
        }

        private static void ClearEventFunction(object source)
        {
            ((UnityEventFunction)source).Clear();
        }

        protected class State
        {
            internal ReorderableList m_ReorderableList;
            public int lastSelectedIndex;
        }

        private class Styles
        {
            public readonly GUIContent iconToolbarMinus = EditorGUIUtility.IconContent("Toolbar Minus");
            public readonly GUIStyle genericFieldStyle = EditorStyles.label;
            public readonly GUIStyle removeButton = "InvisibleButton";
        }

        private struct ValidMethodMap
        {
            public Object target;
            public MethodInfo methodInfo;
            public PersistentListenerMode mode;
        }

        private struct UnityEventFunction
        {
            private readonly SerializedProperty m_Listener;
            private readonly Object m_Target;
            private readonly MethodInfo m_Method;
            private readonly PersistentListenerMode m_Mode;

            public UnityEventFunction(SerializedProperty listener, Object target, MethodInfo method,
                                      PersistentListenerMode mode)
            {
                m_Listener = listener;
                m_Target = target;
                m_Method = method;
                m_Mode = mode;
            }

            public void Assign()
            {
                var propertyRelative1 = m_Listener.FindPropertyRelative("m_Target");
                var propertyRelative2 = m_Listener.FindPropertyRelative("m_MethodName");
                var propertyRelative3 = m_Listener.FindPropertyRelative("m_Mode");
                var propertyRelative4 = m_Listener.FindPropertyRelative("m_Arguments");
                propertyRelative1.objectReferenceValue = m_Target;
                propertyRelative2.stringValue = m_Method.Name;
                propertyRelative3.enumValueIndex = (int)m_Mode;
                if (m_Mode == PersistentListenerMode.Object) {
                    var propertyRelative5 = propertyRelative4.FindPropertyRelative("m_ObjectArgumentAssemblyTypeName");
                    var parameters = m_Method.GetParameters();
                    propertyRelative5.stringValue =
                        parameters.Length != 1 ||
                        !typeof(Object).IsAssignableFrom(parameters[0].ParameterType)
                            ? typeof(Object).AssemblyQualifiedName
                            : parameters[0].ParameterType.AssemblyQualifiedName;
                }

                ValidateObjectParamater(propertyRelative4, m_Mode);
                m_Listener.serializedObject.ApplyModifiedProperties();
            }

            private void ValidateObjectParamater(SerializedProperty arguments, PersistentListenerMode mode)
            {
                var propertyRelative1 = arguments.FindPropertyRelative("m_ObjectArgumentAssemblyTypeName");
                var propertyRelative2 = arguments.FindPropertyRelative("m_ObjectArgument");
                var objectReferenceValue = propertyRelative2.objectReferenceValue;
                if (mode != PersistentListenerMode.Object) {
                    propertyRelative2.objectReferenceValue = null;
                } else {
                    if (objectReferenceValue == null) return;
                    var type = Type.GetType(propertyRelative1.stringValue, false);
                    if (typeof(Object).IsAssignableFrom(type) &&
                        type.IsInstanceOfType(objectReferenceValue)) {
                        return;
                    }

                    propertyRelative2.objectReferenceValue = null;
                }
            }

            public void Clear()
            {
                m_Listener.FindPropertyRelative("m_MethodName").stringValue = null;
                m_Listener.FindPropertyRelative("m_Mode").enumValueIndex = 1;
                m_Listener.serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
