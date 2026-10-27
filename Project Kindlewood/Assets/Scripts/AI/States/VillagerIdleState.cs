namespace AI.States
{
    using UnityEngine;

    public class VillagerIdleState : IState
    {
        private readonly VillagerController _villager;

        public VillagerIdleState(VillagerController villager)
        {
            this._villager = villager;
        }

        public void OnEnter()
        {
            Debug.Log("[Villager AI] Entered Idle state. Looking for work...");
        }

        public void Tick()
        {
            // If resource target is assigned, switch to gathering
            if (_villager.CurrentTarget != null)
            {
                _villager.ChangeState<VillagerGatherState>();
            }
        }
        
        public void OnExit(){ }
    }
}