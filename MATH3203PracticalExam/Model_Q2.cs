using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MATH3203PracticalExam {
    internal class Model_Q2 {

        int[] demand = { 0, 3, 1, 2, 1, 2, 2, 2, 3, 2 };

        int locationCount = 10;

        int timeLimit = 6 * 60; //must return with in 6 hours

        //dist[i][j] gives the travel time (mins) from i to j
        private static int[,] travelTime = { //10 rows and 9 column, first row is from depot
			{0, 30, 50, 120, 140, 180, 120, 210, 160, 100},
            {30, 0, 50, 100, 110, 160, 120, 190, 140, 70},
            {50, 50, 0, 70, 100, 130, 70, 160, 110, 60},
            {120, 100, 70, 0, 60, 60, 60, 90, 40, 30},
            {140, 110, 100, 60, 0, 120, 120, 150, 100, 40},
            {180, 160, 130, 60, 120, 0, 100, 30, 50, 90},
            {120, 120, 70, 60, 120, 100, 0, 130, 50, 90},
            {210, 190, 160, 90, 150, 30, 130, 0, 80, 120},
            {160, 140, 110, 40, 100, 50, 50, 80, 0, 70},
            {100, 70, 60, 30, 40, 90, 90, 120, 70, 0}
        };

        private static string format = "{0, -20}{1, -20}{2, -20}{3}";

        public void FindOptimal() {
            string[] toWrite_firstLine = { "Stage", "Location", "AccumulatedValue", "SpentTime" };
            Console.WriteLine(format, toWrite_firstLine);
            GetValue(0, 0, 0, 0);
            int i = 1;
            while (true) {
                if (toWrite.ContainsKey(i)) {
                    toWrite[i].WiteLine();
                    i++;
                } else {
                    break;
                }
            }
            Console.WriteLine("OptimalValue:" + toWrite[i - 1].AccumulatedValue);
        }

        private List<int> visitedLocations = new List<int>();

        private Dictionary<int, LineInfo> toWrite = new Dictionary<int, LineInfo>();

        public int GetValue(int stageCount, int location, int accumulatedValue, int spentTime) { //the 3 arguments are states
            visitedLocations.Add(location);
            int new_accumulatedValue = accumulatedValue + demand[location];
            int location_next_best = -1;
            int value_next_best = -1;
            LineInfo? states_next_best = null;
            for (int location_next = 0; location_next < locationCount; location_next++) {//The action is to decide which location to go
                if (visitedLocations.Contains(location_next)) continue; //Don't condider already visited places
                int spentTime_next = spentTime + travelTime[location, location_next];
                if (spentTime_next + travelTime[location_next, 0] > timeLimit) {
                    continue; //If go this place cannot return on time
                }
                int value_next = GetValue(stageCount + 1, location_next, new_accumulatedValue, spentTime + travelTime[location, location_next]);
                if (value_next > value_next_best) {
                    value_next_best = value_next;
                    location_next_best = location_next;
                    states_next_best = new LineInfo(stageCount + 1, location_next, new_accumulatedValue + demand[location_next], spentTime + travelTime[location, location_next]);
                }
            }
            if (states_next_best == null) {//all non visited next place cannot return on time
                return new_accumulatedValue;
            } else {
                toWrite.Add(states_next_best.Value.Stage, states_next_best.Value);
                return value_next_best;
            }
        }

        record struct LineInfo(
            int Stage,
            int Location,
            int AccumulatedValue,
            int SpentTime) {

            internal void WiteLine() {
                Console.WriteLine(format, Stage, Location, AccumulatedValue, SpentTime);
            }
        }
    }
}
