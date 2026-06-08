using Malcha.Model;

namespace Malcha.View
{
    // [View 계약] panel6 학습 Passive View — UI 표시·입력만
    internal interface ITrainingView
    {
        IWin32Window Owner { get; }
        string SelectedModelName { get; }
        TrainingResult? SelectedModel { get; }
        int SelectedModelNumber { get; }
        string ModelComment { get; }

        event EventHandler? ViewLoaded;
        event EventHandler? RunTrainingRequested;
        event EventHandler? StopTrainingRequested;
        event EventHandler? UpdateCommentRequested;
        event EventHandler? DeleteModelRequested;
        event EventHandler? RestoreModelFromBackupRequested;
        event EventHandler? ModelSelectionChanged;

        void SetTrainingButtonEnabled(bool enabled);
        void SetTrainingButtonText(string text);
        void SetForceStopTrainingEnabled(bool enabled);
        void ClearLog();
        void BindModelList(IReadOnlyList<TrainingResult> models, int? selectModelNumber = null);
        void AppendLog(string message);
        void BindEpochScores(string modelName, IReadOnlyList<TrainingEpoch> epochs, int? plannedTotal = null);
        void ClearEpochScores(string modelName);
        void ShowInfo(string title, string message);
        void ShowError(string message);
        bool ConfirmDeleteModel(TrainingResult model, string timeLabel);
        bool ConfirmTrainWithStaleSync(string message);
        bool ConfirmRestoreFromBackup(string modelName);

        // mycar 폴더 선택 (취소 시 null)
        string? PromptMycarFolder(string? suggestedUncPath);
    }
}
