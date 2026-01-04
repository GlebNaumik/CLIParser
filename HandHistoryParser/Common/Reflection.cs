namespace HandHistoryParser.Common;

public static class Reflection {

    public static IEnumerable<Type>
    GetAllAssignableTypes(this Type baseType) =>
        baseType
            .Assembly
            .GetTypes()
            .Where(type => baseType.IsAssignableFrom(type) && !type.IsAbstract);
            
    public static ConstructorInfo
    GetMainConstructor (this Type type) =>
        type
            .GetConstructors()
            .OrderByDescending(ctor => ctor.GetParameters().Length)
            .First();

}