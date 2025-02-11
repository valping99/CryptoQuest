using UnityEngine;

namespace CryptoQuest.Church.State
{
    public class OverviewStateBehaviour : BaseStateBehaviour
    {
        private ChurchStateController _stateController;
        private static readonly int SelectCharacter = Animator.StringToHash("SelectCharacterState");
        private static readonly int ExitChurch = Animator.StringToHash("ExitState");

        protected override void OnEnter()
        {
            _stateController = StateMachine.GetComponent<ChurchStateController>();
            _stateController.DialogController.YesPressedEvent += ChangeState;
            _stateController.DialogController.NoPressedEvent += ExitState;
            _stateController.Input.CancelEvent += ExitState;
            _stateController.IsExitState = false;
            _stateController.Presenter.UpdateCurrency();
        }

        protected override void OnExit()
        {
            _stateController.DialogController.YesPressedEvent -= ChangeState;
            _stateController.DialogController.NoPressedEvent -= ExitState;
            _stateController.Input.CancelEvent -= ExitState;
        }

        private void ChangeState()
        {
            StateMachine.Play(SelectCharacter);
        }

        private void ExitState()
        {
            StateMachine.Play(ExitChurch);
            _stateController.IsExitState = true;
            _stateController.DialogController.ChoiceDialog.Hide();
        }
    }
}
