
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
            
            if (_targetBox != null)
            {
                _villager.CurrentTarget = _targetBox.transform;
            }
            else
            {
                Debug.LogWarning("[VillagerReturnState] Kentältä ei löydy ResourceBox-laatikkoa!");
            }
        }

        public void Tick()
        {
            if (_targetBox == null) return;

            // Kävellään kohti varastolaatikkoa
            _villager.MoveTowards(_targetBox.transform.position);

            // Tarkistetaan etäisyys laatikkoon
            float distance = Vector2.Distance(_villager.transform.position, _targetBox.transform.position);
            if (distance <= 1.2f)
            {
                // Tyhjennetään reppu laatikkoon
                _targetBox.DepositResources("MapleWood", _villager.InventoryCount);
                _villager.InventoryCount = 0;

                Debug.Log("[Villager AI] Resurssit palautettu. Palataan takaisin Idle-tilaan.");

                // Siirrytään takaisin Idle-tilaan etsimään uutta työtä
                _villager.CurrentTarget = null;
                _villager.ChangeState<VillagerIdleState>();
            }
        }

        public void OnExit()
        {
            _villager.CurrentTarget = null;
        }
    }
}