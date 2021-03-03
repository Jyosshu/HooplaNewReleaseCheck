using System;

namespace HooplaNewReleaseCheck
{
    public class AppSettings
    {
        public static string SendGridApiKey
        {
            get => Environment.GetEnvironmentVariable("SendGrid_Api_Key");
        }

        public static string Subject
        {
            get => $"Hoopla Recent Releases Results - { DateTime.Now.ToString("yyyy-MM-dd HH:mm") }";
        }

        public string DefaultFromEmail { get; set; }
        public string DefaultToEmail { get; set; }
        public string EmailTemplate { get; set; }
        public string HooplaRecentReleasesUrl { get; set; }
        public string HooplaImageBaseUrl { get; set; }
        public string TitleBaseUrl { get; set; }
    }
}
