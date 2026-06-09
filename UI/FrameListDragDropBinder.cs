using static Malcha.UI.TimelineSelectionBinder;

namespace Malcha.UI
{
    // 프레임 ↔ 삭제 목록 드래그앤드롭 페이로드
    internal sealed class FrameDragPayload
    {
        internal enum SourceKind { Active, Deleted }

        public SourceKind Source { get; init; }
        public int RangeStart { get; init; } = -1;
        public int RangeEnd { get; init; } = -1;
        public List<int> Indices { get; init; } = new();

        public static readonly string FormatName = "Malcha.FrameDragPayload";

        public bool HasRange => RangeStart >= 0 && RangeEnd >= RangeStart;
    }

    // 프레임 리스트 ↔ 삭제 리스트 드래그앤드롭
    internal sealed class FrameListDragDropBinder
    {
        private readonly ListBox _activeList;
        private readonly ListBox _deletedList;
        private readonly Control _restoreDropTarget;
        private readonly FrameRangeSelection _selection;
        private readonly FrameRangeSelection _deletedSelection;
        private readonly Func<FrameDragPayload, int> _onDropToDeleted;
        private readonly Func<FrameDragPayload, int> _onDropToActive;

        private Point _dragStart;
        private bool _dragging;

        public FrameListDragDropBinder(
            ListBox activeList,
            ListBox deletedList,
            Control restoreDropTarget,
            FrameRangeSelection selection,
            FrameRangeSelection deletedSelection,
            Func<FrameDragPayload, int> onDropToDeleted,
            Func<FrameDragPayload, int> onDropToActive)
        {
            _activeList = activeList;
            _deletedList = deletedList;
            _restoreDropTarget = restoreDropTarget;
            _selection = selection;
            _deletedSelection = deletedSelection;
            _onDropToDeleted = onDropToDeleted;
            _onDropToActive = onDropToActive;
        }

        public void Attach()
        {
            _activeList.AllowDrop = true;
            _deletedList.AllowDrop = true;
            _restoreDropTarget.AllowDrop = true;

            _activeList.MouseDown += OnActiveMouseDown;
            _activeList.MouseMove += OnActiveMouseMove;
            _activeList.MouseUp += OnActiveMouseUp;

            _deletedList.MouseDown += OnDeletedMouseDown;
            _deletedList.MouseMove += OnDeletedMouseMove;
            _deletedList.MouseUp += OnDeletedMouseUp;

            _deletedList.DragEnter += OnDeletedDragEnter;
            _deletedList.DragDrop += OnDeletedDragDrop;
            _deletedList.DragLeave += (_, _) => ResetDeletedHighlight();

            _activeList.DragEnter += OnActiveDragEnter;
            _activeList.DragDrop += OnActiveDragDrop;
            _activeList.DragLeave += (_, _) => ResetActiveHighlight();

            _restoreDropTarget.DragEnter += OnActiveDragEnter;
            _restoreDropTarget.DragDrop += OnActiveDragDrop;
        }

        private void OnActiveMouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            _dragStart = e.Location;
            _dragging = true;

            int idx = ListBoxDragSelectHelper.IndexFromPointClamped(_activeList, e.Location);
            if (idx >= 0 && _selection.Contains(idx))
                _activeList.Capture = true;
        }

        private void OnActiveMouseMove(object? sender, MouseEventArgs e)
        {
            if (!_dragging || e.Button != MouseButtons.Left) return;

            var clientPt = _activeList.PointToClient(Cursor.Position);
            if (_activeList.ClientRectangle.Contains(clientPt))
                return;

            if (!ListBoxDragSelectHelper.IsPastDragThreshold(_dragStart, e.Location)) return;

            var payload = BuildActivePayload();
            if (payload == null) { _dragging = false; return; }

            _dragging = false;
            if (_activeList.Capture) _activeList.Capture = false;
            _activeList.DoDragDrop(payload, DragDropEffects.Move);
        }

        private void OnActiveMouseUp(object? sender, MouseEventArgs e)
        {
            _dragging = false;
            if (_activeList.Capture)
                _activeList.Capture = false;
        }

        private void OnDeletedMouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            _dragStart = e.Location;
            _dragging = true;

