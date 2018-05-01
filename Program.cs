using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace FiveThirtyEight_Repeati
{
    class Program
    {
        static void Main(string[] args)
        {
            double startTime = 1d / 30;
            double endTime = 6d;

            List<Task<Result>> taskList = new List<Task<Result>>();

            for (double j = 1/30d; j < 3d; j += startTime)
            {
                var setup = j;
                for (double i = startTime; i <= endTime; i += 1d/30)
                {
                    var step = i;
                    var rep = new RepeatCalc(setup, step);
                    taskList.Add(Task.Run(() => rep.Calc()));
                } 
            }

            Task.WaitAll(taskList.ToArray());

            var results = new Result[taskList.Count];
            for (int i = 0; i < taskList.Count; i++)
            {
                var res = taskList[i].Result;
                results[i] = res;
            }
            string[] strResults = results.Select(res => $"{res.setupTime}, {res.holdTime}, {res.time}").ToArray();
            

            File.WriteAllLines(@"results.csv", strResults);
        }
    }

    public class RepeatCalc
    {
        const int TARGET = 1_000_000;
        const double HOLD_DELAY = 0.5d;
        const double REPEAT_RATE = 30d;
        const double SMALL_STEP = 1d / 30;

        private double _setupTime;
        private double _holdTime;
        private double _time;
        private double _count;

        public RepeatCalc(double setupTime, double holdTime)
        {
            _setupTime = setupTime;
            _holdTime = holdTime;
            _time = 0;
            _count = 0;
        }

        public Result Calc()
        {
            InitialSetup();
            while (true)
            {
                double copyAmount = _count;
                CopyPaste();
                if (_count > TARGET) break;
                HoldPaste(copyAmount);
            }
            //Console.WriteLine($"{_holdTime}, {_time}");
            return new Result
            {
                setupTime = _setupTime,
                holdTime = _holdTime,
                time = _time
            };
        }

        private void HoldPaste(double amount)
        {
            _time += HOLD_DELAY;

            for (double i = _holdTime; i > 0.001; i -= SMALL_STEP)
            {
                _count += amount;
                _time += SMALL_STEP;

                if (_count > TARGET)
                {
                    break;
                }
            }
        }

        private void CopyPaste()
        {
            _time += 1;
            _count *= 2;
        }

        private void InitialSetup()
        {
            _count = 1;
            _time = HOLD_DELAY; 
            for (double i = _setupTime; i > 0.001; i -= SMALL_STEP)
            {
                _count ++;
                _time += SMALL_STEP;
            }
        }
    }

    public class Result
    {
        public double setupTime;
        public double holdTime;
        public double time;
    }
}
