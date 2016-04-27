using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySkin_Alpha
{
    class Nevus
    {
        public bool asymmetric { get;}
        public bool unevenBorder { get;}
        public bool bigDiameter { get; }
        public bool unevenColor { get; }
        public bool evolutes { get; set; }
        public bool stiches { get; set; }
        public bool lostHair { get; set; }
        public bool bleeds { get; set; }
        public bool blueParts { get; }

        public double area { get; }
        public double colorVariation { get; }
        public double borderVariation { get;}
        public double blackness { get;}
        public double blueness { get;}
        public double redness { get;}

        public int edgeCount { get; set; }

        public Nevus (double area, double borderVariation, double colorVariation, double blackness, double blueness, double redness)
        {
            this.area = area;
            this.colorVariation = colorVariation;
            this.borderVariation = borderVariation;
            this.blackness = blackness;
            this.blueness = blueness;
            this.redness = redness;
            if (area > 36)
                this.bigDiameter = true;
            if (colorVariation > 100)
                unevenColor = true;
            if (borderVariation > 20)
                unevenBorder = true;
        }

        private void analyze()
        {
            
        }
    }
}
