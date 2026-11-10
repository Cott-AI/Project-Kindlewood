using UnityEngine;

namespace AI.States
{
    public class VillagerGatherState : IState
    {
        private readonly VillagerController _villager;
        private float _gatherTimer;
        private readonly float _gatherInterval = 1f;

        public VillagerGatherState(VillagerController villager)
        {
            _villager = villager;
        }

        public void OnEnter()
        {
            _gatherTimer = 0f;
            Debug.Log("[Villager AI] Entered Gather state.");
        }

        public void Tick()
        {
            // 1. Jos reppu on täynnä, vaihdetaan suoraan paluutilaan
            if (_villager.InventoryCount >= _villager.MaxInventory)
            {
                Debug.Log("[Villager AI] Reppu on täynnä. Siirrytään paluutilaan.");
                _villager.ChangeState<VillagerReturnState>();
                return;
            }

            // 2. Jos kohde katosi tai puu loppui, takaisin Idle-tilaan
            if (_villager.CurrentTarget == null)
            {
                _villager.ChangeState<VillagerIdleState>();
                return;
            }

            // 3. Siirrytään puun luo
            _villager.MoveTowards(_villager.CurrentTarget.position);

            // 4. Keräysajastin
            _gatherTimer += Time.deltaTime;
            if (_gatherTimer >= _gatherInterval)
            {
                _gatherTimer = 0f;

                ResourceNode node = _villager.CurrentTarget.GetComponent<ResourceNode>();
                if (node != null && node.IsAvailable())
                {
                    node.Gather();
                    _villager.InventoryCount++;
                    Debug.Log($"Kerätty 1. Repussa: {_villager.InventoryCount}/{_villager.MaxInventory}");
                }
                else
                {
                    _villager.CurrentTarget = null;
                    _villager.ChangeState<VillagerIdleState>();
                }
            }
        }

        public void OnExit()
        {
        }
    }
}