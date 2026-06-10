using System;
using System.Collections.Generic;
using System.Text;

namespace Malcha.Data
{
    internal class SaveFileManager
    {
        private static SaveFileManager _Instance = new SaveFileManager();

        public static SaveFileManager Instance { 
            get {
                if(_Instance == null)
                    _Instance = new SaveFileManager();
                return _Instance; 
            }
        }
        // 원본 데이터가 있는 폴더 (.catalog, img 폴더가 있는 경로)
        private string _baseDataDirectory;
        // save 폴더 경로
        public string SaveFolderPath
        {
            get
            {
                // 방어적 프로그래밍: 폴더를 열기도 전에 저장 기능에 접근하는 것 차단
                if (string.IsNullOrEmpty(_baseDataDirectory))
                    throw new InvalidOperationException("먼저 주행 데이터 폴더를 로드해야 합니다.");

                return Path.Combine(_baseDataDirectory, "save");
            }
        }
        private SaveFileManager()
        {
           
        }
        // 데이터 폴더 설정 (Create)
        public void SetDataDirectory(string targetDirectoryPath)
        {
            if (!Directory.Exists(targetDirectoryPath))
                throw new DirectoryNotFoundException("지정된 데이터 폴더를 찾을 수 없습니다.");

            _baseDataDirectory = targetDirectoryPath;
            EnsureDirectoryExists(); // 경로가 설정되면 바로 하위에 save 폴더 생성
        }
        // 폴더 없으면 생성
        private void EnsureDirectoryExists()
        {
            if (!Directory.Exists(SaveFolderPath))
            {
                Directory.CreateDirectory(SaveFolderPath);
            }
        }
        // 파일 목록 불러오기 (Read)
        public List<FileInfo> GetSavedFilesList()
        {
            EnsureDirectoryExists();

            DirectoryInfo dirInfo = new DirectoryInfo(SaveFolderPath);
            // .catalog, .json 등 확장자만 필터링 가능 (여기서는 모든 파일)
            FileInfo[] files = dirInfo.GetFiles("*.*");

            return new List<FileInfo>(files);
        }
        // 파일 이름 변경 (Update)
        public bool RenameFile(string oldFileName, string newFileName)
        {
            try
            {
                string oldFilePath = Path.Combine(SaveFolderPath, oldFileName);
                string newFilePath = Path.Combine(SaveFolderPath, newFileName);

                // 예외 처리 1: 원본 파일이 없는 경우
                if (!File.Exists(oldFilePath))
                    throw new FileNotFoundException("변경할 파일을 찾을 수 없습니다.");

                // 예외 처리 2: 바꿀 이름과 똑같은 파일이 이미 존재하는 경우
                if (File.Exists(newFilePath))
                    throw new IOException("이미 같은 이름의 파일이 존재합니다.");

                // 실제 이름 변경 (C#에서는 Move를 사용하여 이름을 변경함)
                File.Move(oldFilePath, newFilePath);
                return true;
            }
            catch (Exception ex)
            {
                // 실제 프로젝트에서는 로깅(Logging) 처리를 합니다.
                Console.WriteLine($"이름 변경 실패: {ex.Message}");
                throw; // UI 단(Controller)으로 에러를 던져서 UI에서 메세지창을 띄우게 함
            }
        }
        // 4. 파일 삭제 (Delete)
        public bool DeleteFile(string fileName)
        {
            try
            {
                string filePath = Path.Combine(SaveFolderPath, fileName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"파일 삭제 실패: {ex.Message}");
                throw; // 파일이 열려있거나 권한이 없으면 에러를 위로 던짐
            }
        }

    }
}
