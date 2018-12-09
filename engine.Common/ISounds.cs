using System;
using System.Collections.Generic;
using System.Text;

namespace engine.Common
{
    public interface ISounds
    {
        void Play(string path);
        void PlayMusic(string path, bool repeat);
        void Repeat();
    }
}
