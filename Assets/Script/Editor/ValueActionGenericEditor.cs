using UnityEngine;
using UnityEditor; // Editor 관련 네임스페이스 추가

// ValueActionGeneric 클래스의 인스펙터를 커스텀으로 그리겠다고 명시
[CustomEditor(typeof(ValueAction))]
public class ValueActionGenericEditor : Editor
{
    // 인스펙터 GUI를 다시 그리는 메서드
    public override void OnInspectorGUI()
    {
        // base.OnInspectorGUI(); // 기본 인스펙터를 그리는 코드. 필요 없으므로 주석 처리

        // 수정할 대상 스크립트 객체를 가져옴
        var action = (ValueAction)target;

        // SerializedObject를 사용하면 Undo/Redo가 가능하고, 값이 제대로 저장됨
        serializedObject.Update();

        // priority, op 필드는 기본 방식으로 그림
        EditorGUILayout.PropertyField(serializedObject.FindProperty("priority"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("op"));

        // "값 (Generic)" 헤더와 같은 시각적 구분을 위해 공간을 줌
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("값 (Generic)", EditorStyles.boldLabel);

        // valueType Enum 드롭다운을 그림
        EditorGUILayout.PropertyField(serializedObject.FindProperty("valueType"));

        // valueType의 현재 값에 따라 다른 필드를 그림
        switch (action.valueType)
        {
            case ValueAction.ValueType.Int:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("intValue"), new GUIContent("Value"));
                break;

            case ValueAction.ValueType.Float:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("floatValue"), new GUIContent("Value"));
                break;

            case ValueAction.ValueType.String:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("stringValue"), new GUIContent("Value"));
                break;

            case ValueAction.ValueType.Bool:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("boolValue"), new GUIContent("Value"));
                break;
        }

        // 인스펙터에서 변경된 값을 실제 객체에 적용
        serializedObject.ApplyModifiedProperties();
    }
}