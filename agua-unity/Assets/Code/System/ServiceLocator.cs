namespace CleverEdge
{
    public static class ServiceLocator
    {
        private static ServiceLocatorBehaviour _serviceLocatorBehaviour;
        
        public static void Initialize(ServiceLocatorBehaviour serviceLocatorBehaviour)
        {
            _serviceLocatorBehaviour = serviceLocatorBehaviour;
        }
        
        public static T GetInstance<T>()
        {
            return _serviceLocatorBehaviour.GetInstance<T>();
        }
        
        public static void AddInstance<T>(T instance)
        {
            _serviceLocatorBehaviour.AddInstance(instance);
        }
        
        public static void RemoveInstance<T>()
        {
            _serviceLocatorBehaviour.RemoveInstance<T>();
        }
    }
}