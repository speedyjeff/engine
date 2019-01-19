using System;
using System.IO;

namespace engine.Common
{
    public interface ISounds
    {
        void Play(string path);
        void Play(string name, Stream stream);
        void PlayMusic(string path, bool repeat);
        void Repeat();
    }
}
