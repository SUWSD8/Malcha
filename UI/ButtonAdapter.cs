using Malcha.Controller;
using Malcha.Data;
using Malcha.Model;
using Malcha.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace Malcha.UI
{
    internal static class ButtonAdapter
    {
        // Catalog 파일 분석 실행 메서드
        public static async void RunCatalogAnalysis(Button btn)
        {
            // 1. 파일 선택 창(OpenFileDialog) 생성 및 설정
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "동키카 catalog 파일을 선택하세요";
                // catalog 파일만 쉽게 찾을 수 있도록 필터 적용
                openFileDialog.Filter = "Catalog 파일 (catalog*)|catalog*|모든 파일 (*.*)|*.*";

                // 2. 사용자가 파일을 선택하고 '확인'을 누른 경우에만 실행
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedFilePath = openFileDialog.FileName;

                    try
                    {
                        // 중복 클릭 방지: 데이터를 읽는 동안 버튼을 일시적으로 비활성화
                        // (주의: btnTestLoad 부분은 실제 추가하신 버튼의 Name으로 변경하세요)
                        btn.Enabled = false;
                        btn.Text = "데이터 읽는 중...";

                        // 3. DataManager를 통해 비동기로 데이터 로드
                        List<Frame> parsedData = await DataManager.Instance.LoadFrameAsync(selectedFilePath);
                        DonkeyRepository.Instance.SetFrames(parsedData); // Repository에 파싱된 데이터 저장
                        // 4. 파싱 결과 확인용 메시지 박스 출력
                        if (parsedData.Count > 0)
                        {
                            Frame firstFrame = parsedData[0];
                            MessageBox.Show(
                                $"파싱 성공!\n" +
                                $"총 읽어온 프레임 수: {parsedData.Count}개\n\n" +
                                $"[첫 번째 데이터 샘플]\n" +
                                $"인덱스: {firstFrame.Index}\n" +
                                $"이미지: {firstFrame.ImagePath}\n" +
                                $"조향각(Angle): {firstFrame.Angle}\n" +
                                $"쓰로틀(Throttle): {firstFrame.Throttle}",
                                "테스트 성공", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("데이터를 읽어왔지만, 파싱된 프레임이 0개입니다. 파일 내용을 확인하세요.", "경고");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"에러가 발생했습니다: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        // 5. 작업이 성공하든 실패하든 마지막에 다시 버튼 활성화
                        btn.Enabled = true;
                        btn.Text = "테스트: Catalog 파일 열기";
                    }
                }
            }
        }
        // 모델 학습 실행 메서드
        public static async Task RunModelTraining(Button btn,string name)
        {
            // 1. 중복 클릭 방지 (버튼 비활성화)
            btn.Enabled = false;
            btn.Text = "WSL에서 모델 학습 중... (시간이 소요됩니다)";
            try
            {
                // 2. 비동기로 학습 메서드 호출 (데이터 폴더명과 생성될 모델 파일명 전달)
                // 실제 data/ 폴더 안에 있는 tub 폴더 이름과 원하는 모델 이름을 적어주세요.
                bool isSuccess = await DataManager.Instance.TrainModelInWslAsync("data", $"{name}.h5");

                // 3. 결과 확인
                if (isSuccess)
                {
                    MessageBox.Show("WSL 환경에서의 모델 학습이 성공적으로 완료되었습니다!", "학습 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"예상치 못한 에러가 발생했습니다: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // 4. 작업 완료 후 버튼 상태 복구
                btn.Enabled = true;
                btn.Text = "모델 학습 시작";
            }
        }
        
        public static async Task ParseTrainingHistory(Button btn, string modelName)
        {
            // WSL 내부의 database.json 절대 경로 세팅
            string dbPath = @"\\wsl.localhost\Ubuntu-22.04\home\eodbs\mycar\models\database.json";

            bool isSuccess = await TrainModelController.Instance.TrainedModelAnalasys(dbPath, modelName);

            if (isSuccess != null)
            {
                TrainedModelInfo model = DonkeyRepository.Instance.FindByName(modelName);
                if(model == null)
                {
                    MessageBox.Show($"모델 이름 '{modelName}'에 해당하는 학습 결과를 찾을 수 없습니다. 데이터베이스를 확인하세요.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                var history = model.History;
                
                // 마지막 에포크(최종 학습 결과) 데이터 추출
                TrainedData lastEpoch = history.Last();

                // 메시지 박스로 간단하게 결과 요약 출력
                string resultMessage = $"WSL 환경에서의 모델 학습이 성공적으로 완료되었습니다!\n\n" +
                                       $"[학습 결과 요약]\n" +
                                       $"[모델 이름] {model.Name}\n" +
                                       $"- 총 진행된 Epoch: {model.History.Count}회\n" +
                                       $"- 최종 훈련 손실(Loss): {lastEpoch.Loss:F4}\n" +
                                       $"- 최종 검증 손실(Val_Loss): {lastEpoch.ValLoss:F4}";

                MessageBox.Show(resultMessage, "학습 및 파싱 성공", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

    }
}
