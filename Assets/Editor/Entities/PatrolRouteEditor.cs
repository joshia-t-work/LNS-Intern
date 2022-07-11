using LNS.Entities;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Enemy)), CanEditMultipleObjects]
public class PatrolRouteEditor : Editor
{
    protected virtual void OnSceneGUI()
    {
        Enemy enemy = (Enemy)target;

        for (int i = 0; i < enemy.PatrolPoints.Length; i++)
        {
            EditorGUI.BeginChangeCheck();
            Vector3 newTargetPosition = Handles.PositionHandle(enemy.PatrolPoints[i], Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(enemy, "Change Look At Target Position");
                enemy.PatrolPoints[i] = newTargetPosition;
            }
        }
        if (enemy.PatrolPoints.Length > 1)
        {
            for (int i = 0; i < enemy.PatrolPoints.Length - 1; i++)
            {
                Handles.DrawLine(enemy.PatrolPoints[i], enemy.PatrolPoints[i+1]);
            }
            Handles.DrawLine(enemy.PatrolPoints[0], enemy.PatrolPoints[enemy.PatrolPoints.Length - 1]);
        }
    }
}