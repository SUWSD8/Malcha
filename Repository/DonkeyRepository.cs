using Malcha.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Malcha.Repository
{
    internal class DonkeyRepository
    {
        private static readonly DonkeyRepository _instance = new DonkeyRepository();

        public static DonkeyRepository Instance
        {
            get { return _instance; }
        }

        private List<Frame> _frames;
        private List<TrainedModelInfo> _trainedModels;
        private DonkeyRepository()
        {
            _frames = new List<Frame>();
            _trainedModels = new List<TrainedModelInfo>();
        }

        public void SetFrames(List<Frame> frames)
        {
            _frames = frames;
        }
        public List<Frame> GetFrames()
        {
            return _frames;
        }

        public void SetTrainedModels(List<TrainedModelInfo> models)
        {
            _trainedModels = models;
        }
        public List<TrainedModelInfo> GetAllTrainedModels()
        {
            return _trainedModels;
        }
        public void AddTrainedModel(TrainedModelInfo model)
        {
            _trainedModels.Add(model);
        }
        public TrainedModelInfo FindByName(string name)
        {
            
            var model = _trainedModels.Find(m => m.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if(model == null)
                throw new Exception($"Model with name '{name}' not found.");
            return model;
        }

        public void UpdateModelComment(string modelName, string newComment)
        {
            var model = FindByName(modelName);
            model.Comment = newComment;
        }

        public void DeleteModel(string modelName)
        {
            var model = FindByName(modelName);
            _trainedModels.Remove(model);
        } 
    }
}
