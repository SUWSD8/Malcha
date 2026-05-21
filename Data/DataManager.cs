using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
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

        private DataManager()
        {
            // Private constructor to prevent instantiation
        }

        // 파일 I/O 및 전체 흐름 제어
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
     
        public Frame ParseFrameJson(string jsonString)
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
        private List<TrainedData> Train(){
            return new List<TrainedData>();
        }
    }
}
