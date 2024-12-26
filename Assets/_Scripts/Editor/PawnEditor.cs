using _Scripts.Player;
using UnityEditor;
using UnityEngine;

namespace _Scripts.Editor
{
    [CustomEditor(typeof(Pawn))]
    public class PawnEditor : UnityEditor.Editor
    {
        // Add a button in the inspector
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            Pawn pawn = (Pawn)target;

            // Create a button that calls the MovePawnToIndex method when pressed
            if (GUILayout.Button("Move Pawn to Index"))
            {
                pawn.MovePawnToIndex();
            }

            if (GUILayout.Button("Move Pawn to Home"))
            {
                pawn.ResetToHomePosition();
            }
            
        }
    }
}