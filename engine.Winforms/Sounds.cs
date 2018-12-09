using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using engine.Common;

namespace engine.Winforms
{
    public class Sounds : ISounds
    {
        public Sounds(IntPtr hwnHandle)
        {
            HwnHandle = hwnHandle;
        }

        public void Play(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return;

            SoundPlayer player = null;
            if (!All.TryGetValue(path, out player))
            {
                player = new SoundPlayer();
                player.SoundLocation = path;
                All.Add(path, player);
            }
            player.Play();
        }

        public void PlayMusic(string path, bool repeat)
        {
            var alias = "music";

            if (PlayingMusic)
            {
                // close the current then restart
                CheckError(
                    mciSendString(
                        string.Format("stop {0}", alias),
                        null,
                        0,
                        IntPtr.Zero),
                    "stop");
                CheckError(
                    mciSendString(
                        string.Format("close {0}", alias),
                        null,
                        0,
                        IntPtr.Zero),
                    "close");
            }

            // retain details
            PlayingMusic = true;
            PlayingMusicPath = path;

            // play music
            CheckError(
                mciSendString(
                string.Format("open {0} alias {1}", path, alias),
                null,
                0,
                IntPtr.Zero),
            "open");
            CheckError(
                mciSendString(
                    string.Format("play {0} notify", alias),
                    null,
                    0,
                    (repeat) ? HwnHandle : IntPtr.Zero),
                "play");
        }

        public void Repeat()
        {
            if (!PlayingMusic) return;

            var alias = "music";

            CheckError(
                mciSendString(
                    string.Format("seek {0} to 0", alias),
                    null,
                    0,
                    IntPtr.Zero),
                "seek");

            CheckError(
                mciSendString(
                    string.Format("play {0} notify", alias),
                    null,
                    0,
                    HwnHandle),
                "play");
        }

        #region private
        private static Dictionary<string, SoundPlayer> All = new Dictionary<string, SoundPlayer>();
        private IntPtr HwnHandle;
        private bool PlayingMusic = false;
        private string PlayingMusicPath = "";

        [DllImport("winmm.dll")]
        static extern Int32 mciSendString(string command, string buffer, int bufferSize, IntPtr hwndCallback);

        [DllImport("winmm.dll")]
        static extern Int32 mciGetErrorString(Int32 errorCode, StringBuilder errorText, Int32 errorTextSize);

        private void CheckError(Int32 err, string command)
        {
            if (err != 0)
            {
                var sb = new StringBuilder(2048);
                mciGetErrorString(err, sb, sb.Capacity);
                System.Diagnostics.Debug.WriteLine("{0} failed with : {1}", command, sb.ToString());
            }
        }
        #endregion
    }
}
