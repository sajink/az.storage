namespace Az.Storage
{
    using System;

    public static class Keys
    {
        private static DateTime SINCE = new DateTime(2007, 9, 3, 0, 0, 0, DateTimeKind.Utc);
        private static DateTime TILL = new DateTime(2107, 9, 3, 0, 0, 0, DateTimeKind.Utc);

        public static string GetKey(KeyType type, int offset=0) => type switch {
            KeyType.InstaSeconds => InstaSeconds(offset),
            KeyType.InstaMinutes => InstaMinutes(offset),
            KeyType.InstaHour => InstaHour(offset),
            KeyType.InstaDay => InstaDay(offset),
            KeyType.InstaMonth => InstaMonth(offset),
            KeyType.IncreasingKeyLoRes => IncreasingKeyLoRes(),
            KeyType.IncreasingKeyHiRes => IncreasingKeyHiRes(),
            KeyType.DecreasingKeyLoRes => DecreasingKeyLoRes(),
            KeyType.DecreasingKeyHiRes => DecreasingKeyHiRes(),
            _ => string.Empty
        };
        public static string InstaSeconds(int offset = 0) => Insta(offset, "yyyyMMddHHmmss");
        public static string InstaMinutes(int offset = 0) => Insta(offset, "yyyyMMddHHmm");
        public static string InstaHour(int offset = 0) => Insta(offset, "yyyyMMddHH");
        public static string InstaDay(int offset = 0) => Insta(offset, "yyyyMMdd");
        public static string InstaMonth(int offset = 0) => Insta(offset, "yyyyMM");
        public static string IncreasingKeyLoRes(int offset = 0) => Interval(SINCE, offset).ToString();
        public static string IncreasingKeyHiRes(int offset = 0) => Interval(SINCE, offset, true).ToString();
        public static string DecreasingKeyLoRes(int offset = 0) => Interval(TILL, offset).ToString();
        public static string DecreasingKeyHiRes(int offset = 0) => Interval(TILL, offset, true).ToString();

        #region Internal Helpers
        private static string Insta(int offset, string format) => OffsetTime(offset).ToString(format);
        private static double Interval(DateTime from, int offset, bool hi = false) => Math.Abs(hi ? (OffsetTime(offset) - from).TotalMilliseconds : Math.Floor((OffsetTime(offset) - from).TotalMinutes));
        private static DateTime OffsetTime(int offset) => DateTime.UtcNow.AddMinutes(offset);
        #endregion
    }

    public enum KeyType
    {
        None = 0,
        InstaSeconds,
        InstaMinutes,
        InstaHour,
        InstaDay,
        InstaMonth,
        IncreasingKeyLoRes,
        IncreasingKeyHiRes,
        DecreasingKeyLoRes,
        DecreasingKeyHiRes
    }
}
