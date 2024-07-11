using UnityEditor;
using UnityEngine;
using DG.Tweening;

[CustomPropertyDrawer(typeof(EaseTypeWrapper))]
// Custom Drawer that allows DOTween types to be changed in editor with the ability to navigate the dropdown with mousewheel while editing
public class EaseTypeDrawer : PropertyDrawer
{
    private bool isHovering = false;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Find the easeType property
        SerializedProperty easeTypeProp = property.FindPropertyRelative("easeType");

        // Draw the enum popup
        Rect fieldRect = new Rect(position.x, position.y, position.width, position.height);
        isHovering = fieldRect.Contains(Event.current.mousePosition);

        // Handle mouse scroll event
        if (isHovering && Event.current.type == EventType.ScrollWheel)
        {
            int direction = Event.current.delta.y > 0 ? 1 : -1;
            easeTypeProp.enumValueIndex = Mathf.Clamp(easeTypeProp.enumValueIndex + direction, 0, System.Enum.GetValues(typeof(Ease)).Length - 1);
            Event.current.Use();
        }

        // Draw the dropdown
        easeTypeProp.enumValueIndex = (int)(Ease)EditorGUI.EnumPopup(fieldRect, label, (Ease)easeTypeProp.enumValueIndex);

        EditorGUI.EndProperty();
    }
}