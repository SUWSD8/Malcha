using Malcha;
using System.Drawing;

namespace Malcha.UI
{
    // 정제 완료 후 제거 내역 요약
    internal sealed class RefinementResultDialog : Form
    {
        private static readonly Color Bg = Color.FromArgb(33, 28, 29);
        private static readonly Color TextLight = Color.FromArgb(240, 236, 234);
        private static readonly Color TextMuted = Color.FromArgb(168, 162, 160);
        private static readonly Color Accent = Color.FromArgb(255, 158, 48);
        private static readonly Color Ok = Color.FromArgb(120, 200, 140);

        public static void Show(
            IWin32Window owner,
            FrameRefinementFilter.Result result,
            string imageBefore,
            string imageAfter)
        {
            using var dlg = new RefinementResultDialog(result, imageBefore, imageAfter);
            dlg.ShowDialog(owner);
        }

        private RefinementResultDialog(
            FrameRefinementFilter.Result result,
            string imageBefore,
            string imageAfter)
        {
            Text = "정제 완료";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(460, 390);
            BackColor = Bg;
            ForeColor = TextLight;
            Font = new Font("맑은 고딕", 9.75F);

            int removed = result.RemovedTotal;
            double pct = result.OriginalCount > 0 ? 100.0 * removed / result.OriginalCount : 0;
            bool heavy = pct >= 35;

            var headline = new Label
            {
                Text = removed == 0
                    ? "제거된 프레임이 없습니다.\n이미 깨끗한 데이터이거나, 강도를 「강함」으로 올려 보세요."
                    : $"{result.OriginalCount:N0} → {result.Frames.Count:N0} 프레임\n{removed:N0}개 제거 ({pct:F1}%)",
                Location = new Point(16, 12),
                Size = new Size(428, 48),
                Font = new Font("맑은 고딕", 11F, FontStyle.Bold),
                ForeColor = heavy ? Accent : Ok
            };

            var body = new Label
            {
                Location = new Point(16, 68),
                Size = new Size(428, 120),
                ForeColor = TextLight,
                Text = BuildBreakdown(result)
            };

            var images = new Label
            {
                Location = new Point(16, 192),
                Size = new Size(428, 44),
                ForeColor = TextMuted,
                Text = $"이미지\n  {imageBefore}\n  → {imageAfter}"
            };

            var trainingNote = new Label
            {
                Location = new Point(16, 242),
                Size = new Size(428, 56),
                ForeColor = heavy ? Accent : TextMuted,
                Text = BuildTrainingNote(pct, result.RemovedDuplicate)
            };

            var next = new Label
            {
                Location = new Point(16, 304),
                Size = new Size(428, 36),
                ForeColor = Accent,
                Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold),
                Text = "★ 다음: 「정제 데이터 연동」 → 「학습 시작」"
            };

            var ok = new Button
            {
                Text = "확인",
                DialogResult = DialogResult.OK,
                Location = new Point(364, 346),
                Size = new Size(80, 32)
            };
            AcceptButton = ok;

            Controls.AddRange(new Control[] { headline, body, images, trainingNote, next, ok });
        }

        private static string BuildTrainingNote(double removedPct, int removedDup)
        {
            if (removedPct >= 50)
            {
                return "학습 참고: 데이터가 절반 이하로 줄었습니다.\n" +
                       "주행 프레임까지 지워졌다면 교차 테스트 예측이 멈춘 것처럼 보일 수 있습니다.\n" +
                       "Undo/복구 후 「권장」(정지 구간만) 프리셋을 사용하세요.";
            }

            if (removedPct >= 20)
            {
                return "학습 참고: 정제 후 「정제 데이터 연동」 → 학습 → 「교차 테스트」로\n" +
                       "예측 화살표(노랑)가 기록(주황)을 따라가는지 확인하세요.";
            }

            return "학습 참고: 주행 프레임은 유지되었을 가능성이 높습니다.\n" +
                   "학습 후 교차 테스트로 예측 화살표 움직임을 확인하세요.";
        }

        private static string BuildBreakdown(FrameRefinementFilter.Result r)
        {
            var lines = new List<string> { "무엇이 제거됐나요?", "" };
            AppendLine(lines, "정지·중복", r.RemovedStationaryDuplicate, "정지 상태, 같은 라벨");
            AppendLine(lines, "화면·라벨 중복", r.RemovedVisualDuplicate,
                "화면이 거의 같고 라벨도 같음 (스마트 정제)");
            AppendLine(lines, "정지 노이즈", r.RemovedSpike, "정지 상태 센서 글리치");
            AppendLine(lines, "센서 오류", r.RemovedOutOfRange, "비정상 조향·속도");
            if (r.RemovedWrongMode > 0)
                AppendLine(lines, "user 외 모드", r.RemovedWrongMode, "user가 아닌 주행 모드");
            return string.Join(Environment.NewLine, lines);
        }

        private static void AppendLine(List<string> lines, string title, int count, string explain)
        {
            lines.Add(count > 0
                ? $"  · {title}  {count:N0}개  — {explain}"
                : $"  · {title}  없음");
        }
    }
}
