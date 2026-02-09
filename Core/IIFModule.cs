using InfiniteImbueFramework.Configuration;
using ThunderRoad;
using UnityEngine;

namespace InfiniteImbueFramework
{
    public class IIFModule : ThunderScript
    {
        private bool initialCatalogProbeDone;

        public override void ScriptEnable()
        {
            IIFLog.Info($"Infinite Imbue Framework v{IIFModOptions.VERSION} enabled.");
        }

        public override void ScriptUpdate()
        {
            IIFDiagnostics.Update();
            if (!initialCatalogProbeDone && Time.unscaledTime > 2f)
            {
                initialCatalogProbeDone = true;
                ProbeTestItems();
            }
        }

        public override void ScriptDisable()
        {
            IIFLog.Info("Infinite Imbue Framework disabled.");
        }

        private static void ProbeTestItems()
        {
            ProbeItem("DaggerCommon");
            ProbeItem("ThrowablesDagger");
        }

        private static void ProbeItem(string itemId)
        {
            ItemData data = Catalog.GetData<ItemData>(itemId, false);
            if (data == null)
            {
                IIFLog.Warn($"Catalog probe: item not found: {itemId}", true);
                return;
            }

            if (data.TryGetModule<ItemModuleInfiniteImbue>(out ItemModuleInfiniteImbue module))
            {
                int spellCount = module.spells?.Count ?? 0;
                IIFLog.Info($"Catalog probe: module attached on {itemId}. spells={spellCount} keepFilled={module.keepFilled}", true);
            }
            else
            {
                IIFLog.Warn($"Catalog probe: module missing on {itemId}", true);
            }
        }
    }
}
