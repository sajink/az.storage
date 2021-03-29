namespace Az.Storage
{
    using System;

    public static class Keys
    {
        private static DateTime SINCE = new DateTime(2007, 9, 3, 0, 0, 0, DateTimeKind.Utc);

        public static string InstaSeconds(int offset = 0) => Insta(offset, "yyyyMMddHHmmss");
        public static string InstaMinutes(int offset = 0) => Insta(offset, "yyyyMMddHHmm");
        public static string InstaHour(int offset = 0) => Insta(offset, "yyyyMMddHH");
        public static string InstaDay(int offset = 0) => Insta(offset, "yyyyMMdd");
        public static string InstaMonth(int offset = 0) => Insta(offset, "yyyyMM");
        public static string ForwardKeyLoRes(int offset = 0) => Since(offset);
        public static string ForwardKeyHiRes(int offset = 0) => Since(offset, true);

        #region Internal Helpers
        private static string Insta(int offset, string format) => OffsetTime(offset).ToString(format);
        private static string Since(int offset, bool hi = false) => (hi ? (SINCE - OffsetTime(offset)).Ticks : Math.Floor((SINCE - OffsetTime(offset)).TotalMinutes)).ToString();
        private static DateTime OffsetTime(int offset) => DateTime.UtcNow.AddMinutes(offset);
        #endregion
    }
}
