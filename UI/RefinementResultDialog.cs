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
            ClientSize = new Size(460, 340);
            BackColor = Bg;
            ForeColor = TextLight;
            Font = new Font("맑은 고딕", 9.75F);

            int removed = result.RemovedTotal;
            double pct = result.OriginalCount > 0 ? 100.0 * removed / result.OriginalCount : 0;
            bool heavy = pct >= 50;

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

            var next = new Label
            {
                Location = new Point(16, 248),
                Size = new Size(428, 40),
                ForeColor = Accent,
                Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold),
                Text = "★ 다음 단계: 「정제 데이터 연동」 → 「학습 시작」"
            };

            var ok = new Button
            {
                Text = "확인",
                DialogResult = DialogResult.OK,
                Location = new Point(364, 296),
                Size = new Size(80, 32)
            };
            AcceptButton = ok;

            Controls.AddRange(new Control[] { headline, body, images, next, ok });
        }

        private static string BuildBreakdown(FrameRefinementFilter.Result r)
        {
            var lines = new List<string> { "무엇이 제거됐나요?", "" };
            AppendLine(lines, "정지·중복", r.RemovedDuplicate,
                "같은 조향·속도가 반복된 장면");
            AppendLine(lines, "순간 노이즈", r.RemovedSpike,
                "한 장만 튀었다 돌아온 경우");
            AppendLine(lines, "센서 오류", r.RemovedOutOfRange,
                "값이 비정상 범위");
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
