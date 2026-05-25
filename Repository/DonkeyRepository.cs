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
        private DonkeyRepository()
        {
            _frames = new List<Frame>();
        }

        public void SetFrames(List<Frame> frames)
        {
            _frames = frames;
        }
        public List<Frame> GetFrames()
        {
            return _frames;
        }
    }
}
