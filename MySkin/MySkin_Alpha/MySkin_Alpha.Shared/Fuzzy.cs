using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Fuzzy;

namespace MySkin_Alpha
{
    public struct FuzzyDataItem
    {
        public double m_assymmetry;
        public string ass_class;
        public double m_borderIrregularity;
        public string b_class;
        public double m_colourIrregularity;
        public string c_class;
        public double m_area;
        public string ar_class;
        public double m_blueness;
        public string blue_class;
        public double m_redness;
        public string red_class;
    }

    public sealed class Fuzzy
    {
        public static Fuzzy _FuzzyData = new Fuzzy();
        private List<FuzzySet> assym = new List<FuzzySet>();
        private List<FuzzySet> border = new List<FuzzySet>();
        private List<FuzzySet> colour = new List<FuzzySet>();
        private List<FuzzySet> diam = new List<FuzzySet>();
        private List<FuzzySet> blue = new List<FuzzySet>();
        private List<FuzzySet> red = new List<FuzzySet>();

        private LinguisticVariable v_as;
        private LinguisticVariable v_b;
        private LinguisticVariable v_c;
        private LinguisticVariable v_d;
        private LinguisticVariable v_blue;
        private LinguisticVariable v_red;

        public List<FuzzySet> Assym
        {
            get { return assym; }
        }
        public List<FuzzySet> Border
        {
            get { return border; }
        }
        public  List<FuzzySet> Colour
        {
            get { return colour; }
        }
        public List<FuzzySet> Diam
        {
            get { return diam; }
        }
        public List<FuzzySet> Blue
        {
            get { return blue; }
        }
        public List<FuzzySet> Red
        {
            get { return red; }
        }

        FuzzySet assymTN = new FuzzySet("TN", new TrapezoidalFunction(1, 1, 1.1f, 1.2f));
        FuzzySet assymN = new FuzzySet("N", new TrapezoidalFunction(1.1f, 1.2f, 1.25f, 1.3f));
        FuzzySet assymA = new FuzzySet("A", new TrapezoidalFunction(1.25f, 1.35f, 1.4f, 1.45f));
        FuzzySet assymC = new FuzzySet("C", new TrapezoidalFunction(1.4f, 1.55f, 1.6f, 1.75f));
        FuzzySet assymD = new FuzzySet("D", new TrapezoidalFunction(1.6f, 1.8f, 1.9f, 2));
        FuzzySet assymED = new FuzzySet("ED", new TrapezoidalFunction(1.9f, 2.1f, 3.5f, 3.5f));

        FuzzySet borderTN = new FuzzySet("TN", new TrapezoidalFunction(0, 0, 20, 25));
        FuzzySet borderN = new FuzzySet("N", new TrapezoidalFunction(20, 30, 40, 50));
        FuzzySet borderA = new FuzzySet("A", new TrapezoidalFunction(45, 60, 100, 200));
        FuzzySet borderC = new FuzzySet("C", new TrapezoidalFunction(150, 250, 800, 1050));
        FuzzySet borderD = new FuzzySet("D", new TrapezoidalFunction(800, 1100, 1200, 1300));
        FuzzySet borderED = new FuzzySet("ED", new TrapezoidalFunction(1200, 1400, 1700, 1700));

        FuzzySet colourTN = new FuzzySet("TN", new TrapezoidalFunction(0, 0, 30, 50));
        FuzzySet colourN = new FuzzySet("N", new TrapezoidalFunction(40, 60, 90, 105));
        FuzzySet colourA = new FuzzySet("A", new TrapezoidalFunction(100, 110, 150, 200));
        FuzzySet colourC = new FuzzySet("C", new TrapezoidalFunction(150, 250, 800, 1050));
        FuzzySet colourD = new FuzzySet("D", new TrapezoidalFunction(800, 1100, 1200, 1300));
        FuzzySet colourED = new FuzzySet("ED", new TrapezoidalFunction(1200, 1400, 1700, 1700));

        FuzzySet diamTN = new FuzzySet("TN", new TrapezoidalFunction(0, 0, 15, 20));
        FuzzySet diamN = new FuzzySet("N", new TrapezoidalFunction(15, 22, 25, 30));
        FuzzySet diamA = new FuzzySet("A", new TrapezoidalFunction(25, 27, 30, 50));
        FuzzySet diamC = new FuzzySet("C", new TrapezoidalFunction(40, 52, 65, 80));
        FuzzySet diamD = new FuzzySet("D", new TrapezoidalFunction(70, 90, 200, 250));
        FuzzySet diamED = new FuzzySet("ED", new TrapezoidalFunction(230, 260, 560, 560));

