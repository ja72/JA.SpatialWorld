using System;
using System.Collections.Generic;
using System.Linq;

namespace JA.Mathematics.Spatial.Dynamics
{
    public class Frame3
    {
        public Frame3(double time, ObjState[] states)
        {
            Time=time;
            States=states;
        }
        public int Count { get => States.Length; }
        public double Time { get; set; }
        public ObjState[] States { get; set; }
        public static Frame3 Step(Frame3 frame, double timeStep, ObjRate[] rates)
        {
            if (rates.Length!=frame.Count)
            {
                throw new ArgumentException(nameof(rates));
            }
            ObjState[] next = new ObjState[frame.Count];
            for (int i = 0; i < next.Length; i++)
            {
                next[i] = frame.States[i].Step(timeStep, rates[i]);
            }
            return new Frame3(frame.Time + timeStep, next);
        }
        public Frame3 Step(double timeStep, ObjRate[] rates) 
            => Step(this, timeStep, rates);

        public static Frame3 Integrate(double timeStep, Frame3 frame, RateFunction rate)
        {
            var K0 = rate(frame);

            var s1 = frame.Step(timeStep/2, K0);
            var K1 = rate(s1);

            var s2 = frame.Step(timeStep/2, K1);
            var K2 = rate(s2);

            var s3 = frame.Step(timeStep/2, K2);
            var K3 = rate(s3);

            ObjState[] next = new ObjState[frame.Count];
            for (int i = 0; i < next.Length; i++)
            {
                var K = K0[i]/6 + K1[i]/3 + K2[i]/3 + K3[i]/6;
                next[i] = frame.States[i].Step(timeStep, K);
            }
            return new Frame3(frame.Time+timeStep, next);
        }

        public Frame3 Integrate(double timestep, RateFunction rate)
        {
            return Integrate(timestep, this, rate);
        }
    }
}
