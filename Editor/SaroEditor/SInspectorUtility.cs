﻿using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Saro.SaroEditor
{
    public static class SInspectorUtility
    {
        /// <summary>Put multiple properties on a single inspector line, with
        /// optional label overrides.  Passing null as a label (or sublabel) override will
        /// cause the property's displayName to be used as a label.  For no label at all,
        /// pass GUIContent.none.</summary>
        public static void MultiPropertyOnLine(
            Rect rect,
            GUIContent label,
            IList<SerializedProperty> props, IList<GUIContent> subLabels)
        {
            if (props == null || props.Count == 0)
                return;

            const int hSpace = 2;
            int indentLevel = EditorGUI.indentLevel;
            float labelWidth = EditorGUIUtility.labelWidth;

            float totalSubLabelWidth = 0;
            int numBoolColumns = 0;
            List<GUIContent> actualLabels = new List<GUIContent>();
            for (int i = 0; i < props.Count; ++i)
            {
                GUIContent sublabel = new GUIContent(props[i].displayName, props[i].tooltip);
                if (subLabels != null && subLabels.Count > i && subLabels[i] != null)
                    sublabel = subLabels[i];
                actualLabels.Add(sublabel);
                totalSubLabelWidth += GUI.skin.label.CalcSize(sublabel).x;
                if (i > 0)
                    totalSubLabelWidth += hSpace;
                // Special handling for toggles, or it looks stupid
                if (props[i].propertyType == SerializedPropertyType.Boolean)
                {
                    totalSubLabelWidth += rect.height;
                    ++numBoolColumns;
                }
            }

            float subFieldWidth = rect.width - labelWidth - totalSubLabelWidth;
            float numCols = props.Count - numBoolColumns;
            float colWidth = numCols == 0 ? 0 : subFieldWidth / numCols;

            // Main label.  If no first sublabel, then main label must take on that
            // role, for mouse dragging value-scrolling support
            int subfieldStartIndex = 0;
            if (label == null)
            {
                label = new GUIContent(props[0].displayName, props[0].tooltip);
            }
            if (actualLabels[0] != GUIContent.none)
            {
                rect = EditorGUI.PrefixLabel(rect, label);
            }
            else
            {
                rect.width = labelWidth + colWidth;
                EditorGUI.PropertyField(rect, props[0], label);
                rect.x += rect.width + hSpace;
                subfieldStartIndex = 1;
            }

            for (int i = subfieldStartIndex; i < props.Count; ++i)
            {
                EditorGUI.indentLevel = 0;
                EditorGUIUtility.labelWidth = GUI.skin.label.CalcSize(actualLabels[i]).x;
                if (props[i].propertyType == SerializedPropertyType.Boolean)
                {
                    rect.width = EditorGUIUtility.labelWidth + rect.height;
                    props[i].boolValue = EditorGUI.ToggleLeft(rect, actualLabels[i], props[i].boolValue);
                }
                else
                {
                    rect.width = EditorGUIUtility.labelWidth + colWidth;
                    EditorGUI.PropertyField(rect, props[i], actualLabels[i]);
                }
                rect.x += rect.width + hSpace;
            }

            EditorGUIUtility.labelWidth = labelWidth;
            EditorGUI.indentLevel = indentLevel;
        }

        public static AnimationCurve NormalizeCurve(AnimationCurve curve)
        {
            Keyframe[] keys = curve.keys;
            if (keys.Length > 0)
            {
                float minTime = keys[0].time;
                float maxTime = minTime;
                float minVal = keys[0].value;
                float maxVal = minVal;
                for (int i = 0; i < keys.Length; ++i)
                {
                    minTime = Mathf.Min(minTime, keys[i].time);
                    maxTime = Mathf.Max(maxTime, keys[i].time);
                    minVal = Mathf.Min(minVal, keys[i].value);
                    maxVal = Mathf.Max(maxVal, keys[i].value);
                }
                float range = maxTime - minTime;
                float timeScale = range < 0.0001f ? 1 : 1 / range;
                range = maxVal - minVal;
                float valScale = range < 1 ? 1 : 1 / range;
                float valOffset = 0;
                if (range < 1)
                {
                    if (minVal > 0 && minVal + range <= 1)
                        valOffset = minVal;
                    else
                        valOffset = 1 - range;
                }
                for (int i = 0; i < keys.Length; ++i)
                {
                    keys[i].time = (keys[i].time - minTime) * timeScale;
                    keys[i].value = ((keys[i].value - minVal) * valScale) + valOffset;
                }
                curve.keys = keys;
            }
            return curve;
        }

        public static string NicifyClassName(string name)
        {
            if (name.StartsWith("Cinemachine"))
                name = name.Substring(11); // Trim the prefix
            return ObjectNames.NicifyVariableName(name);
        }

        // Temporarily here
        public static GameObject CreateGameObject(string name, params Type[] types)
        {
#if UNITY_2018_3_OR_NEWER
            return ObjectFactory.CreateGameObject(name, types);
#else
            return new GameObject(name, types);
#endif
        }

        public static void RepaintGameView(UnityEngine.Object dirtyObject = null)
        {
#if UNITY_2019_1_OR_NEWER
            if (dirtyObject != null)
            {
                var go = dirtyObject as UnityEngine.GameObject;
                if (go == null || go.scene.name != null)    // don't dirty prefabs
                    EditorUtility.SetDirty(dirtyObject);
            }
#endif
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }
    }
}