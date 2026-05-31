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

        private readonly TrackBar _timeline;
        private readonly ListBox _list;
        private readonly FrameRangeSelection _selection;
        private readonly Action _refreshUi;
        private bool _rangeDrag;
        private bool _listRangeDrag;
        private int _listAnchor = -1;

        public TimelineSelectionBinder(TrackBar timeline, ListBox list,
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
            _list.DrawMode = DrawMode.OwnerDrawFixed;
            _list.DrawItem += OnListDrawItem;
            _list.MouseDown += OnListMouseDown;
            _list.MouseMove += OnListMouseMove;
            _list.MouseUp += OnListMouseUp;
            _timeline.MouseDown += OnTimelineMouseDown;
            _timeline.MouseMove += OnTimelineMouseMove;
            _timeline.MouseUp += (_, _) => _rangeDrag = false;
            _timeline.Paint += OnTimelinePaint;
        }

        // 타임라인 마우스 X → 프레임 인덱스 변환
        private int IndexFromMouse(int mouseX)
        {
            int w = Math.Max(1, _timeline.ClientSize.Width - 8);
            float ratio = Math.Clamp((float)mouseX / w, 0f, 1f);
            return Math.Clamp((int)Math.Round(ratio * (_timeline.Maximum - _timeline.Minimum)) + _timeline.Minimum,
                _timeline.Minimum, _timeline.Maximum);
        }

        // 리스트 항목 그리기 (선택 구간 주황 하이라이트)
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

        // 타임라인에 선택 구간 주황 띠 그리기
        private void OnTimelinePaint(object? sender, PaintEventArgs e)
        {
            var (s, end) = _selection.GetRange();
            if (s < 0) return;
            var tb = _timeline;
            int w = tb.ClientSize.Width, min = tb.Minimum, max = tb.Maximum;
            float r1 = (float)(s - min) / Math.Max(1, max - min);
            float r2 = (float)((end >= 0 ? end : s) - min) / Math.Max(1, max - min);
            int x1 = (int)(r1 * w), x2 = (int)(r2 * w);
            using var brush = new SolidBrush(Color.FromArgb(80, Color.Orange));
            e.Graphics.FillRectangle(brush, Math.Min(x1, x2), 0, Math.Abs(x2 - x1), tb.ClientSize.Height);
        }

        // Ctrl+클릭: 구간 끝/시작 · 일반 드래그: 구간 선택
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
                _refreshUi();
                return;
            }

            // 이미 선택된 구간 위에서 누르면 구간 유지 → 이후 드래그로 이동
            if (_selection.Contains(idx))
            {
                _listRangeDrag = false;
                return;
            }

            _listAnchor = idx;
            _listRangeDrag = true;
            _selection.SetRange(idx, idx);
            _list.SelectedIndex = idx;
            _list.Capture = true;
            _refreshUi();
        }

        // 리스트 내 드래그로 구간 끝 갱신
        private void OnListMouseMove(object? sender, MouseEventArgs e)
        {
            if (!_listRangeDrag || e.Button != MouseButtons.Left) return;

            var pt = _list.PointToClient(Cursor.Position);
            if (!_list.ClientRectangle.Contains(pt)) return;

            int idx = ListBoxDragSelectHelper.IndexFromPointClamped(_list, pt);
            if (idx < 0) return;

            _selection.SetRange(_listAnchor, idx);
            _refreshUi();
        }

        private void OnListMouseUp(object? sender, MouseEventArgs e)
        {
            if (!_listRangeDrag) return;
            _listRangeDrag = false;
            if (_list.Capture) _list.Capture = false;

            var (s, _) = _selection.GetRange();
            if (s >= 0 && _list.SelectedIndex != s)
                _list.SelectedIndex = s;
            _refreshUi();
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
