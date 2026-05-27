using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Malcha.Model;

namespace Malcha
{
    internal sealed class PlaybackController
    {
        private readonly ImageController _imageController;
        private readonly ChartController _chartController;
        private readonly PictureBox _pictureBox;
        private readonly TrackBar _timeline;
        private readonly Label _angleLabel;
        private readonly Label _throttleLabel;
        private readonly Label _modeLabel;
        private readonly Label _recordCountLabel;

        public PlaybackController(
            ImageController imageController,
            ChartController chartController,
            PictureBox pictureBox,
            TrackBar timeline,
            Label angleLabel,
            Label throttleLabel,
            Label modeLabel,
            Label recordCountLabel)
        {
            _imageController = imageController;
            _chartController = chartController;
            _pictureBox = pictureBox;
            _timeline = timeline;
            _angleLabel = angleLabel;
            _throttleLabel = throttleLabel;
            _modeLabel = modeLabel;
            _recordCountLabel = recordCountLabel;
        }

        public int ShowFrame(
            int index,
            IReadOnlyList<Frame> frames,
            IReadOnlyList<string> frameImagePaths,
            Control invokeTarget)
        {
            if (frames == null || frames.Count == 0) return 0;
            index = Math.Max(0, Math.Min(index, frames.Count - 1));

            if (invokeTarget.InvokeRequired)
            {
                invokeTarget.BeginInvoke(new Action(() =>
                    ShowFrame(index, frames, frameImagePaths, invokeTarget)));
                return index;
            }

            var f = frames[index];
            _angleLabel.Text = f.Angle.ToString("+#0.000;-#0.000;0.000");
            _throttleLabel.Text = f.Throttle.ToString("+#0.000;-#0.000;0.000");
            _modeLabel.Text = f.Mode ?? string.Empty;
            _recordCountLabel.Text = $"{index}/{Math.Max(0, frames.Count - 1)}";

            _timeline.Minimum = 0;
            _timeline.Maximum = Math.Max(0, frames.Count - 1);
            _timeline.Enabled = frames.Count > 0;
            if (_timeline.Value != index)
                _timeline.Value = index;

            _chartController.HighlightCurrent(index, frames);
            LoadFrameImage(index, f, frames, frameImagePaths);
            return index;
        }

        private void LoadFrameImage(int index, Frame frame, IReadOnlyList<Frame> frames, IReadOnlyList<string> frameImagePaths)
        {
            try
            {
                string resolved = null;
                if (frameImagePaths != null && frameImagePaths.Count > index)
                    resolved = frameImagePaths[index];

                Image img = null;
                if (!string.IsNullOrEmpty(resolved) && File.Exists(resolved))
                {
                    if (!_imageController.TryGet(index, out img))
                    {
                        var target = _pictureBox.ClientSize;
                        if (target.Width <= 0 || target.Height <= 0) target = new Size(320, 240);
                        try
                        {
                            img = _imageController.LoadAndCompose(resolved, frame, target);
                            if (img != null)
                                _imageController.AddToCache(index, img);
                        }
                        catch { }
                    }
                }

                var old = _pictureBox.Image;
                _pictureBox.Image = img;
                if (old != null && !_imageController.IsImageCached(old))
                {
                    try { old.Dispose(); } catch { }
                }

                _pictureBox.Invalidate();
                PreloadNext(index + 1, frames, frameImagePaths);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ShowFrame image error: {ex.Message}");
            }
        }

        private void PreloadNext(int nextIndex, IReadOnlyList<Frame> frames, IReadOnlyList<string> frameImagePaths)
        {
            if (frameImagePaths == null || nextIndex >= frameImagePaths.Count) return;
            var nextPath = frameImagePaths[nextIndex];
            if (string.IsNullOrEmpty(nextPath) || !File.Exists(nextPath)) return;
            if (_imageController.TryGet(nextIndex, out _)) return;

            Task.Run(() =>
            {
                try
                {
                    var target = _pictureBox.ClientSize;
                    if (target.Width <= 0 || target.Height <= 0) target = new Size(320, 240);
                    var composed = _imageController.LoadAndCompose(nextPath, frames[nextIndex], target);
                    if (composed != null)
                        _imageController.AddToCache(nextIndex, composed);
                }
                catch { }
            });
        }

        public void ClearDisplay()
        {
            _timeline.Enabled = false;
            _pictureBox.Image?.Dispose();
            _pictureBox.Image = null;
            _angleLabel.Text = string.Empty;
            _throttleLabel.Text = string.Empty;
            _modeLabel.Text = string.Empty;
            _recordCountLabel.Text = "0";
        }
    }
}
