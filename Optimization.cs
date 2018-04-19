using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiffSharp.Interop.Float64;
using System.IO;

namespace Optimization
{
    public static class DataGeneration
    {
        public static void MakeDataFile(string file, Func<DV,D> f, DV start, DV end, D accuracy)
        {
            //Open file for editing
            StreamWriter theFile = new StreamWriter(file);

            //Generate data and add to file
            for (D x1 = start[0]; x1 <= end[0]; x1+=accuracy)
            {
                for (D x2 = start[1]; x2 <= end[1]; x2 += accuracy)
                {
                    //Create point and results
                    DV thePoint = new DV(new D[] {x1, x2});
                    D fx = f(thePoint);

                    //Add to file
                    theFile.WriteLine("{0}\t{1}\t{2}", (double)x1, (double)x2, (double)fx);
                }
            }
           
            //Save file
            theFile.Close();
        }
        public static void SaveDescentToFile(string file, DV[] xPositions, double[] fx)
        {
            //Open file for editing
            StreamWriter theFile = new StreamWriter(file);

            //Generate data and add to file
            for (int i=0; i < xPositions.Length; i++)
            {
                //Get data
                D x1 = xPositions[i][0];
                D x2 = xPositions[i][1];
                double fx1x2 = fx[i];

                //check results
                if (double.IsNaN(x1) || double.IsNaN(x2) || double.IsNaN(fx1x2)) continue;
                if (double.IsInfinity(x1) || double.IsInfinity(x2) || double.IsInfinity(fx1x2)) continue;

                //Add to file
                theFile.WriteLine("{0}\t{1}\t{2}", (double)x1, (double)x2, (double)fx1x2);
            }
            

            //Save file
            theFile.Close();
        }
    }
    public static class GradientDescent
    {
        //Methods
        public static DV FirstOrder_OneDimensionalMethod(Func<DV,D> f, DV startPoint, double accuracy, out int calcsF, out int calcsGradient)
        {
            DV[] x;
            double[] fx;
            return FirstOrder_OneDimensionalMethod(f, startPoint, accuracy, out calcsF, out calcsGradient, out x, out fx);
        }
        public static DV FirstOrder_OneDimensionalMethod(Func<DV, D> f, DV startPoint, double accuracy, out int calcsF, out int calcsGradient, out DV[] x, out double[] fx)
        {
            //Counters
            calcsF = 0; //Count how many times the objective function was used.
            calcsGradient = 0; //Count how many times the gradient was calculated.

            //Define our X vector
            int maxIterations = 1000;
            x = new DV[maxIterations];
            fx = new double[maxIterations];

            //Pick an initial guess for x
            int i = 0;
            x[i] = startPoint;
            fx[i] = f(x[i]); calcsF++;
            
            //Loop through gradient steps until min points are found, recompute gradient and repeat.
            while (true)
            {
                //Compute next step, using previous step
                i++;

                //Return failed results
                if (double.IsNaN(x[i-1][0]) || double.IsNaN(x[i - 1][1]) ||  (i == maxIterations))
                {
                    x = x.Take(i).ToArray();
                    fx = fx.Take(i).ToArray();
                    return null;
                }

                //Step 1 - Determine the gradient
                DV gradient = 0-AD.Grad(f, x[i - 1]); calcsGradient++;
                DV direction = gradient / Math.Sqrt(AD.Pow(gradient[0], 2) + AD.Pow(gradient[1], 2)); //Normalize Gradient

                //Step 2 - Build an objective function using the gradient.
                // This objective function moves downward in the direction of the gradient.
                // It uses golden ratio optimization to find the minimum point in this direction  
                DV xPrev = x[i - 1];
                Func<D, D> objFStep = delegate (D alpha)
                {
                    DV xNew = xPrev + (alpha * direction);
                    return f(xNew);
                };
                var stepSearchResults = UnimodalMinimization.goldenRatioSearch(objFStep, 0, 1, accuracy); //alpha can only be between 0 and 1
                double step = (stepSearchResults.a + stepSearchResults.b) / 2; //The step required to get to the bottom
                calcsF += stepSearchResults.CalculationsUntilAnswer; //The number of calculations of f that were required.

                //Step 3 - Move to the discovered minimum point
                x[i] = x[i - 1] + (step * direction);
                fx[i] = f(x[i]); calcsF++;

                //Step 4 - Check if accuracy has been met. If so, then end.               
                double magGradient = Math.Sqrt(AD.Pow(gradient[0], 2) + AD.Pow(gradient[1], 2));
                if (magGradient < accuracy)
                    break;
                DV dx = AD.Abs(x[i] - x[i - 1]);
                if (((dx[0] < accuracy) && (dx[1] < accuracy)))
                    break;
            }

            //Return the minimization point.
            x = x.Take(i+1).ToArray();
            fx = fx.Take(i+1).ToArray();
            return x[i];
        }

