using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace BroPilot.ViewModels
{
    public class SessionManager : ISessionManager
    {
        public IEnumerable<Session> LoadSessions()
        {
            string path = GetBasePath();

            Directory.CreateDirectory(path);

            var files = Directory.GetFiles(path, "*.json");
            var sessions = new List<Session>();

            foreach (var file in files.Where(f => !f.EndsWith("models.json")))
            {
                try
                {
                    var content = File.ReadAllText(file);
                    var session = JsonSerializer.Deserialize<Session>(content);
                    if (session != null)
                    {
                        sessions.Add(session);
                    }
                }
                catch
                {
                }
            }

            return sessions;
        }

        private static string GetBasePath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BroPilot");
        }

        public Task UpdateSession(Session session)
        {
            string path = GetBasePath();

            var options = new JsonSerializerOptions { WriteIndented = true };
            var content = JsonSerializer.Serialize(session, options);

            Directory.CreateDirectory(path);

            var fileName = Path.Combine(path, session.Id + ".json");
            File.WriteAllText(fileName, content);

            return Task.CompletedTask;
        }

        public Task DeleteSession(Session session)
        {
            string path = GetBasePath();
            var fileName = Path.Combine(path, session.Id + ".json");

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            return Task.CompletedTask;
        }
    }
}
