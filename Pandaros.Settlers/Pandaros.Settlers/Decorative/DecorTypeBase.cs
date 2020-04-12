using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pandaros.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Decorative
{
    public class DecorTypeBase : CSType
    {
        public override List<string> categories { get; set; } = new List<string>();
        public override Colliders colliders { get; set; } = new Colliders()
        {
            collidePlayer = true,
            collideSelection = true
        };
        public override int? maxStackSize => 500;
        public override bool? isPlaceable => true;
        public override bool? needsBase => false;
        public override bool? isRotatable => true;

        public override JObject customData { get; set; } = JsonConvert.DeserializeObject<JObject>("{ \"useHeightMap\": true, \"useNormalMap\": true }");
        public override string mesh { get; set; }
        public override string sideall { get; set; }
        public override string icon { get; set; }

    }
}
