using System;
using System.Collections.Generic;
using Malcha.Model;

namespace Malcha
{
    // 카탈로그 프레임 정제: 화면·라벨 기반 중복, 스파이크, 범위 초과 제거
    internal static class FrameRefinementFilter
    {
        internal enum DuplicateRemovalMode
        {
            None,
            StationaryOnly,
            SmartImage,
            AllSimilarLabels
        }

        internal sealed class Options
        {
            public DuplicateRemovalMode DuplicateRemoval { get; set; } = DuplicateRemovalMode.SmartImage;

            public double StationaryThrottleThreshold { get; set; } = 0.05;
            public double ValueEpsilon { get; set; } = 0.015;
            public double SpikeThreshold { get; set; } = 0.55;
            public double OutOfRangeLimit { get; set; } = 1.25;
            public int MinKeepStride { get; set; } = 5;

            // SmartImage — 저속·정지에서만 화면 유사도 적용 (주행 중 64px 비교는 과도 제거 유발)
            public double LowSpeedVisualThreshold { get; set; } = 0.12;

            // SmartImage — 이 값 이상이면 화면이 거의 같다고 보고 제거 후보 (저속 구간만)
            public double ImageSimilarityThreshold { get; set; } = 0.94;

            public bool UserModeOnly { get; set; }

            public bool RemoveDuplicateFrames
            {
                get => DuplicateRemoval != DuplicateRemovalMode.None;
                set => DuplicateRemoval = value
                    ? DuplicateRemovalMode.SmartImage
                    : DuplicateRemovalMode.None;
            }
        }

        internal sealed class ProgressReport
        {
            public int Percent { get; init; }
            public string Message { get; init; } = string.Empty;
        }

        internal sealed class Result
        {
            public List<Frame> Frames { get; init; } = new List<Frame>();
            public int OriginalCount { get; init; }
            public int RemovedStationaryDuplicate { get; init; }
            public int RemovedVisualDuplicate { get; init; }
            public int RemovedDuplicate => RemovedStationaryDuplicate + RemovedVisualDuplicate;
            public int RemovedSpike { get; init; }
            public int RemovedOutOfRange { get; init; }
            public int RemovedWrongMode { get; init; }
            public int RemovedTotal => OriginalCount - Frames.Count;
        }

        internal static Result Preview(
            IReadOnlyList<Frame> frames,
            Options? options = null,
            IReadOnlyList<string>? imagePaths = null)
        {
            var copy = frames is List<Frame> list ? new List<Frame>(list) : new List<Frame>(frames);
            return Refine(copy, options, imagePaths);
        }

        internal static Result Refine(
            IReadOnlyList<Frame> frames,
            Options? options = null,
            IReadOnlyList<string>? imagePaths = null,
            IProgress<ProgressReport>? progress = null,
            CancellationToken cancellationToken = default)
        {
            options ??= new Options();
            var result = new Result { OriginalCount = frames?.Count ?? 0 };
            if (frames == null || frames.Count == 0)
                return result;

            var imageCache = options.DuplicateRemoval == DuplicateRemovalMode.SmartImage
                ? new Dictionary<string, byte[]?>()
                : null;

            var kept = new List<Frame>(frames.Count);
            int stationaryDup = 0, visualDup = 0, spike = 0, outOfRange = 0, wrongMode = 0;
            int lastKeptSourceIndex = -1;
            int reportEvery = Math.Max(1, frames.Count / 100);

            for (int i = 0; i < frames.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (i % reportEvery == 0 || i == frames.Count - 1)
                {
                    int pct = (int)Math.Round(100.0 * i / Math.Max(1, frames.Count - 1));
                    string phase = options.DuplicateRemoval == DuplicateRemovalMode.SmartImage && i % (reportEvery * 3) == 0
                        ? " (화면 비교 중…)"
                        : "";
                    progress?.Report(new ProgressReport
                    {
                        Percent = pct,
                        Message = $"프레임 분석 중… {i + 1}/{frames.Count}{phase}"
                    });
                }

                var cur = frames[i];

                if (options.UserModeOnly && !IsUserMode(cur))
                {
                    wrongMode++;
                    continue;
                }

                if (IsOutOfRange(cur, options.OutOfRangeLimit))
                {
                    outOfRange++;
                    continue;
                }

                if (kept.Count > 0 && lastKeptSourceIndex >= 0)
                {
                    var dupKind = ClassifyDuplicate(
                        cur, kept[^1], i, lastKeptSourceIndex, frames, imagePaths, imageCache, options);
                    if (dupKind != DuplicateKind.None)
                    {
                        bool keepForStride = options.MinKeepStride > 0
                            && i - lastKeptSourceIndex >= options.MinKeepStride;
                        if (!keepForStride)
                        {
                            if (dupKind == DuplicateKind.Stationary)
                                stationaryDup++;
                            else
                                visualDup++;
                            continue;
                        }
                    }
                }

                if (ShouldRemoveAsSpike(frames, i, options))
                {
                    spike++;
                    continue;
                }

                kept.Add(cur);
                lastKeptSourceIndex = i;
            }

            ReindexFrames(kept);
            progress?.Report(new ProgressReport { Percent = 100, Message = "정제 완료" });

            return new Result
            {
                Frames = kept,
                OriginalCount = frames.Count,
                RemovedStationaryDuplicate = stationaryDup,
                RemovedVisualDuplicate = visualDup,
                RemovedSpike = spike,
                RemovedOutOfRange = outOfRange,
                RemovedWrongMode = wrongMode
            };
        }

