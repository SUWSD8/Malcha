using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Malcha.Data;
using Malcha.Model;

namespace Malcha
{
    internal sealed class CatalogEditorService
    {
        private readonly CatalogManager _catalogManager;

        public CatalogEditorService(CatalogManager catalogManager) => _catalogManager = catalogManager;

        public Task<FrameRefinementFilter.Result> RefineAsync(
            IReadOnlyList<Frame> frames,
            IProgress<FrameRefinementFilter.ProgressReport>? progress,
            CancellationToken cancellationToken)
        {
            var copy = frames is List<Frame> list ? list : new List<Frame>(frames);
            return Task.Run(() => FrameRefinementFilter.Refine(copy, null, progress, cancellationToken), cancellationToken);
        }

        public async Task<(List<Frame> BackupFrames, CatalogMerger.MergeResult MergeResult)> MergeWithLatestBackupAsync(
            string workingCatalogPath,
            IReadOnlyList<Frame> refinedFrames,
            CancellationToken cancellationToken = default)
        {
            var backupPath = CatalogPaths.FindLatestBackupPath(workingCatalogPath);
            if (string.IsNullOrEmpty(backupPath) || !File.Exists(backupPath))
                throw new FileNotFoundException("백업 파일을 찾을 수 없습니다.", backupPath);

            var backupFrames = await _catalogManager.LoadCatalogFileAsync(backupPath);
            var mergeResult = await Task.Run(() => CatalogMerger.Merge(backupFrames, refinedFrames), cancellationToken);
            return (backupFrames, mergeResult);
        }

        public async Task<List<Frame>> LoadRefinedFramesAsync(string currentCatalogPath, IReadOnlyList<Frame> memoryFrames)
        {
            var workingPath = CatalogPaths.ResolveWorkingCatalogPath(currentCatalogPath);
            var currentFull = Path.GetFullPath(currentCatalogPath);
            var workingFull = Path.GetFullPath(workingPath);

            if (string.Equals(currentFull, workingFull, System.StringComparison.OrdinalIgnoreCase))
                return new List<Frame>(memoryFrames);

            if (File.Exists(workingPath))
                return await _catalogManager.LoadCatalogFileAsync(workingPath);

            return new List<Frame>(memoryFrames);
        }

        public Task SaveCatalogAsync(string path, List<Frame> frames) =>
            DataManager.Instance.SaveFramesAsync(path, frames);
    }
}
