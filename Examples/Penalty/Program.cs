using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiffSharp.Interop.Float64;
using Optimization;

namespace Penalty
{
    class Program
    {
        static void Main(string[] args)
        {
            //Required accuracy values
            List<double> epsValues = new List<double> { 0.1, 0.01, 0.001 }; //accuracy

            //Functions
            Func<DV, D> objFunc = delegate (DV x)
            {
                D x1 = x[0];
                D x2 = x[1];


               // return AD.Pow(x1 - 2, 2) + AD.Pow(x2 - 2, 2);

                //(1-x1)^2 - 10(x2-x1^2)^2 + x1^2 - 2x1*x2 + exp(-x1-x2)
                return AD.Pow(1.0 - x1, 2) //(1-x1)^2
                        - 10*AD.Pow(x2 - AD.Pow(x1, 2), 2) // - 10(x2-x1^2)^2
                        + AD.Pow(x1, 2) // + x1^2
                        - 2*x1*x2 // - 2x1*x2
                        + AD.Exp(-x1 - x2); //exp(-x1 - x2)
            };
            Func<D, D> penalty = delegate (D t)
            {
                if (t < 0)
                    return 0;
                else
                {
                    //return -100000;
                    return 1000000 * AD.Pow(t, 2);
                }
            };
            Func<DV,D> objFunc_penalized = delegate (DV x)
            {
                D x1 = x[0];
                D x2 = x[1];

                //Constraints
                //Example: x >= 1  ----------------------->  1 - x <= 0
                //x1^2 + x2^2 <= 16     --------->  x1^2 + x2^2 - 16 <= 0
                //(x2 - x1)^2 + x1 <= 6 --------->  (x2 - x1) ^ 2 + x1 - 6 <= 0
                //x1 + x2 >= 2          --------->  2 - x1 - x2 <= 0

                //Combine objective function and penalty functions
                return 0
                    + objFunc(x)
                    + penalty(AD.Pow(x1,2) + AD.Pow(x2,2) - 16)
                    + penalty(AD.Pow(x2 - x1, 2) + x1 - 6)                   
                    + penalty(2 - x1 - x2)
                    ;
            };

            //Get results
            int calcsF;
            int calcsGradient;
            int calcsHessian;
            DV[] xLocations = null;
            double[] fx = null;


            epsValues = new List<double> {0.1, 0.01, 0.001}; //accuracy
            #region 1.)  Penalty Function, First Order, One-Dimensional Method
            
            //Show the table header
            Console.WriteLine("----- Gradient Search, First Order, One-Dimensional Method -----");
            Console.WriteLine("       eps        X1        X2        f(x)     Calcs F     Calcs Gr");
            foreach (double eps in epsValues)
            {
                //Perform calculation
                //DV startPoint = new DV(new D[] { 2, 2 });
                DV startPoint = new DV(new D[] { 1, 2 });
                var xMin = Optimization.GradientDescent.FirstOrder_OneDimensionalMethod(objFunc_penalized, startPoint, eps, out calcsF, out calcsGradient, out xLocations, out fx);
                
                //determine number of decimal places to show
                int dp = BitConverter.GetBytes(decimal.GetBits((decimal)eps)[3])[2] + 1;

                //Display result on console
                if (xMin != null)
                Console.WriteLine("{0,10}{1,10:F" + dp + "}{2,10:F" + dp + "}{3,12:F" + dp + "}{4,10}{5,10}", eps, (double)xMin[0], (double)xMin[1], (double)objFunc_penalized(xMin), calcsF, calcsGradient);
            }
            
            #endregion

            #region 3.) Save results to file, for display in matlab
            //Accuracy of the mesh
            DV start = new DV(new D[] { 0, 0 }); //x1
            DV end = new DV(new D[] { 5, 5 }); //x2
            D accuracy = 0.05;

            //Create the data mesh file
            Optimization.DataGeneration.MakeDataFile(@"..\..\..\..\ObjFunctionSurfacePoints.txt", objFunc_penalized, start, end, accuracy);

            //Save descent path to file
            Optimization.DataGeneration.SaveDescentToFile(@"..\..\..\..\DescentPath.txt", xLocations, fx);
            
            
            #endregion

            Console.ReadKey();
        }
    }
}
