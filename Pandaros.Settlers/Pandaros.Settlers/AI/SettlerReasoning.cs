using System;
using System.Collections.Generic;

namespace Pandaros.Settlers.AI
{
    public static class SettlerReasoning
    {
        private static readonly Random rnd = new Random();

        private static readonly List<string> _massacre = new List<string>
        {
            "We heard about the massacre here. We are here to help to rebuild! {0} settlers have joined your colony."
        };

        private static readonly List<string> _noJob = new List<string>
        {
            "I wish there was a job for me to do.. I guess I will have to find another colony.",
            "I dont feel like I am contrubuting... off to the next chapter in my life.",
            "Why are there no jobs? I'm out of here!",
            "BORING! CYA!"
        };

        private static readonly List<string> _settleReasons = new List<string>
        {
            "Like field of dreams....If you build it, they will come. {0} settlers have decided to join your colony.",
            "It was a long arduous journey. My family of {0} need someplace to stay.",
            "Finally safe from the Zombies! My group of {0} is so grateful for your help!",
            "We thought we where alone! {0} settlers have decided to join your colony."
        };

        private static readonly List<string> _needBed = new List<string>
        {
            "I'm really tired I need somewhere to sleep.",
            "I wish I was laying on something soft...",
            "I cannot sleep standing up!",
            "Soooooooo Tired."
        };

        private static readonly List<string> _noBed = new List<string>
        {
            "I can't take it anymore. Im leaving to find a bed.",
            "This place was nice up until I had nowhere to sleep. I have to go.",
            "WHERE ALL ALL THE BEDS? I'm out of here!"
        };

        public static string GetSettleReason()
        {
            return _settleReasons[rnd.Next(0, _settleReasons.Count)];
        }

        public static string GetNoJobReason()
        {
            return _noJob[rnd.Next(0, _noJob.Count)];
        }

        public static string GetNoBed()
        {
            return _noBed[rnd.Next(0, _noBed.Count)];
        }


        public static string GetNeedBed()
        {
            return _needBed[rnd.Next(0, _needBed.Count)];
        }

        public static string GetMassacre()
        {
            return _massacre[rnd.Next(0, _massacre.Count)];
        }
    }
}