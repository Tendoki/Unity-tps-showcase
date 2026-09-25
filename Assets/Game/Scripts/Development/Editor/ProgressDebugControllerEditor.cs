using UnityEditor;
using UnityEngine;

namespace Game.Scripts.Development
{
    [CustomEditor(typeof(ProgressDebugController))]
    public class ProgressDebugControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            ProgressDebugController controller = (ProgressDebugController)target;
            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField("Progress Debug", EditorStyles.boldLabel);
            
            using (new EditorGUI.DisabledScope(Application.isPlaying == false))
            {
                if (GUILayout.Button("Log Current"))
                    controller.LogCurrent();

                if (GUILayout.Button("Add Coins"))
                    controller.AddCoins();

                if (GUILayout.Button("Level Up"))
                    controller.LevelUp();

                if (GUILayout.Button("Apply Nickname"))
                    controller.ApplyNickname();

                if (GUILayout.Button("Save"))
                    controller.Save();

                if (GUILayout.Button("Test Local Notification"))
                    controller.TestLocalNotification();
            }

            if (Application.isPlaying == false)
                EditorGUILayout.HelpBox("Enter Play Mode to use progress debug actions.", MessageType.Info);
        }
    }
}
