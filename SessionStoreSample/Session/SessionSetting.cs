namespace SessionStoreSample.Session
{
    public class SessionSetting
    {
        private readonly IConfiguration _config;
        private readonly Lazy<TimeSpan> _expireTime;
        public SessionSetting(IConfiguration config)
        {
            _config = config;
            _expireTime = new Lazy<TimeSpan>(() => TimeSpan.FromMinutes(_config.GetValue<int>("SessionExpireTimeSpan")));
        }
        public TimeSpan ExpireTime { get => _expireTime.Value; }
    }
}
