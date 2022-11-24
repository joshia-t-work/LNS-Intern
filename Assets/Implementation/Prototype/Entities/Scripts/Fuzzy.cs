using LNS.ObjectPooling;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

namespace LNS.FuzzyLogic
{
    public static class OP
    {
        //public static float ()

        public static float BOOL(bool x)
        {
            return x ? 1f : 0f;
        }

        public static float AND(float x, float y)
        {
            //return Mathf.Min(x, y);
            return x * y;
        }
        #region AND Extensions
        public static float AND(float x1, float x2, float x3)
        {
            return x1 * x2 * x3;
        }
        public static float AND(float x1, float x2, float x3, float x4)
        {
            return x1 * x2 * x3 * x4;
        }
        public static float AND(float x1, float x2, float x3, float x4, float x5)
        {
            return x1 * x2 * x3 * x4 * x5;
        }
        public static float AND(float x1, float x2, float x3, float x4, float x5, float x6)
        {
            return x1 * x2 * x3 * x4 * x5 * x6;
        }
        #endregion
        public static float OR(float x, float y)
        {
            //return Mathf.Max(x, y);
            return x + y - x * y;
        }
        #region OR Extensions
        public static float OR(float x1, float x2, float x3)
        {
            return OR(OR(x1, x2), x3);
        }
        public static float OR(float x1, float x2, float x3, float x4)
        {
            return OR(OR(x1, x2), OR(x3, x4));
        }
        public static float OR(float x1, float x2, float x3, float x4, float x5)
        {
            return OR(OR(x1, x2), OR(x1, x2, x3));
        }
        #endregion
        public static float NOT(float x)
        {
            return 1f - x;
        }
        public static float NOT(bool x)
        {
            return x ? 0f : 1f;
        }
    }
    public class FuzzyTruth
    {
        FuzzySection lastTruth = null;
        DictionaryList<string, FuzzySection> truths = new DictionaryList<string, FuzzySection>();
        public void Add(FuzzySection truth, float exitTransitionSize)
        {
            if (lastTruth == null)
            {
                truth.Enter = 0;
                truth.Peak = 0;
            }
            else
            {
                truth.Enter = lastTruth.Limit;
                truth.Peak = lastTruth.Exit;
            }
            truth.Exit = truth.Limit + exitTransitionSize;
            truths.Add(truth.Label, truth);
            lastTruth = truth;
        }
        public float Evaluate(float value, FuzzySection truth)
        {
            return truth.Evaluate(value);
        }
        public float Evaluate(float value, string truthName)
        {
            if (truths.TryGetValue(truthName, out FuzzySection truth))
            {
                return truth.Evaluate(value);
            }
            return 0f;
        }
        public float Evaluate(float value, int truthIndex)
        {
            if (truths.TryGetValue(truthIndex, out FuzzySection truth))
            {
                return truth.Evaluate(value);
            }
            return 0f;
        }
        #region Evaluate Extensions
        /// <summary>
        /// Uses OR operators on each truth string under the hood. Cache the truth values if the values are used individually.
        /// </summary>
        public float Evaluate(float value, string t1, string t2)
        {
            float val = 0f;
            val = OP.OR(val, Evaluate(value, t1));
            val = OP.OR(val, Evaluate(value, t2));
            return val;
        }
        /// <inheritdoc cref="LNS.FuzzyLogic.FuzzyTruth.Evaluate(float, string, string)"/>
        public float Evaluate(float value, string t1, string t2, string t3)
        {
            float val = 0f;
            val = OP.OR(val, Evaluate(value, t1, t2));
            val = OP.OR(val, Evaluate(value, t3));
            return val;
        }
        /// <inheritdoc cref="LNS.FuzzyLogic.FuzzyTruth.Evaluate(float, string, string)"/>
        public float Evaluate(float value, string t1, string t2, string t3, string t4)
        {
            float val = 0f;
            val = OP.OR(val, Evaluate(value, t1, t2));
            val = OP.OR(val, Evaluate(value, t3, t4));
            return val;
        }
        /// <inheritdoc cref="LNS.FuzzyLogic.FuzzyTruth.Evaluate(float, string, string)"/>
        public float Evaluate(float value, string t1, string t2, string t3, string t4, string t5)
        {
            float val = 0f;
            val = OP.OR(val, Evaluate(value, t1, t2));
            val = OP.OR(val, Evaluate(value, t3, t4, t5));
            return val;
        }
        /// <summary>
        /// Uses OR operators on each truth string under the hood. Cache the truth values if the values are used individually.
        /// </summary>
        public float Evaluate(float value, FuzzySection t1, FuzzySection t2)
        {
            float val = 0f;
            val = OP.OR(val, Evaluate(value, t1));
            val = OP.OR(val, Evaluate(value, t2));
            return val;
        }
        /// <inheritdoc cref="LNS.FuzzyLogic.FuzzyTruth.Evaluate(float, string, string)"/>
        public float Evaluate(float value, FuzzySection t1, FuzzySection t2, FuzzySection t3)
        {
            float val = 0f;
            val = OP.OR(val, Evaluate(value, t1, t2));
            val = OP.OR(val, Evaluate(value, t3));
            return val;
        }
        /// <inheritdoc cref="LNS.FuzzyLogic.FuzzyTruth.Evaluate(float, string, string)"/>
        public float Evaluate(float value, FuzzySection t1, FuzzySection t2, FuzzySection t3, FuzzySection t4)
        {
            float val = 0f;
            val = OP.OR(val, Evaluate(value, t1, t2));
            val = OP.OR(val, Evaluate(value, t3, t4));
            return val;
        }
        /// <inheritdoc cref="LNS.FuzzyLogic.FuzzyTruth.Evaluate(float, string, string)"/>
        public float Evaluate(float value, FuzzySection t1, FuzzySection t2, FuzzySection t3, FuzzySection t4, FuzzySection t5)
        {
            float val = 0f;
            val = OP.OR(val, Evaluate(value, t1, t2));
            val = OP.OR(val, Evaluate(value, t3, t4, t5));
            return val;
        }
        /// <summary>
        /// Uses OR operators on each truth string under the hood. Cache the truth values if the values are used individually.
        /// </summary>
        public float Evaluate(float value, int t1, int t2)
        {
            float val = 0f;
            val = OP.OR(val, Evaluate(value, t1));
            val = OP.OR(val, Evaluate(value, t2));
            return val;
        }
        /// <inheritdoc cref="LNS.FuzzyLogic.FuzzyTruth.Evaluate(float, string, string)"/>
        public float Evaluate(float value, int t1, int t2, int t3)
        {
            float val = 0f;
            val = OP.OR(val, Evaluate(value, t1, t2));
            val = OP.OR(val, Evaluate(value, t3));
            return val;
        }
        /// <inheritdoc cref="LNS.FuzzyLogic.FuzzyTruth.Evaluate(float, string, string)"/>
        public float Evaluate(float value, int t1, int t2, int t3, int t4)
        {
            float val = 0f;
            val = OP.OR(val, Evaluate(value, t1, t2));
            val = OP.OR(val, Evaluate(value, t3, t4));
            return val;
        }
        /// <inheritdoc cref="LNS.FuzzyLogic.FuzzyTruth.Evaluate(float, string, string)"/>
        public float Evaluate(float value, int t1, int t2, int t3, int t4, int t5)
        {
            float val = 0f;
            val = OP.OR(val, Evaluate(value, t1, t2));
            val = OP.OR(val, Evaluate(value, t3, t4, t5));
            return val;
        }
        #endregion
        public FuzzySection Evaluate(float val)
        {
            FuzzySection retTruth = null;
            float maxVal = 0f;
            foreach (FuzzySection truth in truths.Values)
            {
                float evalVal = truth.Evaluate(val);
                if (evalVal > maxVal)
                {
                    retTruth = truth;
                    maxVal = evalVal;
                }
            }
            return retTruth;
        }
    }
    public class FuzzySection
    {
        bool isLimitDirty = true;
        bool isCenterDirty = true;
        public string Label;
        private float enter;
        public float Enter
        {
            get
            {
                return enter;
            }
            set
            {
                isCenterDirty = true;
                enter = value;
            }
        }
        private float peak;
        public float Peak
        {
            get
            {
                return peak;
            }
            set
            {
                isLimitDirty = true;
                isCenterDirty = true;
                peak = value;
            }
        }
        private float limit;
        public float Limit
        {
            get
            {
                if (isLimitDirty)
                {
                    isLimitDirty = false;
                    limit = Peak + Size;
                }
                return limit;
            }
        }
        private float exit;
        public float Exit
        {
            get
            {
                return exit;
            }
            set
            {
                isCenterDirty = true;
                exit = value;
            }
        }
        private float size;
        public float Size
        {
            get
            {
                return size;
            }
            set
            {
                isLimitDirty = true;
                isCenterDirty = true;
                size = value;
            }
        }
        private float center;
        public float Center
        {
            get
            {
                if (isCenterDirty)
                {
                    isCenterDirty = false;
                    center = ((Exit - Limit) / 2f - (Peak - Enter) / 2f) / Size + Peak + Size / 2f;
                }
                return center;
            }
        }
        public FuzzySection(string label, float thresholdEnter, float thresholdPeak, float thresholdLimit, float thresholdExit)
        {
            Label = label;
            enter = thresholdEnter;
            peak = thresholdPeak;
            size = thresholdLimit - thresholdPeak;
            exit = thresholdExit;
        }
        public FuzzySection(string label, float size)
        {
            Label = label;
            this.size = size;
        }
        public float Evaluate(float value)
        {
            if (value < Enter)
            {
                return 0;
            }
            else if (value < Peak)
            {
                return (value - Enter) / (Peak - Enter);
            }
            else if (value < Limit)
            {
                return 1;
            }
            else if (value < Exit)
            {
                return OP.NOT((value - Limit) / (Exit - Limit));
            }
            else
            {
                return 0;
            }
        }
    }
    public class DictionaryList<TKey, TValue>
    {
        private List<TKey> keys = new List<TKey>();
        private List<TValue> values = new List<TValue>();
        public ReadOnlyCollection<TKey> Keys => keys.AsReadOnly();
        public ReadOnlyCollection<TValue> Values => values.AsReadOnly();
        public bool TryGetValue(TKey item, out TValue value)
        {
            value = default(TValue);
            int index = keys.IndexOf(item);
            if (index < 0)
            {
                return false;
            }
            value = values[index];
            return true;
        }
        public bool TryGetValue(int index, out TValue value)
        {
            value = default(TValue);
            if (keys.Count > index)
            {
                value = values[index];
            } else
            {
                return false;
            }
            return true;
        }
        public void Add(TKey key, TValue val)
        {
            keys.Add(key);
            values.Add(val);
        }
    }
}
