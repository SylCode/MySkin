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
        private Database database = new Database();
        private InferenceSystem IS;
        private List<FuzzySet> assym = new List<FuzzySet>();
        private List<FuzzySet> border = new List<FuzzySet>();
        private List<FuzzySet> colour = new List<FuzzySet>();
        private List<FuzzySet> diam = new List<FuzzySet>();
        private List<FuzzySet> blue = new List<FuzzySet>();
        private List<FuzzySet> red = new List<FuzzySet>();

        //private LinguisticVariable v_as;
        //private LinguisticVariable v_b;
        //private LinguisticVariable v_c;
        //private LinguisticVariable v_d;
        //private LinguisticVariable v_blue;
        //private LinguisticVariable v_red;

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
        FuzzySet assymN = new FuzzySet("OK", new TrapezoidalFunction(1.1f, 1.2f, 1.25f, 1.3f));
        FuzzySet assymA = new FuzzySet("A", new TrapezoidalFunction(1.25f, 1.35f, 1.4f, 1.45f));
        FuzzySet assymC = new FuzzySet("C", new TrapezoidalFunction(1.4f, 1.55f, 1.6f, 1.75f));
        FuzzySet assymD = new FuzzySet("D", new TrapezoidalFunction(1.6f, 1.8f, 1.9f, 2));
        FuzzySet assymED = new FuzzySet("ED", new TrapezoidalFunction(1.9f, 2.1f, 3.5f, 3.5f));

        FuzzySet borderTN = new FuzzySet("TN", new TrapezoidalFunction(0, 0, 20, 25));
        FuzzySet borderN = new FuzzySet("OK", new TrapezoidalFunction(20, 30, 40, 50));
        FuzzySet borderA = new FuzzySet("A", new TrapezoidalFunction(45, 60, 100, 200));
        FuzzySet borderC = new FuzzySet("C", new TrapezoidalFunction(150, 250, 800, 1050));
        FuzzySet borderD = new FuzzySet("D", new TrapezoidalFunction(800, 1100, 1200, 1300));
        FuzzySet borderED = new FuzzySet("ED", new TrapezoidalFunction(1200, 1400, 1700, 1700));

        FuzzySet colourTN = new FuzzySet("TN", new TrapezoidalFunction(0, 0, 30, 50));
        FuzzySet colourN = new FuzzySet("OK", new TrapezoidalFunction(40, 60, 90, 105));
        FuzzySet colourA = new FuzzySet("A", new TrapezoidalFunction(100, 110, 150, 200));
        FuzzySet colourC = new FuzzySet("C", new TrapezoidalFunction(150, 250, 800, 1050));
        FuzzySet colourD = new FuzzySet("D", new TrapezoidalFunction(800, 1100, 1200, 1300));
        FuzzySet colourED = new FuzzySet("ED", new TrapezoidalFunction(1200, 1400, 1700, 1700));

        FuzzySet diamTN = new FuzzySet("TN", new TrapezoidalFunction(0, 0, 15, 20));
        FuzzySet diamN = new FuzzySet("OK", new TrapezoidalFunction(15, 22, 25, 30));
        FuzzySet diamA = new FuzzySet("A", new TrapezoidalFunction(25, 27, 30, 50));
        FuzzySet diamC = new FuzzySet("C", new TrapezoidalFunction(40, 52, 65, 80));
        FuzzySet diamD = new FuzzySet("D", new TrapezoidalFunction(70, 90, 200, 250));
        FuzzySet diamED = new FuzzySet("ED", new TrapezoidalFunction(230, 260, 560, 560));

        FuzzySet blueTN = new FuzzySet("TN", new TrapezoidalFunction(0, 0, 0.5f, 1));
        FuzzySet blueN = new FuzzySet("OK", new TrapezoidalFunction(0.5f, 0.7f, 1, 1.2f));
        FuzzySet blueA = new FuzzySet("A", new TrapezoidalFunction(1, 1.3f, 1.6f, 2));
        FuzzySet blueC = new FuzzySet("C", new TrapezoidalFunction(1.6f, 2.2f, 4, 8));
        FuzzySet blueD = new FuzzySet("D", new TrapezoidalFunction(6, 9, 15, 20));
        FuzzySet blueED = new FuzzySet("ED", new TrapezoidalFunction(17, 22, 200, 200));

        FuzzySet redTN = new FuzzySet("TN", new TrapezoidalFunction(0, 0, 0.5f, 1));
        FuzzySet redN = new FuzzySet("OK", new TrapezoidalFunction(0.5f, 0.7f, 1, 1.2f));
        FuzzySet redA = new FuzzySet("A", new TrapezoidalFunction(1, 1.3f, 1.6f, 2));
        FuzzySet redC = new FuzzySet("C", new TrapezoidalFunction(1.6f, 2.2f, 4, 8));
        FuzzySet redD = new FuzzySet("D", new TrapezoidalFunction(6, 9, 15, 20));
        FuzzySet redED = new FuzzySet("ED", new TrapezoidalFunction(17, 22, 200, 200));

        public void init()
        {
            if (_FuzzyData.Assym.Count>0)
                return;
            _FuzzyData.assym = new List<FuzzySet> { assymTN, assymN, assymA, assymC, assymD, assymED };
            _FuzzyData.border = new List<FuzzySet> { borderTN, borderN, borderA, borderC, borderD, borderED };
            _FuzzyData.colour = new List<FuzzySet> { colourTN, colourN, colourA, colourC, colourD, colourED };
            _FuzzyData.diam = new List<FuzzySet> { diamTN, diamN, diamA, diamC, diamD, diamED };
            _FuzzyData.blue = new List < FuzzySet > { blueTN, blueN, blueA, blueC, blueD, blueED };
            _FuzzyData.red = new List<FuzzySet> { redTN, redN, redA, redC, redD, redED };

            LinguisticVariable v_as = new LinguisticVariable("assymmetry", 1, 3.5f);
            LinguisticVariable v_b = new LinguisticVariable("border", 0, 1700);
            LinguisticVariable v_c = new LinguisticVariable("colour", 0, 1700);
            LinguisticVariable v_d = new LinguisticVariable("diameter", 0, 560);
            LinguisticVariable v_blue = new LinguisticVariable("blue", 0, 200);
            LinguisticVariable v_red = new LinguisticVariable("red", 0, 200);

            FuzzySet DiagN = new FuzzySet("Negative", new TrapezoidalFunction(0, 0, 2, 4));
            FuzzySet DiagC = new FuzzySet("Caution", new TrapezoidalFunction(2, 5, 6, 7));
            FuzzySet DiagP = new FuzzySet("Positive", new TrapezoidalFunction(6, 8, 10, 10));

            LinguisticVariable v_diag = new LinguisticVariable("Diagnosis", 0, 10);
            v_diag.AddLabel(DiagN);
            v_diag.AddLabel(DiagC);
            v_diag.AddLabel(DiagP);

            for (int i=0; i< _FuzzyData.assym.Count; i++)
            {
                v_as.AddLabel(_FuzzyData.Assym[i]);
                v_b.AddLabel(_FuzzyData.Border[i]);
                v_c.AddLabel(_FuzzyData.Colour[i]);
                v_d.AddLabel(_FuzzyData.Diam[i]);
                v_blue.AddLabel(_FuzzyData.Blue[i]);
                v_red.AddLabel(_FuzzyData.Red[i]);
            }

            //_FuzzyData.Assym.AddRange(_FuzzyData.Assym);
            //_FuzzyData.Border.AddRange(_FuzzyData.Border);
            //_FuzzyData.Colour.AddRange(_FuzzyData.Colour);
            //_FuzzyData.Diam.AddRange(_FuzzyData.Diam);
            //_FuzzyData.Blue.AddRange(_FuzzyData.Blue);
            //_FuzzyData.Red.AddRange(_FuzzyData.Red);

            _FuzzyData.database.AddVariable(v_as);
            _FuzzyData.database.AddVariable(v_b);
            _FuzzyData.database.AddVariable(v_c);
            _FuzzyData.database.AddVariable(v_d);
            _FuzzyData.database.AddVariable(v_blue);
            _FuzzyData.database.AddVariable(v_red);
            _FuzzyData.database.AddVariable(v_diag);

            _FuzzyData.IS = new InferenceSystem(_FuzzyData.database, new CentroidDefuzzifier(1000));

            _FuzzyData.IS.NewRule("Rule 1", "IF (colour IS ED OR colour IS ED OR colour IS C OR colour IS A) AND ((blue IS ED OR blue IS D OR blue IS C OR blue IS A) OR (red IS ED OR red IS D OR red IS C OR red IS A)) THEN Diagnosis IS Positive");
            _FuzzyData.IS.NewRule("Rule 2", "IF (assymmetry IS ED OR assymmetry IS D OR assymmetry IS C) AND (border IS ED OR border IS D OR border IS C) AND (colour IS ED OR colour IS ED OR colour IS C) AND (diameter IS ED OR diameter IS ED) AND (blue IS ED OR blue IS D OR blue IS C OR blue IS A) OR (red IS ED OR red IS D OR red IS C OR red IS A) THEN Diagnosis IS Positive");
            _FuzzyData.IS.NewRule("Rule 3", "IF (assymmetry IS TN OR assymmetry IS OK OR assymmetry IS A) AND (border IS TN OR border IS OK OR border IS A) AND (colour IS TN OR colour IS OK OR colour IS A) AND (diameter IS TN OR diameter IS OK OR diameter IS A) AND (blue IS TN OR blue IS OK) AND (red IS TN OR red IS OK) THEN Diagnosis IS Negative");
            _FuzzyData.IS.NewRule("Rule 4", "IF (border IS D OR border IS ED) AND (colour IS D OR colour IS ED) THEN Diagnosis IS Negative");


        }

        private float DoInference(double area, double borderVariation, double assymmetryRate, double colorVariation, double blueness, double redness)
        {
            // Setting inputs
            IS.SetInput("assymmetry", Convert.ToSingle(assymmetryRate));
            IS.SetInput("border", Convert.ToSingle(borderVariation));
            IS.SetInput("colour", Convert.ToSingle(colorVariation));
            IS.SetInput("diameter", Convert.ToSingle(area));
            IS.SetInput("blue", Convert.ToSingle(blueness));
            IS.SetInput("red", Convert.ToSingle(redness));

            // Setting outputs
            try
            {
                return _FuzzyData.IS.Evaluate("Diagnosis");
            }
            catch (Exception ex)
            {
            }
            return 0;
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

        public static string GetDiagnosis(Nevus nev)
        {
            _FuzzyData.init();
            float r = _FuzzyData.DoInference(nev.area, nev.borderVariation, nev.assymmetryRate, nev.colorVariation, nev.blueness, nev.redness);
            
            if (_FuzzyData.database.GetVariable("Diagnosis").GetLabelMembership("Negative", r) > _FuzzyData.database.GetVariable("Diagnosis").GetLabelMembership("Caution", r))
                if (_FuzzyData.database.GetVariable("Diagnosis").GetLabelMembership("Negative", r) > _FuzzyData.database.GetVariable("Diagnosis").GetLabelMembership("Positive", r))
                    return "Negative";
                else return "Positive";
            else if (_FuzzyData.database.GetVariable("Diagnosis").GetLabelMembership("Caution", r) > _FuzzyData.database.GetVariable("Diagnosis").GetLabelMembership("Positive", r))
                    return "Caution";
                else return "Positive";
        }
    }
}
