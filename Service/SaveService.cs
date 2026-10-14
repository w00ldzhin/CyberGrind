using igraUP.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows.Documents;

namespace igraUP.Service
{
    class SaveService
    {
        string savepath = "/Data/Users.cs";

        public void LoadUser() 
        {
            List<string> list = new List<string>();

            if (File.Exists(savepath)) 
            {
            //  var x = File.ReadAllText(savepath);
               User user = JsonSerializer.Deserialize<User>(savepath);

            }
            else
            {
                File.CreateText(savepath);
            }

           
        }

        public void SaveUser() 
        {
            
        }
    }
}
