using System.Reflection;
using PhotoJudge.Interfaces;

namespace PhotoJudge.Core;

public static class StrategyLoader
{
    /// <summary>
    /// 実行中のアセンブリおよびPhotoJudgeアセンブリから
    /// ICropStrategy を実装した具象クラスをすべて検出し、インスタンス化して返す。
    /// </summary>
    public static List<ICropStrategy> LoadAll()
    {
        var strategyType = typeof(ICropStrategy);
        var assemblies = new HashSet<Assembly>
        {
            strategyType.Assembly  // PhotoJudge.dll（Strategies/ はここに含まれる）
        };

        var entryAsm = Assembly.GetEntryAssembly();
        if (entryAsm != null)
            assemblies.Add(entryAsm);

        var strategies = new List<ICropStrategy>();

        foreach (var asm in assemblies)
        {
            var types = asm.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && strategyType.IsAssignableFrom(t));

            foreach (var type in types)
            {
                if (Activator.CreateInstance(type) is ICropStrategy instance)
                    strategies.Add(instance);
            }
        }

        return strategies.OrderBy(s => s.Name).ToList();
    }
}
