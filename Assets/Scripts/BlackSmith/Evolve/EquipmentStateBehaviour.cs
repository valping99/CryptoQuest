﻿using UnityEngine;
using UnityEngine.Localization;

namespace CryptoQuest.BlackSmith.Evolve
{
    public class EquipmentStateBehaviour : StateMachineBehaviour
    {
        [SerializeField] private LocalizedString _selectTargetMessage;
        [SerializeField] private LocalizedString _defaultBlackSmithMessage;

        private Animator _animator;
        private EvolveStateController _stateController;
        private static readonly int ConfirmState = Animator.StringToHash("isConfirm");
        private static readonly int ExitState = Animator.StringToHash("isBlackSmithState");

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _animator = animator;

            _stateController = animator.GetComponent<EvolveStateController>();
            _stateController.Input.CancelEvent += BackToBlackSmithState;
            GetDataFromPresenterAndPassToScrollView();
            SetUpDialogs();
        }

        private void GoToConfirmState()
        {
            _animator.SetTrigger(ConfirmState);
        }

        private void SetUpDialogs()
        {
            _stateController.DialogsPresenter.Dialogue
                .SetMessage(_selectTargetMessage)
                .Show();
        }

        private void GetDataFromPresenterAndPassToScrollView()
        {
            _stateController.ExitConfirmPhaseEvent?.Invoke();
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _stateController.Input.CancelEvent -= BackToBlackSmithState;
        }

        private void BackToBlackSmithState()
        {
            _animator.SetTrigger(ExitState);
            _stateController.DialogsPresenter.Dialogue
                .SetMessage(_defaultBlackSmithMessage)
                .Show();
        }
    }
}