        private enum DuplicateKind { None, Stationary, Visual }

        private static DuplicateKind ClassifyDuplicate(
            Frame cur,
            Frame prevKept,
            int curIndex,
            int prevKeptIndex,
            IReadOnlyList<Frame> frames,
            IReadOnlyList<string>? imagePaths,
            Dictionary<string, byte[]?>? imageCache,
            Options options)
        {
            if (options.DuplicateRemoval == DuplicateRemovalMode.None)
                return DuplicateKind.None;

            if (!ValuesSimilar(cur, prevKept, options.ValueEpsilon))
                return DuplicateKind.None;

            switch (options.DuplicateRemoval)
            {
                case DuplicateRemovalMode.AllSimilarLabels:
                    return DuplicateKind.Visual;

                case DuplicateRemovalMode.StationaryOnly:
                    return IsStationary(cur, options) && IsStationary(prevKept, options)
                        ? DuplicateKind.Stationary
                        : DuplicateKind.None;

                case DuplicateRemovalMode.SmartImage:
                    if (IsStationary(cur, options) && IsStationary(prevKept, options))
                        return DuplicateKind.Stationary;

                    // 주행 중(throttle>저속 기준): 64×48 축소 화면은 연속 프레임이 너무 비슷하게 나와
                    // CNN 학습용 프레임을 대량 삭제함 → 교차 테스트 붕괴. 저속·크롤링 구간만 화면 비교.
                    if (!IsLowSpeedForVisualDedup(cur, prevKept, options))
                        return DuplicateKind.None;

                    if (imagePaths == null || imageCache == null)
                        return DuplicateKind.None;

                    string? pathCur = curIndex < imagePaths.Count ? imagePaths[curIndex] : null;
                    string? pathPrev = prevKeptIndex < imagePaths.Count ? imagePaths[prevKeptIndex] : null;
                    double sim = FrameImageSimilarity.Compare(pathCur, pathPrev, imageCache);
                    return sim >= options.ImageSimilarityThreshold
                        ? DuplicateKind.Visual
                        : DuplicateKind.None;

                default:
                    return DuplicateKind.None;
            }
        }

        private static bool ShouldRemoveAsSpike(IReadOnlyList<Frame> frames, int i, Options options)
        {
            if (i <= 0 || i >= frames.Count - 1)
                return false;

            if (!IsStationary(frames[i], options))
                return false;

            return IsIsolatedSpike(frames, i, options.SpikeThreshold);
        }

        private static bool IsStationary(Frame f, Options options) =>
            Math.Abs(f.Throttle) <= options.StationaryThrottleThreshold;

        private static bool IsLowSpeedForVisualDedup(Frame cur, Frame prev, Options options) =>
            Math.Abs(cur.Throttle) <= options.LowSpeedVisualThreshold
            && Math.Abs(prev.Throttle) <= options.LowSpeedVisualThreshold;

        private static bool IsUserMode(Frame f) =>
            string.Equals(f.Mode, "user", StringComparison.OrdinalIgnoreCase);

        private static void ReindexFrames(List<Frame> frames)
        {
            for (int i = 0; i < frames.Count; i++)
                frames[i].Index = i;
        }

        private static bool IsOutOfRange(Frame f, double limit)
        {
            return Math.Abs(f.Angle) > limit
                || Math.Abs(f.Throttle) > limit
                || double.IsNaN(f.Angle) || double.IsNaN(f.Throttle)
                || double.IsInfinity(f.Angle) || double.IsInfinity(f.Throttle);
        }

        private static bool ValuesSimilar(Frame a, Frame b, double epsilon)
        {
            return Math.Abs(a.Angle - b.Angle) <= epsilon
                && Math.Abs(a.Throttle - b.Throttle) <= epsilon;
        }

        private static bool IsIsolatedSpike(IReadOnlyList<Frame> frames, int i, double spikeThreshold)
        {
            var prev = frames[i - 1];
            var cur = frames[i];
            var next = frames[i + 1];

            return IsChannelSpike(prev.Angle, cur.Angle, next.Angle, spikeThreshold)
                || IsChannelSpike(prev.Throttle, cur.Throttle, next.Throttle, spikeThreshold);
        }

        private static bool IsChannelSpike(double prev, double cur, double next, double threshold)
        {
            double stepIn = Math.Abs(cur - prev);
            double stepOut = Math.Abs(next - cur);
            if (stepIn < threshold)
                return false;

            bool revertsTowardPrev = Math.Abs(next - prev) < stepIn * 0.55;
            bool isolated = stepOut >= threshold * 0.45
                && Math.Sign(cur - prev) != Math.Sign(next - cur);

            return revertsTowardPrev || isolated;
        }
    }
}
