using CryptoQuest.Input;
using UnityEngine;
using UnityEngine.Localization;

namespace CryptoQuest.ChangeClass.State
{
    public class ChangeClassStateBehaviour : StateMachineBehaviour
    {
        [SerializeField] private LocalizedString _currentStateMessage;
        [SerializeField] private LocalizedString _overviewMessage;
        private ChangeClassStateController _stateController;
        private MerchantsInputManager _input;
        private Animator _animator;
        private static readonly int _submit = Animator.StringToHash("isSelectMaterial");
        private static readonly int _exit = Animator.StringToHash("isOverview");

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _animator = animator;
            _stateController = _animator.GetComponent<ChangeClassStateController>();
            _input = _stateController.Input;
            _input.SubmitEvent += ChangeState;
            _input.CancelEvent += ExitState;
            _stateController.DialogController.Dialogue
                .SetMessage(_currentStateMessage).Show();
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _input.SubmitEvent -= ChangeState;
            _input.CancelEvent -= ExitState;
        }

        private void ChangeState()
        {
            if (!_stateController.Presenter.IsValid && !_stateController.ClassBerserkerControllerMaterial.IsValid) return;
            _animator.SetTrigger(_submit);
            _stateController.Presenter.SetSelectedClass(true);
        }

        private void ExitState()
        {
            _stateController.DialogController.Dialogue
                .SetMessage(_overviewMessage).Show();
            _animator.SetTrigger(_exit);
        }
    }
}