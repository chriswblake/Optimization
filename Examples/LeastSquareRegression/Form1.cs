using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Optimization;
using DiffSharp.Interop.Float64;
using System.Windows.Forms.DataVisualization.Charting;

namespace Task3
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            //Main objective function
            Func<DV, D> fOBJ = delegate (DV abx)
            {
                // y = a*ln(x) + b
                D a = abx[0];
                D b = abx[1];
                D x = abx[2];

                //Calculate result
                D r = a * AD.Log(x) + b;
                //D r = a * x*x+ b;
                return r;
            };

            //Original Dataset Variables
            const double aOrig = 3.0;
            const double bOrig = 8.0;
            double[] xOrig = new double[100];
            double[] yOrig = new double[100];

            #region Calculate Original data
            //Calculate original dataset
            for(int x=0; x<100; x++)
            {
                xOrig[x] = x+1;
                DV param = new DV(new D[] { aOrig, bOrig, xOrig[x] });
                yOrig[x] = fOBJ(param);
            }

            //Create series
            Series origSeries = new Series("Original")
            {
                ChartType = SeriesChartType.Line,
                Color = Color.Black,
                BorderDashStyle = ChartDashStyle.Solid,
                BorderWidth = 2,
                ChartArea = chartResults.ChartAreas[0].Name,
                LegendText = "Original a=" + aOrig.ToString("F3") + " b=" + bOrig.ToString("F3")

            };
            for (int i = 0; i < xOrig.Length; i++)
                origSeries.Points.AddXY(xOrig[i], yOrig[i]);
            #endregion

            #region Calculate random error version
            double[] yNoisy = new double[100];
            Random rand = new Random();

            //Noisy version
            for (int i = 0; i < xOrig.Length; i++)
            {
                double percentModifyA = 0.98 + (1.02 - 0.98) * rand.NextDouble();
                double percentModifyB = 0.98 + (1.02 - 0.98) * rand.NextDouble();
                double percentModifyY = 1.0; // 0.98 + (1.02 - 0.98) * rand.NextDouble();

                DV param = new DV(new D[] { aOrig*percentModifyA, bOrig*percentModifyB, xOrig[i] });
                yNoisy[i] = fOBJ(param) * percentModifyY;
            }

            //Create series
            Series noisySeries = new Series("Noisy")
            {
                ChartType = SeriesChartType.Point,
                Color = Color.DarkGray,
                MarkerStyle = MarkerStyle.Star4,
                MarkerSize = 7,
                BorderWidth = 0,
                ChartArea = chartResults.ChartAreas[0].Name
            };
            for (int i = 0; i < xOrig.Length; i++)
                noisySeries.Points.AddXY(xOrig[i], yNoisy[i]);

            #endregion

            #region Calculate a and b using error function and gradient descent    
            //Error equation   
            Func<DV, D> fMSE = delegate (DV ab)
            {
                //Get parameters
                D a = ab[0];
                D b = ab[1];

                //Compute squared error for each point
                D errorSum = 0;
                for (int i=0; i<xOrig.Length; i++)
                {
                    //Compute object function value
                    DV param = new DV(new D[] {a, b, xOrig[i]});
                    D yCalc = fOBJ(param);

                    //Check for error
                    if (double.IsNaN(yCalc))
                        continue;

                    //Compute error between noisy version and calculated version
                    D err = AD.Pow(yNoisy[i] - yCalc, 2);
                    errorSum += err;
                }

                //Compute least square
                D mse = AD.Pow(errorSum, 0.5);

                //return results
                return mse;
            };

            //Calculate optimization for a and b
            DV startPoint = new DV(new D[] { 10, 5 });
            int calcsF;
            int calcsGradient;
            DV result = GradientDescent.FirstOrder_OneDimensionalMethod(fMSE, startPoint, 0.01, out calcsF, out calcsGradient);//, out calcsHessian);
            double aFit = result[0];
            double bFit = result[1];
            DV paramMseFit = new DV(new D[] { aFit, bFit});
            double mseFit = fMSE(paramMseFit);

            //Create series
            Series fitSeries = new Series("Fit")
            {
                ChartType = SeriesChartType.Point,
                Color = Color.Red,
                MarkerSize = 3,
                MarkerStyle = MarkerStyle.Circle,
                ChartArea = chartResults.ChartAreas[0].Name,
                LegendText = "Fit a=" + aFit.ToString("F3") + " b=" + bFit.ToString("F3") + " MSE=" + mseFit.ToString("F3")
            };
            for (int i = 0; i < xOrig.Length; i++)
            {
                DV param = new DV(new D[] { aFit, bFit, xOrig[i] });
                fitSeries.Points.AddXY(xOrig[i], fOBJ(param));
            }
            #endregion

            //Add series to chart
            chartResults.Series.Clear();
            chartResults.Series.Add(origSeries);
            chartResults.Series.Add(noisySeries);
            chartResults.Series.Add(fitSeries);

        }
    }
}
