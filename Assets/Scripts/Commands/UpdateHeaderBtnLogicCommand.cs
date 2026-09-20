using System;
using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Models;

namespace Scripts.Commands
{
    public class UpdateHeaderBtnLogicCommand : ICommand
    {
        private readonly EcsWorld _world;
        private readonly Action _commonBtnCallback;
        private readonly HeaderBtnType _btnType;

        public UpdateHeaderBtnLogicCommand(EcsWorld world, Action commonBtnCallback, HeaderBtnType btnType)
        {
            _world = world;
            _commonBtnCallback = commonBtnCallback;
            _btnType = btnType;
        }

        public void Execute()
        {
            var updateControlPanelEvent = _world.NewEntity();
            updateControlPanelEvent.Replace(new UpdateControlPanelBtnLogicEvent
            {
                CommonBtnCallback = _commonBtnCallback,
                BtnType = _btnType
            });
        }
    }
}