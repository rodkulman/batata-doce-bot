using System;

namespace Rodkulman.Telegram
{
    public static class DB
    {
        public static DayOfWeek GoodMorningMessageLastSent { get; set; }
        public static bool ZeroTwosday { get; set; } = false;
        public static bool WednesdayMyDudes { get; set; } = false;
        public static bool ThursdayMessageSent { get; set; } = false;
    }
}