using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gurobi;
using static MATH3203PracticalExam.Common;

namespace MATH3203PracticalExam {
    internal class Model_Q1 {
        public Model_Q1(bool considerUnbalance = false) {
            this.considerUnbalance = considerUnbalance;
        }

        private readonly bool considerUnbalance;

        private static string[] array_truckType = { "Van", "Small Truck", "Medium Truck", "Large Truck" };
        private static int truckTypeCount => array_truckType.Length;
        private static float[] array_truckTypeValue = { 450, 600, 1000, 1800 };
        private static float[] array_truckTypeLength = { 4.5f, 7, 10, 15 };
        private static float[] array_truckTypeMass = { 1.5f, 2.5f, 5, 9 };
        private static int[] array_truckTypeTotal = { 6, 8, 7, 9 }; // maximum number of truck type to load

        private static int[] array_laneLength = { 40, 40, 40, 40, 35 };
        private static int laneCount => array_laneLength.Length;

        public void FindOptimal() {
            GRBModel model = new(Env);

            GRBVar[,] variables_lane_truckType_amount = new GRBVar[laneCount, truckTypeCount];

            for (int i_lane = 0; i_lane < laneCount; i_lane++) {
                for (int i_truckType = 0; i_truckType < truckTypeCount; i_truckType++) {
                    variables_lane_truckType_amount[i_lane, i_truckType] = model.AddVar(0, array_truckTypeTotal[i_truckType], default, GRB.INTEGER, "");
                }
            }

            GRBLinExpr expression_totalMass = new GRBLinExpr();
            GRBLinExpr[] expressions_lane_totalLength = new GRBLinExpr[laneCount];
            GRBLinExpr expression_totalValue = new GRBLinExpr();
            GRBLinExpr[] expressions_lane_totalMass = new GRBLinExpr[laneCount];
            for (int i_lane = 0; i_lane < laneCount; i_lane++) {
                GRBLinExpr expression_currentLane_totalLength = new GRBLinExpr();
                expressions_lane_totalLength[i_lane] = expression_currentLane_totalLength;
                GRBLinExpr expression_currentLane_totalMass = new GRBLinExpr();
                expressions_lane_totalMass[i_lane] = expression_currentLane_totalMass;
                for (int i_truckType = 0; i_truckType < truckTypeCount; i_truckType++) {
                    expression_currentLane_totalLength.AddTerm(array_truckTypeLength[i_truckType], variables_lane_truckType_amount[i_lane, i_truckType]);
                    expression_currentLane_totalMass.AddTerm(array_truckTypeMass[i_truckType], variables_lane_truckType_amount[i_lane, i_truckType]);
                    expression_totalValue.AddTerm(array_truckTypeValue[i_truckType], variables_lane_truckType_amount[i_lane, i_truckType]);
                }
                model.AddConstr(expression_currentLane_totalLength <= array_laneLength[i_lane], "");
                expression_totalMass.Add(expression_currentLane_totalMass);
            }
            model.AddConstr(expression_totalMass <= 120, "");

            if (considerUnbalance) {
                GRBLinExpr leftTotalMass = new GRBLinExpr();
                leftTotalMass.Add(expressions_lane_totalMass[0] * 2);
                leftTotalMass.Add(expressions_lane_totalMass[1] * 1);
                GRBLinExpr rightTotalMass = new GRBLinExpr();
                rightTotalMass.Add(expressions_lane_totalMass[4] * 2);
                rightTotalMass.Add(expressions_lane_totalMass[3] * 1);
                model.AddConstr(leftTotalMass - rightTotalMass <= leftTotalMass * 0.05, ""); //If left side larger, difference <= 5% of larger, if right side larger, always true
                model.AddConstr(rightTotalMass - leftTotalMass <= rightTotalMass * 0.05, "");
            }

            model.SetObjective(expression_totalValue, GRB.MAXIMIZE);
            model.Optimize();
            Console.WriteLine("OptimalValue:" + model.ObjVal);

            string format = "{0, -20}{1, -20}{2, -20}{3, -20}{4, -20}{5}";
            string[] toWrite_firstLine = (new string[] { "TruckType" }.Concat(Enumerable.Range(0, laneCount).Select(i => "Lane" + i))).ToArray();
            Console.WriteLine(format, toWrite_firstLine);
            for (int i_truckType = 0; i_truckType < truckTypeCount; i_truckType++) {
                string[] toWrite = (new string[] { array_truckType[i_truckType] }.Concat(Enumerable.Range(0, laneCount).Select(i_lane => variables_lane_truckType_amount[i_lane, i_truckType].X.ToString()))).ToArray();
                Console.WriteLine(format, toWrite);
            }
        }
    }
}
