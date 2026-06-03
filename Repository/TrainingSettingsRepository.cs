using Malcha.Repository;
using System.Text.Json;

namespace Malcha.Repository
{
    // mycar WSL 경로 설정 저장·로드
    internal class TrainingSettingsRepository
    {
        private static readonly TrainingSettingsRepository _instance = new();
        public static TrainingSettingsRepository Instance => _instance;

        private static string SettingsPath =>
            Path.Combine(Environment.CurrentDirectory, "Data", "training_settings.json");

        private TrainingSettingsRepository() { }

        public TrainingSettings? Load()
        {
            try
            {
                if (!File.Exists(SettingsPath)) return null;
                var json = File.ReadAllText(SettingsPath);
                return JsonSerializer.Deserialize<TrainingSettings>(json);
            }
            catch { return null; }
        }

        public void Save(TrainingSettings settings)
        {
            var dir = Path.GetDirectoryName(SettingsPath);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(SettingsPath, JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
        }

        internal sealed class TrainingSettings
        {
            public string Distro { get; set; } = "Ubuntu-22.04";
            public string CarDirectoryLinux { get; set; } = string.Empty;
        }
    }
}
