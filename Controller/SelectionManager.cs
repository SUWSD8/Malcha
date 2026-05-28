using System;

namespace Malcha
{
    // Manages a start/end selection (inclusive) for ranges on the timeline/list.
    internal class SelectionManager
    {
        public int Start { get; private set; } = -1;
        public int End { get; private set; } = -1;

        public bool HasSelection => Start >= 0 || End >= 0;

        public void SetStart(int idx)
        {
            Start = idx;
            if (End < Start) End = Start;
        }

        public void SetEnd(int idx)
        {
            End = idx;
            if (Start < 0) Start = End;
            if (End < Start) End = Start;
        }

        public void Clear()
        {
            Start = -1;
            End = -1;
        }

        public (int s, int e) GetRange()
        {
            if (Start < 0 && End < 0) return (-1, -1);
            int s = Start >= 0 ? Start : End;
            int e = End >= 0 ? End : Start;
            if (s > e) { var t = s; s = e; e = t; }
            return (s, e);
        }

        public int FrameCount
        {
            get
            {
                var (s, e) = GetRange();
                return s < 0 ? 0 : e - s + 1;
            }
        }

        /// <summary>
        /// 프레임 삭제 후 구간 인덱스를 조정합니다. 삭제 구간과 겹치면 구간을 해제합니다.
        /// </summary>
        public void OnFramesRemoved(int removedStart, int removedCount)
        {
            if (Start < 0 && End < 0) return;

            var (s, e) = GetRange();
            int removedEnd = removedStart + removedCount - 1;

            if (s <= removedEnd && e >= removedStart)
            {
                Clear();
                return;
            }

            if (s > removedEnd)
            {
                Start -= removedCount;
                if (End >= 0) End -= removedCount;
            }
        }
    }
}
