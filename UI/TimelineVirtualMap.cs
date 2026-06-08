using Malcha.Model;

namespace Malcha.UI
{
    // 활성 프레임 + 삭제 목록을 하나의 가상 타임라인 좌표로 매핑
    internal static class TimelineVirtualMap
    {
        internal readonly record struct DeletedRange(int OriginalStart, int OriginalEnd, int VirtualStart, int Count);

        public static IReadOnlyList<DeletedRange> BuildRanges(IReadOnlyList<DeletedFrameEntry> deleted)
        {
            if (deleted.Count == 0) return Array.Empty<DeletedRange>();

            var sorted = deleted.Select(d => d.OriginalIndex).OrderBy(i => i).ToList();
            var groups = new List<(int Start, int End)>();
            int gs = sorted[0], gp = sorted[0];
            foreach (int idx in sorted.Skip(1))
            {
                if (idx == gp + 1) { gp = idx; continue; }
                groups.Add((gs, gp));
                gs = gp = idx;
            }
            groups.Add((gs, gp));

            var ranges = new List<DeletedRange>(groups.Count);
            foreach (var (start, end) in groups)
            {
                int count = end - start + 1;
                int vStart = start + ranges.Sum(r => r.Count);
                ranges.Add(new DeletedRange(start, end, vStart, count));
            }
            return ranges;
        }

        public static int VirtualMax(int activeCount, IReadOnlyList<DeletedFrameEntry> deleted)
            => Math.Max(0, activeCount + deleted.Count - 1);

        public static int ActiveToVirtual(int activeIndex, IReadOnlyList<DeletedRange> ranges)
            => activeIndex + ranges.Where(r => r.OriginalStart <= activeIndex).Sum(r => r.Count);

        public static int VirtualToActive(int virtualIndex, int activeCount, IReadOnlyList<DeletedRange> ranges)
        {
            if (activeCount <= 0) return 0;

            foreach (var r in ranges)
            {
                int vEnd = r.VirtualStart + r.Count - 1;
                if (virtualIndex >= r.VirtualStart && virtualIndex <= vEnd)
                {
                    int activeBefore = r.OriginalStart
                        - ranges.Where(x => x.OriginalEnd < r.OriginalStart).Sum(x => x.Count);
                    return Math.Clamp(activeBefore - 1, 0, activeCount - 1);
                }
            }

            int a = virtualIndex;
            foreach (var r in ranges.OrderBy(r => r.VirtualStart))
            {
                int vEnd = r.VirtualStart + r.Count - 1;
                if (a > vEnd) a -= r.Count;
            }
            return Math.Clamp(a, 0, activeCount - 1);
        }

        public static DeletedRange? GetDeletedRangeAt(int virtualIndex, IReadOnlyList<DeletedRange> ranges)
        {
            foreach (var r in ranges)
            {
                int vEnd = r.VirtualStart + r.Count - 1;
                if (virtualIndex >= r.VirtualStart && virtualIndex <= vEnd)
                    return r;
            }
            return null;
        }

        public static string FormatDeletedHover(DeletedRange range)
        {
            if (range.Count <= 1)
                return $"잘린 구간 · #{range.OriginalStart} ({range.Count:N0}프레임) · 삭제 목록";
            return $"잘린 구간 · #{range.OriginalStart}~#{range.OriginalEnd} ({range.Count:N0}프레임) · 삭제 목록";
        }
    }
}
