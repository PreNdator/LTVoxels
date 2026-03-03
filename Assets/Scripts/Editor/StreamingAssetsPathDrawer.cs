using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LedenevTV.Editor
{

    [CustomPropertyDrawer(typeof(StreamingAssetsPathAttribute), true)]
    public sealed class StreamingAssetsPathDrawer : PropertyDrawer
    {
        private const string StreamingAssetsRoot = "Assets/StreamingAssets";

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!EnsureString(property, position, label))
                return;

            string[] allowedExtensions = ((StreamingAssetsPathAttribute)attribute).AllowedExtensions;

            string prevValue = property.stringValue;

            UnityEngine.Object currObj = LoadCurrentObject(property.stringValue);

            using (EditorGUI.ChangeCheckScope check = new EditorGUI.ChangeCheckScope())
            {
                UnityEngine.Object picked = EditorGUI.ObjectField(position, label, currObj, typeof(UnityEngine.Object), false);

                if (check.changed)
                {
                    ApplyPicked(property, prevValue, picked, allowedExtensions);
                }
            }
        }

        private static bool EnsureString(SerializedProperty property, Rect position, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.String)
                return true;

            EditorGUI.LabelField(position, label.text, "Use [StreamingAssetsPath] with string fields.");

            return false;
        }

        private static UnityEngine.Object LoadCurrentObject(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                return null;

            string fullAssetPath = (StreamingAssetsRoot + relativePath);

            return AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(fullAssetPath);
        }

        private void ApplyPicked(
            SerializedProperty property,
            string prevValue,
            UnityEngine.Object picked,
            string[] allowedExtensions)
        {
            if (picked == null)
            {
                property.stringValue = string.Empty;
                return;
            }

            string pickedPath = AssetDatabase.GetAssetPath(picked);

            if (!ValidatePickedPath(pickedPath, allowedExtensions))
            {
                property.stringValue = prevValue;
                return;
            }

            property.stringValue = pickedPath.Substring(StreamingAssetsRoot.Length);
        }

        private static bool ValidatePickedPath(string pickedPath, string[] allowedExtensions)
        {
            if (!pickedPath.StartsWith(StreamingAssetsRoot, StringComparison.Ordinal))
            {
                Debug.LogError($"Asset must be inside '{StreamingAssetsRoot}'.");
                return false;
            }

            if (AssetDatabase.IsValidFolder(pickedPath))
            {
                Debug.LogError($"Folders are not allowed. Pick a file {GetAllowedExtensionsList(allowedExtensions)}");
                return false;
            }

            if (HasRestrictions(allowedExtensions) && !HasAllowedExtension(pickedPath, allowedExtensions))
            {
                Debug.LogError($"Only extensions allowed: {GetAllowedExtensionsList(allowedExtensions)}");
                return false;
            }

            return true;
        }

        private static bool HasRestrictions(string[] allowedExtensions)
        {
            return allowedExtensions != null && allowedExtensions.Length > 0;
        }

        private static bool HasAllowedExtension(string pickedPath, string[] allowedExtensions)
        {
            string extension = Path.GetExtension(pickedPath);

            for (int i = 0; i < allowedExtensions.Length; i++)
            {
                if (string.Equals(extension, allowedExtensions[i], StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static string GetAllowedExtensionsList(string[] allowedExtensions)
        {
            if (HasRestrictions(allowedExtensions))
                return $"({string.Join(", ", allowedExtensions)})";
            else
                return "(any)";
        }
    }
}