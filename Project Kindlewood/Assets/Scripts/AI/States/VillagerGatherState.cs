using UnityEngine;

namespace AI.States
{
    public class VillagerGatherState : IState
    {
        private readonly VillagerController _villager;
        private ResourceNode _targetNode;
        private float _gatherTimer;
        private readonly float _gatherInterval = 1f;

        public VillagerGatherState(VillagerController villager)
        {
            _villager = villager;
        }

        public void OnEnter()
        {
            _gatherTimer = 0f;
            
            if (_villager.CurrentTarget != null)
            {
                _targetNode = _villager.CurrentTarget.GetComponent<ResourceNode>();
            }

            Debug.Log("[Villager AI] Entered Gather state.");
        }

        public void Tick()
        {
            // If inventory is already full, go to Return State
            if (_villager.InventoryCount >= _villager.MaxInventory)
            {
                _villager.StopMoving();
                Debug.Log("[Villager AI] Reppu on täynnä. Siirrytään paluutilaan.");
                _villager.ChangeState<VillagerReturnState>();
                return;
            }

            // If resource disappears, stop moving towards it.
            if (!_villager.CurrentTarget || !_targetNode || !_targetNode.IsAvailable())
            {
                _villager.StopMoving();
                HandleNodeDepletedOrMissing();
                return;
            }

            // If not close enough, keep moving towards resource
            if (!_villager.IsInRange(_villager.CurrentTarget.position))
            {
                _villager.MoveTowards(_villager.CurrentTarget.position);
                return;
            }

            // We reach resource, start collecting
            _villager.StopMoving();

            _gatherTimer += Time.deltaTime;
            if (_gatherTimer >= _gatherInterval)
            {
                _gatherTimer = 0f;

                _targetNode.Gather();
                _villager.CarriedResourceType = _targetNode.GetResourceType();
                _villager.InventoryCount++;
                
                Debug.Log($"Collected 1 [{_villager.CarriedResourceType}]. Inventory: {_villager.InventoryCount}/{_villager.MaxInventory}");
            }
        }

        private void HandleNodeDepletedOrMissing()
        {
            _villager.CurrentTarget = null;

            if (_villager.InventoryCount > 0)
            {
                _villager.ChangeState<VillagerReturnState>();
            }
            else
            {
                _villager.ChangeState<VillagerIdleState>();
            }
        }

        public void OnExit()
        {
            _targetNode = null;
        }
    }
}