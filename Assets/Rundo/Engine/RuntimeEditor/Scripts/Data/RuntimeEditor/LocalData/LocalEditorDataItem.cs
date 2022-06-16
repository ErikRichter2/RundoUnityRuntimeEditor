using System.Collections.Generic;
using UnityEngine;

namespace Rundo
{
    public interface IPersistentDataSetMetaData
    {
        string PersistentMetaDataGuid { get; }
    }

    public class PersistentData
    {
        public readonly string Key;
        
        public PersistentData(string key)
        {
            Key = key;
        }
    }

    public class PersistentData<TData> : PersistentData
    {
        private bool _wasLoaded;
        private TData _data;
        
        public PersistentData(string key) : base(key)
        {
        }

        public PersistentData(string key, TData defaultValue) : base(key)
        {
            _data = defaultValue;
        }
        
        public void SaveData(TData data)
        {
            _data = data;
            PlayerPrefs.SetString(Key, RundoEngine.DataSerializer.SerializeObject(data));
        }

        public TData LoadData(bool force = false)
        {
            if (_wasLoaded && force == false)
            {
                return _data;
            }

            _wasLoaded = true;
    
            if (PlayerPrefs.HasKey(Key))
                _data = RundoEngine.DataSerializer.DeserializeObject<TData>(PlayerPrefs.GetString(Key));
            
            return _data;
        }
    }
    
    public class PersistentDataSet<TMetaData, TData> : PersistentData<List<TMetaData>> where TMetaData: IPersistentDataSetMetaData
    {
        public PersistentDataSet(string key) : base(key)
        {
        }

        public PersistentDataSet(string key, List<TMetaData> defaultValue) : base(key, defaultValue)
        {
        }

        public TData LoadData(string dataSetGuid)
        {
            var key = $"{Key}-{dataSetGuid}";
            if (PlayerPrefs.HasKey(key))
                return RundoEngine.DataSerializer.DeserializeObject<TData>(PlayerPrefs.GetString(key));

            return default;
        }

        public void DeleteData(TMetaData metaData)
        {
            var metaDatas = LoadData();
            for (var i = 0; i < metaDatas.Count; ++i)
                if (metaDatas[i].PersistentMetaDataGuid == metaData.PersistentMetaDataGuid)
                {
                    metaDatas.RemoveAt(i);
                    break;
                }
            
            SaveData(metaDatas);

            var key = $"{Key}-{metaData.PersistentMetaDataGuid}";
            PlayerPrefs.DeleteKey(key);
        }

        public void SaveData(TMetaData metaData, TData data)
        {
            var found = false;
            var metaDatas = LoadData();
            metaDatas ??= new List<TMetaData>();
            
            for (var i = 0; i < metaDatas.Count; ++i)
                if (metaDatas[i].PersistentMetaDataGuid == metaData.PersistentMetaDataGuid)
                {
                    metaDatas[i] = metaData;
                    found = true;
                    SaveData(metaDatas);
                    break;
                }

            if (found == false)
            {
                metaDatas.Add(metaData);
                SaveData(metaDatas);
            }
            
            var key = $"{Key}-{metaData.PersistentMetaDataGuid}";
            PlayerPrefs.SetString(key, RundoEngine.DataSerializer.SerializeObject(data));
        }

    }
    
}

