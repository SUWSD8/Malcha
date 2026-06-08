using Malcha.Model;
using Malcha.Service;
using System.Text.Json;
namespace Malcha.Repository
{
    // [Repository] database.json 읽기/쓰기 및 학습 결과 메모리 캐시
    internal class ResultRepository
    {
        private static readonly ResultRepository _instance = new();
        public static ResultRepository Instance => _instance;

        private readonly List<TrainingResult> _results = new();

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private ResultRepository() { }

        // database.json 전체 로드 → 메모리 캐시 갱신
        public async Task<IReadOnlyList<TrainingResult>> LoadAllFromDatabaseAsync(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("database.json 파일을 찾을 수 없습니다.", path);

            string json = await File.ReadAllTextAsync(path);
            var entries = JsonSerializer.Deserialize<List<DatabaseEntry>>(json, JsonOptions) ?? new List<DatabaseEntry>();
            var results = entries.Select((e, i) => MapEntry(e, i)).ToList();

            _results.Clear();
            _results.AddRange(results);
            return results;
        }

        // database.json에서 특정 모델 학습 기록 로드 (이름·.h5 무관 매칭)
        public async Task<TrainingResult?> LoadFromDatabaseAsync(string path, string modelName)
        {
            await LoadAllFromDatabaseAsync(path);
            return FindByName(modelName);
        }

        // 메모리 캐시 전체 목록 반환
        public IReadOnlyList<TrainingResult> GetAll() => _results;

        // 프로그램 내 모델 캐시 비우기 (WSL database.json 기준으로 다시 불러올 때)
        public void ClearCache() => _results.Clear();

        // Number(database.json 고유 ID)로 모델 검색
        public TrainingResult? FindByNumber(int number) =>
            _results.Find(r => r.Number == number);

        // 이름으로 모델 검색 — 동명이면 가장 최근(Time·Number 큰 것)
        public TrainingResult? FindByName(string name)
        {
            var key = WslTrainingService.NormalizeBaseName(name);
            return _results
                .Where(r => WslTrainingService.NormalizeBaseName(r.Name)
                    .Equals(key, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(r => r.Time)
                .ThenByDescending(r => r.Number)
                .FirstOrDefault();
        }

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

        // 강제 종료 등 database.json 미반영 세션 — 목록·교차 테스트용 임시 등록
        public TrainingResult UpsertLiveSession(string modelName, IReadOnlyList<TrainingEpoch> epochs, bool weightsOnDisk)
        {
            if (epochs.Count == 0)
                throw new ArgumentException("epoch 기록이 없습니다.", nameof(epochs));

            modelName = WslTrainingService.NormalizeBaseName(modelName);
            var existing = FindByName(modelName);
            int number = existing?.Number
                ?? (_results.Count > 0 ? _results.Max(r => r.Number) + 1 : 1);

            // staging 이름 등 동일 논리 모델 중복 행 제거
            _results.RemoveAll(r =>
                WslTrainingService.NormalizeBaseName(r.Name)
                    .Equals(modelName, StringComparison.OrdinalIgnoreCase));

            var result = new TrainingResult
            {
                Number = number,
                Name = modelName,
                Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Pilot = existing?.Pilot ?? string.Empty,
                Type = existing?.Type ?? string.Empty,
                Tubs = existing?.Tubs ?? string.Empty,
                Transfer = existing?.Transfer ?? string.Empty,
                Comment = string.IsNullOrWhiteSpace(existing?.Comment)
                    ? (weightsOnDisk ? "학습 중단 저장" : "중단(가중치 미확인)")
                    : existing!.Comment,
                Epochs = epochs.Select(e => new TrainingEpoch
                {
                    Epoch = e.Epoch,
                    Loss = e.Loss,
                    ValLoss = e.ValLoss
                }).ToList()
            };

            if (existing != null)
                _results.Remove(existing);
            _results.Add(result);
            return result;
        }

        // 모델 삭제 (메모리 캐시만)
        public void Delete(string modelName)
        {
            var model = FindByName(modelName)
                ?? throw new InvalidOperationException($"모델 '{modelName}' 없음");
            _results.Remove(model);
        }

        // database.json에서 선택 모델 1건 삭제 (Number → 이름+Time 순 매칭)
        public async Task DeleteFromDatabaseAsync(string databasePath, TrainingResult target)
        {
            if (!File.Exists(databasePath))
                throw new FileNotFoundException("database.json 파일을 찾을 수 없습니다.", databasePath);

            string json = await File.ReadAllTextAsync(databasePath);
            var entries = JsonSerializer.Deserialize<List<DatabaseEntry>>(json, JsonOptions) ?? new List<DatabaseEntry>();
            int before = entries.Count;

            if (target.Number > 0)
                entries.RemoveAll(e => e.Number == target.Number);

            if (entries.Count == before)
            {
                var key = WslTrainingService.NormalizeBaseName(target.Name);
                entries.RemoveAll(e =>
                    WslTrainingService.NormalizeBaseName(e.Name ?? string.Empty)
                        .Equals(key, StringComparison.OrdinalIgnoreCase)
                    && Math.Abs(e.Time - target.Time) < 1.0);
            }

            if (entries.Count == before)
                throw new InvalidOperationException(
                    $"database.json에서 '{target.Name}' ({TrainingTimeFormat.FormatFull(target.Time)}) 기록을 찾을 수 없습니다.");

            await File.WriteAllTextAsync(databasePath, JsonSerializer.Serialize(entries, JsonOptions));
            await LoadAllFromDatabaseAsync(databasePath);
        }

        // 동일 이름 .h5 삭제 여부 — json에 같은 이름 기록이 더 없을 때만
        public bool ShouldDeleteModelFile(string modelName)
        {
            var key = WslTrainingService.NormalizeBaseName(modelName);
            return !_results.Any(r =>
                WslTrainingService.NormalizeBaseName(r.Name)
                    .Equals(key, StringComparison.OrdinalIgnoreCase));
        }

        // JSON 엔트리 → TrainingResult 변환 (Number 없으면 배열 순번 사용)
        private static TrainingResult MapEntry(DatabaseEntry entry, int index)
        {
            int number = entry.Number > 0 ? entry.Number : index + 1;
            var result = new TrainingResult
            {
                Number = number, Name = WslTrainingService.NormalizeBaseName(entry.Name ?? string.Empty), Pilot = entry.Pilot ?? string.Empty,
                Type = entry.Type ?? string.Empty, Tubs = entry.Tubs ?? string.Empty, Time = entry.Time,
                Transfer = entry.Transfer ?? string.Empty, Comment = entry.Comment ?? string.Empty
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
