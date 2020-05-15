namespace GBEmu
{
    using System.Diagnostics;

    public class Clock
    {
        public delegate void TickEvent();
        public event TickEvent Tick;

        public void Start()
        {
            while (true)
            {
                var i = 0;
                var stopwatch = Stopwatch.StartNew();
                while (true)
                {
                    ++i;
                    if (stopwatch.ElapsedMilliseconds > 1000)
                    {
                        Debug.WriteLine($"Clock frequency: {i/1000000.0:F2}MHz");
                        i = 0;
                        stopwatch.Restart();
                    }

                    Tick?.Invoke();
                }
            }
        }
    }
}