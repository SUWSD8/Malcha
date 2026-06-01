using System;
using System.Collections.Generic;
using Malcha.Model;

namespace Malcha
{
    // 카탈로그 프레임 정제: 연속 중복·스파이크·범위 초과 제거
    internal static class FrameRefinementFilter
    {
        // 정제 임계값 옵션
        internal sealed class Options
        {
            // 이전 프레임과 angle·throttle 차이가 모두 이 값 이하면 중복으로 간주
            public double ValueEpsilon { get; set; } = 0.015;

            // 한 스텝에서의 급격한 변화(스파이크) 임계값
            public double SpikeThreshold { get; set; } = 0.4;

            // 허용 범위를 벗어난 절대값(센서 오류)
            public double OutOfRangeLimit { get; set; } = 1.25;
        }

        // 정제 진행률 보고
        internal sealed class ProgressReport
        {
            public int Percent { get; init; }
            public string Message { get; init; } = string.Empty;
        }

        // 정제 결과 (제거 통계 포함)
        internal sealed class Result
        {
            public List<Frame> Frames { get; init; } = new List<Frame>();
            public int OriginalCount { get; init; }
            public int RemovedDuplicate { get; init; }
            public int RemovedSpike { get; init; }
            public int RemovedOutOfRange { get; init; }
            public int RemovedTotal => OriginalCount - Frames.Count;
        }

        // 프레임 목록 정제 실행
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

        // 정제 후 프레임 Index 재부여
        private static void ReindexFrames(List<Frame> frames)
        {
            for (int i = 0; i < frames.Count; i++)
                frames[i].Index = i;
        }

        // angle/throttle 절대값·NaN·Infinity 범위 초과 여부
        private static bool IsOutOfRange(Frame f, double limit)
        {
            return Math.Abs(f.Angle) > limit
                || Math.Abs(f.Throttle) > limit
                || double.IsNaN(f.Angle) || double.IsNaN(f.Throttle)
                || double.IsInfinity(f.Angle) || double.IsInfinity(f.Throttle);
        }

        // 두 프레임의 angle·throttle이 epsilon 이내로 유사한지
        private static bool ValuesSimilar(Frame a, Frame b, double epsilon)
        {
            return Math.Abs(a.Angle - b.Angle) <= epsilon
                && Math.Abs(a.Throttle - b.Throttle) <= epsilon;
        }

        // 이전·다음 프레임 대비 급격히 튀었다가 되돌아오는 단일 프레임(노이즈) 감지
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

        // 단일 채널(angle 또는 throttle) 스파이크 판별
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
