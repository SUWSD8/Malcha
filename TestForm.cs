using Malcha.Data;
using Malcha.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Malcha
{
    public partial class TestForm : Form
    {
        public TestForm()
        {
            InitializeComponent();
        }

        private async void btnTest_Click(object sender, EventArgs e)
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
                        btnTest.Enabled = false;
                        btnTest.Text = "데이터 읽는 중...";

                        // 3. DataManager를 통해 비동기로 데이터 로드
                        List<Frame> parsedData = await DataManager.Instance.LoadFrameAsync(selectedFilePath);

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
                        btnTest.Enabled = true;
                        btnTest.Text = "테스트: Catalog 파일 열기";
                    }
                }
            }
        }
    }
}
