namespace AI.States
{
    using UnityEngine;

    public class VillagerGatherState : IState
    {
        private readonly VillagerController _villager;
        private float _gatherTimer;
        private const float GatherInterval = 1.5f;

        public VillagerGatherState(VillagerController villager)
        {
            this._villager = villager;
        }

        public void OnEnter()
        {
            Debug.Log("[AI State] Villager Gather state alkoi.");
            _gatherTimer = 0f;
        }

        public void Tick()
        {
            if (!_villager.CurrentTarget)
            {
                Debug.Log("[AI State] VIRHE: CurrentTarget on null!");
                _villager.ChangeState<VillagerIdleState>();
                return;
            }
            
            ResourceNode targetNode = _villager.CurrentTarget.GetComponent<ResourceNode>();
            
            if (!targetNode || !targetNode.IsAvailable())
            {
                Debug.Log("[AI State] VIRHE: ResourceNode puuttuu tai resurssi on loppu.");
                _villager.CurrentTarget = null;
                _villager.ChangeState<VillagerIdleState>();
                return;
            }
            
            float distance = Vector2.Distance(_villager.transform.position, _villager.CurrentTarget.position);
            
            // Tulostetaan etäisyys konsoliin, jotta näemme mitä tapahtuu
            Debug.Log($"[Villager Gather] Etäisyys kohteeseen: {distance:F2}");

            if (distance > 1f)
            {
                // Liikutaan kohteeseen
                _villager.MoveTowards(_villager.CurrentTarget.position);
            }
            else
            {
                // Ollaan perillä!
                Debug.Log($"[Villager Gather] Oltu perillä! Ajastin: {_gatherTimer:F2} / {GatherInterval}");

                _gatherTimer += Time.deltaTime;
                if (_gatherTimer >= GatherInterval)
                {
                    _gatherTimer = 0f;
                    
                    Debug.Log("[Villager Gather] ISKU PUUHUN! Kutsutaan Gather(1).");
                    targetNode.Gather(1);
                    
                    _villager.InventoryCount++;
                    Debug.Log($"[Villager AI] Tavaraa repussa: {_villager.InventoryCount}/{_villager.MaxInventory}");
                    
                    if (_villager.InventoryCount >= _villager.MaxInventory)
                    {
                        Debug.Log("[Villager AI] Reppu täynnä!");
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