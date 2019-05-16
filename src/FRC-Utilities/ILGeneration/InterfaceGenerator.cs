﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using FRC.NativeLibraryUtilities;

namespace FRC.ILGeneration
{
    internal class InterfaceGenerator<T>
    {
        private readonly IFunctionPointerLoader functionPointerLoader;
        private readonly IILGenerator ilGenerator;

        public InterfaceGenerator(IFunctionPointerLoader functionPointerLoader, IILGenerator ilGenerator)
        {
            this.functionPointerLoader = functionPointerLoader;
            this.ilGenerator = ilGenerator;
        }

        public T GenerateImplementation()
        {
            AssemblyBuilder asmBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(typeof(T).Name + "Asm"), AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = asmBuilder.DefineDynamicModule(typeof(T).Name + "Module");
            TypeBuilder typeBuilder = moduleBuilder.DefineType("Default" + typeof(T).Name);
            typeBuilder.AddInterfaceImplementation(typeof(T));

            var methods = typeof(T).GetMethods(BindingFlags.Public | BindingFlags.Instance);

            foreach (var method in methods)
            {
                var parameters = method.GetParameters().Select(x => x.ParameterType).ToArray();
                var methodBuilder = typeBuilder.DefineMethod(method.Name, MethodAttributes.Virtual | MethodAttributes.Public, method.ReturnType, parameters);

                ilGenerator.GenerateMethod(methodBuilder.GetILGenerator(), methodBuilder.ReturnType, parameters, functionPointerLoader.GetProcAddress(method.Name), true);
            }

            var typeInfo = typeBuilder.CreateTypeInfo();

            return (T)typeInfo.GetConstructor(new Type[0]).Invoke(null);
        }
    }
}
