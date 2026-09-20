using System;
using Scripts.Models;

namespace Scripts.Components
{
    public struct UpdateControlPanelBtnLogicEvent
    {
        public Action CommonBtnCallback;
        public HeaderBtnType BtnType;
    }
}