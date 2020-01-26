using System;
using System.Reflection;
#if !(!NETFX_CORE && !NET_STANDARD_2_0 && !UNITY_WSA_10_0 && !NETSTANDARD2_0&& !NET_4_6) && !ENABLE_IL2CPP 
using System.Linq.Expressions;
#else
using System.Reflection.Emit;
#endif

namespace Svelto.Utilities
{
    public static class FastInvoke<T> 
    {
#if ENABLE_IL2CPP //Expression.Lambda may work now, it's something to test!
        public static ActionCast<T> MakeSetter(FieldInfo field)
        {
            if (field.FieldType.IsInterfaceEx() == true && field.FieldType.IsValueTypeEx() == false)
            {
                return (ref T target, object o) =>
                       {
                           //TypedReference tr = __makeref(target);
                           //field.SetValueDirect(tr, o);
                           object refo = target;
                           field.SetValue(refo, o);
                           target = (T) refo;
                       };
            }

            throw new ArgumentException("<color=teal>Svelto.ECS</color> unsupported field (must be an interface and a class)");
        }
#elif !NETFX_CORE && !NET_STANDARD_2_0 && !UNITY_WSA_10_0 && !NETSTANDARD2_0 && !NET_4_6
        //https://stackoverflow.com/questions/1272454/generate-dynamic-method-to-set-a-field-of-a-struct-instead-of-using-reflection
        static readonly ILEmitter emit = new ILEmitter();
        public static ActionCast<T> MakeSetter(FieldInfo field)
        {
            if (field.FieldType.IsInterfaceEx() == true && field.FieldType.IsValueTypeEx() == false)
            {
                var setter = new DynamicMethod("setter", typeof(void),    
                                               new[] {typeof(T).MakeByRefType(), typeof(object)}, true);
                
                emit.il = setter.GetILGenerator();

                emit.LoadArg0OntoStack() //load argument 0 on the stack (the ref object)
                    .IfClassLoadIndirectReference(typeof(T)) 
                     //The address is popped from the stack; the object reference located at the address is fetched.
                     //The fetched reference is pushed onto the stack. this must be done only for an object and not for a struct
                     //as in this case the parameter is the location that contains the pointer and not the pointer
                    .LoadArg1OntoStack()
                    .SetField(field) //set value to field (stack 1) of object (stack 0)
                    .Return(); 

                return (ActionCast<T>) setter.CreateDelegate(typeof(ActionCast<T>));
            }
            
            throw new ArgumentException("<color=teal>Svelto.ECS</color> unsupported field (must be an interface and a class)");
        }
 
        class ILEmitter
        {
            public ILGenerator il;

