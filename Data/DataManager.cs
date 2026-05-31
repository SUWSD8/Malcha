using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Malcha.Model;

namespace Malcha.Data
{
    internal class DataManager
    {
        private static readonly DataManager _instance = new DataManager();

        public static DataManager Instance
        {
            get { 
                return _instance;
            }
        }

        // 저장: 주어진 프레임 리스트를 한 줄 JSON(각 라인 하나의 JSON) 형식으로 저장합니다.
        public async Task<bool> SaveFramesAsync(string outPath, List<Frame> frames)
        {
            try
            {
                using (var sw = new StreamWriter(outPath, false))
                {
                    foreach (var f in frames)
                    {
                        var json = JsonSerializer.Serialize(f);
                        await sw.WriteLineAsync(json);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SaveFramesAsync error: {ex.Message}");
                return false;
            }
        }

        private DataManager()
        {
            // Private constructor to prevent instantiation
        }

        // 파일 I/O 및 전체 흐름 제어
        // 설명(한글):
        //  - 주어진 단일 카탈로그 파일(path)을 한 줄씩 비동기적으로 읽어들여
        //    각 줄을 JSON으로 파싱하여 Frame 객체 리스트를 반환합니다.
        //  - 파일 내부의 각 라인은 독립적인 JSON 레코드(한 줄 JSON) 형식입니다.
        //  - 비정상 JSON 라인은 ParseFrameJson에서 null로 처리되어 무시됩니다.
        public async Task<List<Frame>> LoadFrameAsync(string path)
        {
            List<Frame> frames = new List<Frame>();
            // StreamReader를 사용하여 파일을 비동기적으로 읽어들임
            using (StreamReader reader = new StreamReader(path))
            {
                string line;
                // 파일을 한 줄씩 읽어들이면서 Frame 객체로 파싱하여 리스트에 추가
                while (((line = await reader.ReadLineAsync()) != null))
                {
                    // 빈 줄 건너뛰기
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    // 단일 파싱
                    Frame frame = ParseFrameJson(line);
                    if (frame != null)
                    {
                        frames.Add(frame);
                    }
                }
            }
            return frames;
        }
        // 지정한 디렉터리에서 모든 카탈로그(.catalog) 파일을 찾아서 각각의 프레임 리스트를 로드합니다.
        // 설명(한글):
        //  - 폴더 경로를 받아 폴더 내의 "*.catalog" 파일들을 검색합니다.
        //  - 각 카탈로그 파일은 LoadFrameAsync로 개별 비동기 로드되며 파일 경로를 키로
        //    List<Frame>를 값으로 가지는 딕셔너리를 반환합니다.
        //  - 일부 파일이 손상되어 로드에 실패하더라도 다른 파일 로드는 계속 수행됩니다.
        //  - 파일 경로는 절대경로나 상대경로 모두 허용하되, 존재하지 않으면 현재 작업 디렉터리 기준으로 재시도합니다.
        public async Task<Dictionary<string, List<Frame>>> LoadCatalogsAsync(string directory)
        {
            var result = new Dictionary<string, List<Frame>>();

            if (string.IsNullOrWhiteSpace(directory))
                throw new ArgumentException("directory is null or empty", nameof(directory));

            string resolvedDir = directory;
            if (!Directory.Exists(resolvedDir))
            {
                resolvedDir = Path.Combine(Environment.CurrentDirectory, directory);
            }

            if (!Directory.Exists(resolvedDir))
            {
                return result;
            }

            var catalogFiles = Directory.GetFiles(resolvedDir, "*.catalog")
                .Where(CatalogPaths.IsWorkingCatalog)
                .OrderBy(p => p)
                .ToArray();

            foreach (var file in catalogFiles)
            {
                try
                {
                    var frames = await LoadFrameAsync(file);
                    result[file] = frames;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to load catalog '{file}': {ex.Message}");
                }
            }

            return result;
        }

        // 단일 카탈로그 파일을 로드하는 간단한 래퍼
        // 설명(한글):
        //  - 파일 경로 유효성 검사를 수행한 뒤 LoadFrameAsync를 호출합니다.
        //  - 파일이 존재하지 않으면 빈 리스트를 반환합니다.
        public async Task<List<Frame>> LoadCatalogFileAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("filePath is null or empty", nameof(filePath));

            if (!File.Exists(filePath))
                return new List<Frame>();

            return await LoadFrameAsync(filePath);
        }

        // 카탈로그의 manifest 파일(.catalog_manifest)을 읽어 JsonDocument로 반환합니다.
        // 설명(한글):
        //  - manifest 파일(또는 manifest.json)의 내용을 읽어 JsonDocument로 파싱하여 반환합니다.
        //  - 파일이 없거나 파싱에 실패하면 null을 반환합니다.
        public async Task<JsonDocument> LoadCatalogManifestAsync(string manifestPath)
        {
            if (string.IsNullOrWhiteSpace(manifestPath))
                throw new ArgumentException("manifestPath is null or empty", nameof(manifestPath));

            if (!File.Exists(manifestPath))
                return null;

            string json = await File.ReadAllTextAsync(manifestPath);
            try
            {
                return JsonDocument.Parse(json);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        // JSON 문자열을 Frame 객체로 변환하는 메서드
        // 설명(한글):
        //  - 단일 JSON 문자열을 System.Text.Json으로 역직렬화하여 Frame 객체를 만듭니다.
        //  - JSON 형식이 유효하지 않으면 null을 반환하여 호출 쪽에서 무시하도록 설계했습니다.
        private Frame ParseFrameJson(string jsonString)
        {
            try
            {
                // 문자열을 Frame 객체로 변환해서 리턴
                return JsonSerializer.Deserialize<Frame>(jsonString);
            }
            catch (JsonException)
            {
                // JSON 형식이 깨진 에러가 나면 null 리턴
                return null;
            }
        }
    }
}
