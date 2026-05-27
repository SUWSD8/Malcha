using System;
using System.Collections.Generic;
using System.Linq;
using Malcha.Model;

namespace Malcha.Data
{
    /// <summary>
    /// 백업(원본) 카탈로그와 정제된 카탈로그를 타임스탬프·이미지 기준으로 병합합니다.
    /// </summary>
    internal static class CatalogMerger
    {
        internal sealed class MergeResult
        {
            public List<Frame> Frames { get; init; } = new List<Frame>();
            public int FromBackup { get; init; }
            public int FromRefinedOnly { get; init; }
            public int RefinedOverrides { get; init; }
        }

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

        private readonly struct FrameKey : IEquatable<FrameKey>
        {
            public long TimestampMs { get; }
            public string ImagePath { get; }

            private FrameKey(long timestampMs, string imagePath)
            {
                TimestampMs = timestampMs;
                ImagePath = imagePath;
            }

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
