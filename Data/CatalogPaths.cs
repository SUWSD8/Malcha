using System;
using System.IO;
using System.Linq;

namespace Malcha.Data
{
    // 작업용 .catalog 와 백업(.catalog.bak, backups 폴더) 경로 구분
    internal static class CatalogPaths
    {
        public const string BackupsFolderName = "backups";

        // 정제·편집 대상 작업용 카탈로그 여부 (manifest·bak 제외)
        public static bool IsWorkingCatalog(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;
            var name = Path.GetFileName(path);
            if (name.EndsWith(".catalog.bak", StringComparison.OrdinalIgnoreCase)) return false;
            if (name.EndsWith(".catalog_manifest", StringComparison.OrdinalIgnoreCase)) return false;
            return name.EndsWith(".catalog", StringComparison.OrdinalIgnoreCase);
        }

        // .catalog.bak 백업 파일 여부
        public static bool IsBackupCatalog(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;
            return Path.GetFileName(path).EndsWith(".catalog.bak", StringComparison.OrdinalIgnoreCase);
        }

        // backups 폴더 하위 경로 여부
        public static bool IsUnderBackupsFolder(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;
            var dir = Path.GetDirectoryName(path) ?? string.Empty;
            return dir.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .Any(p => string.Equals(p, BackupsFolderName, StringComparison.OrdinalIgnoreCase));
        }

        // 정제 전 스냅샷을 backups/{이름}_{시각}.catalog 에 저장
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

        // UI 표시용 [작업]/[백업] 라벨
        public static string GetDisplayLabel(string catalogPath)
        {
            if (string.IsNullOrWhiteSpace(catalogPath)) return string.Empty;
            if (IsBackupCatalog(catalogPath) || IsUnderBackupsFolder(catalogPath))
                return "[백업]";
            return "[작업]";
        }

        // 백업·작업 경로에서 편집 대상 작업용 .catalog 절대 경로 반환
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

        // catalog_0_20260525_143052.catalog → catalog_0.catalog 변환
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

        // 목록 전체를 최신순으로 가져옵니다.
        public static List<string> GetAllBackupPaths(string workingCatalogPath)
        {
            var candidates = new List<string>();
            if (string.IsNullOrWhiteSpace(workingCatalogPath))
                return candidates;

            workingCatalogPath = ResolveWorkingCatalogPath(workingCatalogPath);
            var dir = Path.GetDirectoryName(workingCatalogPath) ?? Environment.CurrentDirectory;
            var baseName = Path.GetFileNameWithoutExtension(workingCatalogPath);

            var legacyBak = workingCatalogPath + ".bak";
            if (File.Exists(legacyBak))
                candidates.Add(legacyBak);

            var backupDir = Path.Combine(dir, BackupsFolderName);
            if (Directory.Exists(backupDir))
            {
                // 동일한 baseName을 가진 모든 백업 카탈로그 파일을 검색
                candidates.AddRange(Directory.GetFiles(backupDir, $"{baseName}_*.catalog", SearchOption.TopDirectoryOnly));
            }

            // 파일이 존재하는지 확인하고 최신 수정일순(내림차순)으로 정렬 후 리스트로 반환
            return candidates
                .Where(File.Exists)
                .OrderByDescending(p => File.GetLastWriteTimeUtc(p))
                .ToList();
        }

        // 2. 기존 로직(FindLatestBackupPath)은 위의 새 메서드를 재사용하도록 매우 짧게 리팩토링합니다.
        public static string FindLatestBackupPath(string workingCatalogPath)
        {
            return GetAllBackupPaths(workingCatalogPath).FirstOrDefault() ?? string.Empty;
        }
    }
}
