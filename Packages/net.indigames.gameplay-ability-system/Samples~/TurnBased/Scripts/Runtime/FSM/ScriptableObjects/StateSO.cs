using UnityEngine;
using System.Collections.Generic;

namespace IndiGames.GameplayAbilitySystem.FSM
{
    [CreateAssetMenu(fileName = "StateSO", menuName = "Indigames Ability System/FSM/States/Normal State")]
    public class StateSO : BaseStateSO
    {
        public List<FSMActionSO> Actions = new();
        public List<TransitionSO> Transitions = new();
        public BaseStateSO NextState;

        public override void Execute(BaseStateMachine stateMachine)
        {
            foreach (var action in Actions)
            {
                action.Execute(stateMachine);
            }

            foreach (var transition in Transitions)
            {
                transition.Execute(stateMachine);
            }
        }
    }
}