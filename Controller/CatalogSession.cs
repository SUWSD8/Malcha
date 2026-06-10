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
            public List<DeletedFrameEntry> DeletedEntries { get; init; } = new();
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
        public List<DeletedFrameEntry> DeletedEntries { get; } = new();
        public int CurrentIndex { get; set; }

        private readonly List<CrossTestFramePrediction?> _crossTestByFrame = new();
        public string CrossTestModelName { get; private set; } = string.Empty;
        public bool HasCrossTest => _crossTestByFrame.Count > 0;

        public CrossTestFramePrediction? GetCrossTest(int frameIndex)
        {
            if (frameIndex < 0 || frameIndex >= _crossTestByFrame.Count) return null;
            return _crossTestByFrame[frameIndex];
        }

        public void SetCrossTestResults(string modelName, IEnumerable<CrossTestFramePrediction> predictions)
        {
            CrossTestModelName = modelName;
            _crossTestByFrame.Clear();
            for (int i = 0; i < CurrentFrames.Count; i++)
                _crossTestByFrame.Add(null);
            foreach (var p in predictions)
            {
                if (p.Index >= 0 && p.Index < _crossTestByFrame.Count)
                    _crossTestByFrame[p.Index] = p;
            }
        }

        public void ClearCrossTest()
        {
            CrossTestModelName = string.Empty;
            _crossTestByFrame.Clear();
        }

        private readonly Stack<UndoSnapshot> _undoStack = new();

        public bool HasUndo => _undoStack.Count > 0;

        // 현재 상태를 Undo 스택에 저장
        public void PushUndo()
        {
            _undoStack.Push(new UndoSnapshot
            {
                Frames = CurrentFrames.ToList(),
                ImagePaths = FrameImagePaths != null ? new List<string>(FrameImagePaths) : new List<string>(),
                DeletedEntries = DeletedEntries.Select(e => new DeletedFrameEntry
                {
                    Frame = e.Frame,
                    ImagePath = e.ImagePath,
                    OriginalIndex = e.OriginalIndex
                }).ToList(),
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
            DeletedEntries.Clear();
            DeletedEntries.AddRange(snapshot.DeletedEntries ?? new List<DeletedFrameEntry>());
            CurrentIndex = snapshot.CurrentIndex;
        }

        // start~end 프레임을 삭제 목록으로 이동
        public DeleteRangeResult? MoveRangeToDeleted(int start, int end)
        {
            if (CurrentFrames.Count == 0) return null;

            start = System.Math.Max(0, System.Math.Min(start, CurrentFrames.Count - 1));
            end = System.Math.Max(0, System.Math.Min(end, CurrentFrames.Count - 1));
            if (end < start) (start, end) = (end, start);

            PushUndo();

            for (int i = start; i <= end; i++)
            {
                DeletedEntries.Add(new DeletedFrameEntry
                {
                    Frame = CurrentFrames[i],
                    ImagePath = i < FrameImagePaths.Count ? FrameImagePaths[i] : string.Empty,
                    OriginalIndex = i
                });
            }

            int count = end - start + 1;
            int playhead = CurrentIndex;
            CurrentFrames.RemoveRange(start, count);
            if (FrameImagePaths.Count >= start + count)
                FrameImagePaths.RemoveRange(start, count);

            if (CurrentFrames.Count == 0)
                CurrentIndex = 0;
            else if (playhead > end)
                CurrentIndex = playhead - count;
            else if (playhead >= start)
                CurrentIndex = Math.Min(start, CurrentFrames.Count - 1);
            else
                CurrentIndex = playhead;

            CurrentIndex = Math.Clamp(CurrentIndex, 0, CurrentFrames.Count - 1);

            if (!string.IsNullOrEmpty(CurrentCatalogPath))
                Catalogs[CurrentCatalogPath] = CurrentFrames;

            return new DeleteRangeResult { Start = start, Count = count, NewIndex = CurrentIndex };
        }

        // 여러 인덱스를 삭제 목록으로 이동 (내림차순 제거)
        public DeleteRangeResult? MoveIndicesToDeleted(IReadOnlyList<int> indices)
        {
            if (CurrentFrames.Count == 0 || indices.Count == 0) return null;

            var unique = indices.Distinct().Where(i => i >= 0 && i < CurrentFrames.Count).OrderBy(i => i).ToList();
            if (unique.Count == 0) return null;

            PushUndo();

            foreach (var i in unique)
            {
                DeletedEntries.Add(new DeletedFrameEntry
                {
                    Frame = CurrentFrames[i],
                    ImagePath = i < FrameImagePaths.Count ? FrameImagePaths[i] : string.Empty,
                    OriginalIndex = i
                });
            }

            int first = unique[0];
            foreach (var i in unique.OrderByDescending(x => x))
            {
                CurrentFrames.RemoveAt(i);
                if (i < FrameImagePaths.Count)
                    FrameImagePaths.RemoveAt(i);
            }

            CurrentIndex = CurrentFrames.Count == 0
                ? 0
                : System.Math.Max(0, System.Math.Min(CurrentFrames.Count - 1, first));

            if (!string.IsNullOrEmpty(CurrentCatalogPath))
                Catalogs[CurrentCatalogPath] = CurrentFrames;

            return new DeleteRangeResult { Start = first, Count = unique.Count, NewIndex = CurrentIndex };
        }

        // 삭제 목록에서 선택 항목을 원래 위치 근처로 복구
        public int RestoreFromDeleted(IReadOnlyList<int> deletedIndices)
        {
            if (deletedIndices.Count == 0 || DeletedEntries.Count == 0) return 0;

            var unique = deletedIndices.Distinct().Where(i => i >= 0 && i < DeletedEntries.Count).OrderBy(i => i).ToList();
            if (unique.Count == 0) return 0;

            PushUndo();

            var toRestore = unique.Select(i => DeletedEntries[i]).OrderBy(e => e.OriginalIndex).ToList();
            foreach (var i in unique.OrderByDescending(x => x))
                DeletedEntries.RemoveAt(i);

            foreach (var entry in toRestore)
            {
                int insertAt = System.Math.Clamp(entry.OriginalIndex, 0, CurrentFrames.Count);
                CurrentFrames.Insert(insertAt, entry.Frame);
                FrameImagePaths.Insert(insertAt, entry.ImagePath);
            }

            for (int i = 0; i < CurrentFrames.Count; i++)
                CurrentFrames[i].Index = i;

            CurrentIndex = System.Math.Clamp(CurrentIndex, 0, System.Math.Max(0, CurrentFrames.Count - 1));

            if (!string.IsNullOrEmpty(CurrentCatalogPath))
                Catalogs[CurrentCatalogPath] = CurrentFrames;

            return toRestore.Count;
        }

        // start~end 프레임 범위 삭제 (자동 Undo 저장) — 레거시, MoveRangeToDeleted 사용 권장
        public DeleteRangeResult? DeleteRange(int start, int end) => MoveRangeToDeleted(start, end);

        public void ClearDeleted() => DeletedEntries.Clear();

        // Undo 스택 비우기
        public void ClearUndo() => _undoStack.Clear();

        // 세션 전체 초기화
        public void Reset()
        {
            Catalogs.Clear();
            CurrentFrames = new List<Frame>();
            CurrentCatalogPath = string.Empty;
            FrameImagePaths = new List<string>();
            DeletedEntries.Clear();
            ClearCrossTest();
            CurrentIndex = 0;
            _undoStack.Clear();
        }
    }
}
