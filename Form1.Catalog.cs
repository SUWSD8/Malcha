using Malcha.Controller;
using Malcha.Data;
using Malcha.Service;
using Malcha.View;
using Malcha.UI;

namespace Malcha
{
    // Form1 — [View] 카탈로그 편집 Passive View (ICatalogView 구현)
    public partial class Form1 : ICatalogView
    {
        private CatalogEditorController? _catalogController;

        // CatalogEditorController 생성 및 연결
        private void InitializeCatalogController()
        {
            _catalogController = new CatalogEditorController(this, _session, _selection);
        }

        IWin32Window ICatalogView.Owner => this;
        void ICatalogView.SetStatusText(string text) => toolStripStatusLabel1.Text = text;
        void ICatalogView.UpdateCatalogPathFromSession() => UpdateCatalogPathDisplay();

        // 카탈로그 작업 중 버튼·커서 상태 변경
        void ICatalogView.SetCatalogBusy(bool busy)
        {
            UseWaitCursor = busy;
            btnApplyFilter.Enabled = btnRecover.Enabled = btnDeleteSelection.Enabled =
            btnSelectData.Enabled = btnPlayPause.Enabled = btnRefresh.Enabled =
            btnChangeCleanData.Enabled = btnRunTraining.Enabled = btnCrossTest.Enabled = !busy;
        }

        // 최소화 상태면 창 복원·활성화
        void ICatalogView.EnsureVisible()
        {
            if (WindowState == FormWindowState.Minimized) WindowState = FormWindowState.Normal;
            ShowInTaskbar = true;
            Activate();
            BringToFront();
        }

        // MessageBox 표시 (창 활성화 후)
        DialogResult ICatalogView.ShowMessage(string text, string caption,
            MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            ((ICatalogView)this).EnsureVisible();
            return MessageBox.Show(this, text, caption, buttons, icon);
        }

        void ICatalogView.RequestRefreshFrameList() => RefreshFrameListUi();

        // 카탈로그 로드 완료 후 UI 일괄 갱신·이미지 프리로드
        async Task ICatalogView.CompleteCatalogLoadAsync()
        {
            _catalog.PopulateListBox(lstDataList, _session.CurrentFrames, _session.FrameImagePaths);
            RefreshDeletedListUi();
            RefreshChartFromFrames();
            UpdateCatalogPathDisplay();
            ShowFrame(0);
            if (lstDataList.SelectedIndex != 0) lstDataList.SelectedIndex = 0;
            lstDataList.Invalidate();
            InvalidateTimelineMarkers();
            picVideoScreen.Invalidate();
            RefreshPlaybackSpeedIndicators();
            await _display.PreloadAsync(_session.FrameImagePaths, _session.CurrentFrames, 5);
        }

        void ICatalogView.RequestResetAllUi() => ResetAllUi();
        void ICatalogView.RequestClearPlayback() => ClearPlayback();
        void ICatalogView.RequestClearImageCache() => _display.ClearCache();
        void ICatalogView.RequestRefreshSelectionUi() => RefreshSelectionUi();
        void ICatalogView.RequestShowFrame(int index) => ShowFrame(index);
        void ICatalogView.RequestRefreshFrameListDuringPlayback(int playheadIndex)
        {
            _catalog.PopulateListBox(lstDataList, _session.CurrentFrames, _session.FrameImagePaths);
            RefreshDeletedListUi();
            RefreshChartFromFrames();
            UpdateCatalogPathDisplay();
            _session.ClearCrossTest();

            if (_session.CurrentFrames.Count == 0)
            {
                StopPlayback();
                ClearPlayback();
                return;
            }

            playheadIndex = Math.Clamp(playheadIndex, 0, _session.CurrentFrames.Count - 1);
            _session.CurrentIndex = playheadIndex;
            _display.ShowFrame(playheadIndex, _session.CurrentFrames, _session.FrameImagePaths, this);
            SyncTimeline(playheadIndex);
            InvalidateTimelineMarkers();
            lstDataList.Invalidate();
            picVideoScreen.Invalidate();
        }

        void ICatalogView.RequestStopPlayback() => StopPlayback();

        void ICatalogView.RequestClearCrossTestUi()
        {
            HideModelLabels();
            picVideoScreen.Invalidate();
        }
        void ICatalogView.OnFramesRemoved(int start, int count) => _display.DeleteCacheRange(start, count);
        void ICatalogView.ResetChartHighlight() => _display.ResetChartHighlight();

        // 진행 대화상자 표시
        ProgressDialog ICatalogView.ShowProgress(string title)
        {
            var p = new ProgressDialog(title);
            p.ShowFor(this);
            p.Refresh();
            return p;
        }

        // 진행 대화상자 닫기
        void ICatalogView.CloseProgress(ProgressDialog? dialog)
        {
            dialog?.CloseSafely();
        }

        // txtFilePath에 카탈로그 경로·프레임 수 표시
        private void UpdateCatalogPathDisplay()
        {
            if (string.IsNullOrEmpty(_session.CurrentCatalogPath))
            { txtFilePath.Text = string.Empty; return; }
            var label = CatalogPaths.GetDisplayLabel(_session.CurrentCatalogPath);
            txtFilePath.Text = $"{label} {Path.GetFileName(_session.CurrentCatalogPath)}  ({_session.CurrentFrames.Count:N0} 프레임)  —  {_session.CurrentCatalogPath}";
        }

        // 삭제 목록 ListBox 갱신
        private void RefreshDeletedListUi()
        {
            _deletedSelection.Clear();
            _catalog.PopulateDeletedListBox(lstDeleted, _session.DeletedEntries);
            lstDeleted.Invalidate();
        }

        // 프레임 목록·차트·현재 프레임 UI 갱신
        private void RefreshFrameListUi()
        {
            _catalog.PopulateListBox(lstDataList, _session.CurrentFrames, _session.FrameImagePaths);
            RefreshDeletedListUi();
            RefreshChartFromFrames();
            UpdateCatalogPathDisplay();
            if (_session.CurrentFrames.Count > 0)
            {
                _session.CurrentIndex = Math.Clamp(_session.CurrentIndex, 0, _session.CurrentFrames.Count - 1);
                lstDataList.SelectedIndex = _session.CurrentIndex;
                ShowFrame(_session.CurrentIndex);
                RefreshPlaybackSpeedIndicators();
            }
            else ClearPlayback();
            lstDataList.Invalidate();
            InvalidateTimelineMarkers();
        }
    }
}
