using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Microsoft.Win32;

// Independent third-party compatibility patch. PCL author: 龙腾猫跃.
// Restricted to the SHA-256 verified official 2.99.0.0 binary.
public static class PatchCore
{
    public const string SupportedHash = "6B6E1260CB5A1D6A303855F1C60CAB3D0BC1AEBBA7E40A964180188B271E7386";
    public const string Marker = "PCL.DownloadRestore.ThirdParty.v1";
    public const string LegacyPatchedHash = "2FF3A197EDA8F7E9DF707A0CE910A4338D21E898359E09846938984E877A2A90";
    public const string ThirdPartyNotice = "此 PCL 已由 Kuro_Miaow 进行独立第三方修改（下载恢复）。\n不是 PCL 官方版本。原作者：龙腾猫跃 https://afdian.com/a/LTCat\n官方更新或回滚可以覆盖此补丁；助手不会阻止或自动重打补丁。";
    private const string Disabled = "为遵守相关法律法规，PCL 暂时不再提供 MC 下载功能，非常抱歉 qwq……";
    public static string Hash(string path)
    {
        using (var stream = File.OpenRead(path))
        using (var sha = SHA256.Create())
            return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "");
    }
    private static void Require(bool condition, string message)
    { if (!condition) throw new InvalidDataException(message); }
    private static TypeDefinition T(ModuleDefinition m, string name)
    { var t = m.GetType("PCL." + name); Require(t != null, "缺少类型：" + name); return t; }
    private static MethodDefinition M(TypeDefinition t, string name, int count)
    { return t.Methods.Single(x => x.Name == name && x.Parameters.Count == count); }
    private static bool Calls(Instruction i, string name)
    { return i.Operand is MethodReference && ((MethodReference)i.Operand).Name == name; }
    private static IEnumerable<MethodDefinition> Methods(TypeDefinition t)
    { return t.Methods.Concat(t.NestedTypes.SelectMany(Methods)); }
    private static void LongBranches(MethodDefinition method)
    {
        foreach (var i in method.Body.Instructions)
        {
            switch (i.OpCode.Code)
            {
                case Code.Br_S: i.OpCode = OpCodes.Br; break;
                case Code.Brfalse_S: i.OpCode = OpCodes.Brfalse; break;
                case Code.Brtrue_S: i.OpCode = OpCodes.Brtrue; break;
                case Code.Beq_S: i.OpCode = OpCodes.Beq; break;
                case Code.Bge_S: i.OpCode = OpCodes.Bge; break;
                case Code.Bge_Un_S: i.OpCode = OpCodes.Bge_Un; break;
                case Code.Bgt_S: i.OpCode = OpCodes.Bgt; break;
                case Code.Bgt_Un_S: i.OpCode = OpCodes.Bgt_Un; break;
                case Code.Ble_S: i.OpCode = OpCodes.Ble; break;
                case Code.Ble_Un_S: i.OpCode = OpCodes.Ble_Un; break;
                case Code.Blt_S: i.OpCode = OpCodes.Blt; break;
                case Code.Blt_Un_S: i.OpCode = OpCodes.Blt_Un; break;
                case Code.Bne_Un_S: i.OpCode = OpCodes.Bne_Un; break;
                case Code.Leave_S: i.OpCode = OpCodes.Leave; break;
            }
        }
    }
    public static void Patch(string input, string output)
    {
        Require(!String.Equals(Path.GetFullPath(input), Path.GetFullPath(output), StringComparison.OrdinalIgnoreCase), "补丁必须先写入临时文件。");
        Require(Hash(input) == SupportedHash, "仅支持已核验的 PCL 2.99.0.0 正式版；文件版本或内容不匹配，未修改。");
        using (var resolver = new EmbeddedResolver())
        using (var assembly = AssemblyDefinition.ReadAssembly(input, new ReaderParameters { InMemory = true, AssemblyResolver = resolver }))
        {
            var m = assembly.MainModule;
            resolver.AddEmbedded(m);
            Require(assembly.Name.Version.ToString() == "2.99.0.0", "版本不匹配。");
            RestoreVanilla(m);
            RestorePackDispatch(m);
            RestorePackDialog(m);
            RestoreOnlineInstall(m);
            RestoreVersionCard(m);
            AddThirdPartyStartupNotice(m);
            m.Resources.Add(new EmbeddedResource(Marker, ManifestResourceAttributes.Private, System.Text.Encoding.UTF8.GetBytes("Independent third-party download restoration; original author 龙腾猫跃; https://afdian.com/a/LTCat")));
            assembly.Write(output);
        }
        VerifyPatched(output);
    }
    private sealed class EmbeddedResolver : DefaultAssemblyResolver
    {
        private readonly Dictionary<string, AssemblyDefinition> embedded = new Dictionary<string, AssemblyDefinition>();
        public void AddEmbedded(ModuleDefinition module)
        {
            var resource = (EmbeddedResource)module.Resources.Single(x => x.Name == "PCL.Resources.resources");
            using (var reader = new System.Resources.ResourceReader(resource.GetResourceStream()))
            {
                var en = reader.GetEnumerator();
                var wanted = new HashSet<string>(module.AssemblyReferences.Select(x => x.Name));
                while (en.MoveNext())
                {
                    string key = (string)en.Key;
                    if (!wanted.Contains(key) || !(en.Value is byte[])) continue;
                    var bytes = (byte[])en.Value;
                    var item = AssemblyDefinition.ReadAssembly(new MemoryStream(bytes), new ReaderParameters { AssemblyResolver = this });
                    embedded.Add(item.Name.Name, item);
                }
            }
        }
        public override AssemblyDefinition Resolve(AssemblyNameReference name)
        {
            AssemblyDefinition result;
            return embedded.TryGetValue(name.Name, out result) ? result : base.Resolve(name);
        }
        protected override void Dispose(bool disposing)
        { if (disposing) foreach (var a in embedded.Values) a.Dispose(); base.Dispose(disposing); }
    }
    private static void RestoreVanilla(ModuleDefinition m)
    {
        var method = M(T(m, "PageDownloadInstall"), "LoaderInit", 0);
        var code = method.Body.Instructions;
        var start = code.Single(x => x.OpCode == OpCodes.Ldstr && (string)x.Operand == "关闭下载");
        var ctor = code.Skip(code.IndexOf(start)).First(x => x.OpCode == OpCodes.Newobj && ((MethodReference)x.Operand).DeclaringType.Name.StartsWith("LoaderTask"));
        int first = code.IndexOf(start), last = code.IndexOf(ctor);
        for (int j = first; j <= last; j++) { code[j].OpCode = OpCodes.Nop; code[j].Operand = null; }
        start.OpCode = OpCodes.Ldsfld;
        start.Operand = T(m, "ModDownload").Fields.Single(x => x.Name == "DlClientListLoader");
        LongBranches(method);
    }
    private static void RestorePackDispatch(ModuleDefinition m)
    {
        var type = T(m, "ModModpack");
        var method = M(type, "ModpackInstall", 3);
        var b = method.Body;
        var code = b.Instructions;
        var gate = code.Single(x => x.OpCode == OpCodes.Ldstr && (string)x.Operand == Disabled);
        var create = gate.Next; var throwOp = create.Next;
        Require(create.OpCode == OpCodes.Newobj && throwOp.OpCode == OpCodes.Throw, "整合包入口结构不匹配。");
        Require(b.Variables[0].VariableType.FullName == method.ReturnType.FullName && b.Variables[4].VariableType.MetadataType == MetadataType.Int32, "整合包变量结构不匹配。");
        var outer = b.ExceptionHandlers.Single(x => x.HandlerType == ExceptionHandlerType.Finally && x.HandlerEnd == null);
        var finalStart = outer.HandlerStart;
        var result = Instruction.Create(OpCodes.Ldloc, b.Variables[0]);
        var fallback = Instruction.Create(OpCodes.Nop);
        var entries = new Dictionary<int, Instruction>();
        foreach (int key in new[] { 0, 1, 2, 3, 4, 9 }) entries.Add(key, Instruction.Create(OpCodes.Nop));
        gate.OpCode = OpCodes.Ldloc; gate.Operand = b.Variables[4];
        create.OpCode = OpCodes.Switch; create.Operand = Enumerable.Range(0, 10).Select(x => entries.ContainsKey(x) ? entries[x] : fallback).ToArray();
        throwOp.OpCode = OpCodes.Br; throwOp.Operand = fallback;
        var il = b.GetILProcessor();
        var routes = new[] { "InstallPackCurseForge", "InstallPackHMCL", "InstallPackMMC", "InstallPackMCBBS", "InstallPackModrinth" };
        foreach (int key in new[] { 0, 1, 2, 3, 4, 9, -1 })
        {
            var target = type.Methods.Single(x => x.Name == (key == -1 ? "InstallPackCompress" : key == 9 ? "InstallPackLauncherPack" : routes[key]));
            var instructions = new List<Instruction> { key == -1 ? fallback : entries[key], Instruction.Create(OpCodes.Ldarg_0), Instruction.Create(OpCodes.Ldloc, b.Variables[1]) };
            if (target.Parameters.Count >= 3) instructions.Add(Instruction.Create(OpCodes.Ldloc, b.Variables[2]));
            if (target.Parameters.Count >= 4) instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
            if (target.Parameters.Count >= 5) instructions.Add(Instruction.Create(OpCodes.Ldarg_2));
            instructions.Add(Instruction.Create(OpCodes.Call, target));
            instructions.Add(Instruction.Create(OpCodes.Stloc, b.Variables[0]));
            instructions.Add(Instruction.Create(OpCodes.Leave, result));
            foreach (var i in instructions) il.InsertBefore(finalStart, i);
        }
        il.Append(result); il.Append(Instruction.Create(OpCodes.Ret));
        outer.HandlerEnd = result;
        LongBranches(method);
    }
    private static void RestorePackDialog(ModuleDefinition m)
    {
        var type = T(m, "ModModpack");
        var method = M(type, "ModpackInstall", 0);
        var closure = type.NestedTypes.Single(x => x.Name == "_Closure$__0-0");
        var b = new MethodBody(method) { InitLocals = true }; method.Body = b;
        var capture = new VariableDefinition(closure);
        var dialog = new VariableDefinition(m.ImportReference(typeof(OpenFileDialog)));
        var accepted = new VariableDefinition(m.ImportReference(typeof(bool?)));
        b.Variables.Add(capture); b.Variables.Add(dialog); b.Variables.Add(accepted);
        var il = b.GetILProcessor();
        var end = Instruction.Create(OpCodes.Ret);
        il.Emit(OpCodes.Newobj, M(closure, ".ctor", 0)); il.Emit(OpCodes.Stloc, capture);
        il.Emit(OpCodes.Newobj, m.ImportReference(typeof(OpenFileDialog).GetConstructor(Type.EmptyTypes))); il.Emit(OpCodes.Stloc, dialog);
        il.Emit(OpCodes.Ldloc, dialog); il.Emit(OpCodes.Ldstr, "选择整合包压缩文件");
        il.Emit(OpCodes.Callvirt, m.ImportReference(typeof(OpenFileDialog).GetProperty("Title").GetSetMethod()));
        il.Emit(OpCodes.Ldloc, dialog); il.Emit(OpCodes.Ldstr, "整合包文件 (*.zip;*.mrpack)|*.zip;*.mrpack|所有文件 (*.*)|*.*");
        il.Emit(OpCodes.Callvirt, m.ImportReference(typeof(OpenFileDialog).GetProperty("Filter").GetSetMethod()));
        il.Emit(OpCodes.Ldloc, dialog);
        il.Emit(OpCodes.Callvirt, m.ImportReference(typeof(OpenFileDialog).GetMethod("ShowDialog", Type.EmptyTypes)));
        il.Emit(OpCodes.Stloc, accepted); il.Emit(OpCodes.Ldloca, accepted);
        il.Emit(OpCodes.Call, m.ImportReference(typeof(bool?).GetMethod("GetValueOrDefault", Type.EmptyTypes))); il.Emit(OpCodes.Brfalse, end);
        il.Emit(OpCodes.Ldloc, capture); il.Emit(OpCodes.Ldloc, dialog);
        il.Emit(OpCodes.Callvirt, m.ImportReference(typeof(OpenFileDialog).GetProperty("FileName").GetGetMethod()));
        il.Emit(OpCodes.Stfld, closure.Fields.Single(x => x.Name == "$VB$Local_File"));
        il.Emit(OpCodes.Ldloc, capture); il.Emit(OpCodes.Ldftn, M(closure, "_Lambda$__0", 0));
        il.Emit(OpCodes.Newobj, m.ImportReference(typeof(Action).GetConstructor(new[] { typeof(object), typeof(IntPtr) })));
        il.Emit(OpCodes.Call, M(T(m, "ModBase"), "RunInThread", 1));
        il.Append(end);
    }
    private static void RestoreOnlineInstall(ModuleDefinition m)
    {
        var type = T(m, "PageDownloadResourceDetail");
        var method = M(type, "InitializeStackResourceVersions", 1);
        var code = method.Body.Instructions;
        var save = M(type, "Save_Click", 2);
        var install = M(type, "Install_Click", 2);
        var click = code.Single(x => x.OpCode == OpCodes.Ldftn && Calls(x, "Save_Click"));
        var primaryCtor = (MethodReference)click.Next.Operand;
        var optional = click.Next.Next;
        Require(optional.OpCode == OpCodes.Ldnull, "线上整合包按钮结构不匹配。");
        var entry = click.Previous;
        Require(entry.OpCode == OpCodes.Ldarg_0, "资源按钮入口不匹配。");
        var toList = (MethodReference)code.Single(x => Calls(x, "ToListItem")).Operand;
        var iconCtor = new MethodReference(".ctor", m.TypeSystem.Void, toList.Parameters[1].ParameterType) { HasThis = true };
        iconCtor.Parameters.Add(new ParameterDefinition(m.TypeSystem.Object));
        iconCtor.Parameters.Add(new ParameterDefinition(m.TypeSystem.IntPtr));
        int packValue;
        var pclcs = (EmbeddedResource)m.Resources.Single(x => x.Name == "PCL.Resources.resources");
        using (var rr = new System.Resources.ResourceReader(pclcs.GetResourceStream()))
        {
            byte[] bytes = null; var en = rr.GetEnumerator();
            while (en.MoveNext()) if ((string)en.Key == "PCLCS") bytes = (byte[])en.Value;
            Require(bytes != null, "缺少 PCLCS 内嵌组件。");
            using (var embedded = AssemblyDefinition.ReadAssembly(new MemoryStream(bytes)))
                packValue = Convert.ToInt32(embedded.MainModule.GetType("PCLCS.ResourceTypes").Fields.Single(x => x.Name == "ModPack").Constant);
        }
        var il = method.Body.GetILProcessor();
        var instructions = new[] {
            Instruction.Create(OpCodes.Ldarg_0), Instruction.Create(OpCodes.Ldfld, type.Fields.Single(x => x.Name == "TargetResourceType")),
            Instruction.Create(OpCodes.Ldc_I4, packValue), Instruction.Create(OpCodes.Bne_Un, entry),
            Instruction.Create(OpCodes.Ldarg_0), Instruction.Create(OpCodes.Ldftn, install), Instruction.Create(OpCodes.Newobj, primaryCtor),
            Instruction.Create(OpCodes.Ldarg_0), Instruction.Create(OpCodes.Ldftn, save), Instruction.Create(OpCodes.Newobj, iconCtor),
            Instruction.Create(OpCodes.Br, optional.Next)
        };
        foreach (var i in instructions) il.InsertBefore(entry, i);
        LongBranches(method);
    }
    private static void RestoreVersionCard(ModuleDefinition m)
    {
        var method = M(T(m, "PageDownloadModify"), "LoadInstanceFinished", 1);
        var getter = method.Body.Instructions.Single(x => Calls(x, "get_CardMinecraft"));
        Require(getter.Next.OpCode == OpCodes.Ldc_I4_2 && Calls(getter.Next.Next, "set_Visibility"), "版本卡片入口结构不匹配。");
        getter.Next.OpCode = OpCodes.Ldc_I4_0;
    }
    private static void AddThirdPartyStartupNotice(ModuleDefinition m)
    {
        var method = M(T(m, "FormMain"), "FormMain_Loaded", 2);
        Require(method.HasBody && method.Body.Instructions.Count > 0, "主窗口启动入口结构不匹配。");
        var entry = method.Body.Instructions[0];
        var show = typeof(System.Windows.MessageBox).GetMethod("Show", new[] { typeof(string), typeof(string), typeof(System.Windows.MessageBoxButton), typeof(System.Windows.MessageBoxImage) });
        var il = method.Body.GetILProcessor();
        foreach (var instruction in new[] {
            Instruction.Create(OpCodes.Ldstr, ThirdPartyNotice),
            Instruction.Create(OpCodes.Ldstr, "PCL 下载恢复（Kuro_Miaow 第三方修改）"),
            Instruction.Create(OpCodes.Ldc_I4_0),
            Instruction.Create(OpCodes.Ldc_I4, 64),
            Instruction.Create(OpCodes.Call, m.ImportReference(show)),
            Instruction.Create(OpCodes.Pop)
        }) il.InsertBefore(entry, instruction);
        LongBranches(method);
    }
    public static void VerifyPatched(string path)
    {
        using (var a = AssemblyDefinition.ReadAssembly(path, new ReaderParameters { InMemory = true }))
        {
            var m = a.MainModule;
            Require(a.Name.Version.ToString() == "2.99.0.0" && m.Resources.Any(x => x.Name == Marker), "不是受支持的修复版本。");
            var vanilla = M(T(m, "PageDownloadInstall"), "LoaderInit", 0);
            Require(vanilla.Body.Instructions.Any(x => x.OpCode == OpCodes.Ldsfld && ((FieldReference)x.Operand).Name == "DlClientListLoader"), "原版列表未恢复。");
            var pack = M(T(m, "ModModpack"), "ModpackInstall", 3);
            Require(!pack.Body.Instructions.Any(x => x.OpCode == OpCodes.Ldstr && (string)x.Operand == Disabled), "整合包仍被禁用。");
            foreach (var name in new[] { "InstallPackCurseForge", "InstallPackModrinth", "InstallPackHMCL", "InstallPackMMC", "InstallPackMCBBS", "InstallPackLauncherPack", "InstallPackCompress" })
                Require(pack.Body.Instructions.Count(x => Calls(x, name)) == 1, "缺少整合包分发：" + name);
            Require(M(T(m, "ModModpack"), "ModpackInstall", 0).Body.Instructions.Any(x => Calls(x, "RunInThread")), "本地导入未恢复。");
            Require(M(T(m, "PageDownloadResourceDetail"), "InitializeStackResourceVersions", 1).Body.Instructions.Any(x => x.OpCode == OpCodes.Ldftn && Calls(x, "Install_Click")), "线上安装未恢复。");
            var card = M(T(m, "PageDownloadModify"), "LoadInstanceFinished", 1).Body.Instructions.Single(x => Calls(x, "get_CardMinecraft"));
            Require(card.Next.OpCode == OpCodes.Ldc_I4_0, "版本组件卡片未恢复。");
            if (Hash(path) != LegacyPatchedHash)
            {
                var startup = M(T(m, "FormMain"), "FormMain_Loaded", 2).Body.Instructions;
                Require(startup.Count > 6 && startup[0].OpCode == OpCodes.Ldstr && (string)startup[0].Operand == ThirdPartyNotice && Calls(startup[4], "Show") && startup[5].OpCode == OpCodes.Pop, "缺少可见第三方启动标识。");
            }
        }
    }
}
