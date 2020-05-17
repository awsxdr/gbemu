namespace GBEmu
{
    using System;

    public class Timer : Ram
    {
        private bool _isTimerRunning = false;
        private int _timerTickFrequency = 0;

        public Timer(Cpu cpu) : base(0x03)
        {
            WatchWrite(0x00, (a, v) =>
            {
                if (v != 0) return;
                cpu.Interrupt(Interrupts.Timer);
                _memory[0x00] = _memory[0x01];
            });

            WatchWrite(0x02, (a, v) =>
            {
                _isTimerRunning = (v & 0b100) > 0;
                _timerTickFrequency = v & 0b11 switch
                {
                    0b00 => 1024,
                    0b01 => 16,
                    0b10 => 64,
                    0b11 => 256,
                    _ => throw new Exception("Stop complaining Visual Studio, this can't happen")
                };
            });

        }

        public void ConnectClock(Clock clock)
        {
            var tickCount = 0;

            clock.Tick += () =>
            {
                if (!_isTimerRunning) return;

                if(++tickCount == _timerTickFrequency)
                {
                    tickCount = 0;

                    Write(0x00, (byte)(Read(0x00) + 1));
                }

            };
        }
    }
}