using CryptoQuest.BlackSmith.Evolve.UI;

namespace CryptoQuest.BlackSmith.Evolve.States
{
    public class SelectEvolveMaterial : EvolveStateBase
    {
        public SelectEvolveMaterial(EvolveStateMachine stateMachine) : base(stateMachine) { }

        public override void OnEnter()
        {
            base.OnEnter();
            DialogsPresenter.Dialogue.SetMessage(EvolveSystem.SelectMaterialText).Show();
            StateMachine.MaterialItem = null;
            EvolvableEquipmentList.EquipmentSelected += OnSelectMaterial;
            EvolvableEquipmentList.EquipmentHighlighted += OnHighlightItem;

            EquipmentsPresenter.RenderEquipmentsForMaterialItemSelection(StateMachine.ItemToEvolve);
        }

        public override void OnExit()
        {
            base.OnExit();
            EvolvableEquipmentList.EquipmentSelected -= OnSelectMaterial;
            EvolvableEquipmentList.EquipmentHighlighted -= OnHighlightItem;
        }

        private void OnSelectMaterial(UIEquipmentItem equipment)
        {
            StateMachine.MaterialItem = equipment;
            fsm.RequestStateChange(EStates.ConfirmEvolve);
        }

        public override void OnCancel()
        {
            EquipmentsPresenter.ResetAnchorIfExist(StateMachine.ItemToEvolve);
            StateMachine.ItemToEvolve.ResetItemStates();
            EvolveSystem.EquipmentDetailPresenter.ShowEquipment(StateMachine.ItemToEvolve.Equipment);
            fsm.RequestStateChange(EStates.SelectEquipment);
        }

        private void OnHighlightItem(UIEquipmentItem item)
        {
            if (item == StateMachine.ItemToEvolve) return;
            EvolveSystem.EquipmentDetailPresenter.ShowEquipment(item.Equipment);
            EvolveSystem.EquipmentDetailPresenter.ShowPreview(StateMachine.EvolveEquipmentData);
        }
    }
}
