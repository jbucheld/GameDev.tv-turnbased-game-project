using System;
using System.Collections.Generic;
using UnityEngine;

public class Testing : MonoBehaviour
{
    private Unit unit;
   
    private void Start()
    {
        
    }
    
    private void Update()
    {
        unit = UnitActionSystem.Instance.GetSelectedUnit();

        if (InputManager.Instance.testKey)
        {
            // GridPosition mouseGridPosition = LevelGrid.Instance.GetGridPosition(MouseRaycast.GetPosition());
            // GridPosition start = unit.GetGridPosition();
            //
            // List<GridPosition> pathfindingList = Pathfinding.Instance.FindPath(start, mouseGridPosition);
            //
            // for (int i = 0; i < pathfindingList.Count - 1; i++)
            // {
            //     // Debug.Log($"Pathfinding to: {mouseGridPosition} leads through {pathfindingList[i]} -- X={pathfindingList[i].x}, Y={pathfindingList[i].z}" +
            //     //         $"with List Length={pathfindingList.Count}");
            //     Debug.DrawLine(
            //           LevelGrid.Instance.GetWorldPosition(pathfindingList[i]),
            //           LevelGrid.Instance.GetWorldPosition(pathfindingList[i + 1]),
            //           Color.yellow,
            //           20f
            //       );
            // }
        }
        InputManager.Instance.testKey = false;
    }
}
