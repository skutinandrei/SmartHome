namespace SmartHome
{
    public class UserActionLog
    {
        public int Id { get; set; }
        public long UserId { get; set; }
        public DateTime Timestamp { get; set; }
        public string? DeviceId { get; set; }
        public string? ActionType { get; set; }
        public string? Parameters { get; set; }
    }
}
