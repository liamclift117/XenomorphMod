using RoR2.ContentManagement;
using UnityEngine;
using RoR2;
using System.Collections;
using R2API;
using R2API.Utils;
using RoR2.Skills;
using System.Collections.Generic;
using System.Linq;
using System;
using RoR2.Projectile;
using static RoR2.DotController;
using MonoMod.RuntimeDetour.HookGen;
using static RoR2.RoR2Content;
using ShaderSwapper;
using ThreeEyedGames;

namespace Xenomorph
{
    public class XenomorphContent : IContentPackProvider
    {
        public string identifier => XenomorphMain.GUID;

        public static ReadOnlyContentPack readOnlyContentPack => new ReadOnlyContentPack(XenomorphContentPack);
        internal static ContentPack XenomorphContentPack { get; } = new ContentPack();

        //private static ItemDef _myItem;
        private static SkillDef _primarySkill;
        private static SkillFamily _primarySkillFamily;
        private static SkillDef _secondarySkill;
        private static SkillFamily _secondarySkillFamily;
        private static SkillDef _utilitySkill;
        private static SkillFamily _utilitySkillFamily;
        private static SkillDef _specialSkill;
        private static SkillFamily _specialSkillFamily;
        private static EntityStateConfiguration _clawSwipeESC;
        private static EntityStateConfiguration _tailStabESC;
        private static EntityStateConfiguration _xenoBaseESC;
        private static EntityStateConfiguration _xenoDeathESC;
        private static EntityStateConfiguration _xenoSpawnESC;
        private static EntityStateConfiguration _xenoLeapESC;
        private static EntityStateConfiguration _xenoClimbESC;
        private static EntityStateConfiguration _xenoBiteESC;
        private static SurvivorDef _survivor;
        private static GameObject _characterBody;
        private static GameObject _lobbyBody;
        private static GameObject _acidBloodPrefab;
        private static GameObject _acidBloodPoolPrefab;
        private static GameObject _acidBloodGhostPrefab;
        private static GameObject _acidBloodPoolGhostPrefab;
        private static BuffDef _acidBloodBuff;
        private static BuffDef _headbiteBuff;
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
            _primarySkill = _myBundle.LoadAsset<SkillDef>("sdXenomorphPrimary");
            _primarySkillFamily = _myBundle.LoadAsset<SkillFamily>("sfXenomorphPrimary");
            _secondarySkill = _myBundle.LoadAsset<SkillDef>("sdXenomorphSecondary");
            _secondarySkillFamily = _myBundle.LoadAsset<SkillFamily>("sfXenomorphSecondary");
            _utilitySkill = _myBundle.LoadAsset<SkillDef>("sdXenomorphUtility");
            _utilitySkillFamily = _myBundle.LoadAsset<SkillFamily>("sfXenomorphUtility");
            _specialSkill = _myBundle.LoadAsset<SkillDef>("sdXenomorphSpecial");
            _specialSkillFamily = _myBundle.LoadAsset<SkillFamily>("sfXenomorphSpecial");
            _survivor = _myBundle.LoadAsset<SurvivorDef>("Xenomorph");
            _characterBody = _myBundle.LoadAsset<GameObject>("XenomorphBody");
            _lobbyBody = _myBundle.LoadAsset<GameObject>("XenomorphDisplayBody");
            _acidBloodPrefab = _myBundle.LoadAsset<GameObject>("AcidBloodPrefab");
            _acidBloodPoolPrefab = _myBundle.LoadAsset<GameObject>("AcidBloodPoolPrefab");
            _acidBloodGhostPrefab = _myBundle.LoadAsset<GameObject>("AcidBloodGhost");
            _acidBloodPoolGhostPrefab = _myBundle.LoadAsset<GameObject>("AcidBloodPoolGhost");
            _clawSwipeESC = _myBundle.LoadAsset<EntityStateConfiguration>("ClawSwipeState");
            _tailStabESC = _myBundle.LoadAsset<EntityStateConfiguration>("TailStabState");
            _xenoBaseESC = _myBundle.LoadAsset<EntityStateConfiguration>("XenoBaseState");
            _xenoDeathESC = _myBundle.LoadAsset<EntityStateConfiguration>("XenoDeathState");
            _xenoSpawnESC = _myBundle.LoadAsset<EntityStateConfiguration>("XenoSpawnState");
            _xenoLeapESC = _myBundle.LoadAsset<EntityStateConfiguration>("XenoLeapState");
            _xenoClimbESC = _myBundle.LoadAsset<EntityStateConfiguration>("XenoClimbState");
            _xenoBiteESC = _myBundle.LoadAsset<EntityStateConfiguration>("HeadbiteState");
            _acidBloodBuff = _myBundle.LoadAsset<BuffDef>("AcidBloodBuff");
            _headbiteBuff = _myBundle.LoadAsset<BuffDef>("HeadbiteBuff");
            //var expansionDef = _myBundle.LoadAsset<RoR2.ExpansionManagement.ExpansionDef>("XenomorphExpansion");
            yield return _myBundle.UpgradeStubbedShadersAsync();
            if (_acidBloodPoolPrefab.GetComponentInChildren<Decal>())
            {
                _acidBloodPoolPrefab.GetComponentInChildren<Decal>().RenderMode = Decal.DecalRenderMode.Deferred;
            }
            if (_acidBloodPoolGhostPrefab.GetComponentInChildren<Decal>())
            {
                _acidBloodPoolGhostPrefab.GetComponentInChildren<Decal>().RenderMode = Decal.DecalRenderMode.Deferred;
            }

