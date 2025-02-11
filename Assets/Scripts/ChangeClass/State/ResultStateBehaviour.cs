using System.Collections.Generic;
using CryptoQuest.ChangeClass.API;
using CryptoQuest.Character.Hero;
using CryptoQuest.Input;
using IndiGames.Core.Events;
using UnityEngine;
using UnityEngine.Localization;

namespace CryptoQuest.ChangeClass.State
{
    public class ResultStateBehaviour : StateMachineBehaviour
    {
        [SerializeField] private LocalizedString _message;
        private ChangeClassStateController _stateController;
        private MerchantsInputManager _input;
        private Animator _animator;
        private static readonly int _submit = Animator.StringToHash("isChangeClass");

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _animator = animator;
            _stateController = _animator.GetComponent<ChangeClassStateController>();
            _input = _stateController.Input;
            _input.SubmitEvent += ChangeState;
            _stateController.DialogController.Dialogue
                .SetMessage(_message).Show();
            _stateController.Character.HeroSpecEvent += InitializeClassMaterial;
            
            if (_stateController.ClassBerserkerControllerMaterial.IsValid)
                ChangeNewClassBerserker();
            else
                ChangeNewClass();
        }

        private void ChangeNewClassBerserker()
        {
            int classBerserkerId = _stateController.ConfirmMaterial.BerserkerMaterial.Class.Id;
            ActionDispatcher.Dispatch(new GetNewNftClassBerserker(classBerserkerId));
        }

        private void ChangeNewClass()
        {
            int baseUnitId1 = _stateController.ConfirmMaterial.BaseUnitId1.Class.Id;
            int baseUnitId2 = _stateController.ConfirmMaterial.BaseUnitId2.Class.Id;
            var occupation = _stateController.Presenter.Occupation;
            ActionDispatcher.Dispatch(new GetNewNftClass(baseUnitId1, baseUnitId2, occupation));
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _input.SubmitEvent -= ChangeState;
            _stateController.Character.HeroSpecEvent -= InitializeClassMaterial;
        }

        private void ChangeState()
        {
            _animator.SetTrigger(_submit);
            _stateController.Presenter.Init();
        }

        private void InitializeClassMaterial(HeroSpec spec)
        {
            var classId = _stateController.ConfirmMaterial._materialsId;
            _stateController.Party.InitializeParty(classId, spec);
        }
    }
}