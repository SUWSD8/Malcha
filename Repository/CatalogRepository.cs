using Malcha.Data;
using Malcha.Model;

namespace Malcha.Repository
{
    // [Repository] 카탈로그 프레임 데이터 저장·로드
    internal class CatalogRepository
    {
        private static readonly CatalogRepository _instance = new();
        public static CatalogRepository Instance => _instance;

        private List<Frame> _frames = new();
        private CatalogRepository() { }

        // 메모리 프레임 목록 설정
        public void SetFrames(List<Frame> frames) => _frames = frames;

        // 메모리 프레임 목록 반환
        public List<Frame> GetFrames() => _frames;

        // 파일에서 프레임 로드
        public Task<List<Frame>> LoadFramesAsync(string path) =>
            DataManager.Instance.LoadFrameAsync(path);
    }
}