            int idx = ListBoxDragSelectHelper.IndexFromPointClamped(_deletedList, e.Location);
            if (idx >= 0 && _deletedSelection.Contains(idx))
                _deletedList.Capture = true;
        }

        private void OnDeletedMouseMove(object? sender, MouseEventArgs e)
        {
            if (!_dragging || e.Button != MouseButtons.Left) return;

            var clientPt = _deletedList.PointToClient(Cursor.Position);
            if (_deletedList.ClientRectangle.Contains(clientPt))
                return;

            if (!ListBoxDragSelectHelper.IsPastDragThreshold(_dragStart, e.Location)) return;

            var payload = BuildDeletedPayload();
            if (payload == null) { _dragging = false; return; }

            _dragging = false;
            if (_deletedList.Capture) _deletedList.Capture = false;
            _deletedList.DoDragDrop(payload, DragDropEffects.Move);
        }

        private void OnDeletedMouseUp(object? sender, MouseEventArgs e)
        {
            _dragging = false;
            if (_deletedList.Capture)
                _deletedList.Capture = false;
        }

        private FrameDragPayload? BuildDeletedPayload()
        {
            var (s, e) = _deletedSelection.GetRange();
            if (s < 0) return null;

            return new FrameDragPayload
            {
                Source = FrameDragPayload.SourceKind.Deleted,
                RangeStart = s,
                RangeEnd = e >= 0 ? e : s,
                Indices = _deletedSelection.ToIndexList()
            };
        }

        private FrameDragPayload? BuildActivePayload()
        {
            var (s, e) = _selection.GetRange();
            if (s >= 0)
            {
                return new FrameDragPayload
                {
                    Source = FrameDragPayload.SourceKind.Active,
                    RangeStart = s,
                    RangeEnd = e >= 0 ? e : s
                };
            }

            var indices = _activeList.SelectedIndices.Cast<int>().ToList();
            if (indices.Count == 0)
            {
                int idx = _activeList.SelectedIndex;
                if (idx >= 0) indices.Add(idx);
            }
            if (indices.Count == 0) return null;

            if (indices.Count > 1)
            {
                return new FrameDragPayload
                {
                    Source = FrameDragPayload.SourceKind.Active,
                    Indices = indices
                };
            }

            return new FrameDragPayload
            {
                Source = FrameDragPayload.SourceKind.Active,
                RangeStart = indices[0],
                RangeEnd = indices[0]
            };
        }

        private bool ShouldStartDrag(Point current)
        {
            var size = SystemInformation.DragSize;
            var dx = Math.Abs(current.X - _dragStart.X);
            var dy = Math.Abs(current.Y - _dragStart.Y);
            return dx >= size.Width || dy >= size.Height;
        }

        private void OnDeletedDragEnter(object? sender, DragEventArgs e)
        {
            if (TryGetPayload(e, out var payload) && payload.Source == FrameDragPayload.SourceKind.Active)
            {
                e.Effect = DragDropEffects.Move;
                _deletedList.BackColor = Color.FromArgb(68, 52, 48);
            }
            else e.Effect = DragDropEffects.None;
        }

        private void OnDeletedDragDrop(object? sender, DragEventArgs e)
        {
            ResetDeletedHighlight();
            if (!TryGetPayload(e, out var payload) || payload.Source != FrameDragPayload.SourceKind.Active) return;
            _onDropToDeleted(payload);
        }

        private void OnActiveDragEnter(object? sender, DragEventArgs e)
        {
            if (TryGetPayload(e, out var payload) && payload.Source == FrameDragPayload.SourceKind.Deleted)
            {
                e.Effect = DragDropEffects.Move;
                if (sender == _activeList)
                    _activeList.BackColor = Color.FromArgb(58, 52, 48);
            }
            else e.Effect = DragDropEffects.None;
        }

        private void OnActiveDragDrop(object? sender, DragEventArgs e)
        {
            ResetActiveHighlight();
            if (!TryGetPayload(e, out var payload) || payload.Source != FrameDragPayload.SourceKind.Deleted) return;
            _onDropToActive(payload);
        }

        private void ResetDeletedHighlight() =>
            _deletedList.BackColor = Color.FromArgb(48, 42, 41);

        private void ResetActiveHighlight() =>
            _activeList.BackColor = Color.FromArgb(48, 42, 41);

        private static bool TryGetPayload(DragEventArgs e, out FrameDragPayload payload)
        {
            payload = null!;
            if (e.Data?.GetDataPresent(typeof(FrameDragPayload)) != true) return false;
            if (e.Data.GetData(typeof(FrameDragPayload)) is not FrameDragPayload p) return false;
            payload = p;
            return true;
        }
    }
}
