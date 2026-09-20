namespace Scripts.Services.Interfaces
{
    public interface ICurrencyService {
        int GetBalance();
        void Add(int amount);
        bool Spend(int amount);
    }
}