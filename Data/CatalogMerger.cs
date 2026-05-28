using System;
using System.Collections.Generic;
using System.Linq;
using Malcha.Model;

namespace Malcha.Data
{
    // 백업(원본) 카탈로그와 정제된 카탈로그를 타임스탬프·이미지 기준으로 병합
    internal static class CatalogMerger
    {
        // 병합 결과 (통계 포함)
        internal sealed class MergeResult
        {
            public List<Frame> Frames { get; init; } = new List<Frame>();
            public int FromBackup { get; init; }
            public int FromRefinedOnly { get; init; }
            public int RefinedOverrides { get; init; }
        }

        // 백업 프레임을 기준으로 정제본을 덮어쓰고, 정제에만 있는 프레임 추가
        internal static MergeResult Merge(IReadOnlyList<Frame> backup, IReadOnlyList<Frame> refined)
        {
            var refinedMap = new Dictionary<FrameKey, Frame>();
            foreach (var f in refined)
                refinedMap[FrameKey.From(f)] = f;

            var merged = new List<Frame>();
            var usedRefined = new HashSet<FrameKey>();
            int overrides = 0;

            foreach (var b in backup.OrderBy(f => f.TimestampMs).ThenBy(f => f.Index))
            {
                var key = FrameKey.From(b);
                if (refinedMap.TryGetValue(key, out var r))
                {
                    merged.Add(r);
                    usedRefined.Add(key);
                    overrides++;
                }
                else
                {
                    merged.Add(b);
                }
            }

            int refinedOnly = 0;
            foreach (var f in refined.OrderBy(x => x.TimestampMs).ThenBy(x => x.Index))
            {
                var key = FrameKey.From(f);
                if (usedRefined.Contains(key))
                    continue;
                merged.Add(f);
                refinedOnly++;
            }

            for (int i = 0; i < merged.Count; i++)
                merged[i].Index = i;

            return new MergeResult
            {
                Frames = merged,
                FromBackup = backup.Count,
                FromRefinedOnly = refinedOnly,
                RefinedOverrides = overrides
            };
        }

        // 프레임 동일성 판별 키 (타임스탬프 + 이미지 경로)
        private readonly struct FrameKey : IEquatable<FrameKey>
        {
            public long TimestampMs { get; }
            public string ImagePath { get; }

            private FrameKey(long timestampMs, string imagePath)
            {
                TimestampMs = timestampMs;
                ImagePath = imagePath;
            }

            // Frame → FrameKey 변환
            public static FrameKey From(Frame f) =>
                new FrameKey(f.TimestampMs, f.ImagePath ?? string.Empty);

            public bool Equals(FrameKey other) =>
                TimestampMs == other.TimestampMs
                && string.Equals(ImagePath, other.ImagePath, StringComparison.OrdinalIgnoreCase);

            public override bool Equals(object obj) => obj is FrameKey other && Equals(other);

            public override int GetHashCode() =>
                HashCode.Combine(TimestampMs, StringComparer.OrdinalIgnoreCase.GetHashCode(ImagePath ?? string.Empty));
        }
    }
}
