var MonoSupport;
(function (MonoSupport) {
    /**
     * This class is used by https://github.com/mono/mono/blob/fa726d3ac7153d87ed187abd422faa4877f85bb5/sdks/wasm/dotnet_support.js#L88 to perform
     * unmarshaled invocation of javascript from .NET code.
     * */
    class jsCallDispatcher {
        static registerScope(identifier, instance) {
            jsCallDispatcher.registrations.set(identifier, instance);
        }
        static findJSFunction(identifier) {
            if (!identifier) {
                return jsCallDispatcher.dispatch;
            }
            else {
                const { ns, methodName } = jsCallDispatcher.parseIdentifier(identifier);
                var instance = jsCallDispatcher.registrations.get(ns);
                if (instance) {
                    var boundMethod = instance[methodName].bind(instance);
                    var methodId = jsCallDispatcher.cacheMethod(boundMethod);
                    return () => methodId;
                }
                else {
                    throw `Unknown scope ${ns}`;
                }
            }
        }
        static dispatch(id, pParams, pRet) {
            return jsCallDispatcher.methodMap[id](pParams, pRet);
        }
        static parseIdentifier(identifier) {
            var parts = identifier.split(':');
            const ns = parts[0];
            const methodName = parts[1];
            return { ns, methodName };
        }
        static cacheMethod(boundMethod) {
            var methodId = Object.keys(jsCallDispatcher.methodMap).length;
            jsCallDispatcher.methodMap[methodId] = boundMethod;
            return methodId;
        }
    }
    jsCallDispatcher.registrations = new Map();
    jsCallDispatcher.methodMap = {};
    MonoSupport.jsCallDispatcher = jsCallDispatcher;
})(MonoSupport || (MonoSupport = {}));
// Export the DotNet helper for WebAssembly.JSInterop.InvokeJSUnmarshalled
window.DotNet = MonoSupport;