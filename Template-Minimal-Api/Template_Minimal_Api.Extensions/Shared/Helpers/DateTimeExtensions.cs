﻿namespace Template.MinimalApi.Extensions.Shared.Helpers
{
    public static class DateTimeExtensions
    {
        public static DateTime GetGmtDateTime(this DateTime data) => data.AddHours(-3);
    }
}
