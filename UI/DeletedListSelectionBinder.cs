using static Malcha.UI.TimelineSelectionBinder;

namespace Malcha.UI
{
    // 삭제 목록 ListBox — 드래그 구간 선택 (프레임 리스트와 동일 UX)
    internal sealed class DeletedListSelectionBinder
    {
        private readonly ListBox _list;
        private readonly FrameRangeSelection _selection;
        private readonly Action _refreshUi;
        private bool _rangeDrag;
        private int _anchor = -1;

        public DeletedListSelectionBinder(ListBox list, FrameRangeSelection selection, Action refreshUi)
        {
            _list = list;
            _selection = selection;
            _refreshUi = refreshUi;
        }

        public void Attach()
        {
            _list.DrawMode = DrawMode.OwnerDrawFixed;
            _list.DrawItem += OnDrawItem;
            _list.MouseDown += OnMouseDown;
            _list.MouseMove += OnMouseMove;
            _list.MouseUp += OnMouseUp;
        }

        private void OnDrawItem(object? sender, DrawItemEventArgs e)
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

        private void OnMouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            int idx = ListBoxDragSelectHelper.IndexFromPointClamped(_list, e.Location);
            if (idx < 0) return;

            if (_selection.Contains(idx))
            {
                _rangeDrag = false;
                return;
            }

            _anchor = idx;
            _rangeDrag = true;
            _selection.SetRange(idx, idx);
            _list.SelectedIndex = idx;
            _list.Capture = true;
            _refreshUi();
        }

        private void OnMouseMove(object? sender, MouseEventArgs e)
        {
            if (!_rangeDrag || e.Button != MouseButtons.Left) return;

            var pt = _list.PointToClient(Cursor.Position);
            if (!_list.ClientRectangle.Contains(pt)) return;

            int idx = ListBoxDragSelectHelper.IndexFromPointClamped(_list, pt);
            if (idx < 0) return;

            _selection.SetRange(_anchor, idx);
            _refreshUi();
        }

        private void OnMouseUp(object? sender, MouseEventArgs e)
        {
            if (!_rangeDrag) return;
            _rangeDrag = false;
            if (_list.Capture) _list.Capture = false;

            var (s, _) = _selection.GetRange();
            if (s >= 0 && _list.SelectedIndex != s)
                _list.SelectedIndex = s;
            _refreshUi();
        }
    }
}
