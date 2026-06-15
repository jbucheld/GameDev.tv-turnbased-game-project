using System;
using System.Collections.Generic;
using UnityEngine;

public class ShootAction : ActionParentClass
{
    private float totalSpinAmount;
    public event EventHandler<OnShootEventArgs> OnShoot;

    public class OnShootEventArgs : EventArgs
    {
        public Unit targetUnit;
        public Unit shootingUnit;
    }

    private enum State
    {
        Aiming,
        Shooting,
        Cooloff,
    }

    private State state;
    private float stateTimer;
    private float stateCooldown = 0.5f;
    [SerializeField] int shootingRange = 6;
    [SerializeField] private int basicDamage = 40;
    [SerializeField] private LayerMask obstaclesLayerMask;
    private float unitShoulderHeightOffset = 1.4f;
    
    private Unit targetUnit;
    private bool canShootBullet;
    private float unitAimRotationSpeed = 12f;

    protected override void Awake()
    {
        parentUnit = GetComponent<Unit>();
        isActionShootingType = true;
    }

    private void Update()
    {
        if (!isActive) return;

        stateTimer -= Time.deltaTime;
        switch (state)
        {
            case State.Aiming:
                RotateWhileAiming();
                break;
            case State.Shooting:
                if (canShootBullet)
                {
                    ProcessShoot();
                    canShootBullet = false;
                }
                break;
            case State.Cooloff:
                break;
        }

        if (stateTimer <= 0)
        {
            NextState();
        }
    }

    private void RotateWhileAiming()
    {
        Vector3 aimDirection = (targetUnit.GetWorldPosition() - parentUnit.GetWorldPosition()).normalized;
        transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * unitAimRotationSpeed);
    }

    private void NextState()
    {
        switch (state)
        {
            case State.Aiming:
                if (stateTimer <= 0) state = State.Shooting;
                stateTimer = stateCooldown;
                break;
            case State.Shooting:
                if (stateTimer <= 0) state = State.Cooloff;
                stateTimer = stateCooldown;
                break;
            case State.Cooloff:
                if (stateTimer <= 0)
                stateTimer = stateCooldown;
                ActionEnd(onActionComplete);
                break;
        } 
    }


    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(gridPosition);
        canShootBullet = true;
        ShootProcedure(onActionComplete);
        ActionStart(onActionComplete);
    }

    public override List<GridPosition> GetValidActionGridPositionList()
    {
        GridPosition currentGridPosition = parentUnit.GetGridPosition();
        return GetValidActionGridPositionList(currentGridPosition);
    }

    public List<GridPosition> GetValidActionGridPositionList(GridPosition unitGridPosition)
    {
        List<GridPosition> validGridPositionList = new List<GridPosition>();

        for (int x = -shootingRange; x <= shootingRange; x++)
        {
            for (int z = -shootingRange; z <= shootingRange; z++)
            {
                GridPosition offsetGridPosition = new GridPosition(x, z);
                GridPosition testGridPosition = unitGridPosition + offsetGridPosition;
                // check if position exists in LevelGrid
                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition)) continue;
                // check if GridPosition isn't empty
                if (!LevelGrid.Instance.HasAnyUnit(testGridPosition)) continue;
                Unit testTargetUnit = LevelGrid.Instance.GetUnitAtGridPosition(testGridPosition);

                // checks if target exceeds maximum range
                int testDistance = Mathf.Abs(x) + Mathf.Abs(z);
                if (testDistance > shootingRange) continue;
                
                // checks if target is an Enemy
                if (testTargetUnit.IsEnemy() == parentUnit.IsEnemy()) continue;
                
                // check if there is a path to shoot
                Vector3 unitShootDirection = (testTargetUnit.GetWorldPosition() - parentUnit.GetWorldPosition()).normalized ;
                
                if (Physics.Raycast(
                        parentUnit.GetWorldPosition() + Vector3.up * unitShoulderHeightOffset,
                        unitShootDirection,
                        Vector3.Distance(parentUnit.GetWorldPosition(), testTargetUnit.GetWorldPosition()),
                        obstaclesLayerMask))
                {
                    // blocked by obstacle
                    continue;
                }
                
                validGridPositionList.Add(testGridPosition);
            }
        }
        
        return validGridPositionList;
    }
    
    private void ShootProcedure(Action onShootComplete)
    {
        state = State.Aiming;
        stateTimer = stateCooldown;
    }

    private void ProcessShoot()
    {
        OnShoot?.Invoke(this, new OnShootEventArgs
        {
            targetUnit = targetUnit,
            shootingUnit = parentUnit
        });
        targetUnit.TakeDamage(basicDamage);
    }

    public override string GetActionName()
    {
        return "Shoot!";
    }
    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPosition)
    {
        Unit targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(gridPosition);
        float targetRemainingHealthPercentage = targetUnit.GetHealthNormalized();
        
        return new EnemyAIAction()
        {
            gridPosition = gridPosition,
            actionValue = 100 + Mathf.RoundToInt((1-targetRemainingHealthPercentage) * 100),
        };
    }

    public int GetTargetCountAtPosition(GridPosition gridPosition)
    {
        return GetValidActionGridPositionList(gridPosition).Count;
    }
    
}
