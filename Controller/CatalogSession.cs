using System.Collections.Generic;
using System.Linq;
using Malcha.Model;

namespace Malcha
{
    // 카탈로그 편집 세션 상태 — 프레임·경로·Undo·삭제 관리
    internal sealed class CatalogSession
    {
        // Undo 복원용 스냅샷
        internal sealed class UndoSnapshot
        {
            public List<Frame> Frames { get; init; } = new();
            public List<string> ImagePaths { get; init; } = new();
            public int CurrentIndex { get; init; }
        }

        // 프레임 범위 삭제 결과
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

        // 현재 상태를 Undo 스택에 저장
        public void PushUndo()
        {
            _undoStack.Push(new UndoSnapshot
            {
                Frames = CurrentFrames.ToList(),
                ImagePaths = FrameImagePaths != null ? new List<string>(FrameImagePaths) : new List<string>(),
                CurrentIndex = CurrentIndex
            });
        }

        // Undo 스택에서 스냅샷 꺼내기
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

        // 스냅샷으로 세션 상태 복원
        public void RestoreUndo(UndoSnapshot snapshot)
        {
            CurrentFrames = snapshot.Frames ?? new List<Frame>();
            FrameImagePaths = snapshot.ImagePaths ?? new List<string>();
            CurrentIndex = snapshot.CurrentIndex;
        }

        // start~end 프레임 삭제 (자동 Undo 저장)
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

        // Undo 스택 비우기
        public void ClearUndo() => _undoStack.Clear();

        // 세션 전체 초기화
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
