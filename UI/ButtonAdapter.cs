using Malcha.Repository;
using Malcha.UI;

namespace Malcha.UI
{
    internal static class ButtonAdapter
    {
        public static async void RunCatalogAnalysis(Button btn)
        {
            using var dlg = new OpenFileDialog
            {
                Title = "catalog 파일 선택",
                Filter = "Catalog (catalog*)|catalog*|모든 파일 (*.*)|*.*"
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;
            try
            {
                btn.Enabled = false;
                var parsed = await CatalogRepository.Instance.LoadFramesAsync(dlg.FileName);
                CatalogRepository.Instance.SetFrames(parsed);
                MessageBox.Show(parsed.Count > 0 ? $"성공: {parsed.Count}개" : "프레임 0개", "테스트");
            }
            finally { btn.Enabled = true; }
        }
    }
}
