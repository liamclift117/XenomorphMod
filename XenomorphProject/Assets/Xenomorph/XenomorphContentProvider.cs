using RoR2.ContentManagement;
using UnityEngine;
using RoR2;
using System.Collections;
using R2API;
using R2API.Utils;
namespace Xenomorph
{
    public class XenomorphContent : IContentPackProvider
    {
        public string identifier => XenomorphMain.GUID;

        public static ReadOnlyContentPack readOnlyContentPack => new ReadOnlyContentPack(XenomorphContentPack);
        internal static ContentPack XenomorphContentPack { get; } = new ContentPack();

        private static ItemDef _myItem;
        private static AssetBundle _myBundle;

        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            var asyncOperation = AssetBundle.LoadFromFileAsync(XenomorphMain.assetBundleDir);
            while(!asyncOperation.isDone)
            {
                args.ReportProgress(asyncOperation.progress);
                yield return null;
            }

            //Write code here to initialize your mod post assetbundle load
            _myBundle = asyncOperation.assetBundle;
            _myItem = _myBundle.LoadAsset<ItemDef>("MyItem");
            var expansionDef = _myBundle.LoadAsset<RoR2.ExpansionManagement.ExpansionDef>("XenomorphExpansion");

            XenomorphContentPack.itemDefs.Add(new ItemDef[] { _myItem });
            XenomorphContentPack.expansionDefs.Add(new RoR2.ExpansionManagement.ExpansionDef[] { expansionDef });
        }
        public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
             ContentPack.Copy(XenomorphContentPack, args.output);
            args.ReportProgress(1f);
            yield break;
        }
        public IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            R2API.RecalculateStatsAPI.GetStatCoefficients += (body, statArgs) =>
            {
                if (!body.inventory)
                    return;

                int count = body.inventory.GetItemCount(_myItem);
                statArgs.attackSpeedMultAdd += 0.1f * count;
                statArgs.critDamageMultAdd += 0.1f * count;
                statArgs.damageMultAdd += 0.1f * count;
                statArgs.healthMultAdd += 0.1f * count;
                statArgs.jumpPowerMultAdd += 0.1f * count;
                statArgs.moveSpeedMultAdd += 0.1f * count;
                statArgs.regenMultAdd += 0.1f * count;
                statArgs.shieldMultAdd += 0.1f * count;
            };

            args.ReportProgress(1f);
            yield break;
        }
        private void AddSelf(ContentManager.AddContentPackProviderDelegate addContentPackProvider)
        {
            addContentPackProvider(this);
        }
        internal XenomorphContent()
        {
            ContentManager.collectContentPackProviders += AddSelf;
        }
    }
}
