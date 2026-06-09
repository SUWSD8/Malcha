namespace Malcha.UI
{
    // 타임라인·리스트 구간 선택 UI + 구간 상태 관리
    internal class TimelineSelectionBinder
    {
        // 타임라인 구간 선택 상태 (start/end 인덱스)
        internal sealed class FrameRangeSelection
        {
            public int Start { get; private set; } = -1;
            public int End { get; private set; } = -1;
            public bool HasSelection => Start >= 0 || End >= 0;

            // 구간 시작점 설정
            public void SetStart(int idx) { Start = idx; if (End < 0) End = idx; else if (End < Start) End = Start; }

            // 구간 끝점 설정 (위·아래 어느 방향이든 허용, GetRange에서 정규화)
            public void SetEnd(int idx) { End = idx; if (Start < 0) Start = idx; }

            // 드래그 앵커 ↔ 현재 위치로 구간 설정 (양방향)
            public void SetRange(int anchor, int current)
            {
                Start = Math.Min(anchor, current);
                End = Math.Max(anchor, current);
            }

            // 선택 해제
            public void Clear() { Start = End = -1; }

            // 정규화된 (시작, 끝) 인덱스 반환
            public (int s, int e) GetRange()
            {
                if (Start < 0 && End < 0) return (-1, -1);
                int s = Start >= 0 ? Start : End, e = End >= 0 ? End : Start;
                if (s > e) (s, e) = (e, s);
                return (s, e);
            }

            public int InPoint => Start;
            public int OutPoint => End;

            // 인덱스가 현재 구간 안에 있는지
            public bool Contains(int idx)
            {
                if (idx < 0) return false;
                var (s, e) = GetRange();
                return s >= 0 && idx >= s && idx <= e;
            }

            // 선택된 프레임 수
            public int FrameCount { get { var (s, e) = GetRange(); return s < 0 ? 0 : e - s + 1; } }

            // 구간에 포함된 인덱스 목록
            public List<int> ToIndexList()
            {
                var (s, e) = GetRange();
                if (s < 0) return new List<int>();
                var list = new List<int>(e - s + 1);
                for (int i = s; i <= e; i++) list.Add(i);
                return list;
            }

            // 프레임 삭제 후 선택 인덱스 보정
            public void OnFramesRemoved(int removedStart, int removedCount)
            {
                if (Start < 0 && End < 0) return;
                var (s, e) = GetRange();
                int removedEnd = removedStart + removedCount - 1;
                if (s <= removedEnd && e >= removedStart) { Clear(); return; }
                if (s > removedEnd) { Start -= removedCount; if (End >= 0) End -= removedCount; }
            }
        }

        private readonly TimelineTrackBar _timeline;
        private readonly ListBox _list;
        private readonly FrameRangeSelection _selection;
        private readonly Action _refreshUi;
        private bool _rangeDrag;
        private bool _listRangeDrag;
        private bool _listPendingClick;
        private int _listAnchor = -1;
        private Point _listDragStart;

        public TimelineSelectionBinder(TimelineTrackBar timeline, ListBox list,
            FrameRangeSelection selection, Action refreshUi)
        {
            _timeline = timeline;
            _list = list;
            _selection = selection;
            _refreshUi = refreshUi;
        }

        // DrawItem·Mouse·Paint 이벤트 연결
        public void Attach()
        {
            _list.SelectionMode = SelectionMode.MultiExtended;
            _list.DrawMode = DrawMode.OwnerDrawFixed;
            _list.DrawItem += OnListDrawItem;
            _list.MouseDown += OnListMouseDown;
            _list.MouseMove += OnListMouseMove;
            _list.MouseUp += OnListMouseUp;
            _timeline.MouseDown += OnTimelineMouseDown;
            _timeline.MouseMove += OnTimelineMouseMove;
            _timeline.MouseUp += (_, _) => _rangeDrag = false;
            _timeline.PostPaint += OnTimelinePaint;
        }

        public void InvalidateTimeline() => _timeline.Invalidate();

        // 타임라인 마우스 X → 활성 프레임 인덱스
        private int IndexFromMouse(int mouseX)
        {
            int max = Math.Max(0, _timeline.Maximum);
            return Math.Clamp(_timeline.XToValue(mouseX), 0, max);
        }

        // 타임라인 — 주황=선택 구간 (썸=재생 위치)
        private void OnTimelinePaint(object? sender, PaintEventArgs e)
        {
            var tb = _timeline;
            var channel = tb.ChannelRect;
            if (channel.Width <= 1 || channel.Height <= 1) return;

            var (s, end) = _selection.GetRange();
            if (s < 0) return;

            int endIdx = end >= 0 ? end : s;
            int vx1 = tb.ValueToX(s);
            int vx2 = tb.ValueToX(endIdx);
            int selLeft = Math.Min(vx1, vx2);
            int selWidth = Math.Max(2, Math.Abs(vx2 - vx1));

            using var band = new SolidBrush(Color.FromArgb(100, Color.Orange));
            e.Graphics.FillRectangle(band, selLeft, channel.Top + 1, selWidth, Math.Max(4, channel.Height - 2));

            if (s != endIdx)
            {
                DrawEdgeLabel(e.Graphics, vx1, channel, "I", Color.LimeGreen);
                DrawEdgeLabel(e.Graphics, vx2, channel, "O", Color.Gold);
            }
        }
        private void OnListDrawItem(object? sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            var lb = (ListBox)sender!;
            bool sel = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            using (var bg = new SolidBrush(sel ? SystemColors.Highlight : lb.BackColor))
                e.Graphics.FillRectangle(bg, e.Bounds);
            var (s, end) = _selection.GetRange();
            if (s >= 0 && end >= 0 && e.Index >= s && e.Index <= end)
            {
                using var h = new SolidBrush(Color.FromArgb(60, Color.Orange));
                e.Graphics.FillRectangle(h, e.Bounds);
            }
            using (var txt = new SolidBrush(sel ? SystemColors.HighlightText : lb.ForeColor))
                e.Graphics.DrawString(lb.Items[e.Index]?.ToString() ?? "", lb.Font, txt, e.Bounds.Left + 2, e.Bounds.Top + 2);
        }

        private static void DrawEdgeLabel(Graphics g, int x, Rectangle channel, string label, Color color)
        {
            using var font = new Font("Segoe UI", 7f, FontStyle.Bold);
            using var shadow = new SolidBrush(Color.FromArgb(180, 0, 0, 0));
            using var brush = new SolidBrush(color);
            float tx = Math.Clamp(x - 4, channel.Left, channel.Right - 10);
            float ty = channel.Top + 1;
            g.DrawString(label, font, shadow, tx + 1, ty + 1);
            g.DrawString(label, font, brush, tx, ty);
        }

        // 일반 클릭=프레임 이동 · 드래그=구간 선택 · Ctrl+클릭=In/Out
        private void OnListMouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            int idx = ListBoxDragSelectHelper.IndexFromPointClamped(_list, e.Location);
            if (idx < 0) return;

            var mods = Control.ModifierKeys;
            if ((mods & Keys.Control) != 0)
            {
                if ((mods & Keys.Shift) != 0) _selection.SetEnd(idx);
                else _selection.SetStart(idx);
                _listRangeDrag = false;
                _listPendingClick = false;
                SyncListBoxSelectionToRange();
                _refreshUi();
                return;
            }

            _listAnchor = idx;
            _listDragStart = e.Location;
            _listPendingClick = true;
            _listRangeDrag = false;

            if (_selection.Contains(idx))
            {
                // 기존 구간 위: 클릭=이동 · 밖으로 드래그=삭제 목록 DnD (구간 재선택 안 함)
                ListBoxDragSelectHelper.SelectIndexRange(_list, idx, idx);
                return;
            }

            ListBoxDragSelectHelper.SelectIndexRange(_list, idx, idx);
            _list.Capture = true;
        }

        // 드래그 임계값 넘으면 구간 선택 시작 (클릭만이면 In/Out 유지)
        private void OnListMouseMove(object? sender, MouseEventArgs e)
        {
            if (_listPendingClick && !_listRangeDrag && e.Button == MouseButtons.Left)
            {
                if (!ListBoxDragSelectHelper.IsPastDragThreshold(_listDragStart, e.Location))
                    return;

                _listPendingClick = false;

                if (_selection.Contains(_listAnchor))
                    return;

                _listRangeDrag = true;

                var pt = _list.PointToClient(Cursor.Position);
                int idx = _list.ClientRectangle.Contains(pt)
                    ? ListBoxDragSelectHelper.IndexFromPointClamped(_list, pt)
                    : _listAnchor;
                if (idx < 0) idx = _listAnchor;

                _selection.SetRange(_listAnchor, idx);
                SyncListBoxSelectionToRange();
                _refreshUi();
                return;
            }

            if (!_listRangeDrag || e.Button != MouseButtons.Left) return;

            var dragPt = _list.PointToClient(Cursor.Position);
            if (!_list.ClientRectangle.Contains(dragPt)) return;

            int dragIdx = ListBoxDragSelectHelper.IndexFromPointClamped(_list, dragPt);
            if (dragIdx < 0) return;

            _selection.SetRange(_listAnchor, dragIdx);
            SyncListBoxSelectionToRange();
            _refreshUi();
        }

        private void OnListMouseUp(object? sender, MouseEventArgs e)
        {
            if (_list.Capture) _list.Capture = false;

            if (_listRangeDrag)
            {
                _listRangeDrag = false;
                _listPendingClick = false;
                SyncListBoxSelectionToRange();
                _refreshUi();
                return;
            }

            _listPendingClick = false;
        }

        private void SyncListBoxSelectionToRange()
        {
            var (s, end) = _selection.GetRange();
            if (s < 0) return;
            int e = end >= 0 ? end : s;
            ListBoxDragSelectHelper.SelectIndexRange(_list, s, e);
        }

        // Ctrl+드래그로 타임라인 구간 선택, Shift로 해제
        private void OnTimelineMouseDown(object? sender, MouseEventArgs e)
        {
            var mods = Control.ModifierKeys;
            if ((mods & Keys.Shift) != 0 && (mods & Keys.Control) == 0)
            { _selection.Clear(); _refreshUi(); return; }
            if ((mods & Keys.Control) == 0) return;
            int idx = IndexFromMouse(e.X);
            if ((mods & Keys.Shift) != 0) _selection.SetEnd(idx);
            else { _selection.SetStart(idx); _rangeDrag = true; }
            _refreshUi();
        }

        // Ctrl+드래그 중 구간 끝 갱신
        private void OnTimelineMouseMove(object? sender, MouseEventArgs e)
        {
            if (!_rangeDrag || e.Button != MouseButtons.Left) return;

            _selection.SetEnd(IndexFromMouse(e.X));
            _refreshUi();
        }
    }
}
