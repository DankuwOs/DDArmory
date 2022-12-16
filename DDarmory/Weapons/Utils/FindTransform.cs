using System;
using UnityEngine;

namespace DDArmory.Weapons.Utils
{
    public class FindTransform
    {
        public static Transform FindTranny(Transform startTf, string path)
        {
            var t = startTf;

            var trannies = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var tran in trannies)
            {
                var nextTf = t.Find(tran);
                if (!nextTf)
                    return null;
                t = nextTf;
            }

            return t;
        }
    }
}