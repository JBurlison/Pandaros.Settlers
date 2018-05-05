using System;
using System.IO;
using System.Collections.Generic;
using Pipliz;
using Pipliz.JSON;
using Pipliz.Threading;
using NPC;
using System.Text;

namespace Pandaros.Settlers
{
    [ModLoader.ModManager]
    public static class AutoLoad
    {
        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnTryChangeBlock, GameLoader.NAMESPACE + ".AutoLoad.trychangeblock")]
        public static void OnTryChangeBlockUser(ModLoader.OnTryChangeBlockData userData)
        {
            if (userData.CallbackState == ModLoader.OnTryChangeBlockData.ECallbackState.Cancelled)
                return;

            if (userData.CallbackOrigin == ModLoader.OnTryChangeBlockData.ECallbackOrigin.ClientPlayerManual)
            {
                VoxelSide side = userData.PlayerClickedData.VoxelSideHit;
                ushort newType = userData.TypeNew;
                string suffix = string.Empty;

                switch (side)
                {
                    case VoxelSide.xPlus:
                        suffix = "right";
                        break;

                    case VoxelSide.xMin:
                        suffix = "left";
                        break;

                    case VoxelSide.yPlus:
                        suffix = "bottom";
                        break;

                    case VoxelSide.yMin:
                        suffix = "top";
                        break;

                    case VoxelSide.zPlus:
                        suffix = "front";
                        break;

                    case VoxelSide.zMin:
                        suffix = "back";
                        break;
                }

                if (newType != userData.TypeOld && ItemTypes.IndexLookup.TryGetName(newType, out string typename))
                {
                    string otherTypename = typename + suffix;

                    if (ItemTypes.IndexLookup.TryGetIndex(otherTypename, out ushort otherIndex))
                    {
                        Vector3Int position = userData.Position;
                        ThreadManager.InvokeOnMainThread(delegate () {
                            ServerManager.TryChangeBlock(position, otherIndex);
                        }, 0.1f);
                    }
                }
            }
        }
    }
}