            if (_characterBody.GetComponentInChildren<FootstepHandler>() && _characterBody.GetComponentInChildren<FootstepHandler>().footstepDustPrefab==null)
            {
                _characterBody.GetComponentInChildren<FootstepHandler>().footstepDustPrefab = RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/FootstepEffects/DefaultFootstepEffect");
            }

            XenomorphDamageTypes.AcidBloodDotIndex = DotAPI.RegisterDotDef(0.25f, 0.25f, DamageColorIndex.Poison, _acidBloodBuff);
            R2API.DamageAPI.AddModdedDamageType(ref _acidBloodPrefab.GetComponent<ProjectileDamage>().damageType,XenomorphDamageTypes.AcidBlood);
            R2API.DamageAPI.AddModdedDamageType(ref _acidBloodPoolPrefab.GetComponent<ProjectileDamage>().damageType, XenomorphDamageTypes.AcidBlood);

            _survivor.bodyPrefab = _characterBody;
            XenomorphContentPack.skillDefs.Add(new SkillDef[] { _primarySkill,_secondarySkill, _utilitySkill, _specialSkill });
            XenomorphContentPack.skillFamilies.Add(new SkillFamily[] { _primarySkillFamily, _secondarySkillFamily, _utilitySkillFamily, _specialSkillFamily });
            XenomorphContentPack.survivorDefs.Add(new SurvivorDef[] { _survivor });
            XenomorphContentPack.bodyPrefabs.Add(new GameObject[] { _survivor.bodyPrefab });
            XenomorphContentPack.entityStateConfigurations.Add(new EntityStateConfiguration[] { _clawSwipeESC,
                _tailStabESC, 
                _xenoBaseESC, 
                _xenoDeathESC, 
                _xenoSpawnESC, 
                _xenoLeapESC, 
                _xenoClimbESC,
                _xenoBiteESC});
            XenomorphContentPack.entityStateTypes.Add(new Type[] { typeof(XenomorphSkills.XenomorphStates.ClawSwipeState),
                typeof(XenomorphSkills.XenomorphStates.TailStabState),
                typeof(XenomorphSkills.XenomorphStates.SpawnState),
                typeof(XenomorphSkills.XenomorphStates.XenoBaseState),
                typeof(XenomorphSkills.XenomorphStates.DeathState),
                typeof(XenomorphSkills.XenomorphStates.XenoLeapState),
                typeof(XenomorphSkills.XenomorphStates.XenoClimbState),
                typeof(XenomorphSkills.XenomorphStates.HeadbiteState) });
            XenomorphContentPack.projectilePrefabs.Add(new GameObject[] { _acidBloodPrefab, _acidBloodPoolPrefab });
            XenomorphContentPack.buffDefs.Add(new BuffDef[] { _acidBloodBuff, _headbiteBuff });
            XenomorphContentPack.effectDefs.Add(new EffectDef[] {
                new EffectDef(_myBundle.LoadAsset<GameObject>("AcidBloodImpact")),
                new EffectDef(_myBundle.LoadAsset<GameObject>("ClawHitEffectPrefab")),
                new EffectDef(_myBundle.LoadAsset<GameObject>("ClawSwipeEffect")),
                new EffectDef(_myBundle.LoadAsset<GameObject>("ClawSwipeEffect2Left")),
                new EffectDef(_myBundle.LoadAsset<GameObject>("ClawSwipeEffect2Right")),
                new EffectDef(_myBundle.LoadAsset<GameObject>("TailStabEffectPrefab")),
                new EffectDef(_myBundle.LoadAsset<GameObject>("TailReadyEffect")),
                new EffectDef(_myBundle.LoadAsset<GameObject>("TailHitEffectPrefab")),
                new EffectDef(_myBundle.LoadAsset<GameObject>("HeadbiteEffect")),
                //new EffectDef(_myBundle.LoadAsset<GameObject>("AcidBloodTrail"))
                //new EffectDef(_myBundle.LoadAsset<GameObject>("AcidBloodGhost")),
                //new EffectDef(_myBundle.LoadAsset<GameObject>("AcidBloodPoolGhost"))
            });
            //XenomorphContentPack.expansionDefs.Add(new RoR2.ExpansionManagement.ExpansionDef[] { expansionDef });

        }
        public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
             ContentPack.Copy(XenomorphContentPack, args.output);
            args.ReportProgress(1f);
            yield break;
        }
        public IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
            R2API.RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;

            args.ReportProgress(1f);
            yield break;
        
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody self, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (!self) { return; }
            if (self.HasBuff(_acidBloodBuff))
            {
                args.armorAdd -= 40f;
            }
            if (self.HasBuff(_headbiteBuff))
            {
                args.baseHealthAdd+=10f*self.GetBuffCount(_headbiteBuff);
            }
        }

        private void GlobalEventManager_onServerDamageDealt(DamageReport obj)
        {
            if(obj.damageInfo.dotIndex == DotIndex.None && obj.victimBody.baseNameToken == "LIAMXENOMORPH_XENOMORPH_BODY_NAME")
            {
                DotController.InflictDot(obj.victimBody.gameObject, obj.attackerBody.gameObject, DotIndex.Bleed, 5f);
            }
            if(obj.damageInfo.HasModdedDamageType(XenomorphDamageTypes.AcidBlood) && Util.CheckRoll(100f * obj.damageInfo.procCoefficient, obj.attackerBody.master))
            {
                DotController.InflictDot(obj.victim.gameObject, obj.attacker.gameObject, XenomorphDamageTypes.AcidBloodDotIndex, 3f);
            }
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

    public static class XenomorphDamageTypes
    {
        public static DamageAPI.ModdedDamageType AcidBlood = DamageAPI.ReserveDamageType();
        public static DotController.DotIndex AcidBloodDotIndex;
    }
}
