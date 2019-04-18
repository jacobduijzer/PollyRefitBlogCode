using System;

namespace PollyRefitTest.Logging
{
    public interface ILogger
    {
        void Debug(string message);

        void Debug(Exception exception, string message);
    }
}
