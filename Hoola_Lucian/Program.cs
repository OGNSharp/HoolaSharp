using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms.VisualStyles;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;
using ItemData = LeagueSharp.Common.Data.ItemData;
using SharpDX;


namespace HoolaLucian
{
    public class Program
    {
        private static Menu Menu;
        private static Orbwalking.Orbwalker Orbwalker;
        private static Obj_AI_Hero Player = ObjectManager.Player;
        private static HpBarIndicator Indicator = new HpBarIndicator();
        private static Spell Q, Q1, W, E, R;
        private static bool AAPassive;
        private static bool HEXQ => Menu.Item("HEXQ").GetValue<bool>();
        private static bool KillstealQ => Menu.Item("KillstealQ").GetValue<bool>();
        private static bool CQ => Menu.Item("CQ").GetValue<bool>();
        private static bool CW => Menu.Item("CW").GetValue<bool>();
        private static int CE => Menu.Item("CE").GetValue<StringList>().SelectedIndex;
        private static bool HQ => Menu.Item("HQ").GetValue<bool>();
        private static bool HW => Menu.Item("HW").GetValue<bool>();
        private static int HE => Menu.Item("HE").GetValue<StringList>().SelectedIndex;
        private static int HMinMana => Menu.Item("HMinMana").GetValue<Slider>().Value;
        private static bool JQ => Menu.Item("JQ").GetValue<bool>();
        private static bool JW => Menu.Item("JW").GetValue<bool>();
        private static bool JE => Menu.Item("JE").GetValue<bool>();
        private static bool LHQ => Menu.Item("LHQ").GetValue<bool>();
        private static int LQ => Menu.Item("LQ").GetValue<Slider>().Value;
        private static bool LW => Menu.Item("LW").GetValue<bool>();
        private static bool LE => Menu.Item("LE").GetValue<bool>();
        private static int LMinMana => Menu.Item("LMinMana").GetValue<Slider>().Value;
        private static bool Dind => Menu.Item("Dind").GetValue<bool>();
        private static bool DEQ => Menu.Item("DEQ").GetValue<bool>();
        private static bool DQ => Menu.Item("DQ").GetValue<bool>();
        private static bool DW => Menu.Item("DW").GetValue<bool>();
        private static bool DE => Menu.Item("DE").GetValue<bool>();
        static bool AutoQ => Menu.Item("AutoQ").GetValue<KeyBind>().Active;
        private static int MinMana => Menu.Item("MinMana").GetValue<Slider>().Value;
        private static int HHMinMana => Menu.Item("HHMinMana").GetValue<Slider>().Value;
        private static int Humanizer => Menu.Item("Humanizer").GetValue<Slider>().Value;
        static bool ForceR => Menu.Item("ForceR").GetValue<KeyBind>().Active;
        static bool LT => Menu.Item("LT").GetValue<KeyBind>().Active;

        static void Main()
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        static void OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != "Lucian") return;
            Game.PrintChat("훌라 루시안 - 'SBTW is always Right' :)");
            Q = new Spell(SpellSlot.Q, 675);
            Q1 = new Spell(SpellSlot.Q, 1200);
            W = new Spell(SpellSlot.W, 1200, TargetSelector.DamageType.Magical);
            E = new Spell(SpellSlot.E, 475f);
            R = new Spell(SpellSlot.R, 1400);

            OnMenuLoad();

            Q.SetTargetted(0.25f, 1400f);
            Q1.SetSkillshot(0.5f, 50, float.MaxValue, false, SkillshotType.SkillshotLine);
            W.SetSkillshot(0.30f, 80f, 1600f, true, SkillshotType.SkillshotLine);
            R.SetSkillshot(0.2f, 110f, 2500, true, SkillshotType.SkillshotLine);

