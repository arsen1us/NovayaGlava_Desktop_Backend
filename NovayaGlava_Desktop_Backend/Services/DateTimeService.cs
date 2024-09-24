namespace NovayaGlava_Desktop_Backend.Services
{
    public interface IDateTimeService
    {
        public string GetDateTimeNow();
    }
    public class DateTimeService : IDateTimeService
    {
        ILogger<DateTimeService> _logger;
        public DateTimeService(ILogger<DateTimeService> logger)
        {
            _logger = logger;
        }

        public string GetDateTimeNow()
        {
            _logger.LogInformation($"INFO: [{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] Now datetime successfully received");
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}
