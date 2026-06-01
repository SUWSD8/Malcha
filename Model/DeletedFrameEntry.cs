namespace Malcha.Model
{
    // 삭제 목록에 보관되는 프레임 (복구 시 원래 위치 힌트)
    internal sealed class DeletedFrameEntry
    {
        public required Frame Frame { get; init; }
        public string ImagePath { get; init; } = string.Empty;
        public int OriginalIndex { get; init; }
    }
}