        public static DV FirstOrder_DivisionMethod(Func<DV, D> f, DV startPoint, double accuracy, out int calcsF, out int calcsGradient)
        {
            DV[] x;
            double[] fx;

            return FirstOrder_DivisionMethod(f, startPoint, accuracy, out calcsF, out calcsGradient, out x, out fx);
        }
        public static DV FirstOrder_DivisionMethod(Func<DV, D> f, DV startPoint, double accuracy, out int calcsF, out int calcsGradient, out DV[] x, out double[] fx)
        {
            //Counters
            calcsF = 0; //Count how many times the objective function was used.
            calcsGradient = 0; //Count how many times the gradient was calculated.

            //Define our X vector
            int maxIterations = 10000;
            x = new DV[maxIterations];
            fx = new double[maxIterations];

            //Pick an initial guess for x
            int i = 0;
            x[0] = startPoint;
            fx[0] = f(x[0]); calcsF++;

            //Loop through gradient steps until min points are found, recompute gradient and repeat.
            double alpha = 1;
            while (true)
            {
                //Compute next step, using previous step
                i++;

                //Step 1 - Determine the gradient
                DV gradient = AD.Grad(f, x[i - 1]); calcsGradient++;

                //Step 2 - Division method, to compute the new x[i] and fx[i]             
                DV xPrev = x[i - 1];
                Func<D, D> objFAlpha = delegate (D a)
                {
                    DV xNext = xPrev - (a * gradient);
                    return f(xNext);
                };
                alpha = alpha * 0.8;
                double beta = UnimodalMinimization.DivisionSearch(objFAlpha, fx[i - 1], alpha, out fx[i], ref calcsF);
                x[i] = x[i - 1] - (beta * gradient);

                //Step 3 - Check if accuracy has been met. If so, then end.
                double magGradient = Math.Sqrt(AD.Pow(gradient[0], 2) + AD.Pow(gradient[1], 2));
                if (magGradient < accuracy)
                    break;
                //DV dx = AD.Abs(x[i] - x[i - 1]);
                //if (((err[0] < accuracy) && (err[1] < accuracy)))
                //    break;
            }

            //Return the minimization point.
            x = x.Take(i + 1).ToArray();
            fx = fx.Take(i + 1).ToArray();
            return x[i];
        }

