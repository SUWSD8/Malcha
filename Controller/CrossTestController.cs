using Malcha.Model;
using Malcha.Repository;
using Malcha.Service;
using Malcha.View;
using static Malcha.UI.TimelineSelectionBinder;

namespace Malcha.Controller
{
    // 교차 테스트 — WSL inference 후 세션·HUD 연동
    internal sealed class CrossTestController
    {
        private readonly CatalogSession _session;
        private readonly FrameRangeSelection _selection;
        private readonly WslCrossTestService _crossTest = WslCrossTestService.Instance;
        private readonly WslTrainingService _wsl = WslTrainingService.Instance;

        public CrossTestController(CatalogSession session, FrameRangeSelection selection)
        {
            _session = session;
            _selection = selection;
        }

        public async Task RunAsync(ITrainingView trainingView, ICatalogView catalogView, Action refreshOverlay)
        {
            if (_session.CurrentFrames.Count == 0)
            {
                trainingView.ShowError("교차 테스트할 카탈로그를 먼저 열어 주세요.");
                return;
            }

            string modelName = trainingView.SelectedModelName;
            if (string.IsNullOrWhiteSpace(modelName))
            {
                trainingView.ShowError("모델 목록에서 테스트할 모델을 선택하세요.");
                return;
            }

            if (!await EnsureMycarPathAsync(trainingView)) return;

            string? modelType = ResolveModelType(modelName);

            var (start, end, count) = ResolveTargetRange();
            var frames = BuildFrameList(start, end);
            if (frames.Count == 0)
            {
                trainingView.ShowError("이미지가 있는 프레임이 없습니다.");
                return;
            }

            string rangeLabel = start == 0 && end == _session.CurrentFrames.Count - 1
                ? $"전체 {_session.CurrentFrames.Count:N0} 프레임"
                : $"구간 {start}~{end} ({count:N0})";

            catalogView.SetCatalogBusy(true);
            ProgressDialog? progress = null;
            try
            {
                progress = catalogView.ShowProgress("교차 테스트");
                progress.Report(0, $"{modelName} · {rangeLabel}");

                var uiProgress = new Progress<(int Percent, string Message)>(u =>
                {
                    if (progress.IsClosed) return;
                    progress.Report(u.Percent, u.Message);
                });

                var result = await _crossTest.RunBatchAsync(
                    modelName,
                    frames,
                    modelType,
                    uiProgress,
                    progress.Token);

                var predictions = result.Predictions.Select(p => new CrossTestFramePrediction
                {
                    Index = p.Index,
                    Angle = p.Angle,
                    Throttle = p.Throttle
                }).ToList();

                _session.SetCrossTestResults(modelName, predictions);
                refreshOverlay();

                double mae = ComputeAngleMae(start, end);
                var predAngles = predictions.Select(p => p.Angle).ToList();
                var predThrottles = predictions.Select(p => p.Throttle).ToList();
                string spread = predAngles.Count > 0
                    ? $"\n예측 조향 범위: {predAngles.Min():F3} ~ {predAngles.Max():F3}"
                    : "";
                double angleSpan = predAngles.Count > 0 ? predAngles.Max() - predAngles.Min() : 0;
                double throttleSpan = predThrottles.Count > 0 ? predThrottles.Max() - predThrottles.Min() : 0;
                string collapseWarn = angleSpan < 0.05 && throttleSpan < 0.05
                    ? "\n\n⚠ 예측값이 거의 변하지 않습니다.\n" +
                      "  · 정제가 주행 프레임까지 지웠거나 학습 epoch가 부족할 수 있습니다.\n" +
                      "  · 「권장」 정제(정지 구간만) 후 재학습·재연동을 권장합니다."
                    : "";
                string errNote = result.Errors.Count > 0
                    ? $"\n\n실패 {result.Errors.Count:N0}건 (이미지 없음 등)"
                    : "";
                catalogView.SetStatusText(
                    $"교차 테스트 — {modelName} · 예측 {predictions.Count:N0} · MAE {mae:F3}{errNote}");

                trainingView.ShowInfo("교차 테스트 완료",
                    $"모델: {modelName}\n{rangeLabel}\n예측: {predictions.Count:N0} 프레임\n조향 MAE: {mae:F3}{spread}\n\n" +
                    "주황 = 기록값 · 노랑 = 모델 예측\n프레임을 이동하며 비교하세요." + collapseWarn + errNote);
            }
            catch (OperationCanceledException)
            {
                catalogView.SetStatusText("교차 테스트 취소됨");
            }
            catch (Exception ex)
            {
                trainingView.ShowError($"교차 테스트 실패:\n{ex.Message}");
                catalogView.SetStatusText("교차 테스트 실패");
            }
            finally
            {
                catalogView.CloseProgress(progress);
                catalogView.SetCatalogBusy(false);
            }
        }

        private (int Start, int End, int Count) ResolveTargetRange()
        {
            var (s, e) = _selection.GetRange();
            if (s >= 0)
            {
                int end = e >= 0 ? e : s;
                if (end < s) (s, end) = (end, s);
                return (s, end, end - s + 1);
            }
            return (0, _session.CurrentFrames.Count - 1, _session.CurrentFrames.Count);
        }

        private List<(int Index, string ImagePath)> BuildFrameList(int start, int end)
        {
            var list = new List<(int, string)>();
            for (int i = start; i <= end; i++)
            {
                string path = i < _session.FrameImagePaths.Count ? _session.FrameImagePaths[i] : string.Empty;
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                    list.Add((i, path));
            }
            return list;
        }

        private double ComputeAngleMae(int start, int end)
        {
            double sum = 0;
            int n = 0;
            for (int i = start; i <= end; i++)
            {
                var pred = _session.GetCrossTest(i);
                if (pred == null || i >= _session.CurrentFrames.Count) continue;
                sum += Math.Abs(pred.Angle - _session.CurrentFrames[i].Angle);
                n++;
            }
            return n == 0 ? 0 : sum / n;
        }

        private static string? ResolveModelType(string modelName)
        {
            var meta = ResultRepository.Instance.FindByName(modelName);
            if (meta == null) return null;
            if (!string.IsNullOrWhiteSpace(meta.Type)) return meta.Type.Trim();
            if (!string.IsNullOrWhiteSpace(meta.Pilot))
            {
                // KerasLinear → linear, KerasCategorical → categorical
                var p = meta.Pilot.Trim();
                if (p.Contains("Linear", StringComparison.OrdinalIgnoreCase)) return "linear";
                if (p.Contains("Categorical", StringComparison.OrdinalIgnoreCase)) return "categorical";
                return p;
            }
            return null;
        }

        private Task<bool> EnsureMycarPathAsync(ITrainingView view)
        {
            try
            {
                string? suggested = null;
                try
                {
                    var probe = WslPathHelper.ToUncPath(WslTrainingService.DefaultDistro, "/home/heejun/mycar");
                    if (Directory.Exists(probe)) suggested = probe;
                }
                catch { }

                return Task.FromResult(_wsl.TryConfigure(() => view.PromptMycarFolder(suggested)));
            }
            catch (Exception ex)
            {
                view.ShowError(ex.Message);
                return Task.FromResult(false);
            }
        }
    }
}
