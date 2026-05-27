using System;
using System.Collections.Generic;
using Malcha.Model;

namespace Malcha
{
    /// <summary>
    /// 카탈로그 프레임 정제: 연속 중복 조향/쓰로틀 제거, 급격한 스파이크(오류·사고) 제거.
    /// </summary>
    internal static class FrameRefinementFilter
    {
        internal sealed class Options
        {
            /// <summary>이전 프레임과 angle·throttle 차이가 모두 이 값 이하면 중복으로 간주.</summary>
            public double ValueEpsilon { get; set; } = 0.015;

            /// <summary>한 스텝에서의 급격한 변화(스파이크) 임계값.</summary>
            public double SpikeThreshold { get; set; } = 0.4;

            /// <summary>허용 범위를 벗어난 절대값(센서 오류).</summary>
            public double OutOfRangeLimit { get; set; } = 1.25;
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
            public int RemovedDuplicate { get; init; }
            public int RemovedSpike { get; init; }
            public int RemovedOutOfRange { get; init; }
            public int RemovedTotal => OriginalCount - Frames.Count;
        }

        internal static Result Refine(
            IReadOnlyList<Frame> frames,
            Options? options = null,
            IProgress<ProgressReport>? progress = null,
            CancellationToken cancellationToken = default)
        {
            options ??= new Options();
            var result = new Result { OriginalCount = frames?.Count ?? 0 };
            if (frames == null || frames.Count == 0)
                return result;

            var kept = new List<Frame>(frames.Count);
            int dup = 0, spike = 0, outOfRange = 0;
            int reportEvery = Math.Max(1, frames.Count / 100);

            for (int i = 0; i < frames.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (i % reportEvery == 0 || i == frames.Count - 1)
                {
                    int pct = (int)Math.Round(100.0 * i / Math.Max(1, frames.Count - 1));
                    progress?.Report(new ProgressReport
                    {
                        Percent = pct,
                        Message = $"프레임 분석 중… {i + 1}/{frames.Count}"
                    });
                }

                var cur = frames[i];

                if (IsOutOfRange(cur, options.OutOfRangeLimit))
                {
                    outOfRange++;
                    continue;
                }

                if (kept.Count > 0 && ValuesSimilar(cur, kept[kept.Count - 1], options.ValueEpsilon))
                {
                    dup++;
                    continue;
                }

                if (IsIsolatedSpike(frames, i, options.SpikeThreshold))
                {
                    spike++;
                    continue;
                }

                kept.Add(cur);
            }

            ReindexFrames(kept);

            progress?.Report(new ProgressReport { Percent = 100, Message = "정제 완료" });

            return new Result
            {
                Frames = kept,
                OriginalCount = frames.Count,
                RemovedDuplicate = dup,
                RemovedSpike = spike,
                RemovedOutOfRange = outOfRange
            };
        }

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

        /// <summary>
        /// 이전·다음 프레임 대비 급격히 튀었다가 되돌아오는 단일 프레임(노이즈/사고 글리치)을 감지합니다.
        /// </summary>
        private static bool IsIsolatedSpike(IReadOnlyList<Frame> frames, int i, double spikeThreshold)
        {
            if (i <= 0 || i >= frames.Count - 1)
                return false;

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

            // 다음 샘플이 이전 쪽으로 되돌아감 (V자 글리치)
            bool revertsTowardPrev = Math.Abs(next - prev) < stepIn * 0.55;
            // 또는 진입·이탈 모두 큼 (고립된 스파이크)
            bool isolated = stepOut >= threshold * 0.45
                && Math.Sign(cur - prev) != Math.Sign(next - cur);

            return revertsTowardPrev || isolated;
        }
    }
}