        public static DV SecondOrder_FullStep(Func<DV, D> f, DV startPoint, double accuracy, out int calcsF, out int calcsGradient, out int calcsHessian)
        {
            DV[] x;
            double[] fx;
            return SecondOrder_FullStep(f, startPoint, accuracy, out calcsF, out calcsGradient, out calcsHessian, out x, out fx);
        }
        public static DV SecondOrder_FullStep(Func<DV, D> f, DV startPoint, double accuracy, out int calcsF, out int calcsGradient, out int calcsHessian, out DV[] x, out double[] fx)
        {
            //Counters
            calcsF = 0; //Count how many times the objective function was used.
            calcsGradient = 0; //Count how many times the gradient was calculated.
            calcsHessian = 0; //Count how many times the second gradient was calculated.

            //Define our X vector
            int maxIterations = 10000;
            x = new DV[maxIterations];
            fx = new double[maxIterations];

            //Pick an initial guess for x
            int i = 0;
            x[0] = startPoint;

            //Loop through gradient steps until zeros are found
            while (true)
            {
                //Compute next step, using previous step
                i++;

                //Step 1 - Determine the gradients
                DV gradient = AD.Grad(f, x[i - 1]); calcsGradient++;
                var hess = AD.Hessian(f, x[i - 1]); calcsHessian++;

                //Loop through every entry in the DV and compute the step for each one.
                List<D> listSteps = new List<D>();
                while (true)
                {
                    try
                    {
                        int v = listSteps.Count;
                        listSteps.Add(-gradient[v] / hess[v, v]); // first-gradient divided by second-gradient
                    }
                    catch
                    { break; }
                }
                DV fullStep = new DV(listSteps.ToArray());

                //Compute the new position using the step
                x[i] = x[i - 1] + fullStep;
                fx[i] = f(x[i]); calcsF++;

                //Check if accuracy has been met
                double magGradient = Math.Sqrt(AD.Pow(gradient[0], 2) + AD.Pow(gradient[1], 2));
                if (magGradient < accuracy)
                    break;
                //DV dx = AD.Abs(x[i] - x[i - 1]);
                //if (((err[0] < accuracy) && (err[1] < accuracy)))
                //    break;
            }

            //Return the minimization point.
            x = x.Take(i + 1).ToArray();
            fx = fx.Take(i + 1).ToArray();
            return x[i];

        }

