using System;

namespace BanID.Services
{
    public static class CaptchaService
    {
        private static Random rand = new Random();

        public static string GenerateCaptchaText()
        {
            return rand.Next(1000, 9999).ToString();
        }

        public static string GenerateCaptchaTextOnly()
        {
            return GenerateCaptchaText();
        }

        public static bool Verify(string input, string captcha)
        {
            return input == captcha;
        }
    }
}