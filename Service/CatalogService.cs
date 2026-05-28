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
            string workingPath, IReadOnlyList<Frame> refinedFrames, CancellationToken token = default)
        {
            var backupPath = CatalogPaths.FindLatestBackupPath(workingPath)
                ?? throw new FileNotFoundException("백업 없음", workingPath);
            var backupFrames = await LoadCatalogFileAsync(backupPath);
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
    }
}
