using Scripts.Services.Interfaces;

namespace Scripts.Components
{
    public struct SaveDataEvent
    {
        public IStorable StorableObject;
    }
}