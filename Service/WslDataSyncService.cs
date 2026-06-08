using Malcha.Model;
using Malcha.Repository;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Malcha.Service
{
    // [Service] 정제된 카탈로그·이미지를 WSL mycar/data(tub)로 복사
    internal class WslDataSyncService
    {
        public const string CatalogFileName = "merged_final.catalog";
        public const string CatalogManifestFileName = "merged_final.catalog_manifest";
        public const string ManifestFileName = "manifest.json";
        // DonkeyCar tub — cam/image_array 파일은 data/images/ 아래
        public const string ImagesSubDir = "images";

        private static readonly WslDataSyncService _instance = new();
        public static WslDataSyncService Instance => _instance;
        private WslDataSyncService()
        {
            LoadSyncState();
        }

        public string? LastSyncedFingerprint { get; private set; }
        public int LastSyncedFrameCount { get; private set; }

        public bool IsSyncedWith(string fingerprint) =>
            !string.IsNullOrEmpty(LastSyncedFingerprint)
            && !string.IsNullOrEmpty(fingerprint)
            && string.Equals(LastSyncedFingerprint, fingerprint, StringComparison.OrdinalIgnoreCase);

        public void RecordSuccessfulSync(string fingerprint, int frameCount)
        {
            LastSyncedFingerprint = fingerprint;
            LastSyncedFrameCount = frameCount;
            PersistSyncState();
        }

        public void ClearSyncState()
        {
            LastSyncedFingerprint = null;
            LastSyncedFrameCount = 0;
            PersistSyncState();
        }

        private void LoadSyncState()
        {
            var saved = TrainingSettingsRepository.Instance.Load();
            if (saved == null) return;
            LastSyncedFingerprint = saved.LastSyncedFingerprint;
            LastSyncedFrameCount = saved.LastSyncedFrameCount;
        }

        private void PersistSyncState()
        {
            var saved = TrainingSettingsRepository.Instance.Load() ?? new TrainingSettingsRepository.TrainingSettings();
            saved.LastSyncedFingerprint = LastSyncedFingerprint;
            saved.LastSyncedFrameCount = LastSyncedFrameCount;
            TrainingSettingsRepository.Instance.Save(saved);
        }

        // WSL tub 폴더 UNC 경로 (설정된 mycar/data)
        public string TubUncPath => WslTrainingService.Instance.TubUncPath;

        // train.py --tub data 에 사용할 catalog 존재 여부 (프레임·이미지 1:1 일치 필요)
        public bool HasTrainingData()
        {
            if (!WslTrainingService.Instance.IsConfigured) return false;
            return TryGetSyncedDataCounts(out int frames, out int images)
                && frames > 0
                && frames == images;
        }

        // tub 내 catalog 줄 수와 images 폴더 파일 수
        public bool TryGetSyncedDataCounts(out int frameCount, out int imageCount)
        {
            frameCount = 0;
            imageCount = 0;
            if (!WslTrainingService.Instance.IsConfigured) return false;

            var catalogPath = Path.Combine(TubUncPath, CatalogFileName);
            if (!File.Exists(catalogPath) || new FileInfo(catalogPath).Length == 0)
                return false;

            frameCount = File.ReadLines(catalogPath).Count(l => !string.IsNullOrWhiteSpace(l));
            var imagesDir = Path.Combine(TubUncPath, ImagesSubDir);
            imageCount = CountImagesInDir(imagesDir);
            if (imageCount == 0)
                imageCount = CountImagesInDir(TubUncPath);
            return true;
        }

        // 연동된 data 폴더 상태 요약 (로그·확인용)
        public string DescribeSyncedData()
        {
            if (!WslTrainingService.Instance.IsConfigured)
                return "data: mycar 경로 미설정";

            var catalogPath = Path.Combine(TubUncPath, CatalogFileName);
            if (!File.Exists(catalogPath))
                return "data: catalog 없음 → '정제 데이터 연동' 필요";

            int frameCount = File.ReadLines(catalogPath).Count(l => !string.IsNullOrWhiteSpace(l));
            var imagesDir = Path.Combine(TubUncPath, ImagesSubDir);
            int imageCount = CountImagesInDir(imagesDir);
            if (imageCount == 0)
                imageCount = CountImagesInDir(TubUncPath); // 구버전(루트) 호환 표시

            return $"data: {frameCount} 프레임, {imageCount} 이미지 ({TubUncPath})";
        }

        private static int CountImagesInDir(string dir) =>
            Directory.Exists(dir)
                ? Directory.GetFiles(dir, "*.*", SearchOption.TopDirectoryOnly)
                    .Count(f => f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                             || f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
                             || f.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                : 0;

        // 현재 세션 프레임·이미지를 WSL data 폴더로 동기화
        public async Task<SyncResult> SyncAsync(
            IReadOnlyList<Frame> frames,
            IReadOnlyList<string> imagePaths,
            string? sourceCatalogPath,
            IProgress<(int percent, string message)>? progress = null,
            CancellationToken cancellationToken = default)
        {
            if (frames.Count == 0)
                throw new InvalidOperationException("연동할 프레임이 없습니다.");

            return await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                progress?.Report((0, "WSL data 폴더 준비 중…"));

                var targetDir = TubUncPath;
                if (!Directory.Exists(targetDir))
                    Directory.CreateDirectory(targetDir);

                ClearTubContents(targetDir);

                var exportFrames = new List<Frame>(frames.Count);
                var lineLengths = new List<int>(frames.Count);
                var catalogPath = Path.Combine(targetDir, CatalogFileName);

                using (var sw = new StreamWriter(catalogPath, false, new UTF8Encoding(false)))
                {
                    for (int i = 0; i < frames.Count; i++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var src = frames[i];
                        // DonkeyCar tub 규칙: 프레임 인덱스와 이미지 파일명 1:1 (병합·정제 후 basename 충돌 방지)
                        string imageName = $"{i}_cam_image_array_.jpg";
                        var export = new Frame
                        {
                            Index = i,
                            SessionId = src.SessionId,
                            TimestampMs = src.TimestampMs,
                            ImagePath = imageName,
                            Angle = src.Angle,
                            Throttle = src.Throttle,
                            Mode = src.Mode
                        };
                        exportFrames.Add(export);

                        string line = JsonSerializer.Serialize(export);
                        lineLengths.Add(Encoding.UTF8.GetByteCount(line));
                        sw.WriteLine(line);

                        if (i % Math.Max(1, frames.Count / 20) == 0)
                        {
                            int pct = (int)Math.Round(100.0 * i / frames.Count);
                            progress?.Report((pct, $"카탈로그 작성… {i + 1}/{frames.Count}"));
                        }
                    }
                }

                progress?.Report((60, "이미지 복사 중…"));
                var copyResult = CopyImages(frames, imagePaths, targetDir, exportFrames, cancellationToken);

                progress?.Report((85, "manifest 작성 중…"));
                WriteCatalogManifest(targetDir, lineLengths);
                WriteManifestJson(targetDir, sourceCatalogPath, exportFrames);

                progress?.Report((100, "연동 완료"));

                return new SyncResult
                {
                    FrameCount = exportFrames.Count,
                    ImageCount = copyResult.CopiedCount,
                    MissingImageIndices = copyResult.MissingIndices,
                    TargetPath = targetDir
                };
            }, cancellationToken);
        }

        // 연동된 WSL data(tub) 내용 삭제
        public Task ClearSyncedDataAsync(CancellationToken cancellationToken = default)
        {
            return Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!WslTrainingService.Instance.IsConfigured)
                    throw new InvalidOperationException("mycar 경로가 설정되지 않았습니다.");

                if (!Directory.Exists(TubUncPath))
                    return;

                ClearTubContents(TubUncPath);
            }, cancellationToken);
        }

        // tub 폴더의 이전 catalog·manifest·이미지 제거
        private static void ClearTubContents(string targetDir)
        {
            foreach (var pattern in new[] { "*.catalog", "*.catalog_manifest", "manifest.json", "*.jpg", "*.jpeg", "*.png" })
            {
                foreach (var file in Directory.GetFiles(targetDir, pattern, SearchOption.TopDirectoryOnly))
                {
                    try { File.Delete(file); } catch (Exception ex) { Debug.WriteLine($"삭제 실패 {file}: {ex.Message}"); }
                }
            }

            var imagesDir = Path.Combine(targetDir, ImagesSubDir);
            if (Directory.Exists(imagesDir))
            {
                try { Directory.Delete(imagesDir, recursive: true); }
                catch (Exception ex) { Debug.WriteLine($"images 폴더 삭제 실패 {imagesDir}: {ex.Message}"); }
            }
        }

        private static string ImagesDirectory(string tubDir)
        {
            var dir = Path.Combine(tubDir, ImagesSubDir);
            Directory.CreateDirectory(dir);
            return dir;
        }

        // 이미지 파일을 tub/data/images/ 로 복사 — 프레임마다 고유 파일명, 누락 인덱스 수집
        private static CopyImagesResult CopyImages(
            IReadOnlyList<Frame> frames,
            IReadOnlyList<string> imagePaths,
            string targetDir,
            IReadOnlyList<Frame> exportFrames,
            CancellationToken cancellationToken)
        {
            var imagesDir = ImagesDirectory(targetDir);
            int copied = 0;
            var missing = new List<int>();
            for (int i = 0; i < frames.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string? srcPath = i < imagePaths.Count ? imagePaths[i] : null;
                string destName = exportFrames[i].ImagePath;
                string destPath = Path.Combine(imagesDir, destName);

                if (string.IsNullOrEmpty(srcPath) || !File.Exists(srcPath))
                {
                    missing.Add(i);
                    continue;
                }

                try
                {
                    File.Copy(srcPath, destPath, overwrite: true);
                    copied++;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"이미지 복사 실패 {srcPath}: {ex.Message}");
                    missing.Add(i);
                }
            }
            return new CopyImagesResult { CopiedCount = copied, MissingIndices = missing };
        }

        private sealed class CopyImagesResult
        {
            public int CopiedCount { get; init; }
            public IReadOnlyList<int> MissingIndices { get; init; } = Array.Empty<int>();
        }

        // catalog_0.catalog_manifest 생성
        private static void WriteCatalogManifest(string targetDir, IReadOnlyList<int> lineLengths)
        {
            var manifest = new
            {
                created_at = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0,
                line_lengths = lineLengths,
                path = CatalogManifestFileName,
                start_index = 0
            };
            File.WriteAllText(
                Path.Combine(targetDir, CatalogManifestFileName),
                JsonSerializer.Serialize(manifest),
                new UTF8Encoding(false));
        }

        // manifest.json 생성 (원본 tub에 있으면 앞 4줄 유지, 5번째 줄만 갱신)
        private static void WriteManifestJson(string targetDir, string? sourceCatalogPath, IReadOnlyList<Frame> frames)
        {
            var destPath = Path.Combine(targetDir, ManifestFileName);
            var sourceDir = !string.IsNullOrEmpty(sourceCatalogPath)
                ? Path.GetDirectoryName(sourceCatalogPath) : null;
            var sourceManifest = !string.IsNullOrEmpty(sourceDir)
                ? Path.Combine(sourceDir!, ManifestFileName) : null;

            if (!string.IsNullOrEmpty(sourceManifest) && File.Exists(sourceManifest))
            {
                var lines = File.ReadAllLines(sourceManifest);
                if (lines.Length >= 5)
                {
                    lines[4] = JsonSerializer.Serialize(new
                    {
                        paths = new[] { CatalogFileName },
                        current_index = Math.Max(0, frames.Count - 1),
                        max_len = frames.Count,
                        deleted_indexes = Array.Empty<int>()
                    });
                    File.WriteAllLines(destPath, lines, new UTF8Encoding(false));
                    return;
                }
            }

            var sessionId = frames[0].SessionId ?? "malcha_sync";
            var defaultLines = new[]
            {
                "[\"cam/image_array\", \"user/angle\", \"user/throttle\", \"user/mode\"]",
                "[\"image_array\", \"float\", \"float\", \"str\"]",
                "{}",
                JsonSerializer.Serialize(new
                {
                    created_at = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0,
                    sessions = new { all_full_ids = new[] { sessionId }, last_id = 0, last_full_id = sessionId }
                }),
                JsonSerializer.Serialize(new
                {
                    paths = new[] { CatalogFileName },
                    current_index = Math.Max(0, frames.Count - 1),
                    max_len = frames.Count,
                    deleted_indexes = Array.Empty<int>()
                })
            };
            File.WriteAllLines(destPath, defaultLines, new UTF8Encoding(false));
        }

        internal sealed class SyncResult
        {
            public int FrameCount { get; init; }
            public int ImageCount { get; init; }
            public IReadOnlyList<int> MissingImageIndices { get; init; } = Array.Empty<int>();
            public bool IsComplete => FrameCount > 0 && FrameCount == ImageCount && MissingImageIndices.Count == 0;
            public string TargetPath { get; init; } = string.Empty;
        }
    }
}
