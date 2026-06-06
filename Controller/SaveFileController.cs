using Malcha.Data;
using Malcha.Model;
using Malcha.Service;
using System;
using System.Collections.Generic;
using System.Text;

namespace Malcha.Controller
{
    internal class SaveFileController
    {
        CatalogSession _session;
        CatalogEditorController _catalogController;
        CatalogService _catalog;
        private SaveFileManager _fileManager;

        public SaveFileController(CatalogSession session,CatalogEditorController catalogController)
        {
            _fileManager = SaveFileManager.Instance;
            _catalogController = catalogController;
            _session = session;
            _catalog = CatalogService.Instance;
        }
        // 1. 목록 새로고침 요청
        // UI(폼)에서 이 메서드를 호출하여 파일 목록을 가져간 뒤 ListView에 그립니다.
        public List<FileInfo> GetSaveFilesForUI()
        {
            try
            {
                return _fileManager.GetSavedFilesList();
            }
            catch (Exception ex)
            {
                // 아직 데이터 폴더를 열지 않은 경우 등의 에러 처리
                throw new Exception($"세이브 목록을 불러올 수 없습니다: {ex.Message}");
            }
        }
        // 2. 파일 로드 요청 (더블 클릭 시)
        public async Task<bool> RequestLoadSaveFile(string fileName)
        {
            // TODO: 기존 DataManager나 Session에 현재 작업 중인 내용이 있는지 확인 (상태 검증)
             //bool isDirty = _Session.Instance.IsDirty;
            // if (isDirty) return false; (또는 사용자에게 저장 여부 묻는 이벤트 발생)

            try
            {
                string targetPath = Path.Combine(_fileManager.SaveFolderPath, fileName);

                // 프레임 로드 및 세션 업데이트
                var frames = await DataManager.Instance.LoadCatalogFileAsync(targetPath);
                _session.CurrentFrames = frames;
                bool isSuccess = await _catalog.SaveCatalogAsync(_session.CurrentCatalogPath, _session.CurrentFrames);
                return true; // 로드 성공
            }
            catch (Exception ex)
            {
                throw new Exception($"파일을 불러오는 중 오류가 발생했습니다: {ex.Message}");
            }
        }
        // 3. 파일 이름 변경 요청 (우클릭 -> 이름 변경 또는 F2 키)
        public bool RequestRename(string oldName, string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
                throw new ArgumentException("새 파일 이름을 입력해야 합니다.");

            if (oldName.Equals(newName, StringComparison.OrdinalIgnoreCase))
                return false; // 이름이 바뀌지 않았으면 무시

            try
            {
                return _fileManager.RenameFile(oldName, newName);
            }
            catch (IOException)
            {
                // Model에서 올라온 시스템 에러를 UI 친화적인 메시지로 변환
                throw new Exception("이미 동일한 이름의 파일이 존재하거나 파일을 사용 중입니다.");
            }
            catch (Exception ex)
            {
                throw new Exception($"이름 변경 실패: {ex.Message}");
            }
        }
        // 4. 파일 삭제 요청 (우클릭 -> 삭제 또는 Delete 키)
        public bool RequestDelete(string fileName)
        {
            try
            {
                return _fileManager.DeleteFile(fileName);
            }
            catch (Exception ex)
            {
                throw new Exception($"파일 삭제 실패: {ex.Message}");
            }
        }

    }
}
