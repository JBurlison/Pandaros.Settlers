using Science;
using System;

namespace Pandaros.Settlers.Research
{
    public class ResearchCompleteEventArgs : EventArgs
    {
        public ResearchCompleteEventArgs(PandaResearch research, ColonyScienceState player)
        {
            Research = research;
            Manager = player;
        }

        public PandaResearch Research { get; }

        public ColonyScienceState Manager { get; }
    }
}