        FuzzySet blueTN = new FuzzySet("TN", new TrapezoidalFunction(0, 0, 0.5f, 1));
        FuzzySet blueN = new FuzzySet("N", new TrapezoidalFunction(0.5f, 0.7f, 1, 1.2f));
        FuzzySet blueA = new FuzzySet("A", new TrapezoidalFunction(1, 1.3f, 1.6f, 2));
        FuzzySet blueC = new FuzzySet("C", new TrapezoidalFunction(1.6f, 2.2f, 4, 8));
        FuzzySet blueD = new FuzzySet("D", new TrapezoidalFunction(6, 9, 15, 20));
        FuzzySet blueED = new FuzzySet("ED", new TrapezoidalFunction(17, 22, 200, 200));

        FuzzySet redTN = new FuzzySet("TN", new TrapezoidalFunction(0, 0, 0.5f, 1));
        FuzzySet redN = new FuzzySet("N", new TrapezoidalFunction(0.5f, 0.7f, 1, 1.2f));
        FuzzySet redA = new FuzzySet("A", new TrapezoidalFunction(1, 1.3f, 1.6f, 2));
        FuzzySet redC = new FuzzySet("C", new TrapezoidalFunction(1.6f, 2.2f, 4, 8));
        FuzzySet redD = new FuzzySet("D", new TrapezoidalFunction(6, 9, 15, 20));
        FuzzySet redED = new FuzzySet("ED", new TrapezoidalFunction(17, 22, 200, 200));

        public void init()
        {
            _FuzzyData.v_as = new LinguisticVariable("assymmetry", 1, 3.5f);
            _FuzzyData.v_b = new LinguisticVariable("broder", 0, 1700);
            _FuzzyData.v_c = new LinguisticVariable("colour", 0, 1700);
            _FuzzyData.v_d = new LinguisticVariable("diameter", 0, 560);
            _FuzzyData.v_blue = new LinguisticVariable("blue", 0, 200);
            _FuzzyData.v_red = new LinguisticVariable("red", 0, 200);

            _FuzzyData.Assym.AddRange(new List<FuzzySet> { assymTN, assymN, assymA, assymC, assymD, assymED });
            _FuzzyData.Border.AddRange(new List<FuzzySet> { borderTN, borderN, borderA, borderC, borderD, borderED });
            _FuzzyData.Colour.AddRange(new List<FuzzySet> { colourTN, colourN, colourA, colourC, colourD, colourED });
            _FuzzyData.Diam.AddRange(new List<FuzzySet> { diamTN, diamN, diamA, diamC, diamD, diamED });
            _FuzzyData.Blue.AddRange(new List<FuzzySet> { blueTN, blueN, blueA, blueC, blueD, blueED });
            _FuzzyData.Red.AddRange(new List<FuzzySet> { redTN, redN, redA, redC, redD, redED });
        }

        public static void GetMembership(double assymmetry, double borderIrregularity, double colourIrregularity, double area, double blueness, double redness, out FuzzyDataItem item)
        {
            _FuzzyData.init();
            item = new FuzzyDataItem();
            double maxMembership = 0;
            string dominantClass = string.Empty;
            foreach(FuzzySet set in _FuzzyData.Assym)
            {
                var val = set.GetMembership((float)assymmetry);
                if (val > maxMembership)
                {
                    maxMembership = val;
                    dominantClass = set.Name;
                }
            }
            item.m_assymmetry = maxMembership;
            item.ass_class = dominantClass;
            maxMembership = 0;
            foreach (FuzzySet set in _FuzzyData.Border)
            {
                var val = set.GetMembership((float)borderIrregularity);
                if (val > maxMembership)
                {
                    maxMembership = val;
                    dominantClass = set.Name;
                }
            }
            item.m_borderIrregularity = maxMembership;
            item.b_class = dominantClass;
            maxMembership = 0;
            foreach (FuzzySet set in _FuzzyData.Colour)
            {
                var val = set.GetMembership((float)colourIrregularity);
                if (val > maxMembership)
                {
                    maxMembership = val;
                    dominantClass = set.Name;
                }
            }
            item.m_colourIrregularity = maxMembership;
            item.c_class = dominantClass;
            maxMembership = 0;
            foreach (FuzzySet set in _FuzzyData.Diam)
            {
                var val = set.GetMembership((float)area);
                if (val > maxMembership)
                {
                    maxMembership = val;
                    dominantClass = set.Name;
                }
            }
            item.m_area = maxMembership;
            item.ar_class = dominantClass;
            maxMembership = 0;
            foreach (FuzzySet set in _FuzzyData.Blue)
            {
                var val = set.GetMembership((float)blueness);
                if (val > maxMembership)
                {
                    maxMembership = val;
                    dominantClass = set.Name;
                }
            }
            item.m_blueness = maxMembership;
            item.blue_class = dominantClass;
            maxMembership = 0;
            foreach (FuzzySet set in _FuzzyData.Red)
            {
                var val = set.GetMembership((float)redness);
                if (val > maxMembership)
                {
                    maxMembership = val;
                    dominantClass = set.Name;
                }
            }
            item.m_redness = maxMembership;
            item.red_class = dominantClass;
            maxMembership = 0;
        }
    }
}
