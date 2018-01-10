using System.Reflection;

static Dictionary<Tuple<ActionTask, string>, object> DefaultParameterValues = new Dictionary<Tuple<ActionTask, string>, object>();

public static CakeTaskBuilder<ActionTask> HasOptionalParameter(this CakeTaskBuilder<ActionTask> builder, string name, object defaultValue)
{
    DefaultParameterValues.Add(Tuple.Create(builder.Task, name), defaultValue);
    return builder;
}

public static CakeTaskBuilder<ActionTask> Does<T1>(this CakeTaskBuilder<ActionTask> builder, Action<T1> action)
{
    return ParameterizedDoes(builder, action);
}

public static CakeTaskBuilder<ActionTask> Does<T1, T2>(this CakeTaskBuilder<ActionTask> builder, Action<T1, T2> action)
{
    return ParameterizedDoes(builder, action);
}

public static CakeTaskBuilder<ActionTask> Does<T1, T2, T3>(this CakeTaskBuilder<ActionTask> builder, Action<T1, T2, T3> action)
{
    return ParameterizedDoes(builder, action);
}

public static CakeTaskBuilder<ActionTask> Does<T1, T2, T3, T4>(this CakeTaskBuilder<ActionTask> builder, Action<T1, T2, T3, T4> action)
{
    return ParameterizedDoes(builder, action);
}

public static CakeTaskBuilder<ActionTask> ParameterizedDoes(CakeTaskBuilder<ActionTask> builder, Delegate action)
{
    return CakeTaskBuilderExtensions.Does(builder, context =>
    {
        var parameters = action.Method.GetParameters();
        var arguments = new object[parameters.Length];

        for (var i = 0; i < parameters.Length; ++i)
        {
            object defaultValue;
            arguments[i] = DefaultParameterValues.TryGetValue(Tuple.Create(builder.Task, parameters[i].Name), out defaultValue)
                ? context.Argument(parameters[i].ParameterType, parameters[i].Name, defaultValue)
                : context.Argument(parameters[i].ParameterType, parameters[i].Name);
        }

        try
        {
            action.DynamicInvoke(arguments);
        }
        catch (TargetInvocationException ex)
        {
            throw ex.InnerException;
        }
    });
}

public static object Argument(this ICakeContext context, Type type, string name)
{
    try
    {
        return typeof(ArgumentsHelper)
            .GetMethod("GetArgumentWithoutDefault", BindingFlags.Static | BindingFlags.Public)
            .MakeGenericMethod(new[] { type })
            .Invoke(null, new object[] { context, name });
    }
    catch (TargetInvocationException ex)
    {
        throw ex.InnerException;
    }
}

public static object Argument(this ICakeContext context, Type type, string name, object defaultValue)
{
    try
    {
        return typeof(ArgumentsHelper)
            .GetMethod("GetArgumentWithDefault", BindingFlags.Static | BindingFlags.Public)
            .MakeGenericMethod(new[] { type })
            .Invoke(null, new object[] { context, name, defaultValue });
    }
    catch (TargetInvocationException ex)
    {
        throw ex.InnerException;
    }
}

private static class ArgumentsHelper
{
    public static T GetArgumentWithoutDefault<T>(ICakeContext context, string name)
    {
        return context.Argument<T>(name);
    }

    public static T GetArgumentWithDefault<T>(ICakeContext context, string name, T defaultValue)
    {
        return context.Argument<T>(name, defaultValue);
    }
}
