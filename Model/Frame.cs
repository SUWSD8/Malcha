using System;
using System.Collections.Generic;
using System.Text;

namespace Malcha.Model
{
    internal class Frame
    {
        /*public Frame()
        {
           파싱테스트용
        }*/
        private string id { get; set; }
        private long timestamp { get; set; }
        private string imagePath { get; set; }
        private double angle { get; set; }
        private string model { get; set; }
        private double throttle { get; set; }

        public Frame(string id, long timestamp, string imagePath, double angle, string model, double throttle)
        {
            this.id = id;
            this.timestamp = timestamp;
            this.imagePath = imagePath;
            this.angle = angle;
            this.model = model;
            this.throttle = throttle;
        }

       
    }
}