            Spellbook.OnCastSpell += Spellbook_OnCastSpell;
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnEndScene += Drawing_OnEndScene;
            Obj_AI_Base.OnDoCast += OnDoCast;
            Drawing.OnDraw += OnDraw;
            Obj_AI_Base.OnDoCast += OnDoCastLC;
        }
        private static void OnMenuLoad()
        {
            Menu = new Menu("훌라 루시안", "hoolalucian", true);

            Menu.AddSubMenu(new Menu("오브워커", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Menu.SubMenu("Orbwalking"));

            var targetSelectorMenu = new Menu("타겟셀렉터", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Menu.AddSubMenu(targetSelectorMenu);

            var Combo = new Menu("콤보", "Combo");
            Combo.AddItem(new MenuItem("CQ", "Q 사용").SetValue(true));
            Combo.AddItem(new MenuItem("CW", "W 사용").SetValue(true));
            Combo.AddItem(new MenuItem("CE", "E 사용모드").SetValue(new StringList(new[] { "측면", "커서방향", "적에게", "사용안함" })));
            Combo.AddItem(new MenuItem("ForceR", "선택된적에게 강제 궁극기").SetValue(new KeyBind('T', KeyBindType.Press)));
            Menu.AddSubMenu(Combo);

            var Misc = new Menu("기타", "Misc");
            Misc.AddItem(new MenuItem("Humanizer", "휴머나이저 딜레이 (ms)").SetValue(new Slider(5,5,300)));
            Misc.AddItem(new MenuItem("Nocolision", "W충돌 체크 안함").SetValue(true));
            Menu.AddSubMenu(Misc);


            var Harass = new Menu("견제", "Harass");
            Harass.AddItem(new MenuItem("HEXQ", "확장된 Q 사용").SetValue(true));
            Harass.AddItem(new MenuItem("HMinMana", "확장Q 사용 최소마나 (%)").SetValue(new Slider(80)));
            Harass.AddItem(new MenuItem("HQ", "Q 사용").SetValue(true));
            Harass.AddItem(new MenuItem("HW", "W 사용").SetValue(true));
            Harass.AddItem(new MenuItem("HE", "E 사용모드").SetValue(new StringList(new [] {"측면","커서방향","적에게","사용안함"})));
            Harass.AddItem(new MenuItem("HHMinMana", "견제 최소 마나 (%)").SetValue(new Slider(80)));
            Menu.AddSubMenu(Harass);

            var LC = new Menu("라인클리어", "LaneClear");
            LC.AddItem(new MenuItem("LT", "라인클리어에 스킬사용 (On/Off)").SetValue(new KeyBind('J', KeyBindType.Toggle)));
            LC.AddItem(new MenuItem("LHQ", "견제용 확장된 Q 사용").SetValue(true));
            LC.AddItem(new MenuItem("LQ", "Q 사용 (0 = 사용안함)").SetValue(new Slider(0,0,5)));
            LC.AddItem(new MenuItem("LW", "W 사용").SetValue(true));
            LC.AddItem(new MenuItem("LE", "E 사용").SetValue(true));
            LC.AddItem(new MenuItem("LMinMana", "라인클리어 최소 마나 (%)").SetValue(new Slider(80)));
            Menu.AddSubMenu(LC);

            var JC = new Menu("정글클리어", "JungleClear");
            JC.AddItem(new MenuItem("JQ", "Q 사용").SetValue(true));
            JC.AddItem(new MenuItem("JW", "W 사용").SetValue(true));
            JC.AddItem(new MenuItem("JE", "E 사용").SetValue(true));
            Menu.AddSubMenu(JC);

            var Auto = new Menu("자동화", "Auto");
            Auto.AddItem(new MenuItem("AutoQ", "자동으로 확장된 Q 사용 (On/Off)").SetValue(new KeyBind('G', KeyBindType.Toggle)));
            Auto.AddItem(new MenuItem("MinMana", "최소 마나 (%)").SetValue(new Slider(80)));
            Menu.AddSubMenu(Auto);

            var Draw = new Menu("드로잉", "Draw");
            Draw.AddItem(new MenuItem("Dind", "데미지 계산 드로잉").SetValue(true));
            Draw.AddItem(new MenuItem("DEQ", "확장된 Q 사거리").SetValue(false));
            Draw.AddItem(new MenuItem("DQ", "Q 사거리").SetValue(false));
            Draw.AddItem(new MenuItem("DW", "W 사거리").SetValue(false));
            Draw.AddItem(new MenuItem("DE", "E 사거리").SetValue(false));
            Menu.AddSubMenu(Draw);

            var killsteal = new Menu("킬스틸", "Killsteal");
            killsteal.AddItem(new MenuItem("KillstealQ", "Q로 킬스틸").SetValue(true));
            Menu.AddSubMenu(killsteal);

            Menu.AddToMainMenu();
        }

        private static void OnDoCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            var spellName = args.SData.Name;
            if (!sender.IsMe || !Orbwalking.IsAutoAttack(spellName)) return;

            if (args.Target is Obj_AI_Hero)
            {
                var target = (Obj_AI_Base)args.Target;
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && target.IsValid)
                {
                    Utility.DelayAction.Add(Humanizer, () => OnDoCastDelayed(args));
                }
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed && target.IsValid)
                {
                    Utility.DelayAction.Add(Humanizer, () => OnDoCastDelayed(args));
                }
            }
            if (args.Target is Obj_AI_Minion)
            {
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear && args.Target.IsValid)
                {
                    Utility.DelayAction.Add(Humanizer, () => OnDoCastDelayed(args));
                }
            }
        }
        private static void OnDoCastLC(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            var spellName = args.SData.Name;
            if (!sender.IsMe || !Orbwalking.IsAutoAttack(spellName)) return;

            if (args.Target is Obj_AI_Minion)
            {
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear && args.Target.IsValid)
                {
                    Utility.DelayAction.Add(Humanizer, () => OnDoCastDelayedLC(args));
                }
            }
        }

        static void killsteal()
        {
            if (KillstealQ && Q.IsReady())
            {
                var targets = HeroManager.Enemies.Where(x => x.IsValidTarget(Q.Range) && !x.IsZombie);
                foreach (var target in targets)
                {
                    if (target.Health < Q.GetDamage(target) && (!target.HasBuff("kindrednodeathbuff") && !target.HasBuff("Undying Rage") && !target.HasBuff("JudicatorIntervention")))
                        Q.Cast(target);
                }
            }
        }
        private static void OnDoCastDelayedLC(GameObjectProcessSpellCastEventArgs args)
        {
            AAPassive = false;
            if (args.Target is Obj_AI_Minion && args.Target.IsValid)
            {
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear && Player.ManaPercent > LMinMana)
                {
                    var Minions = MinionManager.GetMinions(Orbwalking.GetRealAutoAttackRange(Player), MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.Health);
                    if (Minions[0].IsValid && Minions.Count != 0)
                    {
                        if (!LT) return;

                        if (E.IsReady() && !AAPassive && LE) E.Cast(Player.Position.Extend(Game.CursorPos, 70));
                        if (Q.IsReady() && (!E.IsReady() || (E.IsReady() && !LE)) && LQ != 0 && !AAPassive)
                        {
                            var QMinions = MinionManager.GetMinions(Q.Range);
                            var exminions = MinionManager.GetMinions(Q1.Range);
                            foreach (var Minion in QMinions)
                            {
                                var QHit = new Geometry.Polygon.Rectangle(Player.Position,Player.Position.Extend(Minion.Position, Q1.Range),Q1.Width);
                                if (exminions.Count(x => !QHit.IsOutside(x.Position.To2D())) >= LQ)
                                {
                                    Q.Cast(Minion);
                                    break;
                                }
                            }
                        }
                        if ((!E.IsReady() || (E.IsReady() && !LE)) && (!Q.IsReady() || (Q.IsReady() && LQ == 0)) && LW && W.IsReady() && !AAPassive) W.Cast(Minions[0].Position);
                    }
                }
            }
        }
        public static Vector2 Deviation(Vector2 point1, Vector2 point2, double angle)
        {
            angle *= Math.PI / 180.0;
            Vector2 temp = Vector2.Subtract(point2, point1);
            Vector2 result = new Vector2(0);
            result.X = (float)(temp.X * Math.Cos(angle) - temp.Y * Math.Sin(angle)) / 4;
            result.Y = (float)(temp.X * Math.Sin(angle) + temp.Y * Math.Cos(angle)) / 4;
            result = Vector2.Add(result, point1);
            return result;
        }
        private static void OnDoCastDelayed(GameObjectProcessSpellCastEventArgs args)
        {
            AAPassive = false;
            if (args.Target is Obj_AI_Hero)
            {
                var target = (Obj_AI_Base)args.Target;
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && target.IsValid)
                {
                    if (ItemData.Youmuus_Ghostblade.GetItem().IsReady()) ItemData.Youmuus_Ghostblade.GetItem().Cast();
                    if (E.IsReady() && !AAPassive && CE == 0) E.Cast((Deviation(Player.Position.To2D(), target.Position.To2D(), 65).To3D()));
                    if (E.IsReady() && !AAPassive && CE == 1) E.Cast(Game.CursorPos);
                    if (E.IsReady() && !AAPassive && CE == 2) E.Cast(Player.Position.Extend(target.Position, 50));
                    if (Q.IsReady() && (!E.IsReady() || (E.IsReady() && CE == 3)) && CQ && !AAPassive) Q.Cast(target);
                    if ((!E.IsReady() || (E.IsReady() && CE == 3)) && (!Q.IsReady() || (Q.IsReady() && !CQ)) && CW && W.IsReady() && !AAPassive) W.Cast(target.Position);
                }
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed && target.IsValid)
                {
                    if (Player.ManaPercent < HHMinMana) return;

                    if (E.IsReady() && !AAPassive && HE == 0) E.Cast((Deviation(Player.Position.To2D(), target.Position.To2D(),65).To3D()));
                    if (E.IsReady() && !AAPassive && HE == 1) E.Cast(Player.Position.Extend(Game.CursorPos, 50));
                    if (E.IsReady() && !AAPassive && HE == 2) E.Cast(Player.Position.Extend(target.Position, 50));
                    if (Q.IsReady() && (!E.IsReady() || (E.IsReady() && HE == 3)) && HQ && !AAPassive) Q.Cast(target);
                    if ((!E.IsReady() || (E.IsReady() && HE == 3)) && (!Q.IsReady() || (Q.IsReady() && !HQ)) && HW && W.IsReady() && !AAPassive) W.Cast(target.Position);
                }
            }
            if (args.Target is Obj_AI_Minion && args.Target.IsValid)
            {
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
                {
                    var Mobs = MinionManager.GetMinions(Orbwalking.GetRealAutoAttackRange(Player), MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
                    if (Mobs[0].IsValid && Mobs.Count != 0)
                    {
                        if (E.IsReady() && !AAPassive && JE) E.Cast(Player.Position.Extend(Game.CursorPos, 70));
                        if (Q.IsReady() && (!E.IsReady() || (E.IsReady() && !JE)) && JQ && !AAPassive) Q.Cast(Mobs[0]);
                        if ((!E.IsReady() || (E.IsReady() && !JE)) && (!Q.IsReady() || (Q.IsReady() && !JQ)) && JW && W.IsReady() && !AAPassive) W.Cast(Mobs[0].Position);
                    }
                }
            }
        }

        private static void Harass()
        {
            if (Player.ManaPercent < HMinMana) return;

            if (Q.IsReady() && HEXQ)
            {
                var target = TargetSelector.GetTarget(Q1.Range, TargetSelector.DamageType.Physical);
                var Minions = MinionManager.GetMinions(Q.Range);
                foreach (var Minion in Minions)
                {
                    var QHit = new Geometry.Polygon.Rectangle(Player.Position, Player.Position.Extend(Minion.Position, Q1.Range),Q1.Width);
                    var QPred = Q1.GetPrediction(target);
                    if (!QHit.IsOutside(QPred.UnitPosition.To2D()) && QPred.Hitchance == HitChance.High)
                    {
                        Q.Cast(Minion);
                        break;
                    }
                }
            }
        }
        static void LaneClear()
        {
            if (Player.ManaPercent < LMinMana) return;

            if (Q.IsReady() && LHQ)
            {
                var extarget = TargetSelector.GetTarget(Q1.Range, TargetSelector.DamageType.Physical);
                var Minions = MinionManager.GetMinions(Q.Range);
                foreach (var Minion in Minions)
                {
                    var QHit = new Geometry.Polygon.Rectangle(Player.Position, Player.Position.Extend(Minion.Position, Q1.Range), Q1.Width);
                    var QPred = Q1.GetPrediction(extarget);
                    if (!QHit.IsOutside(QPred.UnitPosition.To2D()) && QPred.Hitchance == HitChance.High)
                    {
                        Q.Cast(Minion);
                        break;
                    }
                }
            }
        }
        static void AutoUseQ()
        {
            if (Q.IsReady() && AutoQ && Player.ManaPercent > MinMana)
            {
                var extarget = TargetSelector.GetTarget(Q1.Range, TargetSelector.DamageType.Physical);
                var Minions = MinionManager.GetMinions(Q.Range);
                foreach (var Minion in Minions)
                {
                    var QHit = new Geometry.Polygon.Rectangle(Player.Position, Player.Position.Extend(Minion.Position, Q1.Range), Q1.Width);
                    var QPred = Q1.GetPrediction(extarget);
                    if (!QHit.IsOutside(QPred.UnitPosition.To2D()) && QPred.Hitchance == HitChance.High)
                    {
                        Q.Cast(Minion);
                        break;
                    }
                }
            }
        }

        static void UseRTarget()
        {
            var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
            if (ForceR && R.IsReady() && target.IsValid && target is Obj_AI_Hero && !Player.HasBuff("LucianR")) R.Cast(target.Position);
        }
        static void Game_OnUpdate(EventArgs args)
        {
            W.Collision = Menu.Item("Nocolision").GetValue<bool>();
            AutoUseQ();

            if (ForceR) UseRTarget();
            killsteal();
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed) Harass();
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear) LaneClear();
        }
        static void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (args.Slot == SpellSlot.Q || args.Slot == SpellSlot.W || args.Slot == SpellSlot.E) AAPassive = true;
            if (args.Slot == SpellSlot.E) Orbwalking.ResetAutoAttackTimer();
            if (args.Slot == SpellSlot.R) ItemData.Youmuus_Ghostblade.GetItem().Cast();
        }

        static float getComboDamage(Obj_AI_Base enemy)
        {
            if (enemy != null)
            {
                float damage = 0;
                if (E.IsReady()) damage = damage + (float)Player.GetAutoAttackDamage(enemy) * 2;
                if (W.IsReady()) damage = damage + W.GetDamage(enemy) + (float)Player.GetAutoAttackDamage(enemy);
                if (Q.IsReady())
                {
                    damage = damage + Q.GetDamage(enemy) + (float)Player.GetAutoAttackDamage(enemy);
                }
                damage = damage + (float)Player.GetAutoAttackDamage(enemy);

                return damage;
            }
            return 0;
        }

        static void OnDraw(EventArgs args)
        {
            if (DEQ) Render.Circle.DrawCircle(Player.Position, Q1.Range, Q.IsReady() ? Color.LimeGreen : Color.IndianRed);
            if (DQ) Render.Circle.DrawCircle(Player.Position, Q.Range, Q.IsReady() ? Color.LimeGreen : Color.IndianRed);
            if (DW) Render.Circle.DrawCircle(Player.Position, W.Range, W.IsReady() ? Color.LimeGreen : Color.IndianRed);
            if (DE) Render.Circle.DrawCircle(Player.Position, E.Range, E.IsReady() ? Color.LimeGreen : Color.IndianRed);
        }
        static void Drawing_OnEndScene(EventArgs args)
        {
            if (Dind)
            {
                foreach (
                    var enemy in
                        ObjectManager.Get<Obj_AI_Hero>()
                            .Where(ene => ene.IsValidTarget() && !ene.IsZombie))
                {
                    Indicator.unit = enemy;
                    Indicator.drawDmg(getComboDamage(enemy), new ColorBGRA(255, 204, 0, 160));

                }
            }
        }
    }
}
