using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LayerDataConfig", menuName = "Game/Layer Data Config")]
public class LayerDataConfig : ScriptableObject
{
    [System.Serializable]
    public class LayerConfig
    {
        public int layerIndex;
        public int tileIndex;
        public LayerState layerState;
        public float layerHeight;
        public int bossIndex = -1;
        public string layerName;
        
        public LayerData ToLayerData()
        {
            return new LayerData(layerIndex, tileIndex, layerState, layerHeight, bossIndex);
        }
    }
    
    [Header("Layer Configuration")]
    public List<LayerConfig> layerConfigs = new List<LayerConfig>();
    
    private void OnValidate()
    {
        // 에디터에서 자동으로 layerName 설정
        foreach (var config in layerConfigs)
        {
            config.layerName = GetLayerName(config.layerIndex);
        }
    }
    
    private string GetLayerName(int layerIndex)
    {
        return layerIndex switch
        {
            0 => "Mine Zone",
            1 => "Skull Zone 1",
            2 => "Boss Chamber I",
            3 => "",
            4 => "Skull Zone 2",
            5 => "Lava Zone",
            6 => "Ultimate Zone",
            7 => "Boss Chamber II",
            8 => "",
            _ => $"Layer{layerIndex}"
        };
    }
    
    public List<LayerData> GetLayerDataList()
    {
        List<LayerData> layerDataList = new List<LayerData>();
        foreach (var config in layerConfigs)
        {
            layerDataList.Add(config.ToLayerData());
        }
        return layerDataList;
    }
    
    public LayerData GetLayerData(int layerIndex)
    {
        foreach (var config in layerConfigs)
        {
            if (config.layerIndex == layerIndex)
            {
                return config.ToLayerData();
            }
        }
        return null;
    }
}
