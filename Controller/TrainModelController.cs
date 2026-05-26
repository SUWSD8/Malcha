using Malcha.Data;
using Malcha.Model;
using Malcha.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace Malcha.Controller
{
    internal class TrainModelController
    {
        private static readonly TrainModelController _instance = new TrainModelController();

        public static TrainModelController Instance { get { return _instance; } }

        private TrainModelController() { }
        // 모델 훈련 결과 분석 실행 메서드

        public async Task<bool> RunTraining(string path, string targetModelName)
        {
            try
            {
                TrainedModelInfo model = await DataManager.Instance.LoadTrainingHistoryAsync(path, targetModelName);
                if (model != null)
                {
                    DonkeyRepository.Instance.AddTrainedModel(model);
                    return true;
                }
                else return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
        // 모든 훈련된 모델 정보 가져오기 메서드
        public List<TrainedModelInfo> GetAllTrainedModels()
        {
            return DonkeyRepository.Instance.GetAllTrainedModels();
        }
        // 차트 데이터 가져오기 메서드



    }
}
