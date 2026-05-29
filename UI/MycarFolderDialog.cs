namespace Malcha.UI
{
    // mycar(WSL) 폴더 선택 대화상자
    internal static class MycarFolderDialog
    {
        public static string? Show(IWin32Window owner, string? suggestedUncPath = null)
        {
            using var dlg = new FolderBrowserDialog
            {
                Description = "train.py가 있는 mycar 폴더를 선택하세요\n(\\\\wsl.localhost\\배포판\\home\\...\\mycar)",
                UseDescriptionForTitle = true,
            };
            if (!string.IsNullOrEmpty(suggestedUncPath) && Directory.Exists(suggestedUncPath))
                dlg.SelectedPath = suggestedUncPath;

            return dlg.ShowDialog(owner) == DialogResult.OK ? dlg.SelectedPath : null;
        }
    }
}
