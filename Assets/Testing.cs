using LNS.AI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Testing : MonoBehaviour
{
    protected DirectionalPathfinder _pathfinder = new DirectionalPathfinder(1.5f, 0.5f, 0.5f);
    private void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _pathfinder.AddConsideration((new Vector3(mousePos.x, mousePos.y, 0f)).normalized, 1f);
        _pathfinder.EvaluateDirectionToTarget(transform.position);
    }
    private void OnDrawGizmos()
    {
        _pathfinder.DebugGizmos(transform);
    }
}
