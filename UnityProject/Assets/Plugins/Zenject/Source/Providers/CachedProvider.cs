using System;
using System.Collections.Generic;
using ModestTree;

namespace Zenject
{
    public class CachedProvider : IProvider
    {
        readonly IProvider _creator;

        List<object> _instances;

#if !ZEN_MULTITHREADING
        bool _isCreatingInstance;
#endif

        public CachedProvider(IProvider creator)
        {
            _creator = creator;
        }

        public Type GetInstanceType(InjectContext context)
        {
            return _creator.GetInstanceType(context);
        }

        public List<object> GetAllInstancesWithInjectSplit(
            InjectContext context, List<TypeValuePair> args, out Action injectAction)
        {
            Assert.IsNotNull(context);

            if (_instances != null)
            {
                injectAction = null;
                return _instances;
            }

#if !ZEN_MULTITHREADING
            // This should only happen with constructor injection
            // Field or property injection should allow circular dependencies
            if (_isCreatingInstance)
            {
                throw Assert.CreateException(
                    "Found circular dependency when creating type '{0}'. Object graph: {1}",
                    _creator.GetInstanceType(context), context.GetObjectGraphString());
            }

            _isCreatingInstance = true;
#endif
            
            _instances = _creator.GetAllInstancesWithInjectSplit(context, args, out injectAction);
            Assert.IsNotNull(_instances);

#if !ZEN_MULTITHREADING
            _isCreatingInstance = false;
#endif
            return _instances;
        }
    }
}
