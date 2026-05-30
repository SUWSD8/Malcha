using Malcha.Controller;
using Malcha.Data;
using Malcha.Model;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Malcha.Service
{
    // [Service] 카탈로그 로드·저장·정제·병합·경로 해석 (UI 의존성 없음)
    internal class CatalogService
    {
        private static readonly CatalogService _instance = new();
        public static CatalogService Instance => _instance;
        private CatalogService() { }

        // 단일 .catalog 파일에서 프레임 목록 로드
        public Task<List<Frame>> LoadCatalogFileAsync(string path) =>
            DataManager.Instance.LoadCatalogFileAsync(path);

        // 프레임별 이미지 파일 절대 경로 해석
        public List<string> ResolveFrameImagePaths(string catalogPath, List<Frame> frames)
        {
            var result = new List<string>(frames?.Count ?? 0);
            var baseDir = Path.GetDirectoryName(catalogPath) ?? string.Empty;

            for (int i = 0; frames != null && i < frames.Count; i++)
            {
                string img = frames[i].ImagePath ?? string.Empty;
                string? resolved = null;

                if (!string.IsNullOrEmpty(img))
                {
                    if (Path.IsPathRooted(img) && File.Exists(img))
                        resolved = img;
                    else
                    {
                        var candidate = Path.Combine(baseDir, img);
                        if (File.Exists(candidate)) resolved = candidate;
                        else
                        {
                            foreach (var sub in new[] { "images", "cam", "imgs", "image" })
                            {
                                var c2 = Path.Combine(baseDir, sub, img);
                                if (File.Exists(c2)) { resolved = c2; break; }
                            }
                            if (resolved == null)
                            {
                                try
                                {
                                    var nameOnly = Path.GetFileName(img);
                                    resolved = Directory.GetFiles(baseDir)
                                        .FirstOrDefault(p => string.Equals(Path.GetFileName(p), nameOnly, StringComparison.OrdinalIgnoreCase));
                                }
                                catch { }
                            }
                        }
                    }
                }
                result.Add(resolved ?? string.Empty);
            }
            return result;
        }

        // ListBox에 프레임 목록 표시 (이미지 유무 표시)
        public void PopulateListBox(ListBox listBox, List<Frame> frames, List<string> imagePaths)
        {
            listBox.Items.Clear();
            for (int i = 0; i < frames.Count; i++)
            {
                bool hasImg = i < imagePaths.Count && !string.IsNullOrEmpty(imagePaths[i]);
                listBox.Items.Add(hasImg ? $"frame_{i} (img)" : $"frame_{i} (no image)");
            }
        }

        // 프레임 목록을 .catalog 파일로 저장
        public Task<bool> SaveCatalogAsync(string path, List<Frame> frames) =>
            DataManager.Instance.SaveFramesAsync(path, frames);

        // FrameRefinementFilter로 프레임 자동 정제
        public Task<FrameRefinementFilter.Result> RefineAsync(
            IReadOnlyList<Frame> frames,
            IProgress<FrameRefinementFilter.ProgressReport>? progress,
            CancellationToken cancellationToken)
        {
            var copy = frames is List<Frame> list ? list : new List<Frame>(frames);
            return Task.Run(() => FrameRefinementFilter.Refine(copy, null, progress, cancellationToken), cancellationToken);
        }

        // 최신 백업과 정제본을 병합
        public async Task<(List<Frame> BackupFrames, CatalogMerger.MergeResult MergeResult)> MergeWithBackupAsync(
            string selectedBackupPath, IReadOnlyList<Frame> refinedFrames, CancellationToken token = default)
        {
            if (!File.Exists(selectedBackupPath))
                throw new FileNotFoundException("선택된 백업 파일을 찾을 수 없습니다.", selectedBackupPath);

            var backupFrames = await LoadCatalogFileAsync(selectedBackupPath);
            var mergeResult = await Task.Run(() => CatalogMerger.Merge(backupFrames, refinedFrames), token);
            return (backupFrames, mergeResult);
        }

        // 작업용 .catalog 파일에서 정제된 프레임 로드 (없으면 메모리 프레임 반환)
        public async Task<List<Frame>> LoadRefinedFramesAsync(string currentPath, IReadOnlyList<Frame> memoryFrames)
        {
            var workingPath = CatalogPaths.ResolveWorkingCatalogPath(currentPath);
            if (string.Equals(Path.GetFullPath(currentPath), Path.GetFullPath(workingPath), StringComparison.OrdinalIgnoreCase))
                return new List<Frame>(memoryFrames);
            return File.Exists(workingPath)
                ? await LoadCatalogFileAsync(workingPath)
                : new List<Frame>(memoryFrames);
        }
        

        // 여러 카탈로그 파일을 로드하여 시간순으로 하나의 프레임 리스트로 이어붙입니다.
        public async Task<List<Frame>> LoadAndConcatCatalogsAsync(string[] catalogPaths)
        {
            var allFrames = new List<Frame>();

            // 1. 모든 경로의 파일을 순회하며 메모리에 올림
            foreach (var path in catalogPaths)
            {
                // DataManager.Instance.LoadCatalogFileAsync 사용
                var frames = await LoadCatalogFileAsync(path);

                if (frames != null && frames.Count > 0)
                {
                    allFrames.AddRange(frames);
                }
            }

            // 2. 시간순(TimestampMs)으로 전체 데이터 재정렬
            // (예: catalog_0과 catalog_1이 순서가 뒤죽박죽으로 들어왔을 수 있으므로 필수)
            var sortedFrames = allFrames.OrderBy(f => f.TimestampMs).ToList();

            // 3. 인덱스(Index) 재부여 (0번부터 다시 순서대로 덮어씌움)
            for (int i = 0; i < sortedFrames.Count; i++)
            {
                sortedFrames[i].Index = i;
            }

            return sortedFrames;
        }
    }
}
