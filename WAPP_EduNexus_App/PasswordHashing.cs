using System;
using System.Security.Cryptography;
using System.Text;

namespace WAPP_EduNexus_App
{
    public static class PasswordHashing
    {

        public static string Hash(string password)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }
    }
}