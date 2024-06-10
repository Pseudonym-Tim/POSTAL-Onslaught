using System.Linq;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Serializable dictionary class...
/// </summary>
[System.Serializable]
public class SerializedDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    [SerializeField] private List<SerializedDictionaryKVPProps<TKey, TValue>> dictionaryList = new();

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
        foreach(KeyValuePair<TKey, TValue> kvp in this)
        {
            SerializedDictionaryKVPProps<TKey, TValue> existingEntry = dictionaryList.FirstOrDefault(entry => this.Comparer.Equals(entry.Key, kvp.Key));

            if(existingEntry is SerializedDictionaryKVPProps<TKey, TValue> serializedKVP)
            {
                // Update the value of the existing entry...
                serializedKVP.Value = kvp.Value;
            }
            else
            {
                dictionaryList.Add(kvp);
            }
        }

        dictionaryList.RemoveAll(value => ContainsKey(value.Key) == false);

        for(int i = 0; i < dictionaryList.Count; i++)
        {
            dictionaryList[i].index = i;
        }
    }

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
        Clear();

        dictionaryList.RemoveAll(r => r.Key == null);

        foreach(SerializedDictionaryKVPProps<TKey, TValue> serializedKVP in dictionaryList)
        {
            if(!(serializedKVP.isKeyDuplicated = ContainsKey(serializedKVP.Key)))
            {
                Add(serializedKVP.Key, serializedKVP.Value);
            }
        }
    }

    public new TValue this[TKey key]
    {
        get
        {
            if(ContainsKey(key))
            {
                // Group entries by key...
                IEnumerable<IGrouping<TKey, SerializedDictionaryKVPProps<TKey, TValue>>> groupedByKey = dictionaryList.GroupBy(item => item.Key);

                // Filter groups where the key occurs more than once...
                IEnumerable<IGrouping<TKey, SerializedDictionaryKVPProps<TKey, TValue>>> groupsWithDuplicates = groupedByKey.Where(group => group.Count() > 1);

                // Select the key and count of occurrences...
                var duplicateKeys = groupsWithDuplicates.Select(group => new
                {
                    Key = group.Key,
                    Count = group.Count()
                });

                foreach(var duplicatedKey in duplicateKeys)
                {
                    Debug.LogError($"Key '{ duplicatedKey.Key }' is duplicated { duplicatedKey.Count } times in the dictionary.");
                }

                return base[key];
            }
            else
            {
                Debug.LogError($"Key '{ key }' not found in dictionary.");
                return default(TValue);
            }
        }
    }

    [System.Serializable]
    public class SerializedDictionaryKVPProps<TypeKey, TypeValue>
    {
        public TypeKey Key;
        public TypeValue Value;

        [HideInInspector] public int index;
        [HideInInspector] public bool isKeyDuplicated;

        public SerializedDictionaryKVPProps(TypeKey key, TypeValue value)
        {
            this.Key = key;
            this.Value = value;
        }

        public static implicit operator SerializedDictionaryKVPProps<TypeKey, TypeValue>(KeyValuePair<TypeKey, TypeValue> kvp)
        {
            return new SerializedDictionaryKVPProps<TypeKey, TypeValue>(kvp.Key, kvp.Value);
        }

        public static implicit operator KeyValuePair<TypeKey, TypeValue>(SerializedDictionaryKVPProps<TypeKey, TypeValue> props)
        {
            return new KeyValuePair<TypeKey, TypeValue>(props.Key, props.Value);
        }
    }
}