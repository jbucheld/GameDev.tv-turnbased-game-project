using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class MoveAction : ActionParentClass
{
    private List<Vector3> targetPositionPath;
    private int currentPathPositionIndex;
    private GridPosition currentGridPosition;
    public event EventHandler OnUnitStartMoving;
    public event EventHandler OnUnitStopMoving;
    
    [Header("Movement")]
    [SerializeField] private float stoppingDistance = 0.05f;
    [SerializeField] private float unitMoveSpeed = 4f;
    [SerializeField] private float unitRotationSpeed = 12f;
    [SerializeField] private int maxMoveDistance = 4;
    
   

    private void Start()
    {
        currentGridPosition = LevelGrid.Instance.GetGridPosition(transform.position);
        LevelGrid.Instance.AddUnitAtGridPosition(currentGridPosition, parentUnit);
    }
    
    private void Update()
    {
        if(isActive) Move();
    }

    public void OrderMove(GridPosition givenOrderPosition)
    {
        currentPathPositionIndex = 0;
        List<GridPosition> gridPositionMovementPath = Pathfinding.Instance.FindPath(parentUnit.GetGridPosition(), givenOrderPosition, out int pathLength);
        
        targetPositionPath = new List<Vector3>();
        foreach (GridPosition gridPosition in gridPositionMovementPath)
        {
            targetPositionPath.Add(LevelGrid.Instance.GetWorldPosition(gridPosition));
        }
        
        OnUnitStartMoving?.Invoke(this, EventArgs.Empty);
    }
    
    private void Move()
    {
        // targetPosition = LevelGrid.Instance.GetWorldPosition(givenOrderPosition);
        Vector3 targetPosition = targetPositionPath[currentPathPositionIndex]; 
        Vector3 moveDirection = (targetPosition - transform.position).normalized;
        transform.forward = Vector3.Lerp(transform.forward, moveDirection, Time.deltaTime * unitRotationSpeed);

        // apply simple move mechanism
        if (Vector3.Distance(transform.position, targetPosition) > stoppingDistance)
        {
            transform.position +=  moveDirection * (Time.deltaTime * unitMoveSpeed);
            
        }
        else
        {
            CheckGridPosition();
            currentPathPositionIndex++;
            if (currentPathPositionIndex >= targetPositionPath.Count)
            {
                ActionEnd(onActionComplete);    
                OnUnitStopMoving?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private void CheckGridPosition()
    {
        GridPosition newGridPosition = LevelGrid.Instance.GetGridPosition(transform.position);
        if (newGridPosition != currentGridPosition)
        {
            GridPosition oldGridPosition = currentGridPosition;
            currentGridPosition = newGridPosition;
            
            LevelGrid.Instance.UnitMovedGridPosition(oldGridPosition, currentGridPosition, parentUnit);
        }
    }

    public override List<GridPosition> GetValidActionGridPositionList()
    {
        List<GridPosition> validGridPositionList = new List<GridPosition>();
        GridPosition currentGridPosition = parentUnit.GetGridPosition();

        for (int x = -maxMoveDistance; x <= maxMoveDistance; x++)
        {
            for (int z = -maxMoveDistance; z <= maxMoveDistance; z++)
            {
                GridPosition offsetGridPosition = new GridPosition(x, z);
                GridPosition testGridPosition = currentGridPosition + offsetGridPosition;

                // check if position exists in LevelGrid
                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition)) continue;
                // check if unit isn't standing at target position;
                if (parentUnit.GetGridPosition() == testGridPosition) continue;
                // check for other units standing at target position
                if (LevelGrid.Instance.HasAnyUnit(testGridPosition)) continue;      
                // check if position isWalkable
                if (!Pathfinding.Instance.IsWalkableGridPosition(testGridPosition)) continue;
                // check if there is a way to reach a target
                if (!Pathfinding.Instance.hasPathAvailable(parentUnit.GetGridPosition(), testGridPosition)) continue;

                int pathfindingDistanceMultiplier = 10;
                if ((Pathfinding.Instance.GetPathLength(parentUnit.GetGridPosition(), testGridPosition) 
                     > maxMoveDistance * pathfindingDistanceMultiplier)) continue;
                
                
                validGridPositionList.Add(testGridPosition);
            }
        }
        
        return validGridPositionList;
    }
    
    public override string GetActionName()
    {
        return "Move";
    }
    
    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        OrderMove(gridPosition);
        ActionStart(onActionComplete);
    }
    
    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPosition)
    {
        int targetCountAtGridPosition = parentUnit.GetAction<ShootAction>().GetTargetCountAtPosition(gridPosition);
        
        return new EnemyAIAction()
        {
            gridPosition = gridPosition,
            actionValue = 10 + (targetCountAtGridPosition * 10),
        };
    }
}
