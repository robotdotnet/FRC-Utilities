using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace FRC.ILGeneration
{
    /// <summary>
    /// Helpers for IL testing
    /// </summary>
    public static class ILHelpers
    {
        /// <summary>
        /// Test if parameters are blittable
        /// </summary>
        /// <param name="returnType"></param>
        /// <param name="parameters"></param>
        public static void VerifyBlittableParameters(Type? returnType, Type[]? parameters)
        {
            return;
            //if (returnType != null && !returnType.IsValueType)
            //{
            //    throw new InvalidOperationException("Cannot generate for returning a non blittable type");
            //}
            //if (parameters == null)
            //{
            //    return;
            //}
            //foreach(var param in parameters)
            //{
            //    if (!param.IsValueType)
            //    {
            //        throw new InvalidOperationException("Cannot generate parameter for a non blittable type");
            //    }
            //}
        }
    }
}
