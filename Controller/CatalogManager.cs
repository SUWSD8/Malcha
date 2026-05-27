using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Malcha.Data;
using Malcha.Model;

namespace Malcha
{
    // Responsible for catalog-related utilities: loading via DataManager,
    // resolving image paths, populating UI lists, and preloading images using
    // a caller-provided image factory (so Form1 can supply Compose/scale logic).
    internal class CatalogManager
    {
        internal Task<Dictionary<string, List<Frame>>> LoadCatalogsAsync(string directory)
        {
            return DataManager.Instance.LoadCatalogsAsync(directory);
        }

        internal Task<List<Frame>> LoadCatalogFileAsync(string catalogFilePath)
        {
            return DataManager.Instance.LoadCatalogFileAsync(catalogFilePath);
        }

        internal List<string> ResolveFrameImagePaths(string catalogPath, List<Frame> frames)
        {
            var result = new List<string>(frames?.Count ?? 0);
            var baseDir = Path.GetDirectoryName(catalogPath) ?? string.Empty;

            for (int i = 0; frames != null && i < frames.Count; i++)
            {
                var f = frames[i];
                string img = f.ImagePath ?? string.Empty;
                string resolved = null;

                if (!string.IsNullOrEmpty(img))
                {
                    if (Path.IsPathRooted(img) && File.Exists(img))
                    {
                        resolved = img;
                    }
                    else
                    {
                        var candidate = Path.Combine(baseDir, img);
                        if (File.Exists(candidate)) resolved = candidate;
                        else
                        {
                            var candidates = new[] { "images", "cam", "imgs", "image" };
                            foreach (var sub in candidates)
                            {
                                var c2 = Path.Combine(baseDir, sub, img);
                                if (File.Exists(c2))
                                {
                                    resolved = c2;
                                    break;
                                }
                            }

                            if (resolved == null)
                            {
                                try
                                {
                                    var files = Directory.GetFiles(baseDir);
                                    var nameOnly = Path.GetFileName(img);
                                    var found = files.FirstOrDefault(p => string.Equals(Path.GetFileName(p), nameOnly, StringComparison.OrdinalIgnoreCase));
                                    if (found != null) resolved = found;
                                }
                                catch { }
                            }
                        }
                    }
                }

                result.Add(resolved);
            }

            return result;
        }

        internal void PopulateListBoxWithFrames(ListBox listBox, List<Frame> frames, List<string> frameImagePaths)
        {
            if (listBox == null) return;
            listBox.Items.Clear();
            for (int i = 0; frames != null && i < frames.Count; i++)
            {
                var hasImg = frameImagePaths != null && frameImagePaths.Count > i && !string.IsNullOrEmpty(frameImagePaths[i]);
                listBox.Items.Add(hasImg ? $"frame_{i} (img)" : $"frame_{i} (no image)");
            }
        }

        // Preload a small number of images in background. The caller provides an image factory
        // that takes (path, index) and returns a ready-to-cache Image (may be composed/annotated).
        internal async Task PreloadImagesAsync(List<string> frameImagePaths, List<Frame> frames, int preloadCount, Func<string, int, Image> imageFactory, Action<int, Image> addToCache)
        {
            if (frameImagePaths == null || imageFactory == null || addToCache == null) return;
            int initialPreload = Math.Min(preloadCount, frameImagePaths.Count);
            var tasks = new List<Task>();
            for (int i = 0; i < initialPreload; i++)
            {
                var p = frameImagePaths[i];
                if (!string.IsNullOrEmpty(p) && File.Exists(p))
                {
                    int idx = i;
                    tasks.Add(Task.Run(() =>
                    {
                        try
                        {
                            var img = imageFactory(p, idx);
                            if (img != null)
                            {
                                addToCache(idx, img);
                            }
                        }
                        catch { }
                    }));
                }
            }

            try { await Task.WhenAll(tasks); } catch { }
        }
    }
}
