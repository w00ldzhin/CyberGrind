using igraUP.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

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

        public static bool SaveUsers(List<User> users)
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

        public static List<Score> LoadScores()
        {
            if (!File.Exists("Data/scores.json")) return new List<Score>();
            var json = File.ReadAllText("Data/scores.json");
            return JsonSerializer.Deserialize<List<Score>>(json) ?? new List<Score>();
        }

        public static void SaveScore(Score score)
        {
            var scores = LoadScores();
            scores.Add(score);
            scores = scores.OrderByDescending(s => s.Points).Take(50).ToList();
            var json = JsonSerializer.Serialize(scores, new JsonSerializerOptions { WriteIndented = true });
            Directory.CreateDirectory("Data");
            File.WriteAllText("Data/scores.json", json);
        }
    }
}