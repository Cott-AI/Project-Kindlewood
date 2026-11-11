using UnityEngine;
using Economy;
using Object = UnityEngine.Object;

namespace AI.States
{
    public class VillagerReturnState : IState
    {
        private readonly VillagerController _villager;
        private ResourceBox _targetBox;

        public VillagerReturnState(VillagerController villager)
        {
            _villager = villager;
        }
        
        public void OnEnter()
        {
            Debug.Log("[Villager AI] Entered Return state. Heading to ResourceBox.");
            _targetBox = Object.FindAnyObjectByType<ResourceBox>();
            
            if (!_targetBox)
            {
                Debug.LogWarning("[VillagerReturnState] Cannot find Resource Box!");
            }
        }

        public void Tick()
        {
            if (!_targetBox) return;

            // If not yet at the resource box, keep walking.
            if (!_villager.IsInRange(_targetBox.transform.position))
            {
                _villager.MoveTowards(_targetBox.transform.position);
                return;
            }

            // Reach the resource box and deposit carried resources.
            _villager.StopMoving();
            _targetBox.DepositResources(_villager.CarriedResourceType, _villager.InventoryCount);
            _villager.InventoryCount = 0;

            // If the resource node wasn't depleted, go back to gather from it.
            // Or go to idle-state if it was depleted.
            if (_villager.CurrentTarget != null && 
                _villager.CurrentTarget.TryGetComponent<ResourceNode>(out var node) && 
                node.IsAvailable())
            {
                Debug.Log("[Villager AI] Resources returned, go gather more.");
                _villager.ChangeState<VillagerGatherState>();
            }
            else
            {
                Debug.Log("[Villager AI] Resources returned, go to Idle-state.");
                _villager.CurrentTarget = null;
                _villager.ChangeState<VillagerIdleState>();
            }
        }

        public void OnExit()
        {
            _targetBox = null;
        }
    }
}