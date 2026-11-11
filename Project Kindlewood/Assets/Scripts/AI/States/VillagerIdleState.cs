using UnityEngine;

namespace AI.States
{
    public class VillagerIdleState : IState
    {
        private readonly VillagerController _villager;
        private float _searchTimer;
        private const float SearchInterval = 1f; // Find new work every 1s

        public VillagerIdleState(VillagerController villager)
        {
            _villager = villager;
        }

        public void OnEnter()
        {
            _villager.StopMoving();
            _searchTimer = 0f;
            Debug.Log("[Villager AI] Entered Idle state. Looking for work...");
        }

        public void Tick()
        {
            // If villager has set target already, go gather from it
            if (_villager.CurrentTarget)
            {
                if (_villager.CurrentTarget.TryGetComponent<ResourceNode>(out var node) && node.IsAvailable())
                {
                    _villager.ChangeState<VillagerGatherState>();
                    return;
                }
                
                // If old target resource node was empty, null target
                _villager.CurrentTarget = null;
            }

            // Find nearest free resource node
            _searchTimer += Time.deltaTime;
            if (_searchTimer >= SearchInterval)
            {
                _searchTimer = 0f;
                ResourceNode nearestNode = FindNearestAvailableNode();

                if (nearestNode)
                {
                    _villager.CurrentTarget = nearestNode.transform;
                    _villager.ChangeState<VillagerGatherState>();
                }
            }
        }

        private ResourceNode FindNearestAvailableNode()
        {
            ResourceNode[] allNodes = Object.FindObjectsByType<ResourceNode>(FindObjectsSortMode.None);
            ResourceNode closest = null;
            float minDistance = float.MaxValue;

            foreach (var node in allNodes)
            {
                if (!node.IsAvailable()) continue;

                float dist = Vector2.Distance(_villager.transform.position, node.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closest = node;
                }
            }

            return closest;
        }

        public void OnExit()
        {
        }
    }
}