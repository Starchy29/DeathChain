using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace DeathChain
{
    public enum Songs {
        Snow
    }

    public static class Audio
    {
        public static SoundEffect SnowSong { get; set; }

        public static void PlaySong(Songs song) {
            switch(song) {
                case Songs.Snow:
                    SoundEffectInstance instance = SnowSong.CreateInstance();
                    instance.IsLooped = true;
                    instance.Play();
                    break;
            }
        }
    }
}
