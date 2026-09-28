namespace Fives.Models
{
    /// <summary>Where BoardHudSystem sends the HUD; the gameplay presenter shows it.</summary>
    public interface IBoardHud
    {
        void Show(in BoardHud hud);
    }
}
