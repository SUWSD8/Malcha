using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
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

        public Frame ParseFrameJson(string jsonString)
        {
            try
            {
                // 문자열을 Frame 객체로 변환해서 리턴
                return JsonSerializer.Deserialize<Frame>(jsonString);
            }
            catch (JsonException)
            {
                // JSON 형식이 깨진 에러가 나면 null 리턴
                return null;
            }
        }
        private List<TrainedData> Train(){
            return new List<TrainedData>();
        }
    }
}
