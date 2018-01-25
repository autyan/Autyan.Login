using System;
using System.IO;
using System.Linq;

namespace DataGenerater
{
    internal static class Program
    {
        private static readonly Random Random = new Random();

        static void Main()
        {
            using (var file = new StreamWriter("c:\\generate.txt"))
            {
                //file.WriteLine("INSERT INTO IdentityUsers (LoginName, LoginNameFirstChar, LoginNameHashCode, PasswordHash) VALUES ");
                var count = 1;
                while (count < 10000000)
                {
                    var length = Random.Next(4, 15);
                    var loginName = RandomString(length);
                    file.WriteLine($"{count},{loginName},userlogin,0,,,{DateTime.Now:yyyy-MM-dd HH:mm:ss},,");
                    count++;
                }
            }
        }

        public static string RandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[Random.Next(s.Length)]).ToArray());
        }
    }
}
