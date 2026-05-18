namespace GLMS.Services
{
    public interface ICurrencyService
    {
        Task<decimal> GetUSDtoZARRate();
        Task<decimal> ConvertUSDtoZAR(decimal usdAmount);
    }
}