namespace AI.States
{
    using UnityEngine;

    public class VillagerGatherState : IState
    {
        private  readonly VillagerController _villager;
        private float _gatherTimer;
        private const float GatherInterval = 1.5f;

        public VillagerGatherState(VillagerController villager)
        {
            this._villager = villager;
        }

        public void OnEnter()
        {
            Debug.Log("[AI State] Villager Gather state");
            _gatherTimer = 0f;
        }

        public void Tick()
        {
            if (_villager.CurrentTarget == null)
            {
                _villager.ChangeState<VillagerIdleState>();
                return;
            }
            
            float distance = Vector2.Distance(_villager.transform.position, _villager.CurrentTarget.transform.position);
            
            // Walk to target node
            if (distance > 0.8f)
            {
                _villager.MoveTowards(_villager.CurrentTarget.position);
            }
            else
            {
                //In range: run harvesting loop
                _gatherTimer += Time.deltaTime;
                if (_gatherTimer >= GatherInterval)
                {
                    _gatherTimer = 0f;
                    _villager.InventoryCount++;
                    Debug.Log($"[Villager AI] Harvesting... Carrying {_villager.InventoryCount}/{_villager.MaxInventory}");

                    if (_villager.InventoryCount >= _villager.MaxInventory)
                    {
                        Debug.Log("[Villager AI] Backpack full.");
                    }
                }
            }
        }
        public void OnExit()
        {
            _gatherTimer = 0f;
        }
    }
}