using engine.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace engine.Maui
{
    public class MauiSounds : ISounds
    {
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
