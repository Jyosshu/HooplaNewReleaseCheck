using System;

namespace HooplaNewReleaseCheck
{
    public static class AppSettings
    {
        public static string SendGridApiKey = Environment.GetEnvironmentVariable("SendGrid_Api_Key");

        public static string Subject
        {
            get => $"Hoopla Recent Releases Results - { DateTime.Now.ToString("yyyy-MM-dd HH:mm") }";
        }
    }
}
