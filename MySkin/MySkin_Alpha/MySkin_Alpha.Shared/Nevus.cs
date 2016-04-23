using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySkin_Alpha
{
    class Nevus
    {
        private bool asymmetric;
        private bool unevenBorder;
        private bool colorVariation;
        private bool bigDiameter;
        private bool evolutes;

        private int diameter, colorVariety, edgeCount, symetryDeviation;

        Nevus (int diam, int colorVar,int edgeCt, int symDev)
        {
            diameter = diam;
            colorVariety = colorVar;
            edgeCount = edgeCt;
            symetryDeviation = symDev;

            asymmetric = false;
            unevenBorder = false;
            colorVariation = false;
            bigDiameter = false;
            evolutes = false;
        }

        public void analyze()
        {

        }
    }
}
