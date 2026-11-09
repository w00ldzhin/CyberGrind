
using System.Text;
using System.Security.Cryptography;
using System.Runtime.CompilerServices;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Windows.Documents;
using System.Windows;
using System.ComponentModel;
using igraUP.Models;

namespace igraUP.Service
{
    public  static class AuthService
    {



        public static bool Register(string login, string password, out string regerror)
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



                if (login == string.Empty)
                {
                    regerror = "Введите логин";
                    return false;
                }

                else if (password.Length < 3)
                {
                    regerror = "Пароль не должен быть короче 3 символов";
                    return false;
                }
                else if (users.Any(u => u.login == login))
                {
                    regerror = "Пользователь с таким логином уже существует";
                    return false;
                }

               
                else
                {
                    users.Add(user);

                    SaveService.SaveUsers(users);

                    regerror = "Успешная регистрация";
                    return true;
                }

            }
            catch
            {
                regerror = "Ошибка с регистрацией";
                return false;
            } 
        }
        public static bool Login(string login, string password, out string loginerror)
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

                List<User> users = SaveService.LoadUsers();


                if (login == string.Empty)
                {
                    loginerror = "Введите логин";
                    return false;
                }
                else if (password == string.Empty)
                {
                    loginerror = "Введите пароль";
                    return false;
                }

                else if (users.Any(u => u.login == login && u.passwordhash == passwordhash))
                {
                    loginerror = string.Empty;
                    return true;
                   
                }
               
                else
                {
                    loginerror = "Неправильный логин или пароль";
                    return false;   
                }
               
            }
            catch 
            {
                loginerror = "Ошибка при входе в систему";
                return false;
            }
        }

        public static string GetCurrentUser(string login, out string currentuser)
        {


            return currentuser = login;
        }
    }
}
