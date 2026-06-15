using TMPro;
using UnityEngine;

public class PathfindingGridDebugObject : GridDebugObject
{
    [SerializeField] private TextMeshPro gCostText;
    [SerializeField] private TextMeshPro hCostText;
    [SerializeField] private TextMeshPro fCostText;
    [SerializeField] private Transform isWalkableIcon;
    [SerializeField] private Transform isNotWalkableIcon;

    private PathNode pathNode;
    public override void SetGridObject(object gridObject)
    {
        pathNode = gridObject as PathNode;
        
        base.SetGridObject(gridObject);
    }

    protected override void Update()
    {
        base.Update();
        gCostText.text = pathNode.GetGCost().ToString();
        hCostText.text = pathNode.GetHCost().ToString();
        fCostText.text = pathNode.GetFCost().ToString();
        SetWalkableIcon();
    }

    private void SetWalkableIcon()
    {
        if (pathNode.IsWalkable())
        {
            isWalkableIcon.gameObject.SetActive(true);
            isNotWalkableIcon.gameObject.SetActive(false);
        }
        else
        {
            isWalkableIcon.gameObject.SetActive(false);
            isNotWalkableIcon.gameObject.SetActive(true);
        }
    }
}
