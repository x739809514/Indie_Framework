using System.Collections;
using System.Collections.Generic;
using System.IO;
using Core.Tools;
using Core.Transition;
using Newtonsoft.Json;
using UnityEditor.Overlays;
using UnityEngine;

namespace Saveable
{
    public class SaveLoadManager : SingletonMono<SaveLoadManager>
    {
        private const string SaveFileName = "data.sav";
        private const string WebGLSaveKey = "LunamiPuzzle.SaveData";

        private string folderPath;
        private bool canSave = false;
        private bool saveCacheLoaded;
        private Coroutine autoSaveCoroutine;
        private Dictionary<string, SaveData> saveCacheDic = new Dictionary<string, SaveData>();

        public List<ISaveable> dataList = new List<ISaveable>(); //存放具体模块
        public Dictionary<string, SaveData> saveDataDic = new Dictionary<string, SaveData>(); // 存储具体的数据

        protected override void OnAwake()
        {
            folderPath = Application.persistentDataPath + "/SAVE/";
        }

        private void OnEnable()
        {
            // register listener
        }

        private void OnDisable()
        {
            // unregister listener
        }

        public void DoRegister(ISaveable saveable)
        {
            if (saveable == null) return;
            if (!dataList.Contains(saveable))
            {
                dataList.Add(saveable);
            }
        }

        private static string GetSaveKey(ISaveable saveable)
        {
            var type = saveable.GetType();
            return type.FullName ?? type.Name;
        }

        private static string GetLegacySaveKey(ISaveable saveable)
        {
            return saveable.GetType().Name;
        }


        public void OnStartGameEvent(object obj)
        {
            if (obj is not int) return;
            //CancelAutoSave();
            DeleteSaveData();
            saveCacheLoaded = false;
            saveCacheDic.Clear();
        }

        /// <summary>
        /// 序列化存储
        /// </summary>
        public void Serialize()
        {
            saveDataDic.Clear();

            foreach (var saveable in dataList)
            {
                saveDataDic.Add(GetSaveKey(saveable), saveable.GenerateSaveData());
            }

            var jsonData = JsonConvert.SerializeObject(saveDataDic);
            WriteSaveData(jsonData);
            canSave = true;
            saveCacheLoaded = false;
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        public void AntiSerializeObject()
        {
            if (TryLoadSaveCache() == false) return; //saveCacheDic

            foreach (var saveable in dataList)
            {
                if (saveCacheDic.TryGetValue(GetSaveKey(saveable), out var saveData) ||
                    saveCacheDic.TryGetValue(GetLegacySaveKey(saveable), out saveData))
                {
                    saveable.ReadGameData(saveData);
                }
            }

            canSave = true;
        }

        private bool TryGetSavedData(string saveableTypeName, out SaveData saveData)
        {
            saveData = null;
            if (string.IsNullOrEmpty(saveableTypeName))
            {
                return false;
            }

            if (TryLoadSaveCache() == false)
            {
                return false;
            }

            return saveCacheDic.TryGetValue(saveableTypeName, out saveData);
        }

        public bool TryGetSavedData<TSaveable>(out SaveData saveData) where TSaveable : ISaveable
        {
            return TryGetSavedData(typeof(TSaveable).FullName ?? typeof(TSaveable).Name, out saveData) ||
                   TryGetSavedData(typeof(TSaveable).Name, out saveData);
        }

        /// <summary>
        /// Whether any persisted save data exists for the current player.
        /// </summary>
        public bool HasSaveData()
        {
            return TryLoadSaveCache();
        }

        // public void RequestAutoSave()
        // {
        //     if (dataList.Count == 0)
        //     {
        //         return;
        //     }
        //
        //     if (autoSaveCoroutine != null)
        //     {
        //         StopCoroutine(autoSaveCoroutine);
        //     }
        //
        //     autoSaveCoroutine = StartCoroutine(CoAutoSave());
        // }

        private bool TryLoadSaveCache()
        {
            if (saveCacheLoaded)
            {
                return saveCacheDic.Count > 0;
            }

            saveCacheLoaded = true;
            saveCacheDic.Clear();

            if (TryReadSaveData(out var stringData) == false)
            {
                return false;
            }

            var jsonData = JsonConvert.DeserializeObject<Dictionary<string, SaveData>>(stringData);
            if (jsonData == null || jsonData.Count == 0)
            {
                return false;
            }

            saveCacheDic = jsonData;
            return true;
        }

        private void WriteSaveData(string jsonData)
        {
            var resultPath = Path.Combine(folderPath, SaveFileName);
            Directory.CreateDirectory(folderPath);
            File.WriteAllText(resultPath, jsonData);
        }

        private bool TryReadSaveData(out string jsonData)
        {
            var resultPath = Path.Combine(folderPath, SaveFileName);
            if (File.Exists(resultPath) == false)
            {
                jsonData = null;
                return false;
            }

            jsonData = File.ReadAllText(resultPath);
            return true;
        }

        private void DeleteSaveData()
        {
            var resultPath = Path.Combine(folderPath, SaveFileName);
            if (File.Exists(resultPath))
            {
                File.Delete(resultPath);
            }
        }

        // private void OnAutoSaveEvent(object obj)
        // {
        //     RequestAutoSave();
        // }
        //
        // private IEnumerator CoAutoSave()
        // {
        //     yield return null;
        //     yield return new WaitForEndOfFrame();
        //
        //     if (dataList.Count > 0)
        //     {
        //         Serialize();
        //     }
        //
        //     autoSaveCoroutine = null;
        // }
        //
        // private void CancelAutoSave()
        // {
        //     if (autoSaveCoroutine == null)
        //     {
        //         return;
        //     }
        //
        //     StopCoroutine(autoSaveCoroutine);
        //     autoSaveCoroutine = null;
        // }

        private void OnApplicationQuit()
        {
            //CancelAutoSave();
            Serialize();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (canSave)
            {
                Serialize();
            }
        }

        private void OnApplicationFocus(bool focusStatus)
        {
            if (canSave && focusStatus == false)
            {
                Serialize();
            }
        }
    }
}