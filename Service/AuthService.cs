
using System.Text;
using System.Security.Cryptography;
using igraUP.Data;
using System.Runtime.CompilerServices;
using System.Diagnostics.Eventing.Reader;
using System.IO;

namespace igraUP.Service
{
    public  static class AuthService
    {
       public static bool Register(string login, string password, out string error)
        {
            try
            {
                var x = Encoding.UTF8;
                byte[] hash = x.GetBytes(password);
                string passwordhash;
                using (SHA256 sHA256 = SHA256.Create())
                {
                   passwordhash = Convert.ToHexString(sHA256.ComputeHash(hash));

                }

                var user = new User { login = login, passwordhash = passwordhash };

                List<User> users = SaveService.LoadUsers();

           
                if (users.Any(u => u.login == login))
                {
                    error = "Пользователь с таким логином уже существует";
                    return false;
                }

                if (login == null)
                {
                    error = "Введите логин";
                    return false;
                }

                if (password.Length < 3)
                {
                    error = "Пароль не должен быть короче 3 символов";
                    return false;
                }
                else
                {
                    users.Add(user);

                    SaveService.SaveUsers(users);

                    error = "";
                    return true;
                }
            }
            catch
            {
                 error = "Ошибка с регистрацией";
                return false;
            } 
        }

        public static bool Login(string login, string password)
        {
            try
            {
                return true;
            }
            catch 
            {
                return false;
            }
        }
    }
}
