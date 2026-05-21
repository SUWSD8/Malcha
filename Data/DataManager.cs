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
        // JSON 문자열을 Frame 객체로 변환하는 메서드

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
        // WSL에서 모델 학습을 실행하는 메서드
        public async Task<bool> TrainModelInWslAsync(string tubDirName, string modelFileName)
        {
            return await Task.Run(() =>
            {
                try
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.FileName = "wsl.exe"; // 윈도우의 WSL 실행 파일

                    // 알려주신 리눅스 내부 경로 하드코딩 (필요시 변수로 빼셔도 좋습니다)
                    string wslCarDirectory = "/home/eodbs/mycar";

                    // WSL 배포판(-d Ubuntu-22.04)을 명확히 지정하고, bash 쉘을 통해 명령어 실행
                    startInfo.Arguments = $"-d Ubuntu-22.04 -- bash -ic \"conda activate e2e_env && cd {wslCarDirectory} && python3 train.py --tub {tubDirName} --model models/{modelFileName}\"";

                    startInfo.UseShellExecute = false; // cmd 창을 직접 띄우지 않음
                    startInfo.CreateNoWindow = true;   // 백그라운드 실행

                    using (Process process = Process.Start(startInfo))
                    {
                        // 파이썬 스크립트가 완전히 종료될 때까지 대기
                        process.WaitForExit();

                        // 종료 코드(ExitCode)가 0이면 정상 종료, 그 외는 에러
                        return process.ExitCode == 0;
                    }
                }
                catch (Exception ex)
                {
                    // WSL 프로세스 실행 자체에 실패한 경우
                    Console.WriteLine($"WSL 실행 중 에러 발생: {ex.Message}");
                    return false;
                }
            });
        }
    }
}
