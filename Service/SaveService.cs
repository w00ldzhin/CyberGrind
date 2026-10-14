using igraUP.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Documents;

namespace igraUP.Service
{
    static class SaveService
    {
        private const string savepath = "Data/users.json";


        public static List<User> LoadUsers() 
        {
            try
            {
                List<User>? users = new List<User>();

                if (File.Exists(savepath))
                {
                    var x = File.ReadAllText(savepath);
                    users = JsonSerializer.Deserialize<List<User>>(x) ?? new List<User>();
                    
                }
              

                return users;
            }
            catch
            {
                return new List<User>();
            }
        }

        public  static bool SaveUsers(List<User> users) 
        {
            try
            {
                if (!Directory.Exists("Data"))
                {
                    Directory.CreateDirectory("Data");

                }
                var x = JsonSerializer.Serialize(users);
                    File.WriteAllText(savepath, x);
                return true;
            }
            catch
           {
                return false;
                
            }
        }
    }
}
