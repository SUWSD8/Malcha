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
        // JSON 파일에서 특정 모델의 훈련 기록을 읽어와 TrainedData 리스트로 반환하는 메서드

        public async Task<TrainedModelInfo> LoadTrainingHistoryAsync(string path, string targetModelName)
        {
            TrainedModelInfo resultInfo = null; // 최종 반환할 상위 객체
            try
            {
                if (!File.Exists(path)) { throw new FileNotFoundException("database.json 파일을 찾을 수 없습니다."); }
                // 파일을 비동기적으로 읽어들여 JSON 문자열로 저장
                string jsonContent = await File.ReadAllTextAsync(path);

                // 2. ⭐ 전체 데이터를 List<DatabaseEntry> 형태(배열)로 직렬화하여 가져옵니다.
                var entries = JsonSerializer.Deserialize<List<DatabaseEntry>>(jsonContent);

                // 3. ⭐ 리스트 안에 있는 여러 모델 중, 우리가 찾는 모델 이름("mypilot")과 일치하는 것 하나만 쏙 골라냅니다.
                var targetEntry = entries?.FirstOrDefault(e => e.Name == targetModelName);

                if (targetEntry != null)
                {
                    // 1. 메타데이터 옮겨 담기
                    resultInfo = new TrainedModelInfo
                    {
                        Number = targetEntry.Number,
                        Name = targetEntry.Name,
                        Pilot = targetEntry.Pilot,
                        Type = targetEntry.Type,
                        Tubs = targetEntry.Tubs,
                        Time = targetEntry.Time,
                        Transfer = targetEntry.Transfer,
                        Comment = targetEntry.Comment
                    };
                    // 2. 차트용 데이터 재포장해서 리스트에 담기
                    if (targetEntry.History != null && targetEntry.History.Loss != null)
                    {
                        var losses = targetEntry.History.Loss;
                        var valLosses = targetEntry.History.ValLoss;

                        for (int i = 0; i < losses.Count; i++)
                        {
                            resultInfo.History.Add(new TrainedData
                            {
                                Epoch = i + 1,
                                Loss = losses[i],
                                ValLoss = (valLosses != null && i < valLosses.Count) ? valLosses[i] : 0
                            });
                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"JSON 파싱 중 에러 발생: {ex.Message}");
            }
            return resultInfo;
        }
    }
}