            public ILEmitter Return()                              { il.Emit(OpCodes.Ret); return this; }
            public ILEmitter cast(Type type)                       { il.Emit(OpCodes.Castclass, type); return this; }
            public ILEmitter box(Type type)                        { il.Emit(OpCodes.Box, type); return this; }
            public ILEmitter unbox_any(Type type)                  { il.Emit(OpCodes.Unbox_Any, type); return this; }
            public ILEmitter unbox(Type type)                      { il.Emit(OpCodes.Unbox, type); return this; }
            public ILEmitter call(MethodInfo method)               { il.Emit(OpCodes.Call, method); return this; }
            public ILEmitter callvirt(MethodInfo method)           { il.Emit(OpCodes.Callvirt, method); return this; }    
            public ILEmitter ldnull()                              { il.Emit(OpCodes.Ldnull); return this; }
            public ILEmitter bne_un(Label target)                  { il.Emit(OpCodes.Bne_Un, target); return this; }
            public ILEmitter beq(Label target)                     { il.Emit(OpCodes.Beq, target); return this; }
            public ILEmitter ldc_i4_0()                            { il.Emit(OpCodes.Ldc_I4_0); return this; }
            public ILEmitter ldc_i4_1()                            { il.Emit(OpCodes.Ldc_I4_1); return this; }
            public ILEmitter ldc_i4(int c)                         { il.Emit(OpCodes.Ldc_I4, c); return this; }
            public ILEmitter ldc_r4(float c)                       { il.Emit(OpCodes.Ldc_R4, c); return this; }
            public ILEmitter ldc_r8(double c)                      { il.Emit(OpCodes.Ldc_R8, c); return this; }
            public ILEmitter LoadArg0OntoStack()                              { il.Emit(OpCodes.Ldarg_0); return this; }
            public ILEmitter LoadArg1OntoStack()                              { il.Emit(OpCodes.Ldarg_1); return this; }
            public ILEmitter ldarg2()                              { il.Emit(OpCodes.Ldarg_2); return this; }
            public ILEmitter ldarga(int idx)                       { il.Emit(OpCodes.Ldarga, idx); return this; }
            public ILEmitter ldarga_s(int idx)                     { il.Emit(OpCodes.Ldarga_S, idx); return this; }
            public ILEmitter ldarg(int idx)                        { il.Emit(OpCodes.Ldarg, idx); return this; }
            public ILEmitter ldarg_s(int idx)                      { il.Emit(OpCodes.Ldarg_S, idx); return this; }
            public ILEmitter ldstr(string str)                     { il.Emit(OpCodes.Ldstr, str); return this; }
            public ILEmitter IfClassLoadIndirectReference(Type type)		   { if (!type.IsValueType) il.Emit(OpCodes.Ldind_Ref); return this; }
            public ILEmitter ldloc0()                              { il.Emit(OpCodes.Ldloc_0); return this; }
            public ILEmitter ldloc1()                              { il.Emit(OpCodes.Ldloc_1); return this; }
            public ILEmitter ldloc2()                              { il.Emit(OpCodes.Ldloc_2); return this; }
            public ILEmitter ldloca_s(int idx)                     { il.Emit(OpCodes.Ldloca_S, idx); return this; }
            public ILEmitter ldloca_s(LocalBuilder local)          { il.Emit(OpCodes.Ldloca_S, local); return this; }
            public ILEmitter ldloc_s(int idx)                      { il.Emit(OpCodes.Ldloc_S, idx); return this; }
            public ILEmitter ldloc_s(LocalBuilder local)           { il.Emit(OpCodes.Ldloc_S, local); return this; }
            public ILEmitter ldloca(int idx)                       { il.Emit(OpCodes.Ldloca, idx); return this; }
            public ILEmitter ldloca(LocalBuilder local)            { il.Emit(OpCodes.Ldloca, local); return this; }
            public ILEmitter ldloc(int idx)                        { il.Emit(OpCodes.Ldloc, idx); return this; }
            public ILEmitter ldloc(LocalBuilder local)             { il.Emit(OpCodes.Ldloc, local); return this; }
            public ILEmitter initobj(Type type)                    { il.Emit(OpCodes.Initobj, type); return this; }
            public ILEmitter newobj(ConstructorInfo ctor)          { il.Emit(OpCodes.Newobj, ctor); return this; }
            public ILEmitter Throw()                               { il.Emit(OpCodes.Throw); return this; }
            public ILEmitter throw_new(Type type)                  { var exp = type.GetConstructor(Type.EmptyTypes); newobj(exp).Throw(); return this; }
            public ILEmitter stelem_ref()                          { il.Emit(OpCodes.Stelem_Ref); return this; }
            public ILEmitter ldelem_ref()                          { il.Emit(OpCodes.Ldelem_Ref); return this; }
            public ILEmitter ldlen()                               { il.Emit(OpCodes.Ldlen); return this; }
            public ILEmitter stloc(int idx)                        { il.Emit(OpCodes.Stloc, idx); return this; }
            public ILEmitter stloc_s(int idx)                      { il.Emit(OpCodes.Stloc_S, idx); return this; }
            public ILEmitter stloc(LocalBuilder local)             { il.Emit(OpCodes.Stloc, local); return this; }
            public ILEmitter stloc_s(LocalBuilder local)           { il.Emit(OpCodes.Stloc_S, local); return this; }
            public ILEmitter stloc0()                              { il.Emit(OpCodes.Stloc_0); return this; }
            public ILEmitter stloc1()                              { il.Emit(OpCodes.Stloc_1); return this; }
            public ILEmitter mark(Label label)                     { il.MarkLabel(label); return this; }
            public ILEmitter ldfld(FieldInfo field)                { il.Emit(OpCodes.Ldfld, field); return this; }
            public ILEmitter ldsfld(FieldInfo field)               { il.Emit(OpCodes.Ldsfld, field); return this; }
            public ILEmitter lodfld(FieldInfo field)               { if (field.IsStatic) ldsfld(field); else ldfld(field); return this; }
            public ILEmitter ifvaluetype_box(Type type)            { if (type.IsValueType) il.Emit(OpCodes.Box, type); return this; }
            public ILEmitter SetField(FieldInfo field)                { il.Emit(OpCodes.Stfld, field); return this; }
            public ILEmitter setstaticfield(FieldInfo field)       { il.Emit(OpCodes.Stsfld, field); return this; }
            public ILEmitter unboxorcast(Type type)                { if (type.IsValueType) unbox(type); else cast(type); return this; }
            public ILEmitter callorvirt(MethodInfo method)         { if (method.IsVirtual) il.Emit(OpCodes.Callvirt, method); else il.Emit(OpCodes.Call, method); return this; }
            public ILEmitter stind_ref()                           { il.Emit(OpCodes.Stind_Ref); return this; }
            public ILEmitter ldind_ref()                           { il.Emit(OpCodes.Ldind_Ref); return this; }
            public LocalBuilder declocal(Type type)                { return il.DeclareLocal(type); }
            public Label deflabel()                                { return il.DefineLabel(); }
            public ILEmitter ifclass_ldarg_else_ldarga(int idx, Type type) { if (type.IsValueType) emit.ldarga(idx); else emit.ldarg(idx); return this; }
            public ILEmitter ifclass_ldloc_else_ldloca(int idx, Type type) { if (type.IsValueType) emit.ldloca(idx); else emit.ldloc(idx); return this; }
            public ILEmitter perform(Action<ILEmitter, MemberInfo> action, MemberInfo member) { action(this, member); return this; }
            public ILEmitter ifbyref_ldloca_else_ldloc(LocalBuilder local, Type type) { if (type.IsByRef) ldloca(local); else ldloc(local); return this; }
        }
#else
       //https://stackoverflow.com/questions/321650/how-do-i-set-a-field-value-in-an-c-sharp-expression-tree/321686#321686
        public static ActionCast<T> MakeSetter(FieldInfo field)
        {
            if (field.FieldType.IsInterfaceEx() == true && field.FieldType.IsValueTypeEx() == false)
            {
                ParameterExpression targetExp = Expression.Parameter(typeof(T).MakeByRefType(), "target");
                ParameterExpression valueExp = Expression.Parameter(typeof(object), "value");

                MemberExpression fieldExp = Expression.Field(targetExp, field);
                UnaryExpression convertedExp = Expression.TypeAs(valueExp, field.FieldType);
                BinaryExpression assignExp = Expression.Assign(fieldExp, convertedExp);

                var setter = Expression.Lambda<ActionCast<T>>(assignExp, targetExp, valueExp).Compile();

                return setter; 
            }

            throw new ArgumentException("<color=teal>Svelto.ECS</color> unsupported field (must be an interface and a class)");
        }
#endif
    }
    
    public delegate void ActionCast<T>(ref T target, object value);
}
