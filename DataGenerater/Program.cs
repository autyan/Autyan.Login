using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DataGenerater
{
    internal static class Program
    {
        private static readonly Random Random = new Random();

        static void Main()
        {
            var loginNames = new Dictionary<string, string>();
            using (var file = new StreamWriter("c:\\generate.txt"))
            {
                var count = 1;
                while (count < 10000000)
                {
                    string loginName;
                    do
                    {
                        var length = Random.Next(8, 25);
                        loginName = RandomString(length);
                    } while (loginNames.ContainsKey(loginName));
                    loginNames.Add(loginName, null);
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
