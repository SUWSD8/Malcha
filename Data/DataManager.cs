using System;
using System.Collections.Generic;
using System.Text;
using Malcha.Model;

namespace Malcha.Data
{
    internal class DataManager
    {
        private static readonly DataManager _instance = new DataManager();

        public static DataManager Instance
        {
            get { 
                return _instance;
            }
        }

        private DataManager()
        {
            // Private constructor to prevent instantiation
        }

        private List<Frame> ReadCatalog()
        {             // Implement logic to read the catalog and return a list of Frame objects
            return new List<Frame>();
        }

        private List<TrainedData> Train(){
            return new List<TrainedData>();
        }
    }
}
