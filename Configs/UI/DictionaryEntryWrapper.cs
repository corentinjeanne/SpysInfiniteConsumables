using System.Collections;
using System.Collections.Specialized;
using System.Reflection;
using Newtonsoft.Json;
using Terraria.ModLoader.Config;

namespace SPIC.Configs.UI;

public interface IDictionaryEntryWrapper {

    public PropertyInfo ValueProp { get; }
    object Key { get; set;}

    object Value { get; set;}
}

public class DictionaryEntryWrapper<Tkey,Tvalue> : IDictionaryEntryWrapper {

    [JsonIgnore]
    public PropertyInfo ValueProp => typeof(DictionaryEntryWrapper<Tkey, Tvalue>).GetProperty("Value", BindingFlags.Instance | BindingFlags.Public);

    public Tkey Key {
        get => _key;
        set {
            if(_dict is IOrderedDictionary ordered){
                int index = 0;
                foreach(DictionaryEntry entry in ordered){
                    if(entry.Key.Equals(_key)) break;
                    index++;
                }
                ordered.RemoveAt(index);
                ordered.Insert(index, value, _dict[_key]);
            } else {
                _dict.Remove(_key);
                _dict.Add(value, _dict[_key]);
            }

            _key = value;
        }
    }

    [ColorNoAlpha, ColorHSLSlider]
    public Tvalue Value {
        get => (Tvalue)_dict[_key];
        set {
            _dict[_key] = value;
        }
    }

    object IDictionaryEntryWrapper.Key {
        get => Key;
        set => Key = (Tkey)value;
    }
    object IDictionaryEntryWrapper.Value {
        get => Value;
        set => Value = (Tvalue)value;
    }

    public DictionaryEntryWrapper(IDictionary dict, Tkey key) {
        _key = key;
        _dict = dict;
    }

    private readonly IDictionary _dict;
    private Tkey _key;
}
