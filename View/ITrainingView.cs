using Malcha.Model;

namespace Malcha.View
{
    // [View 계약] panel6 학습 Passive View — UI 표시·입력만
    internal interface ITrainingView
    {
        string SelectedModelName { get; }
        string ModelComment { get; }

        event EventHandler? ViewLoaded;
        event EventHandler? RunTrainingRequested;
        event EventHandler? UpdateCommentRequested;
        event EventHandler? DeleteModelRequested;
        event EventHandler? ShowChartRequested;

        void SetTrainingButtonEnabled(bool enabled);
        void SetTrainingButtonText(string text);
        void BindModelList(IReadOnlyList<TrainingResult> models);
        void AppendLog(string message);
        void BindScoreSummary(TrainingSummary summary);
        void ShowInfo(string title, string message);
        void ShowError(string message);
    }
}
