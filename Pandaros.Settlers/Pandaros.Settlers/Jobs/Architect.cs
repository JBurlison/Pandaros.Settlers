using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Jobs
{
    //you call AreaJobTracker.SendData(player), then that triggers the OnSendAreaHighlights callback in which you can add any area you want. It'll show up for the player till the callback happens again
    //or you add an actual area to the areajobtracker & then call SendData(player), it'll automatically get the data from that area then
    public class Architect
    {
    }
}
