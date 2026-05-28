using Malcha.Model;
using System.Text.Json;

namespace Malcha.Repository
{
    // [Repository] database.json 읽기/쓰기 및 학습 결과 메모리 캐시
    internal class ResultRepository
    {
        private static readonly ResultRepository _instance = new();
        public static ResultRepository Instance => _instance;

        private readonly List<TrainingResult> _results = new();
        private ResultRepository() { }

        // database.json에서 모델 학습 기록 로드
        public async Task<TrainingResult?> LoadFromDatabaseAsync(string path, string modelName)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("database.json 파일을 찾을 수 없습니다.", path);

            string json = await File.ReadAllTextAsync(path);
            var entries = JsonSerializer.Deserialize<List<DatabaseEntry>>(json);
            var entry = entries?.FirstOrDefault(e =>
                e.Name.Equals(modelName, StringComparison.OrdinalIgnoreCase));
            return entry == null ? null : MapEntry(entry);
        }

        // 메모리 캐시 전체 목록 반환
        public IReadOnlyList<TrainingResult> GetAll() => _results;

        // 이름으로 모델 검색
        public TrainingResult? FindByName(string name) =>
            _results.Find(r => r.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        // 모델 추가 또는 동명 교체
        public void AddOrReplace(TrainingResult result)
        {
            var existing = FindByName(result.Name);
            if (existing != null) _results.Remove(existing);
            _results.Add(result);
        }

        // 모델 코멘트 수정
        public void UpdateComment(string modelName, string comment)
        {
            var model = FindByName(modelName)
                ?? throw new InvalidOperationException($"모델 '{modelName}' 없음");
            model.Comment = comment;
        }

        // 모델 삭제
        public void Delete(string modelName)
        {
            var model = FindByName(modelName)
                ?? throw new InvalidOperationException($"모델 '{modelName}' 없음");
            _results.Remove(model);
        }

        // JSON 엔트리 → TrainingResult 변환
        private static TrainingResult MapEntry(DatabaseEntry entry)
        {
            var result = new TrainingResult
            {
                Number = entry.Number, Name = entry.Name, Pilot = entry.Pilot,
                Type = entry.Type, Tubs = entry.Tubs, Time = entry.Time,
                Transfer = entry.Transfer, Comment = entry.Comment
            };
            if (entry.History?.Loss == null) return result;
            var losses = entry.History.Loss;
            var valLosses = entry.History.ValLoss;
            for (int i = 0; i < losses.Count; i++)
            {
                result.Epochs.Add(new TrainingEpoch
                {
                    Epoch = i + 1, Loss = losses[i],
                    ValLoss = valLosses != null && i < valLosses.Count ? valLosses[i] : 0
                });
            }
            return result;
        }
    }
}
