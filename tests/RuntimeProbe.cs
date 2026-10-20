using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Mono.Cecil;
public static class RuntimeProbe
{
 const BindingFlags All=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.Instance;
 static string Root,Lib,Game; static Assembly Pcl; static StreamWriter Writer; static int Failures; static Application App;
 [STAThread] public static int Main(string[] args) {
  System.Net.ServicePointManager.SecurityProtocol=(System.Net.SecurityProtocolType)3072;Root=AppDomain.CurrentDomain.BaseDirectory;Directory.SetCurrentDirectory(Root);
  Lib=Path.GetFullPath(Path.Combine(Root,"../runtime-lib"));Game=Path.Combine(Root,"game")+Path.DirectorySeparatorChar;
  Directory.CreateDirectory(Lib);Directory.CreateDirectory(Game);Directory.CreateDirectory(Path.Combine(Root,"PCL"));Directory.CreateDirectory(Path.Combine(Root,"temp"));
  Environment.SetEnvironmentVariable("TEMP",Path.Combine(Root,"temp"));Environment.SetEnvironmentVariable("TMP",Path.Combine(Root,"temp"));
  File.WriteAllText(Path.Combine(Root,"PCL","Setup.ini"),"SystemSystemUpdate:3\r\nSystemLastVersionReg:413\r\nLaunchArgumentIndieV2:1\r\n",new UTF8Encoding(false));
  string mode=args.Length>1?args[1]:"jit";Writer=new StreamWriter(Path.Combine(Root,"probe-"+mode+".log"),false,new UTF8Encoding(false));Writer.AutoFlush=true;
  try {
   if(args.Length==0)throw new ArgumentException("RuntimeProbe PCL.exe [jit|list|vanilla|packs|inspect] [timeout seconds]");
   string target=Path.GetFullPath(args[0]);Log("TARGET "+target+" SHA256="+Hash(target));Extract(target);AppDomain.CurrentDomain.AssemblyResolve+=Resolve;Pcl=Assembly.LoadFrom(target);
   Jit();if(mode=="jit")return Failures==0?0:1;Initialize();Log("SETTINGS SystemSystemUpdate="+T("Settings").GetMethods(All).Single(x=>x.Name=="Get"&&x.IsGenericMethodDefinition).MakeGenericMethod(typeof(int)).Invoke(null,new object[]{"SystemSystemUpdate",null}));if(mode=="online"){Online();return Failures==0?0:1;}if(mode=="cfmeta"){CfMeta();return Failures==0?0:1;}if(mode=="inspect"){Inspect();return Failures==0?0:1;}
   int timeout=args.Length>2?Int32.Parse(args[2]):180;object list=T("ModDownload").GetField("DlClientListLoader",All).GetValue(null);
   Invoke(list,"Start",new object[]{null,false});WaitLoader(list,timeout);object output=Get(list,"Output");Log("LIST OUTPUT "+(output==null?"null":output.GetType().FullName));
   if(output!=null)foreach(var f in output.GetType().GetFields(All))Log("LIST "+f.Name+"="+Summary(f.GetValue(output)));
   if(mode=="vanilla")Vanilla(timeout);if(mode=="packs"||mode=="cfpack")Packs(timeout,mode=="cfpack");Log("RESULT failures="+Failures);return Failures==0?0:1;
  }catch(Exception ex){Failures++;Log("FATAL "+Unwrap(ex));return 2;}finally{Writer.Flush();Environment.Exit(Failures==0?0:1);}
 }
 static void Log(string s){string line=DateTime.UtcNow.ToString("o")+" "+s;Console.WriteLine(line);if(Writer!=null)Writer.WriteLine(line);}
 static Exception Unwrap(Exception e){while(e is TargetInvocationException&&e.InnerException!=null)e=e.InnerException;return e;}
 static string Hash(string p){using(var s=File.OpenRead(p))using(var h=SHA256.Create())return BitConverter.ToString(h.ComputeHash(s)).Replace("-","");}
 static string Summary(object o){if(o==null)return "null";if(o is string)return (string)o;var c=o as ICollection;return c!=null?o.GetType().FullName+" Count="+c.Count:o.ToString();}
 static Type T(string n){return Pcl.GetType("PCL."+n,true);}
 static object Get(object o,string n){var p=o.GetType().GetProperty(n,All);if(p!=null)return p.GetValue(o,null);var f=o.GetType().GetField(n,All);return f==null?null:f.GetValue(o);}
 static object Invoke(object o,string n,object[] a){return o.GetType().GetMethods(All).Single(m=>m.Name==n&&m.GetParameters().Length==a.Length).Invoke(o,a);}
 static Assembly Resolve(object s,ResolveEventArgs e){string n=new AssemblyName(e.Name).Name;if(Pcl!=null&&n==Pcl.GetName().Name)return Pcl;string f=Path.Combine(Lib,n+".dll");return File.Exists(f)?Assembly.LoadFrom(f):null;}
 static void Extract(string p){using(var a=AssemblyDefinition.ReadAssembly(p)){var r=(EmbeddedResource)a.MainModule.Resources.Single(x=>x.Name=="PCL.Resources.resources");using(var rr=new ResourceReader(r.GetResourceStream())){var e=rr.GetEnumerator();while(e.MoveNext()){byte[] b=e.Value as byte[];string n=(string)e.Key;if(b==null||b.Length<2||b[0]!=77||b[1]!=90)continue;File.WriteAllBytes(Path.Combine(Lib,n+".dll"),b);Log("DEPENDENCY "+n+" bytes="+b.Length);}}}}
 static void Jit(){string[] types={"PageDownloadInstall","ModModpack","ModModpack","PageDownloadResourceDetail","PageDownloadModify","FormMain"};string[] names={"LoaderInit","ModpackInstall","ModpackInstall","InitializeStackResourceVersions","LoadInstanceFinished","FormMain_Loaded"};int[] argc={0,0,3,1,1,2};for(int i=0;i<types.Length;i++){try{var m=T(types[i]).GetMethods(All).Single(x=>x.Name==names[i]&&x.GetParameters().Length==argc[i]);RuntimeHelpers.PrepareMethod(m.MethodHandle);Log("JIT PASS "+m);}catch(Exception ex){Failures++;Log("JIT FAIL "+types[i]+"."+names[i]+" "+Unwrap(ex));}}}
 static void Initialize(){App=(Application)Activator.CreateInstance(T("Application"),true);App.Startup-=(StartupEventHandler)Delegate.CreateDelegate(typeof(StartupEventHandler),App,T("Application").GetMethod("Application_Startup",All));App.DispatcherUnhandledException-=(DispatcherUnhandledExceptionEventHandler)Delegate.CreateDelegate(typeof(DispatcherUnhandledExceptionEventHandler),App,T("Application").GetMethod("Application_DispatcherUnhandledException",All));App.DispatcherUnhandledException+=delegate(object sender,DispatcherUnhandledExceptionEventArgs e){Log("DISPATCHER ERROR "+e.Exception);e.Handled=true;};App.ShutdownMode=ShutdownMode.OnExplicitShutdown;Assembly.LoadFrom(Path.Combine(Lib,"MeloongCore.dll")).GetType("MeloongCore.Main").GetMethod("Init").Invoke(null,new object[]{"PCL"});T("Application").GetMethod("InitializeComponent").Invoke(App,null);foreach(var f in typeof(Application).GetFields(All).Where(x=>x.FieldType==typeof(Uri)&&x.Name.IndexOf("startup",StringComparison.OrdinalIgnoreCase)>=0))f.SetValue(App,null);T("ModMinecraft").GetField("_McFolderSelected",All).SetValue(null,Game);T("ModBase").GetField("PathTemp",All).SetValue(null,Path.Combine(Root,"temp")+Path.DirectorySeparatorChar);Log("APP resources initialized; Startup deliberately not invoked. GAME="+Game);}
 static void Pump(){var frame=new DispatcherFrame();Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,new Action(delegate{frame.Continue=false;}));Dispatcher.PushFrame(frame);Thread.Sleep(100);}
 static void WaitLoader(object loader,int timeout){var watch=Stopwatch.StartNew();string previous=null;long tick=0;while(watch.Elapsed.TotalSeconds<timeout){string state=Convert.ToString(Get(loader,"State"));string line="LOADER "+Get(loader,"Name")+" state="+state+" progress="+Get(loader,"Progress");if(state!=previous||watch.ElapsedMilliseconds-tick>10000){Log(line);previous=state;tick=watch.ElapsedMilliseconds;}if(state=="Finished")return;if(state=="Failed"||state=="Aborted"||state=="Canceled"){DumpLoader(loader,0);throw new Exception(line,Get(loader,"Error") as Exception);}Pump();}throw new TimeoutException("Loader exceeded "+timeout+" seconds: "+Get(loader,"Name"));}
 static void Vanilla(int timeout){string folder=Path.Combine(Game,"versions","RuntimeProbe-1.20.1")+Path.DirectorySeparatorChar;Directory.CreateDirectory(folder);var m=T("ModDownloadLib").GetMethods(All).Single(x=>x.Name=="McDownloadClientLoader");object loaders=m.Invoke(null,new object[]{"1.20.1",null,folder});Type combo=T("ModLoader+LoaderCombo\u00601").MakeGenericType(typeof(string));object loader=Activator.CreateInstance(combo,new object[]{"RuntimeProbe vanilla 1.20.1",loaders});Invoke(loader,"Start",new object[]{folder,false});WaitLoader(loader,timeout);foreach(var file in Directory.GetFiles(folder,"*",SearchOption.AllDirectories))Log("VANILLA FILE "+file+" bytes="+new FileInfo(file).Length+" SHA256="+Hash(file));}
 static void DumpLoader(object l,int depth){if(depth>4)return;Log("DUMP "+Get(l,"Name"));var fs=Get(l,"Files") as IEnumerable;if(fs!=null)foreach(var f in fs)Log("NETFILE path=["+Get(f,"LocalPath")+"]");var ls=Get(l,"Loaders") as IEnumerable;if(ls!=null)foreach(var x in ls)DumpLoader(x,depth+1);} static void Packs(int timeout,bool cfOnly){
 object form=System.Runtime.Serialization.FormatterServices.GetUninitializedObject(T("FormMain"));
 object button=Activator.CreateInstance(T("MyExtraButton"));
 T("FormMain").GetField("_BtnExtraDownload",All).SetValue(form,button);
 T("ModMain").GetField("FrmMain",All).SetValue(null,form);
 Log("PACK host uses only uninitialized FormMain holder and real MyExtraButton; no window shown.");
 foreach(string kind in (cfOnly?new[]{"curseforge"}:new[]{"curseforge","modrinth"})){
  string pack=Path.GetFullPath(Path.Combine(Root,"../runtime-"+kind+(kind=="modrinth"?".mrpack":".zip")));
  string name="RuntimeProbe-"+kind+"-"+DateTime.UtcNow.ToString("HHmmss");
  var method=T("ModModpack").GetMethods(All).Single(x=>x.Name=="ModpackInstall"&&x.GetParameters().Length==3);
  object loader=null;Exception failure=null;bool done=false;
  var thread=new Thread(delegate(){try{loader=method.Invoke(null,new object[]{pack,name,null});}catch(Exception ex){failure=Unwrap(ex);}finally{done=true;}});thread.IsBackground=true;thread.Start();
  var watch=Stopwatch.StartNew();while(!done&&watch.Elapsed.TotalSeconds<30)Pump();
  if(!done)throw new TimeoutException("Pack dispatch did not return: "+kind);if(failure!=null)throw failure;if(loader==null)throw new Exception("Pack dispatch returned null: "+kind);
  WaitLoader(loader,timeout);
  string dir=Path.Combine(Game,"versions",name);Log("PACK PASS "+kind+" path="+dir);
  foreach(string file in Directory.GetFiles(dir,"*",SearchOption.AllDirectories).Where(x=>x.EndsWith(".jar")||x.EndsWith(".json")||x.EndsWith(".txt")))Log("PACK FILE "+file+" bytes="+new FileInfo(file).Length+" SHA256="+Hash(file));
 }
}
 static void CfMeta(){
 var m=T("ModDownload").GetMethod("DlModRequest",All);
 var http=m.GetParameters()[1].ParameterType.GetProperty("Get").GetValue(null,null);
 object result=null;Exception error=null;bool done=false;var thread=new Thread(delegate(){try{result=m.Invoke(null,new object[]{"https://api.curseforge.com/v1/mods/search?gameId=432&searchFilter=sodium&gameVersion=1.20.1&modLoaderType=4&pageSize=5",http,null,"application/json"});}catch(Exception ex){error=Unwrap(ex);}finally{done=true;}});thread.IsBackground=true;thread.Start();var w=Stopwatch.StartNew();while(!done&&w.Elapsed.TotalSeconds<90)Pump();if(!done)throw new TimeoutException("CF metadata timeout");if(error!=null)throw error;
 File.WriteAllText(Path.GetFullPath(Path.Combine(Root,"../runtime-cf-search.json")),result.ToString(),new UTF8Encoding(false));
 Log("CF metadata response saved; no credentials read or printed.");
}
static void Online(){
 object page=System.Runtime.Serialization.FormatterServices.GetUninitializedObject(T("PageDownloadResourceDetail"));
 var target=T("PageDownloadResourceDetail").GetField("TargetResourceType",All);
 foreach(string kind in new[]{"ModPack","Mod"}){
  target.SetValue(page,Enum.Parse(target.FieldType,kind));
  object version=Activator.CreateInstance(T("ResourceVersion"));T("ResourceVersion").GetField("Display").SetValue(version,"RuntimeProbe version");T("ResourceVersion").GetField("ReleaseDate").SetValue(version,DateTime.UtcNow);
  var list=(IList)Activator.CreateInstance(typeof(System.Collections.Generic.List<>).MakeGenericType(T("ResourceVersion")));list.Add(version);
  var stack=new System.Windows.Controls.StackPanel();stack.Tag=list;
  T("PageDownloadResourceDetail").GetMethod("InitializeStackResourceVersions",All).Invoke(page,new object[]{stack});
  if(stack.Children.Count!=2)throw new Exception("Wrong online item count "+stack.Children.Count);
  object item=stack.Children[0];var initializer=(Delegate)item.GetType().GetField("Initializer",All).GetValue(item);object capture=initializer.Target;
  var primary=(Delegate)capture.GetType().GetField("$VB$Local_ClickHandler",All).GetValue(capture);
  var secondary=(Delegate)capture.GetType().GetField("$VB$Local_SaveAsButtonHandler",All).GetValue(capture);
  Log("ONLINE "+kind+" primary="+primary.Method.Name+" type="+primary.GetType().FullName+" secondary="+(secondary==null?"null":secondary.Method.Name+" type="+secondary.GetType().FullName));
  if(primary.Method.Name!=(kind=="ModPack"?"Install_Click":"Save_Click"))throw new Exception("Wrong primary click");
  if(kind=="ModPack"&&(secondary==null||secondary.Method.Name!="Save_Click"))throw new Exception("Missing save-as");
  if(kind=="Mod"&&secondary!=null)throw new Exception("Non-pack acquired save-as button");
 }
 Log("ONLINE PASS both resource branches executed");
}
static void Inspect(){foreach(var name in new[]{"ModDownloadLib+McInstallRequest","ModLoader+LoaderBase","ModDownload+DlClientListResult","Settings"}){Type t=T(name);Log("TYPE "+t);foreach(var f in t.GetFields(All))Log("FIELD "+f);foreach(var p in t.GetProperties(All))Log("PROPERTY "+p);foreach(var m in t.GetMethods(All).Where(x=>x.DeclaringType==t&&!x.IsSpecialName))Log("METHOD "+m);}}
}
