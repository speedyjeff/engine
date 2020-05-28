using engine.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace engine.Blazor
{
    public class BlazorSounds : ISounds
    {
        // todo https://stackoverflow.com/questions/60006592/play-sound-on-the-client-in-blazor

        public void Play(string path)
        {
            throw new NotImplementedException("Play is not implemented");
        }

        public void Play(string name, Stream stream)
        {
            throw new NotImplementedException("Play is not implemented");
        }

        public void PlayMusic(string path, bool repeat)
        {
            throw new NotImplementedException("PlayMusic is not implemented");
        }

        public void Repeat()
        {
            throw new NotImplementedException("Repeat is not implemented");
        }
    }
}
