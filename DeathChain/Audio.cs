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
        Snow,
        Forest
    }

    public static class Audio
    {
        public static SoundEffect SnowSong { get; set; }
        public static SoundEffect ForestSong { get; set; }

        public static void PlaySong(Songs song) {
            SoundEffectInstance instance;
            switch(song) {
                case Songs.Snow:
                    instance = SnowSong.CreateInstance();
                    instance.IsLooped = true;
                    instance.Play();
                    break;

                case Songs.Forest:
                    instance = ForestSong.CreateInstance();
                    instance.IsLooped = true;
                    instance.Play();
                    break;
            }
        }
    }
}
