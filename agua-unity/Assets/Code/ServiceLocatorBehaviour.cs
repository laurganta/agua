using System;
using System.Collections.Generic;
using UnityEngine;

namespace CleverEdge
{
    public class ServiceLocatorBehaviour : MonoBehaviour
    {
        private Dictionary<Type, object> _instances;
        
        private void Awake()
        {
            _instances = new Dictionary<Type, object>();
            
            Build();
            
            ServiceLocator.Initialize(this);
        }

        private void Build()
        {
            var inputSystemActions = new InputSystem_Actions();
            AddInstance(inputSystemActions);
        }
        
        public void AddInstance<T>(T instance)
        {
            _instances.Add(typeof(T), instance);
        }
        
        public T GetInstance<T>()
        {
            return (T) _instances[typeof(T)];
        }
        
        public void RemoveInstance<T>()
        {
            _instances.Remove(typeof(T));
        }
    }
}