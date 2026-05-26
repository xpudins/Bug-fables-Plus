using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BFPlus.Extensions.MiscStuff
{
    internal static class MaterialCache
    {
        public static readonly string emission = "_Emission";
        static readonly ConditionalWeakTable<object, MatCache> cache = new ConditionalWeakTable<object, MatCache>();
        internal class MatCache
        {
            public Material[] mats;
            public Material mat;
        }

        internal static MatCache Get(object key)
        {
            return cache.GetValue(key, _ => new MatCache());
        }

        public static void SetupCache(Renderer[] objs)
        {
            for (int i = 0; i < objs.Length; i++)
            {
                MatCache cache = Get(objs[i]);
                if(cache.mats == null)
                    cache.mats = objs[i].materials;

                if (cache.mat == null)
                    cache.mat = objs[i].material;
            }
        }

    }
}
