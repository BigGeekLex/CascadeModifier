using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using UnityEngine.Events;
namespace MSFD
{
    [Serializable]
    public class CascadeModifier<T>
    {
        List<Func<T, T>> valueModFuncs = new List<Func<T, T>>();
        //[HorizontalGroup("Main")]
        [SerializeField]
        T value;

/*//#if UNITY_EDITOR
        [OnStateUpdate("@modifiedValue = CalculateValue()")]
        [HorizontalGroup("Main")]
        [ReadOnly]
        [SerializeField]
        T modifiedValue;
//#endif
*/
        bool isExternalScriptsCanChangeValue = false;
        Action onModifierChanged;
        public CascadeModifier(bool _isCanChangeValue = false)
        {
            isExternalScriptsCanChangeValue = _isCanChangeValue;
        }
        public CascadeModifier(T _value, bool _isExternalScriptsCanChangeValue = false)
        {
            isExternalScriptsCanChangeValue = _isExternalScriptsCanChangeValue;
            value = _value;
        }
        public CascadeModifier(T _value, ref Action<T> SetNewValueFunc)
        {
            value = _value;
            isExternalScriptsCanChangeValue = false;
            SetNewValueFunc = (T _newValue) => { value = _newValue; };
        }
        /// <summary>
        /// Be careful! Scripts can set value only when it is allowed by class which contained DelegateCascade
        /// </summary>
        /// <param name="_value"></param>
        public virtual void SetNewValue(T _value)
        {
            if (isExternalScriptsCanChangeValue)
            {
                value = _value;
                onModifierChanged?.Invoke();
            }
            else
            {
                Debug.LogError($"Attempt to set new value: change from {value} to {_value}. This Delegate Cascade forbides to change base value");
            }
        }
        public T GetCleanValue()
        {
            return value;
        }
        //[HorizontalGroup("Main")]
        //[Button]
        public T CalculateValue()
        {
            return CalculateValueWithModifiers(value);
        }
        /// <summary>
        /// This method can be used to calcaulate some value with currently installed modifiers
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual T CalculateValueWithModifiers(T value)
        {
            for (int i = 0; i < valueModFuncs.Count; i++)
            {
                value = valueModFuncs[i].Invoke(value);
            }
            return value;
        }
        public void Add(Func<T, T> func, int priority = 0)
        {
            valueModFuncs.Add(func);
            onModifierChanged?.Invoke();
        }
        public void Remove(Func<T, T> func)
        {
            valueModFuncs.Remove(func);
            onModifierChanged?.Invoke();
        }
        public static implicit operator T(CascadeModifier<T> delegateCascade)
        {
            //Можно сохранять просчитанные значения и выводить их, если не было изменений, но это может привести к неправильному результату, если функциии внутри зависят от внешних условий
            return delegateCascade.CalculateValueWithModifiers(delegateCascade.value);
        }
        /// <summary>
        /// OmModiferChanged is invoked when valueModFuncs are chaged or when new value is set
        /// </summary>
        /// <param name="listener"></param>
        public void AddListenerOnModifierChanged(System.Action listener)
        {
            onModifierChanged += listener;
        }
        public void RemoveListenerOnModifierChanged(System.Action listener)
        {
            onModifierChanged -= listener;
        }

    }
}