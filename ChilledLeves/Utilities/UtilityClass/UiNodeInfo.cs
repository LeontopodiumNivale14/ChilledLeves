using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilledLeves.Utilities.UtilityClass
{
    public static unsafe partial class Utils
    {
        public static unsafe bool IsNodeVisible(string addonName, params int[] ids)
        {
            var ptr = Svc.GameGui.GetAddonByName(addonName, 1);
            if (ptr == nint.Zero)
                return false;

            var addon = (AtkUnitBase*)ptr.Address;
            var node = GetNodeByIDChain(addon->GetRootNode(), ids);
            return node != null && node->IsVisible();
        }

        /// <summary>
        /// AddonName is the name of the Window itself
        /// nodeNumbers are IN the Node List section, and they're the first [#] in the brackets
        /// Need to go down through each Nodelist Tree...
        /// </summary>
        /// <param name="addonName"></param>
        /// <param name="nodeNumbers"></param>
        /// <returns></returns>
        public static unsafe string GetNodeText(string addonName, params int[] nodeNumbers)
        {

            var ptr = Svc.GameGui.GetAddonByName(addonName, 1);

            var addon = (AtkUnitBase*)ptr.Address;
            var uld = addon->UldManager;

            AtkResNode* node = null;
            var debugString = string.Empty;
            for (var i = 0; i < nodeNumbers.Length; i++)
            {
                var nodeNumber = nodeNumbers[i];

                var count = uld.NodeListCount;

                node = uld.NodeList[nodeNumber];
                debugString += $"[{nodeNumber}]";

                // More nodes to traverse
                if (i < nodeNumbers.Length - 1)
                {
                    uld = ((AtkComponentNode*)node)->Component->UldManager;
                }
            }

            if (node->Type == NodeType.Counter)
                return ((AtkCounterNode*)node)->NodeText.ToString();

            var textNode = (AtkTextNode*)node;
            return textNode->NodeText.GetText();
        }
        private static unsafe AtkResNode* GetNodeByIDChain(AtkResNode* node, params int[] ids)
        {
            if (node == null || ids.Length <= 0)
                return null;

            if (node->NodeId == ids[0])
            {
                if (ids.Length == 1)
                    return node;

                var newList = new List<int>(ids);
                newList.RemoveAt(0);

                var childNode = node->ChildNode;
                if (childNode != null)
                    return GetNodeByIDChain(childNode, [.. newList]);

                if ((int)node->Type >= 1000)
                {
                    var componentNode = node->GetAsAtkComponentNode();
                    var component = componentNode->Component;
                    var uldManager = component->UldManager;
                    childNode = uldManager.NodeList[0];
                    return childNode == null ? null : GetNodeByIDChain(childNode, [.. newList]);
                }

                return null;
            }

            //check siblings
            var sibNode = node->PrevSiblingNode;
            return sibNode != null ? GetNodeByIDChain(sibNode, ids) : null;
        }
    }
}
