// // Assets/Script/Editor/ValueActionGenericEditor.cs (이 코드로 교체하세요)

// using UnityEngine;
// using UnityEditor;

// [CustomEditor(typeof(ValueAction))]
// public class ValueActionGenericEditor : Editor
// {
//     // ValueAction에 ValueType Enum이 있어야 합니다.
//     private SerializedProperty valueTypeProp;
    
//     // 각 타입별 프로퍼티
//     private SerializedProperty intValueProp;
//     private SerializedProperty floatValueProp;
//     private SerializedProperty stringValueProp;
//     private SerializedProperty boolValueProp;
//     private SerializedProperty objectValueProp;

//     private void OnEnable()
//     {
//         // SerializedProperty들을 미리 찾아둡니다.
//         valueTypeProp = serializedObject.FindProperty("valueType");
//         intValueProp = serializedObject.FindProperty("intValue");
//         floatValueProp = serializedObject.FindProperty("floatValue");
//         stringValueProp = serializedObject.FindProperty("stringValue");
//         boolValueProp = serializedObject.FindProperty("boolValue");
//         objectValueProp = serializedObject.FindProperty("objectValue");
//     }

//     public override void OnInspectorGUI()
//     {
//         var action = (ValueAction)target;
//         serializedObject.Update();

//         // 공통 필드들을 그립니다.
//         EditorGUILayout.PropertyField(serializedObject.FindProperty("calcType"));
//         EditorGUILayout.PropertyField(serializedObject.FindProperty("op"));
//         EditorGUILayout.PropertyField(serializedObject.FindProperty("priority"));
        
//         EditorGUILayout.Space();
//         EditorGUILayout.LabelField("값 설정", EditorStyles.boldLabel);

//         // 값 제공자(Provider) 스크립트가 아닌, 기본 ValueAction일 때만 값 타입 선택 UI를 보여줍니다.
//         if (action.GetType() == typeof(ValueAction))
//         {
//             EditorGUILayout.PropertyField(valueTypeProp);

//             // 선택된 valueType에 따라 해당하는 값 필드만 보여줍니다.
//             switch ((ValueAction.ValueType)valueTypeProp.enumValueIndex)
//             {
//                 case ValueAction.ValueType.Int:
//                     EditorGUILayout.PropertyField(intValueProp, new GUIContent("Value"));
//                     break;
//                 case ValueAction.ValueType.Float:
//                     EditorGUILayout.PropertyField(floatValueProp, new GUIContent("Value"));
//                     break;
//                 case ValueAction.ValueType.String:
//                     EditorGUILayout.PropertyField(stringValueProp, new GUIContent("Value"));
//                     break;
//                 case ValueAction.ValueType.Bool:
//                     EditorGUILayout.PropertyField(boolValueProp, new GUIContent("Value"));
//                     break;
//             }
//         }
//         else
//         {
//             // CountFromTargetSelectorValueProvider 같은 자식 클래스일 경우
//             // 이 커스텀 에디터 대신 기본 인스펙터를 그리도록 할 수 있습니다.
//             // 또는 자식 클래스에 맞는 필드를 여기서 직접 그려줄 수도 있습니다.
//             EditorGUILayout.HelpBox($"값 제공자: {action.GetType().Name}", MessageType.Info);
//             base.OnInspectorGUI(); // 자식 클래스의 나머지 필드를 기본으로 그립니다.
//         }

//         serializedObject.ApplyModifiedProperties();
//     }
// }