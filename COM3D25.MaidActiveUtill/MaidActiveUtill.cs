using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace COM3D2.MaidActiveUtill
{
    class MyAttribute
    {
        public const string PLAGIN_NAME = "MaidActiveUtill";
        public const string PLAGIN_VERSION = "22.2.26";
        public const string PLAGIN_FULL_NAME = "COM3D25.MaidActiveUtill.Plugin";
    }

    [BepInPlugin(MyAttribute.PLAGIN_FULL_NAME, MyAttribute.PLAGIN_NAME, MyAttribute.PLAGIN_VERSION)]
    public class MaidActiveUtill : BaseUnityPlugin
    {
        public static Maid[] maids = new Maid[3];
        public static string[] maidNames = new string[3];

        public static Dictionary<int, Maid> maids2 = new Dictionary<int, Maid>();
        public static Dictionary<int, string> maidNames2 = new Dictionary<int, string>();

        public static event Action selectionGrid = delegate { };
        public static event Action<int> selectionGrid2 = delegate { };

        const float cWidth = 265;

        private static int max = 2;
        private static int maxb = 2;

        const int c_max = 6;//41-9=32 25*.25

        /// <summary>
        /// HarmonyPatch(typeof(CharacterMgr), "SetActive") HarmonyPostfix
        /// </summary>
        public static event Action setActive = delegate { };
        public static event Action<Maid> setActiveMaid = delegate { };
        /// <summary>
        /// HarmonyPatch(typeof(CharacterMgr), "SetActive") HarmonyPostfix
        /// </summary>
        public static event Action<int> setActiveMaidNum = delegate { };
        /// <summary>
        /// HarmonyPatch(typeof(CharacterMgr), "SetActive") HarmonyPostfix
        /// </summary>
        public static event Action<int, Maid> setActiveMaidNum2 = delegate { };

        public static event Action deactivate = delegate { };
        public static event Action<Maid> deactivateMaid = delegate { };
        public static event Action<int> deactivateMaidNum = delegate { };
        public static event Action<int, Maid> deactivateMaidNum2 = delegate { };

        //public static int selected = 0;

        //public static event Action<int> selectedMaid = delegate { };

        public static event Action<int> maidCntChg = delegate { };

        private static ManualLogSource Log;
        private static Harmony harmony;

        internal void Awake()
        {
            Log = Logger;
            maidCntChg(3);
            if (harmony == null)
            {
                harmony = Harmony.CreateAndPatchAll(typeof(MaidActiveUtill));
            }
        }

        public static Maid[] GetMaidAll()
        {
            return maids2.Values.ToArray();
        }

        /// <summary>
        /// Maid SelectMaid(int select)
        /// MaidActivePatch.seleced
        /// selecedMaid
        /// </summary>
        /// <param name="select"></param>
        /// <returns></returns>
        public static Maid SelectMaid(int select)
        {
            if (maids2.ContainsKey(select))
            {
             //   selected = select;
             //   selectedMaid(select);
                return maids2[select];
            }
            //selected = 0;
            //selectedMaid(0);
            return null;
        }

        public static Maid GetMaid(int select)
        {
            if (maids2.ContainsKey(select))
            {
                return maids2[select];
            }
            return null;
        }

        public static string GetMaidName(int select)
        {
            if (maidNames2.ContainsKey(select))
            {
                return maidNames2[select];
            }
            return string.Empty;
        }

        private static void SetMaid(int select, Maid maid)
        {
            if (maid == null)
            {
                if (maids2.ContainsKey(select))
                {
                    maids2.Remove(select);
                    maidNames2.Remove(select);
                }
            }
            else if (maids2.ContainsKey(select))
            {
                maids2[select] = maid;
                maidNames2[select] = maid.status.fullNameEnStyle;
            }
            else
            {
                maids2.Add(select, maid);
                maidNames2.Add(select, maid.status.fullNameEnStyle);
            }

            int c = 0;
            if (maids2.Count > 0)
            {
                c = maids2.Keys.Max() + 1;//1
            }

            int i1 = c / 3;//0
            int i2 = (i1 + 1) * 3 - c;//1

            // max =  (c / 3 + 1) * 3 ;

            if (i2 == 3)
            {
                max = c;
            }
            else
            {
                max = c + i2;//3
            }

            if (maxb != max)
            {
                Array.Resize(ref maids, max);
                Array.Resize(ref maidNames, max);

                if (maxb < max)
                    for (int i = maxb; i < max; i++)
                    {

                            maids[i] = null;
                            maidNames[i] = string.Empty;
                        
                    }
                maidCntChg(max);
            }

            

                if (select < max)
                {
                    maids[select] = maid;
                    if (maid == null)
                    {
                        maidNames[select] = string.Empty;
                    }
                    else
                    {
                        maidNames[select] = maid.status.fullNameEnStyle;
                    }
                }
            
            maxb = max;
        }



        /// <summary>
        /// 메이드가 슬롯에 넣었을때 
        /// 
        /// </summary>
        /// <param name="f_maid">어떤 메이드인지</param>
        /// <param name="f_nActiveSlotNo">활성화된 메이드 슬롯 번호. 다시말하면 메이드를 집어넣을 슬롯</param>
        /// <param name="f_bMan">남잔지 여부</param>
        [HarmonyPatch(typeof(CharacterMgr), "SetActive")]
        [HarmonyPostfix]// CharacterMgr의 SetActive가 실행 후에 아래 메소드 작동
        public static void SetActive(Maid f_maid, int f_nActiveSlotNo, bool f_bMan)
        {

            if (!f_bMan)// 남자가 아닐때
            {                
                try
                {
                    SetMaid(f_nActiveSlotNo, f_maid);
                }
                catch (Exception e)
                {
                    Log.LogWarning($"CharacterMgr.SetMaid {e.ToString()}");
                }
                try
                {
                    setActive();
                }
                catch (Exception e)
                {
                    Log.LogFatal($"CharacterMgr.setActive {e.ToString()}");
                }
                try
                {
                    setActiveMaid(f_maid);
                }
                catch (Exception e)
                {
                    Log.LogFatal($"CharacterMgr.setActiveMaid {e.ToString()}");
                }
                try
                {
                    setActiveMaidNum(f_nActiveSlotNo);
                }
                catch (Exception e)
                {
                    Log.LogFatal($"CharacterMgr.setActiveMaid2 {e.ToString()}");
                }
                try
                {
                    setActiveMaidNum2(f_nActiveSlotNo, f_maid);
                }
                catch (Exception e)
                {
                    Log.LogFatal($"CharacterMgr.setActiveMaid3 {e.ToString()}");
                }
            }
            
        }

        /// <summary>
        /// 메이드가 슬롯에서 빠졌을때
        /// </summary>
        /// <param name="f_nActiveSlotNo"></param>
        /// <param name="f_bMan"></param>
        [HarmonyPatch(typeof(CharacterMgr), "Deactivate")]
        [HarmonyPrefix] // CharacterMgr의 SetActive가 실행 전에 아래 메소드 작동
        public static void Deactivate(int f_nActiveSlotNo, bool f_bMan)
        {
            if (!f_bMan)
            {
                //LillyUtill.Log.LogInfo("CharacterMgr.Deactivate", f_nActiveSlotNo);

                try
                {
                    deactivate();
                }
                catch (Exception e)
                {
                    Log.LogFatal($"CharacterMgr.deactivate {e.ToString()}");
                }
                try
                {
                    if (maids2.ContainsKey(f_nActiveSlotNo))
                        deactivateMaidNum(f_nActiveSlotNo);
                }
                catch (Exception e)
                {
                    Log.LogFatal($"CharacterMgr.deactivateMaid {e.ToString()}");
                }
                try
                {
                    if (maids2.ContainsKey(f_nActiveSlotNo))
                    {
                        deactivateMaid(maids2[f_nActiveSlotNo]);
                    }
                }
                catch (Exception e)
                {
                    Log.LogFatal($"CharacterMgr.deactivateMaid2 {e.ToString()}");
                }
                try
                {
                    if (maids2.ContainsKey(f_nActiveSlotNo))
                        deactivateMaidNum2(f_nActiveSlotNo, maids2[f_nActiveSlotNo]);
                }
                catch (Exception e)
                {
                    Log.LogFatal($"CharacterMgr.deactivateMaid3 {e.ToString()}");
                }
                // 이건 무조건 나중에
                try
                {
                    SetMaid(f_nActiveSlotNo, null);
                }
                catch (Exception e)
                {
                    Log.LogFatal($"CharacterMgr.SetMaid {e.ToString()}");
                }
            }
            
        }

        /// <summary>
        /// no event action selectionGrid,selectionGrid2
        /// </summary>ㅇ
        /// <param name="seleted"></param> 메이드 슬롯번호
        /// <param name="cul"></param> 가로 버튼 갯수
        /// <param name="Width"></param> 가로 GUI 크기
        /// <param name="changed"></param> 미사용
        /// <returns></returns>
        public static int SelectionGrid(int seleted, int cul = 3, float Width = cWidth)
        {
            //selected = seleted;
            return GUILayout.SelectionGrid(seleted, maidNames, cul, GUILayout.Width(Width));
        }
    }
}
