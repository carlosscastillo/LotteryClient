using Lottery.LotteryServiceReference;
using System;

namespace Lottery
{
    public static class SessionManager
    {
        public static UserDto CurrentUser { get; private set; }

        public static bool IsLoggedIn => CurrentUser != null;

        public static void Login(UserDto user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            CurrentUser = user;
        }

        public static void Logout()
        {
            CurrentUser = null;
        }
    }
}