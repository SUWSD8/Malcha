using System;

namespace Malcha
{
    // Manages a start/end selection (inclusive) for ranges on the timeline/list.
    internal class SelectionManager
    {
        public int Start { get; private set; } = -1;
        public int End { get; private set; } = -1;

        public void SetStart(int idx)
        {
            Start = idx;
            if (End < Start) End = Start;
        }

        public void SetEnd(int idx)
        {
            End = idx;
            if (Start < 0) Start = End;
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
    }
}
