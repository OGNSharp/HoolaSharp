using System;
using System.Linq;
using System.Windows.Forms.VisualStyles;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;
using ItemData = LeagueSharp.Common.Data.ItemData;
using SharpDX;


namespace HoolaMasterYi
{
    public class Program
    {
        private static Menu Menu;
        private static Orbwalking.Orbwalker Orbwalker;
        private static readonly Obj_AI_Hero Player = ObjectManager.Player;
        private static readonly HpBarIndicator Indicator = new HpBarIndicator();
        private static Spell Q, W, E, R;
        private static bool AutoQ => Menu.Item("AutoQ").GetValue<bool>();
        private static bool AutoQOnly => Menu.Item("AutoQOnly").GetValue<bool>();
        private static bool KsQ => Menu.Item("KsQ").GetValue<bool>();
        private static bool KsT => Menu.Item("KsT").GetValue<bool>();
        private static bool KsB => Menu.Item("KsB").GetValue<bool>();
        private static bool CQ => Menu.Item("CQ").GetValue<bool>();
        private static bool CW => Menu.Item("CW").GetValue<bool>();
        private static bool CE => Menu.Item("CE").GetValue<bool>();
        private static bool CR => Menu.Item("CR").GetValue<bool>();
        private static bool CT => Menu.Item("CT").GetValue<bool>();
        private static bool CY => Menu.Item("CY").GetValue<bool>();
        private static bool CB => Menu.Item("CB").GetValue<bool>();
        private static bool HQ => Menu.Item("HQ").GetValue<bool>();
        private static bool HW => Menu.Item("HW").GetValue<bool>();
        private static bool HE => Menu.Item("HE").GetValue<bool>();
        private static bool HT => Menu.Item("HT").GetValue<bool>();
        private static bool HY => Menu.Item("HY").GetValue<bool>();
        private static bool HB => Menu.Item("HB").GetValue<bool>();
        private static bool LW => Menu.Item("LW").GetValue<bool>();
        private static bool LE => Menu.Item("LE").GetValue<bool>();
        private static bool LI => Menu.Item("LI").GetValue<bool>();
        private static bool JQ => Menu.Item("JQ").GetValue<bool>();
        private static bool JW => Menu.Item("JW").GetValue<bool>();
        private static bool JE => Menu.Item("JE").GetValue<bool>();
        private static bool JI => Menu.Item("JI").GetValue<bool>();
        private static bool AutoY => Menu.Item("AutoY").GetValue<bool>();
        private static bool DQ => Menu.Item("DQ").GetValue<bool>();
        private static bool Dind => Menu.Item("Dind").GetValue<bool>();

        static void Main()
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        static void OnGameLoad(EventArgs args)
        {
            Game.PrintChat("Hoola Master Yi - Loaded Successfully, Good Luck! :)");
            Q = new Spell(SpellSlot.Q, 600);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R);

            OnMenuLoad();

            Q.SetTargetted(0.25f, float.MaxValue);

            Game.OnUpdate += Game_OnUpdate;
            Game.OnUpdate += DetectSpell;
            Drawing.OnEndScene += Drawing_OnEndScene;
            Obj_AI_Base.OnDoCast += OnDoCast;
            Obj_AI_Base.OnPlayAnimation += OnPlay;
            Spellbook.OnCastSpell += OnCast;
            Obj_AI_Base.OnDoCast += OnDoCastJC;
            Obj_AI_Base.OnProcessSpellCast += BeforeAttack;
            Obj_AI_Base.OnProcessSpellCast += BeforeAttackJC;
            Obj_AI_Base.OnProcessSpellCast += DetectBlink;
            Drawing.OnDraw += OnDraw;
        }

        private static void DetectBlink(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe || args.SData.IsAutoAttack()) return;

