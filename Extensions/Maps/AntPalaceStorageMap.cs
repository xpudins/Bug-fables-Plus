using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BFPlus.Extensions.Maps
{
    public class AntPalaceStorageMap : CustomMap
    {
        protected override void LoadCustomData()
        {
            SetBaseMapData(Color.gray, new Vector3(0, 0f, 0f), new Vector3(20, 0, 0), MainManager.Areas.BugariaCity, Resources.Load("Audio/Music/Inside2") as AudioClip);

            map.camlimitneg = new Vector3(0, 0, -8);
            map.camlimitpos = new Vector3(0, 999, 3);
            
            AddCorrectMaterials(map.transform);
            map.skyboxmat = Resources.Load<Material>("materials/skybox/Black");
            map.canfollowID = new int[0];

            Material new3dMain = MainManager_Ext.mapPrefabs.LoadAsset<Material>("3DMainPlus");

            map.transform.Find("Base/SusPot").gameObject.GetComponent<Renderer>().materials = new Material[] { new3dMain, MainManager.outlinemain};

        }

        public override GameObject LoadBattleMap()
        {
            return UnityEngine.Object.Instantiate(Resources.Load("Prefabs/BattleMaps/" + MainManager.BattleMaps.Grasslands1) as GameObject);
        }
    }
}