        public static DV SecondOrder_DivisionMethod(Func<DV, D> f, DV startPoint, double accuracy, out int calcsF, out int calcsGradient, out int calcsHessian)
        {
            DV[] x;
            double[] fx;
            return SecondOrder_DivisionMethod(f, startPoint, accuracy, out calcsF, out calcsGradient, out calcsHessian, out x, out fx);
        }
        public static DV SecondOrder_DivisionMethod(Func<DV, D> f, DV startPoint, double accuracy, out int calcsF, out int calcsGradient, out int calcsHessian, out DV[] x, out double[] fx)
        {
            //Counters
            calcsF = 0; //Count how many times the objective function was used.
            calcsGradient = 0; //Count how many times the gradient was calculated.
            calcsHessian = 0; //Count how many times the second gradient was calculated.

            //Define our X vector
            int maxIterations = 10000;
            x = new DV[maxIterations];
            fx = new double[maxIterations];

            //Pick an initial guess for x
            int i = 0;
            x[i] = startPoint;
            fx[i] = f(x[i]); calcsF++;

            //Loop through gradient steps until zeros are found
            double alpha = 1;
            while (true)
            {
                //Compute next step, using previous step
                i++;

                //Step 1 - Determine the gradients
                DV gradient = AD.Grad(f, x[i - 1]); calcsGradient++;
                var hess = AD.Hessian(f, x[i - 1]); calcsHessian++;

                //Step 2 - Compute full step (alpha = 1). Loop through every entry in the DV and compute the step for each one.
                List<D> listSteps = new List<D>();
                while (true)
                {
                    try
                    {
                        int c = listSteps.Count;
                        listSteps.Add(-gradient[c] / hess[c, c]); // first-gradient divided by second-gradient
                    }
                    catch
                    { break; }
                }
                DV fullStep = new DV(listSteps.ToArray());

                //Step 3 - Division method, to compute the new x[i] and fx[i]             
                DV xPrev = x[i - 1];
                Func<D, D> objFAlpha = delegate (D a)
                {
                    DV xNext = xPrev + (a * fullStep);
                    return f(xNext);
                };
                alpha = alpha * 0.8;
                double beta = UnimodalMinimization.DivisionSearch(objFAlpha, fx[i - 1], alpha, out fx[i], ref calcsF);
                x[i] = x[i - 1] + (beta * fullStep);

                //Check if accuracy has been met
                double magGradient = Math.Sqrt(AD.Pow(gradient[0], 2) + AD.Pow(gradient[1], 2));
                if (magGradient < accuracy)
                    break;
                DV dx = AD.Abs(x[i] - x[i - 1]);
                if ((dx[0] < accuracy * 0.1) && (dx[1] < accuracy * 0.1))
                    break;
            }

            //Return the minimization point
            x = x.Take(i + 1).ToArray();
            fx = fx.Take(i + 1).ToArray();
            return x[i];

        }
      
    }
    public static class UnimodalMinimization
    {
        //Methods
        public static SearchResult directUniformSearch(Func<D,D> f, double rangeStart, double rangeEnd, int nIntervals, double accuracy)
        {
            // Relationship to variables from classroom
            double a = rangeStart;
            double b = rangeEnd;
            int n = nIntervals;
            double eps = accuracy;

            //Counter
            int fCounter = 0;

            //Loop until a solution is found
            while (true)
            {
                //Determine the step size (h)
                double h = (b - a) / n;

                //For storing previous results
                double[] x = new double[n];
                double[] fX = new double[n];

                //Caclulate first two points
                int i = 0;
                x[i] = a + h * i; //i=0;
                fX[i] = f(x[i]); fCounter++;
                i = 1;
                x[i] = a + h * i; //i=1;
                fX[i] = f(x[i]); fCounter++;

                //Loop through each additional step until f(x) on both sides is larger
                for (i = 2; i < n; i++)
                {
                    //Calculate x and f(x)
                    x[i] = a + h * i;
                    fX[i] = f(x[i]); fCounter++;

                    //Check if f(x) is greater on both sides
                    double fxBefore = fX[i - 2];
                    double fxMiddle = fX[i - 1];
                    double fxAfter = fX[i];
                    if ((fxBefore > fxMiddle) && (fxMiddle < fxAfter))
                    {
                        //Change the interval
                        a = x[i - 2];
                        b = x[i];

                        //Check if the accuracy has been achieved. If so, return the result.
                        if (Math.Abs(b - a) < eps)
                        {
                            return new SearchResult
                            {
                                //Inputs
                                rangeStart = rangeStart,
                                rangeEnd = rangeEnd,
                                nIntervals = nIntervals,
                                accuracy = accuracy,

                                //Results
                                a = x[i - 2],
                                b = x[i],
                                Fa = fX[i - 2],
                                Fb = fX[i],
                                CalculationsUntilAnswer = fCounter
                            };
                        }

                        //stop this loop.
                        break;
                    }
                }



            }
        }
        public static SearchResult dichotomySearch(Func<D,D> f, double rangeStart, double rangeEnd, double accuracy)
        {
            //Relationship to variables from classroom
            double a = rangeStart;
            double b = rangeEnd;
            double eps = accuracy;

            //Counter
            int fCounter = 0;

            //Variables for processing
            double x1, x, x2;
            double f1, fX, f2;

            //Loop until solution found
            while (true)
            {
                //Calculate delta
                double delta = Math.Abs(a - b) * 0.01;

                //Create X values
                x = (a + b) / 2;
                x1 = x - delta;
                x2 = x + delta;

                //Calculate Y values
                f1 = f(x1); fCounter++;
                fX = f(x); fCounter++;
                f2 = f(x2); fCounter++;

                //Check if solution found
                if (Math.Abs(b - a) < eps)
                {
                    //Return the current values
                    return new SearchResult
                    {
                        //Inputs
                        rangeStart = rangeStart,
                        rangeEnd = rangeEnd,
                        nIntervals = 0,
                        accuracy = accuracy,

                        //Results
                        a = a,
                        b = b,
                        Fa = f1,
                        Fb = f2,
                        CalculationsUntilAnswer = fCounter
                    };
                }

                //Change the range and repeat
                if (f1 > fX)
                    a = x;
                else if (f2 > fX)
                    b = x;
            }
        }
        public static SearchResult goldenRatioSearch(Func<D,D> f, double rangeStart, double rangeEnd, double accuracy)
        {
            // https://en.wikipedia.org/wiki/Golden_section_search

            // Relationship to variables from classroom
            double a = rangeStart;
            double b = rangeEnd;
            double eps = accuracy;

            //For storing results
            int fCounter = 0;
            Dictionary<double, double> fX = new Dictionary<double, double>();

            //Calculate probe points (explained on website)
            double phi = (1 + Math.Sqrt(5)) / 2.0; //golden ratio
            double x1 = b - (b - a) / phi; // (b-a) = distance between x2 and x4
            double x2 = a + (b - a) / phi;

            //Calculate first 2 points
            fX.Add(x1, f(x1)); fCounter++;
            fX.Add(x2, f(x2)); fCounter++;

            //loop until results found
            while (Math.Abs(b - a) > eps)
            {
                //Modify the range
                if (fX[x1] < fX[x2])
                {
                    //Change range
                    b = x2;

                    //Set new probe point
                    x2 = x1;
                    x1 = b - (b - a) / phi;

                    //Calculate at new x1
                    fX.Add(x1, f(x1)); fCounter++;
                }
                else
                {
                    //Change range
                    a = x1;

                    //Set new probe point
                    x1 = x2;
                    x2 = a + (b - a) / phi;

                    //Calculate at new x2
                    fX.Add(x2, f(x2)); fCounter++;
                }
            }

            //Return the current values
            return new SearchResult
            {
                //Inputs
                rangeStart = rangeStart,
                rangeEnd = rangeEnd,
                nIntervals = 0, //not used by this method
                accuracy = accuracy,

                //Results
                a = a,
                b = b,
                Fa = f(a),
                Fb = f(b),
                CalculationsUntilAnswer = fCounter
            };
        }

