using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MySkin_Alpha
{

    [DataContract]
    public class Nevus
    {
        
        public bool asymmetric { get; private set; }
        public bool unevenBorder { get; private set; }
        public bool bigDiameter { get; private set; }
        public bool unevenColor { get; private set; }
        public bool evolutes { get; set; }
        public bool stiches { get; set; }
        public bool lostHair { get; set; }
        public bool bleeds { get; set; }
        public bool blueParts { get; private set; }
        [DataMember]
        public string safe { get; private set; }
        [DataMember]
        public string name { get; private set; }
        [DataMember]
        public string description { get; private set; }
        [DataMember]
        public double area { get; private set; }
        [DataMember]
        public double assymmetryRate { get; private set; }
        [DataMember]
        public double colorVariation { get; private set; }
        [DataMember]
        public double borderVariation { get; private set; }
        [DataMember]
        public double blackness { get; private set; }
        [DataMember]
        public double blueness { get; private set; }
        [DataMember]
        public double redness { get; private set; }
        [DataMember]
        public string imagePath { get; private set; }
        [DataMember]
        public double risk { get; private set; }

        private static double total;
        private static double aW = 0.13;
        private static double bW = 0.15;
        private static double cW = 0.18;
        private static double dW = 0.12;
        private static double blueW = 0.2;
        private static double blackW = 0.1;
        private static double redW = 0.12;

        public static double aMax = 2.5;
        public static double bMax = 1600;
        public static double cMax = 1600;
        public static double dMax = 550;
        private static double blackMax = 33;
        private static double blueMax = 33;
        private static double redMax = 33;

        public int edgeCount { get; set; }

        public Nevus (string name, string description, double area, double borderVariation, double assymmetryRate, double colorVariation, double blackness, double blueness, double redness, string imagePath)
        {
            total = (aW * aMax) + (bW * bMax) + (cW * cMax) + (dW * dMax) + (blueW * blueMax) + (blackW * blackMax) + (redW * redMax);
            this.name = name;
            this.description = description;
            this.area = Math.Round(area, 2);
            this.colorVariation = Math.Round(colorVariation, 2);
            this.assymmetryRate = Math.Round(assymmetryRate, 2);
            this.borderVariation = Math.Round(borderVariation, 2);
            this.blackness = Math.Round(blackness,2);
            this.blueness = Math.Round(blueness);
            this.redness = Math.Round(redness,2);
            this.imagePath = imagePath;
            if (area > 36)
                bigDiameter = true;
            if (area > dMax)
                this.area = dMax;
            if (colorVariation > 100)
                unevenColor = true;
            if (colorVariation > cMax)
                this.colorVariation = cMax;
            if (borderVariation > 40)
                unevenBorder = true;
            if (borderVariation > bMax)
                this.borderVariation = bMax;
            if (assymmetryRate > 1.3)
                asymmetric = true;
            if (assymmetryRate > aMax)
                this.assymmetryRate = aMax;
            if(redness+blueness+blackness > 100)
            {
                if (blueness > 100 && redness != 0)
                    this.blueness = 100 - redness;
                else if (redness > 100 && blueness != 0)
                    this.redness = 100 - blueness;
                if (blueness > 100 && redness == 0)
                    this.blueness = 100;
                else if (redness > 100 && blueness == 0)
                    this.redness = 100;
            }

            if ((bigDiameter && unevenColor && unevenBorder && asymmetric) || (blueness > 0.01 && redness > 0.01 && blackness > 0.01) || (colorVariation>600 && borderVariation>40)|| (blueness > 0.1 && blackness > 0.1) || (redness > 0.1 && borderVariation > 40) || (bigDiameter && blackness > 0.1) || (bigDiameter && blackness > 0.1 && blueness > 0.1) || (bigDiameter && colorVariation > 300))
                safe = "Dangerous";
            else safe = "Safe";

            risk = (this.area * dW) + (this.colorVariation * cW) + (this.borderVariation * bW) + (this.assymmetryRate * aW) + (this.blueness * blueW) + (this.blackness * blackW) + (this.redness * redW);
            risk = (risk / total) * 100;
            if (risk > 100)
                risk = 100;

            if (risk < 10)
                safe = "Totally safe";
            else if (risk < 20)
                safe = "Safe";
            else if (risk < 50)
                safe = "Attention";
            else if (risk < 65)
                safe = "Caution";
            else if (risk < 85)
                safe = "Dangerous";
            else safe = "Extremely Dangerous";
            risk = Math.Round(risk, 0);
        }

        private void analyze()
        {
            
        }
        
    }
}
