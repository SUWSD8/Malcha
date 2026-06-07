using Malcha.UI;

namespace Malcha.View
{
    // [View 계약] 카탈로그 편집 Passive View — UI 표시·입력만
    internal interface ICatalogView
    {
        IWin32Window Owner { get; }

        void SetStatusText(string text);
        void UpdateCatalogPathFromSession();
        void SetCatalogBusy(bool busy);
        void EnsureVisible();

        DialogResult ShowMessage(string text, string caption,
            MessageBoxButtons buttons = MessageBoxButtons.OK,
            MessageBoxIcon icon = MessageBoxIcon.Information);

        void RequestRefreshFrameList();
        Task CompleteCatalogLoadAsync();
        void RequestResetAllUi();
        void RequestClearPlayback();
        void RequestClearImageCache();
        void RequestRefreshSelectionUi();
        void RequestShowFrame(int index);
        void RequestRefreshFrameListDuringPlayback(int playheadIndex);
        void RequestStopPlayback();
        void RequestClearCrossTestUi();
        void OnFramesRemoved(int start, int count);
        void ResetChartHighlight();

        ProgressDialog ShowProgress(string title);
        void CloseProgress(ProgressDialog? dialog);
    }
}
