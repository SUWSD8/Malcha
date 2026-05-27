using System.Collections.Generic;
using System.Linq;
using Malcha.Model;

namespace Malcha
{
    internal sealed class CatalogSession
    {
        internal sealed class UndoSnapshot
        {
            public List<Frame> Frames { get; init; } = new();
            public List<string> ImagePaths { get; init; } = new();
            public int CurrentIndex { get; init; }
        }

        internal sealed class DeleteRangeResult
        {
            public int Start { get; init; }
            public int Count { get; init; }
            public int NewIndex { get; init; }
        }

        public Dictionary<string, List<Frame>> Catalogs { get; } = new();
        public List<Frame> CurrentFrames { get; set; } = new();
        public string CurrentCatalogPath { get; set; } = string.Empty;
        public List<string> FrameImagePaths { get; set; } = new();
        public int CurrentIndex { get; set; }

        private readonly Stack<UndoSnapshot> _undoStack = new();

        public bool HasUndo => _undoStack.Count > 0;

        public void PushUndo()
        {
            _undoStack.Push(new UndoSnapshot
            {
                Frames = CurrentFrames.ToList(),
                ImagePaths = FrameImagePaths != null ? new List<string>(FrameImagePaths) : new List<string>(),
                CurrentIndex = CurrentIndex
            });
        }

        public bool TryPopUndo(out UndoSnapshot snapshot)
        {
            if (_undoStack.Count == 0)
            {
                snapshot = null!;
                return false;
            }
            snapshot = _undoStack.Pop();
            return true;
        }

        public void RestoreUndo(UndoSnapshot snapshot)
        {
            CurrentFrames = snapshot.Frames ?? new List<Frame>();
            FrameImagePaths = snapshot.ImagePaths ?? new List<string>();
            CurrentIndex = snapshot.CurrentIndex;
        }

        public DeleteRangeResult? DeleteRange(int start, int end)
        {
            if (CurrentFrames == null || CurrentFrames.Count == 0)
                return null;

            PushUndo();

            start = System.Math.Max(0, System.Math.Min(start, CurrentFrames.Count - 1));
            end = System.Math.Max(0, System.Math.Min(end, CurrentFrames.Count - 1));
            if (end < start) (start, end) = (end, start);

            int count = end - start + 1;
            CurrentFrames.RemoveRange(start, count);
            if (FrameImagePaths != null && FrameImagePaths.Count >= start + count)
                FrameImagePaths.RemoveRange(start, count);

            CurrentIndex = System.Math.Max(0, System.Math.Min(CurrentFrames.Count - 1, start));
            if (!string.IsNullOrEmpty(CurrentCatalogPath))
                Catalogs[CurrentCatalogPath] = CurrentFrames;

            return new DeleteRangeResult { Start = start, Count = count, NewIndex = CurrentIndex };
        }

        public void Reset()
        {
            Catalogs.Clear();
            CurrentFrames = new List<Frame>();
            CurrentCatalogPath = string.Empty;
            FrameImagePaths = new List<string>();
            CurrentIndex = 0;
            _undoStack.Clear();
        }
    }
}
