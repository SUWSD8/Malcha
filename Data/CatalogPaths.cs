using System;
using System.IO;
using System.Linq;

namespace Malcha.Data
{
    /// <summary>
    /// 작업용 .catalog 와 백업(.catalog.bak, backups 폴더) 경로 구분.
    /// </summary>
    internal static class CatalogPaths
    {
        public const string BackupsFolderName = "backups";

        /// <summary>정제·편집 대상 작업용 카탈로그 (manifest·bak 제외).</summary>
        public static bool IsWorkingCatalog(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;
            var name = Path.GetFileName(path);
            if (name.EndsWith(".catalog.bak", StringComparison.OrdinalIgnoreCase)) return false;
            if (name.EndsWith(".catalog_manifest", StringComparison.OrdinalIgnoreCase)) return false;
            return name.EndsWith(".catalog", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsBackupCatalog(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;
            return Path.GetFileName(path).EndsWith(".catalog.bak", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsUnderBackupsFolder(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;
            var dir = Path.GetDirectoryName(path) ?? string.Empty;
            return dir.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .Any(p => string.Equals(p, BackupsFolderName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>정제 전 스냅샷을 backups/{이름}_{시각}.catalog 에 저장.</summary>
        public static string CreateTimestampedBackup(string catalogPath)
        {
            if (string.IsNullOrWhiteSpace(catalogPath) || !File.Exists(catalogPath))
                return string.Empty;

            var dir = Path.GetDirectoryName(catalogPath) ?? Environment.CurrentDirectory;
            var backupDir = Path.Combine(dir, BackupsFolderName);
            Directory.CreateDirectory(backupDir);

            var baseName = Path.GetFileNameWithoutExtension(catalogPath);
            if (baseName.EndsWith(".catalog", StringComparison.OrdinalIgnoreCase))
                baseName = Path.GetFileNameWithoutExtension(baseName);

            var stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var backupPath = Path.Combine(backupDir, $"{baseName}_{stamp}.catalog");

            File.Copy(catalogPath, backupPath, overwrite: false);
            return backupPath;
        }

        public static string GetDisplayLabel(string catalogPath)
        {
            if (string.IsNullOrWhiteSpace(catalogPath)) return string.Empty;
            if (IsBackupCatalog(catalogPath) || IsUnderBackupsFolder(catalogPath))
                return "[백업]";
            return "[작업]";
        }

        /// <summary>백업·작업 경로에서 편집 대상 작업용 .catalog 절대 경로를 구합니다.</summary>
        public static string ResolveWorkingCatalogPath(string catalogPath)
        {
            if (string.IsNullOrWhiteSpace(catalogPath))
                return string.Empty;

            if (IsWorkingCatalog(catalogPath))
                return Path.GetFullPath(catalogPath);

            var dir = Path.GetDirectoryName(catalogPath) ?? Environment.CurrentDirectory;
            var name = Path.GetFileName(catalogPath);

            if (name.EndsWith(".catalog.bak", StringComparison.OrdinalIgnoreCase))
            {
                var workingName = name.Substring(0, name.Length - 4);
                return Path.GetFullPath(Path.Combine(dir, workingName));
            }

            if (IsUnderBackupsFolder(catalogPath)
                && TryGetWorkingNameFromBackupFile(name, out var workingFileName))
            {
                var candidate = Path.GetFullPath(Path.Combine(dir, "..", workingFileName));
                if (File.Exists(candidate))
                    return candidate;
            }

            return Path.GetFullPath(catalogPath);
        }

        /// <summary>catalog_0_20260525_143052.catalog → catalog_0.catalog</summary>
        private static bool TryGetWorkingNameFromBackupFile(string fileName, out string workingFileName)
        {
            workingFileName = string.Empty;
            if (!fileName.EndsWith(".catalog", StringComparison.OrdinalIgnoreCase))
                return false;

            var stem = Path.GetFileNameWithoutExtension(fileName);
            var parts = stem.Split('_');
            if (parts.Length < 3)
                return false;

            var datePart = parts[^2];
            var timePart = parts[^1];
            if (datePart.Length != 8 || timePart.Length != 6
                || !datePart.All(char.IsDigit)
                || !timePart.All(char.IsDigit))
                return false;

            var baseName = string.Join("_", parts.Take(parts.Length - 2));
            if (string.IsNullOrEmpty(baseName))
                return false;

            workingFileName = baseName + ".catalog";
            return true;
        }

        /// <summary>작업용 카탈로그에 대응하는 가장 최근 백업 파일 경로 (없으면 null).</summary>
        public static string FindLatestBackupPath(string workingCatalogPath)
        {
            if (string.IsNullOrWhiteSpace(workingCatalogPath))
                return string.Empty;

            workingCatalogPath = ResolveWorkingCatalogPath(workingCatalogPath);
            var dir = Path.GetDirectoryName(workingCatalogPath) ?? Environment.CurrentDirectory;
            var baseName = Path.GetFileNameWithoutExtension(workingCatalogPath);

            var candidates = new List<string>();

            var legacyBak = workingCatalogPath + ".bak";
            if (File.Exists(legacyBak))
                candidates.Add(legacyBak);

            var backupDir = Path.Combine(dir, BackupsFolderName);
            if (Directory.Exists(backupDir))
            {
                candidates.AddRange(
                    Directory.GetFiles(backupDir, $"{baseName}_*.catalog", SearchOption.TopDirectoryOnly));
            }

            return candidates
                .Where(File.Exists)
                .OrderByDescending(p => File.GetLastWriteTimeUtc(p))
                .FirstOrDefault() ?? string.Empty;
        }
    }
}
