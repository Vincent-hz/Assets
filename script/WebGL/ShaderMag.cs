using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderMag : MonoBehaviour {
    /*
     * 该脚本主要管理修改模型shader
     * */

	Material material;//承载shader的材质球
    Texture texture;//构件的贴图备份
    public static ShaderMag instance;
    //改变模型的所有shader
   public void ShaderCha()
    {
        foreach (GameObject obj in ComponentWebGL.instance.Dic.Values)
        {
            if (obj.GetComponent<MeshRenderer>() != null)
            {
                //记录当前构件纹理
                //Color cnm = obj.GetComponent<MeshRenderer>().material.color;

                //if (obj.GetComponent<MeshRenderer>().material.mainTexture != null)
                //{
                //    texture = obj.GetComponent<MeshRenderer>().material.mainTexture;

                //}
                //else {
                //    texture = null;
                //}
                material = new Material(Shader.Find("Custom/dfs"));
                //material.color = cnm;
                //material.mainTexture = texture;
                // obj.GetComponent<MeshRenderer>().material = material;
                //loadMoudle.instance.material.Add(material);
               // obj.GetComponent<MeshRenderer>().materials = loadMoudle.instance.material.ToArray();
                //obj.GetComponent<MeshRenderer>().materials.SetValue(material, obj.GetComponent<MeshRenderer>().materials.Length);
                }
            //构件动态添加碰撞体
            //if (obj.GetComponent<MeshCollider>() == null)
            //{
            //    obj.AddComponent<MeshCollider>();
            //}
           
        }
        //foreach (GameObject ss in ComponentWebGL.instance.Dic.Values)
        //{
        //    if (ss.GetComponent<MeshRenderer>() != null)
        //    {
        //        for (int mdex = 0; mdex < ss.GetComponent<MeshRenderer>().materials.Length; ++mdex)
        //        {
        //            Color cnm = ss.GetComponent<MeshRenderer>().materials[mdex].color;
        //            if (ss.GetComponent<MeshRenderer>().materials[mdex].mainTexture != null)
        //            {
        //                texture = ss.GetComponent<MeshRenderer>().materials[mdex].mainTexture;

        //            }
        //            material = new Material(Shader.Find("Custom/dfs"));
        //            material.color = cnm;
        //            material.mainTexture = texture;
        //            ss.GetComponent<MeshRenderer>().materials[mdex] = material;
        //            //Debug.Log("改变shader");
        //        }
        //    }
        //}
    }
   void Awake() {
       instance = this;
   }
}
