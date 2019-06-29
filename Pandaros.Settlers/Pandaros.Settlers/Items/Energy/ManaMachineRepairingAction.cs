using Pandaros.Settlers.Items.Machines;
using Pandaros.Settlers.Jobs.Roaming;
using Pandaros.Settlers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Energy
{
    public class ManaMachineRepairingAction : IRoamingJobObjectiveAction
    {
        public float TimeToPreformAction => 10;

        public string AudioKey => GameLoader.NAMESPACE + ".SpellRepair";

        public ItemId ObjectiveLoadEmptyIcon => ItemId.GetItemId(GameLoader.NAMESPACE + ".Repairing");

        public string name => GameLoader.NAMESPACE + ".ManaMachineRepair";

        public ItemId PreformAction(Colony colony, RoamingJobState state)
        {
            var retval = ItemId.GetItemId(GameLoader.NAMESPACE + ".Repairing");

            if (state.GetActionEnergy(GameLoader.NAMESPACE + ".ManaMachineRepair") < .50f)
            {
                var repaired = false;
                var requiredForFix = new List<InventoryItem>();
                var stockpile = colony.Stockpile;

                requiredForFix.Add(new InventoryItem(SettlersBuiltIn.ItemTypes.ADAMANTINE.Id));
                requiredForFix.Add(new InventoryItem(SettlersBuiltIn.ItemTypes.MAGICWAND.Id));
                requiredForFix.Add(new InventoryItem(ColonyBuiltIn.ItemTypes.COATEDPLANKS.Id));
                requiredForFix.Add(new InventoryItem(SettlersBuiltIn.ItemTypes.REFINEDEMERALD.Id));
                requiredForFix.Add(new InventoryItem(SettlersBuiltIn.ItemTypes.REFINEDSAPPHIRE.Id));
                requiredForFix.Add(new InventoryItem(SettlersBuiltIn.ItemTypes.REFINEDRUBY.Id));

                if (stockpile.Contains(requiredForFix))
                {
                    stockpile.TryRemove(requiredForFix);
                    repaired = true;
                }
                else
                {
                    foreach (var item in requiredForFix)
                        if (!stockpile.Contains(item))
                        {
                            retval = ItemId.GetItemId(item.Type);
                            break;
                        }
                }

                if (repaired)
                    state.ResetActionToMaxLoad(GameLoader.NAMESPACE + ".ManaMachineRepair");
            }

            return retval;
        }
    }
}