            if (Spelldatabase.list.Contains(args.SData.Name.ToLower()) &&
                (((Player.Distance(args.End) >= Q.Range) && AutoQOnly) || !AutoQOnly) && Q.IsReady() && AutoQ)
                Q.Cast((Obj_AI_Base)args.Target);
        }

        private static void DetectSpell(EventArgs args)
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            if (target.IsDashing() && (((Player.Distance(target.GetWaypoints().Last()) >= Q.Range) && AutoQOnly) || !AutoQOnly) && Q.IsReady() && AutoQ && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
                Q.Cast(target);
        }

        static void OnDraw(EventArgs args)
        {
            if (DQ) Render.Circle.DrawCircle(Player.Position, Q.Range, Q.IsReady() ? Color.LimeGreen : Color.IndianRed);
        }

        static void BeforeAttack(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe || !args.Target.IsValid || !Orbwalking.IsAutoAttack(args.SData.Name)) return;

            if (args.Target is Obj_AI_Hero)
            {
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                {
                    if (CR) R.Cast();
                    if (CY) CastYoumoo();
                    if (CE) E.Cast();
                }
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
                {
                    if (HY) CastYoumoo();
                    if (HE) E.Cast();
                }
            }
            if (args.Target is Obj_AI_Minion)
            {
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
                {
                    var Minions = MinionManager.GetMinions(ItemData.Ravenous_Hydra_Melee_Only.Range);
                    if (Minions[0].IsValid && Minions.Count != 0) if (LE) E.Cast();
                }
            }
        }

        static void BeforeAttackJC(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe || !args.Target.IsValid || !Orbwalking.IsAutoAttack(args.SData.Name)) return;

            if (args.Target is Obj_AI_Minion)
            {
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
                {
                    var Mobs = MinionManager.GetMinions(ItemData.Ravenous_Hydra_Melee_Only.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
                    if (Mobs[0].IsValid && Mobs.Count != 0) if (JE) E.Cast();
                }
            }
        }

        private static void OnCast(Spellbook Sender, SpellbookCastSpellEventArgs args)
        {
            if (args.Slot == SpellSlot.R && AutoY) CastYoumoo();
        }

        private static void OnPlay(Obj_AI_Base Sender, GameObjectPlayAnimationEventArgs args)
        {
            if (!Sender.IsMe) return;
            if (args.Animation.Contains("Spell2"))
            {
                Orbwalking.LastAATick = 0;
            }
        }

        static void UseCastItem(int t)
        {
            for (int i = 0; i < t; i = i + 1)
            {
                if (HasItem)
                    Utility.DelayAction.Add(i, CastItem);
            }
        }
        static void CastItem()
        {

            if (ItemData.Tiamat_Melee_Only.GetItem().IsReady())
                ItemData.Tiamat_Melee_Only.GetItem().Cast();
            if (ItemData.Ravenous_Hydra_Melee_Only.GetItem().IsReady())
                ItemData.Ravenous_Hydra_Melee_Only.GetItem().Cast();
        }
        static void CastYoumoo()
        {
            if (ItemData.Youmuus_Ghostblade.GetItem().IsReady())
                ItemData.Youmuus_Ghostblade.GetItem().Cast();
        }
        static void CastBOTRK(Obj_AI_Hero target)
        {
            if (ItemData.Blade_of_the_Ruined_King.GetItem().IsReady())
                ItemData.Blade_of_the_Ruined_King.GetItem().Cast(target);
            if (ItemData.Bilgewater_Cutlass.GetItem().IsReady())
                ItemData.Bilgewater_Cutlass.GetItem().Cast(target);
        }
        static bool HasItem => (ItemData.Tiamat_Melee_Only.GetItem().IsReady() || ItemData.Ravenous_Hydra_Melee_Only.GetItem().IsReady());


        private static void OnDoCast(Obj_AI_Base Sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!Sender.IsMe || !args.Target.IsValid && !Orbwalking.IsAutoAttack(args.SData.Name)) return;

            if (args.Target is Obj_AI_Hero && args.Target.IsValid)
            {
                var target = (Obj_AI_Hero)args.Target;
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                {
                    if (CB) CastBOTRK(target);
                    if (CT) UseCastItem(300);
                    if (CW) Utility.DelayAction.Add(1, () => W.Cast());
                }
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
                {
                    if (HB) CastBOTRK(target);
                    if (HT) UseCastItem(300);
                    if (HW) Utility.DelayAction.Add(1, () => W.Cast());
                }
            }
            if (args.Target is Obj_AI_Minion && args.Target.IsValid)
            {
                var Minions = MinionManager.GetMinions(ItemData.Ravenous_Hydra_Melee_Only.Range);
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
                {
                    if (Minions.Count != 0 && Minions[0].IsValid)
                    {
                        if (LI) UseCastItem(300);
                        if (LW) Utility.DelayAction.Add(1, () => W.Cast());
                    }
                }
            }

        }
        private static void OnDoCastJC(Obj_AI_Base Sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!Sender.IsMe || !args.Target.IsValid && !Orbwalking.IsAutoAttack(args.SData.Name)) return;

            if (args.Target is Obj_AI_Minion && args.Target.IsValid)
            {
                var Mobs = MinionManager.GetMinions(ItemData.Ravenous_Hydra_Melee_Only.Range, MinionTypes.All, MinionTeam.Neutral);
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
                {
                    if (Mobs[0].IsValid && Mobs.Count != 0)
                    {
                        if (Q.IsReady() && JQ) Q.Cast(Mobs[0]);
                        if (!Q.IsReady() || (Q.IsReady() && !JQ))
                        {
                            if (JI) UseCastItem(300);
                            if (JW) Utility.DelayAction.Add(1, () => W.Cast());
                        }
                    }
                }
            }

        }
        private static void OnMenuLoad()
        {
            Menu = new Menu("훌라 마스터이", "hoolamasteryi", true);

            Menu.AddSubMenu(new Menu("오브워커", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Menu.SubMenu("Orbwalking"));

            var targetSelectorMenu = new Menu("타겟셀렉터", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Menu.AddSubMenu(targetSelectorMenu);


            var Combo = new Menu("콤보", "Combo");
            Combo.AddItem(new MenuItem("CQ", "Q 사용").SetValue(false));
            Combo.AddItem(new MenuItem("CW", "W 사용").SetValue(true));
            Combo.AddItem(new MenuItem("CE", "E 사용").SetValue(true));
            Combo.AddItem(new MenuItem("CR", "R 사용").SetValue(false));
            Combo.AddItem(new MenuItem("CT", "티아멧,히드라 사용").SetValue(true));
            Combo.AddItem(new MenuItem("CY", "요우무의 유령검 사용").SetValue(true));
            Combo.AddItem(new MenuItem("CB", "몰락한 왕의 검 사용").SetValue(false));
            Menu.AddSubMenu(Combo);


            var Harass = new Menu("견제", "Harass");
            Harass.AddItem(new MenuItem("HQ", "Q 사용").SetValue(true));
            Harass.AddItem(new MenuItem("HW", "W 사용").SetValue(true));
            Harass.AddItem(new MenuItem("HE", "E 사용").SetValue(true));
            Harass.AddItem(new MenuItem("HT", "티아멧,히드라 사용").SetValue(true));
            Harass.AddItem(new MenuItem("HY", "요우무의 유령검 사용").SetValue(true));
            Harass.AddItem(new MenuItem("HB", "몰락한 왕의 검 사용").SetValue(true));
            Menu.AddSubMenu(Harass);

            var Laneclear = new Menu("라인클리어", "Laneclear");
            Laneclear.AddItem(new MenuItem("LW", "W 사용").SetValue(false));
            Laneclear.AddItem(new MenuItem("LE", "E 사용").SetValue(false));
            Laneclear.AddItem(new MenuItem("LI", "티아멧, 히드라 사용").SetValue(false));
            Menu.AddSubMenu(Laneclear);

            var Jungleclear = new Menu("정글클리어", "Jungleclear");
            Jungleclear.AddItem(new MenuItem("JQ", "Q 사용").SetValue(true));
            Jungleclear.AddItem(new MenuItem("JW", "W 사용").SetValue(false));
            Jungleclear.AddItem(new MenuItem("JE", "E 사용").SetValue(true));
            Jungleclear.AddItem(new MenuItem("JI", "티아멧, 히드라 사용").SetValue(true));
            Menu.AddSubMenu(Jungleclear);

            var killsteal = new Menu("킬스틸", "Killsteal");
            killsteal.AddItem(new MenuItem("KsQ", "Q로 킬스틸").SetValue(false));
            killsteal.AddItem(new MenuItem("KsT", "티아멧, 히드라로 킬스틸").SetValue(true));
            killsteal.AddItem(new MenuItem("KsB", "몰락한 왕의 검으로 킬스틸").SetValue(true));
            Menu.AddSubMenu(killsteal);

            var Draw = new Menu("드로잉", "Draw");
            Draw.AddItem(new MenuItem("Dind", "데미지 계산 드로잉").SetValue(true));
            Draw.AddItem(new MenuItem("DQ", "Q 사거리").SetValue(false));
            Menu.AddSubMenu(Draw);

            var Misc = new Menu("기타", "Misc");
            Misc.AddItem(new MenuItem("AutoQ", "Q로 적팀 추격").SetValue(true));
            Misc.AddItem(new MenuItem("AutoQOnly", "Q사거리 벗어나기전 Q사용").SetValue(new KeyBind('C', KeyBindType.Press)));
            Misc.AddItem(new MenuItem("AutoY", "궁 쓸때 요유무 사용").SetValue(true));
            Menu.AddSubMenu(Misc);

            Menu.AddToMainMenu();
        }

        static void killsteal()
        {
            if (KsQ && Q.IsReady())
            {
                var targets = HeroManager.Enemies.Where(x => x.IsValidTarget(Q.Range) && !x.IsZombie);
                foreach (var target in targets)
                {
                    if (target.IsValid && target.Health < Q.GetDamage(target) && (!target.HasBuff("kindrednodeathbuff") || !target.HasBuff("Undying Rage") || !target.HasBuff("JudicatorIntervention")) && (!Orbwalking.InAutoAttackRange(target) || !Orbwalking.CanAttack))
                        Q.Cast(target);
                }
            }
            if (KsB &&
                (ItemData.Bilgewater_Cutlass.GetItem().IsReady() ||
                 ItemData.Blade_of_the_Ruined_King.GetItem().IsReady()))
            {
                var targets =
                    HeroManager.Enemies.Where(
                        x => x.IsValidTarget(ItemData.Blade_of_the_Ruined_King.Range) && !x.IsZombie);
                foreach (var target in targets)
                {
                    if (target.Health < Damage.GetItemDamage(Player, target, Damage.DamageItems.Bilgewater)) ItemData.Bilgewater_Cutlass.GetItem().Cast(target);
                    if (target.Health < Damage.GetItemDamage(Player, target, Damage.DamageItems.Botrk)) ItemData.Blade_of_the_Ruined_King.GetItem().Cast(target);
                }
            }
            if (KsT &&
                (ItemData.Tiamat_Melee_Only.GetItem().IsReady() ||
                 ItemData.Ravenous_Hydra_Melee_Only.GetItem().IsReady()))
            {
                var targets =
                    HeroManager.Enemies.Where(
                        x => x.IsValidTarget(ItemData.Ravenous_Hydra_Melee_Only.Range) && !x.IsZombie);
                foreach (var target in targets)
                {
                    if (target.Health < Damage.GetItemDamage(Player, target, Damage.DamageItems.Tiamat)) ItemData.Tiamat_Melee_Only.GetItem().Cast();
                    if (target.Health < Damage.GetItemDamage(Player, target, Damage.DamageItems.Hydra)) ItemData.Ravenous_Hydra_Melee_Only.GetItem().Cast();
                }
            }
        }

        static void Game_OnUpdate(EventArgs args)
        {
            killsteal();
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && CQ) Combo();
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed && HQ) Harass();
        }

        static void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            if (Q.IsReady() && target.IsValid) Q.Cast(target);
        }

        static void Harass()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            if (Q.IsReady() && target.IsValid) Q.Cast(target);
        }

        static float getComboDamage(Obj_AI_Base enemy)
        {
            if (enemy != null)
            {
                float damage = 0;

                if (Q.IsReady())
                    damage += Q.GetDamage(enemy) + (float)Player.GetAutoAttackDamage(enemy, true);

                if (E.IsReady())
                    damage += E.GetDamage(enemy);

                if (W.IsReady())
                    damage += (float)Player.GetAutoAttackDamage(enemy, true);

                if (!Player.IsWindingUp)
                    damage += (float)Player.GetAutoAttackDamage(enemy, true);

                return damage;
            }
            return 0;
        }

        static void Drawing_OnEndScene(EventArgs args)
        {
            foreach (
                var enemy in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(ene => ene.IsValidTarget() && !ene.IsZombie))
            {
                if (Dind)
                {
                    Indicator.unit = enemy;
                    Indicator.drawDmg(getComboDamage(enemy), new ColorBGRA(255, 204, 0, 160));
                }


            }
        }
    }
}