        //Support search function
        public static double DivisionSearch(Func<D, D> f, double fPrev, double alphaStart, out double fNext, ref int calcsF)
        {
            //Cycle through until the new alpha creates a new fx less than the current.
            double alpha = alphaStart;
            while (true)
            {
                //Compute new point
                fNext = f(alpha); calcsF++;

                //Check for end.
                if (fNext <= fPrev) break;

                //Divide the step by half then repeat
                alpha = alpha * 0.5;
            }

            //return the final calculation of f
            return alpha;
        }

        //Class
        public class SearchResult
        {
            //Input parameters
            public double rangeStart;
            public double rangeEnd;
            public int nIntervals;
            public double accuracy;

            //X
            public double a;
            public double b;

            //F(x)
            public double Fa;
            public double Fb;
            public int CalculationsUntilAnswer;

            //Methods
            public string getTableHeader()
            {
                string s = "";

                //Input parameters
                s += "origA";
                s += "\t" + "origB";
                s += "\t" + "n";
                s += "\t" + "eps";

                //Results
                s += "\t" + "finalA";
                s += "\t" + "finalB";
                s += "\t" + "f(a)";
                s += "\t" + "f(b)";
                s += "\t" + "calcs";

                return s;
            }
            public string getTabbedResults()
            {
                //determine number of decimal places to show
                int decPlaces = BitConverter.GetBytes(decimal.GetBits((decimal)accuracy)[3])[2] + 1;

                string s = "";

                //Input parameters
                s += rangeStart;
                s += "\t" + rangeEnd;
                s += "\t" + nIntervals;
                s += "\t" + accuracy;

                //Results
                s += "\t" + a.ToString("F" + decPlaces);
                s += "\t" + b.ToString("F" + decPlaces);
                s += "\t" + Fa.ToString("F" + decPlaces);
                s += "\t" + Fb.ToString("F" + decPlaces);
                s += "\t" + CalculationsUntilAnswer;

                return s;
            }
        }

    }
}
