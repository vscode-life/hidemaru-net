/*
 * HmNetPInvoke ver 1.841
 * Copyright (C) 2021 Akitsugu Komiyama
 * under the MIT License
 **/



using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace HmNetPInvoke
{
    public partial class HmMacroCOMVar {
        private const string HmMacroCOMVarInterface = "f9277cf4-27dd-4a07-ad00-3b449ea6d8c5";
    }
}


namespace HmNetPInvoke
{
    // 秀丸のCOMから呼び出して、マクロ⇔COMといったように、マクロとプログラムで変数値を互いに伝搬する
    [ComVisible(true)]
    [Guid(HmMacroCOMVarInterface)]
    public partial class HmMacroCOMVar
    {
        private static object marcroVar = null;
        public object DllToMacro()
        {
            return marcroVar;
        }
        public int MacroToDll(object variable)
        {
            marcroVar = variable;
            return 1;
        }
    }

    public partial class HmMacroCOMVar
    {
        static HmMacroCOMVar()
        {
            var h = new HmMacroCOMVar();
            myGuidLabel = h.GetType().GUID.ToString();
            myClassFullName = h.GetType().FullName;
        }

        internal static void SetMacroVar(object obj)
        {
            marcroVar = obj;
        }
        internal static object GetMacroVar()
        {
            return marcroVar;
        }
        private static string myGuidLabel = "";
        private static string myClassFullName = "";

        internal static string GetMyTargetDllFullPath(string thisDllFullPath)
        {
            string myTargetClass = myClassFullName;
            string thisComHostFullPath = System.IO.Path.ChangeExtension(thisDllFullPath, "comhost.dll");
            if (System.IO.File.Exists(thisComHostFullPath))
            {
                return thisComHostFullPath;
            }

            return thisDllFullPath;
        }

        internal static string GetMyTargetClass(string thisDllFullPath)
        {
            string myTargetClass = myClassFullName;
            string thisComHostFullPath = System.IO.Path.ChangeExtension(thisDllFullPath, "comhost.dll");
            if (System.IO.File.Exists(thisComHostFullPath))
            {
                myTargetClass = "{" + myGuidLabel + "}";
            }

            return myTargetClass;
        }

        internal static object GetVar(string var_name)
        {
            string myDllFullPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string myTargetDllFullPath = GetMyTargetDllFullPath(myDllFullPath);
            string myTargetClass = GetMyTargetClass(myDllFullPath);
            ClearVar();
            var result = Hm.Macro.Eval($@"
                #_COM_NET_PINVOKE_MACRO_VAR = createobject(@""{myTargetDllFullPath}"", @""{myTargetClass}"" );
                #_COM_NET_PINVOKE_MACRO_VAR_RESULT = member(#_COM_NET_PINVOKE_MACRO_VAR, ""MacroToDll"", {var_name});
                releaseobject(#_COM_NET_PINVOKE_MACRO_VAR);
                #_COM_NET_PINVOKE_MACRO_VAR_RESULT = 0;
            ");
            if (result.Error != null)
            {
                throw result.Error;
            }
            return HmMacroCOMVar.marcroVar;
        }

        internal static int SetVar(string var_name, object obj)
        {
            string myDllFullPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string myTargetDllFullPath = GetMyTargetDllFullPath(myDllFullPath);
            string myTargetClass = GetMyTargetClass(myDllFullPath);
            ClearVar();
            HmMacroCOMVar.marcroVar = obj;
            var result = Hm.Macro.Eval($@"
                #_COM_NET_PINVOKE_MACRO_VAR = createobject(@""{myTargetDllFullPath}"", @""{myTargetClass}"" );
                {var_name} = member(#_COM_NET_PINVOKE_MACRO_VAR, ""DllToMacro"" );
                releaseobject(#_COM_NET_PINVOKE_MACRO_VAR);
            ");
            if (result.Error != null)
            {
                throw result.Error;
            }
            return 1;
        }

        internal static void ClearVar()
        {
            HmMacroCOMVar.marcroVar = null;
        }
    }


#if BUILD_DLL
    public partial class Hm
#else
    internal partial class Hm
#endif
    {
        public static partial class Edit
        {
            static partial void SetTotalText(string text)
            {
                string myDllFullPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string myTargetDllFullPath = HmMacroCOMVar.GetMyTargetDllFullPath(myDllFullPath);
                string myTargetClass = HmMacroCOMVar.GetMyTargetClass(myDllFullPath);
                HmMacroCOMVar.SetMacroVar(text);
                string cmd = $@"
                begingroupundo;
                selectall;
                #_COM_NET_PINVOKE_MACRO_VAR = createobject(@""{myTargetDllFullPath}"", @""{myTargetClass}"" );
                insert member(#_COM_NET_PINVOKE_MACRO_VAR, ""DllToMacro"" );
                releaseobject(#_COM_NET_PINVOKE_MACRO_VAR);
                endgroupundo;
                ";
                Macro.IResult result = null;
                if (Macro.IsExecuting)
                {
                    result = Hm.Macro.Eval(cmd);
                } else
                {
                    result = Hm.Macro.Exec.Eval(cmd);
                }

                HmMacroCOMVar.ClearVar();
                if (result.Error != null)
                {
                    throw result.Error;
                }
            }

            static partial void SetSelectedText(string text)
            {
                string myDllFullPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string myTargetDllFullPath = HmMacroCOMVar.GetMyTargetDllFullPath(myDllFullPath);
                string myTargetClass = HmMacroCOMVar.GetMyTargetClass(myDllFullPath);
                HmMacroCOMVar.SetMacroVar(text);
                string cmd = $@"
                if (selecting) {{
                #_COM_NET_PINVOKE_MACRO_VAR = createobject(@""{myTargetDllFullPath}"", @""{myTargetClass}"" );
                insert member(#_COM_NET_PINVOKE_MACRO_VAR, ""DllToMacro"" );
                releaseobject(#_COM_NET_PINVOKE_MACRO_VAR);
                }}
                ";

                Macro.IResult result = null;
                if (Macro.IsExecuting)
                {
                    result = Hm.Macro.Eval(cmd);
                }
                else
                {
                    result = Hm.Macro.Exec.Eval(cmd);
                }

                HmMacroCOMVar.ClearVar();
                if (result.Error != null)
                {
                    throw result.Error;
                }
            }

            static partial void SetLineText(string text)
            {
                string myDllFullPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string myTargetDllFullPath = HmMacroCOMVar.GetMyTargetDllFullPath(myDllFullPath);
                string myTargetClass = HmMacroCOMVar.GetMyTargetClass(myDllFullPath);
                HmMacroCOMVar.SetMacroVar(text);
                var pos = Edit.CursorPos;
                string cmd = $@"
                begingroupundo;
                selectline;
                #_COM_NET_PINVOKE_MACRO_VAR = createobject(@""{myTargetDllFullPath}"", @""{myTargetClass}"" );
                insert member(#_COM_NET_PINVOKE_MACRO_VAR, ""DllToMacro"" );
                releaseobject(#_COM_NET_PINVOKE_MACRO_VAR);
                moveto2 {pos.Column}, {pos.LineNo};
                endgroupundo;
                ";

                Macro.IResult result = null;
                if (Macro.IsExecuting)
                {
                    result = Hm.Macro.Eval(cmd);
                }
                else
                {
                    result = Hm.Macro.Exec.Eval(cmd);
                }

                HmMacroCOMVar.ClearVar();
                if (result.Error != null)
                {
                    throw result.Error;
                }
            }

        }


        public static partial class Macro
        {
            // マクロでの問い合わせ結果系
            public interface IStatementResult
            {
                int Result { get; }
                String Message { get; }
                Exception Error { get; }
            }


            private class TStatementResult : IStatementResult
            {
                public int Result { get; set; }
                public string Message { get; set; }
                public Exception Error { get; set; }

                public TStatementResult(int Result, String Message, Exception Error)
                {
                    this.Result = Result;
                    this.Message = Message;
                    this.Error = Error;
                }
            }

            private static int statement_base_random = 0;
            internal static IStatementResult Statement(string funcname, params object[] args)
            {
                if (statement_base_random == 0)
                {
                    statement_base_random = new System.Random().Next(Int16.MaxValue) + 1;

                }

                var arg_list = new List<KeyValuePair<String, Object>>();
                int cur_random = new Random().Next(Int16.MaxValue) + 1;
                foreach (var value in args)
                {
                    bool success = false;
                    cur_random++;
                    object normalized_arg = null;
                    // Boolean型であれば、True:1 Flase:0にマッピングする
                    if (value is bool)
                    {
                        success = true;
                        if ((bool)value == true)
                        {
                            normalized_arg = 1;
                        }
                        else
                        {
                            normalized_arg = 0;
                        }
                    }

                    // まずは整数でトライ
                    int itmp = 0;
                    success = Int32.TryParse(value.ToString(), out itmp);

                    if (success == true)
                    {
                        normalized_arg = itmp;
                    }

                    else
                    {
                        // 次に少数でトライ
                        Double dtmp = 0;
                        success = Double.TryParse(value.ToString(), out dtmp);
                        if (success)
                        {
                            normalized_arg = (int)(dtmp);
                        }

                        else
                        {
                            normalized_arg = 0;
                        }
                    }


                    // 成功しなかった
                    if (!success)
                    {
                        normalized_arg = value.ToString();
                    }

                    if (normalized_arg is int)
                    {
                        string key = "#AsStatement_" + statement_base_random.ToString() + '_' + cur_random.ToString();
                        arg_list.Add(new KeyValuePair<string, object>(key, normalized_arg));
                        Macro.Var[key] = normalized_arg;
                    }
                    else if (normalized_arg is string)
                    {
                        string key = "$AsStatement_" + statement_base_random.ToString() + '_' + cur_random.ToString();
                        arg_list.Add(new KeyValuePair<string, object>(key, normalized_arg));
                        Macro.Var[key] = normalized_arg;
                    }
                }

                // keyをリスト化
                var arg_keys = new List<String>();
                foreach (var l in arg_list)
                {
                    arg_keys.Add(l.Key);
                }

                // それを「,」で繋げる
                string args_string = String.Join(", ", arg_keys);
                // それを指定の「文」で実行する形
                string expression = $"{funcname} {args_string};\n";

                // 実行する
                IResult ret = Macro.Eval(expression);
                // 成否も含めて結果を入れる。
                // new TResult(ret.Result, ret.Message, ret.Error);

                // 使ったので削除
                foreach (var l in arg_list)
                {
                    if (l.Value is Int32 || l.Value is Int64)
                    {
                        Macro.Var[l.Key] = 0;
                    }
                    else if (l.Value is string)
                    {
                        Macro.Var[l.Key] = "";
                    }
                }

                IStatementResult result = new TStatementResult(ret.Result, ret.Message, ret.Error);
                return result;
            }

            // マクロでの問い合わせ結果系
            public interface IFunctionResult
            {
                object Result { get; }
                String Message { get; }
                Exception Error { get; }
            }

            private class TFunctionResult : IFunctionResult
            {
                public object Result { get; set; }
                public string Message { get; set; }
                public Exception Error { get; set; }

                public TFunctionResult(object Result, String Message, Exception Error)
                {
                    this.Result = Result;
                    this.Message = Message;
                    this.Error = Error;
                }
            }

            private static int funciton_base_random = 0;
            public static IFunctionResult Function(string funcname, params object[] args)
            {
                if (funciton_base_random == 0)
                {
                    funciton_base_random = new System.Random().Next(Int16.MaxValue) + 1;

                }

                var arg_list = new List<KeyValuePair<String, Object>>();
                int cur_random = new Random().Next(Int16.MaxValue) + 1;
                foreach (var value in args)
                {
                    bool success = false;
                    cur_random++;
                    object normalized_arg = null;
                    // Boolean型であれば、True:1 Flase:0にマッピングする
                    if (value is bool)
                    {
                        success = true;
                        if ((bool)value == true)
                        {
                            normalized_arg = 1;
                        }
                        else
                        {
                            normalized_arg = 0;
                        }
                    }

                    // まずは整数でトライ
                    int itmp = 0;
                    success = int.TryParse(value.ToString(), out itmp);

                    if (success == true)
                    {
                        normalized_arg = itmp;
                    }

                    else
                    {
                        // 次に少数でトライ
                        Double dtmp = 0;
                        success = Double.TryParse(value.ToString(), out dtmp);
                        if (success)
                        {
                            normalized_arg = (int)(dtmp);
                        }

                        else
                        {
                            normalized_arg = 0;
                        }
                    }

                    // 成功しなかった
                    if (!success)
                    {
                        normalized_arg = value.ToString();
                    }

                    if (normalized_arg is int)
                    {
                        string key = "#AsStatement_" + funciton_base_random.ToString() + '_' + cur_random.ToString();
                        arg_list.Add(new KeyValuePair<string, object>(key, normalized_arg));
                        Macro.Var[key] = normalized_arg;
                    }
                    else if (normalized_arg is string)
                    {
                        string key = "$AsStatement_" + funciton_base_random.ToString() + '_' + cur_random.ToString();
                        arg_list.Add(new KeyValuePair<string, object>(key, normalized_arg));
                        Macro.Var[key] = normalized_arg;
                    }
                }

                // keyをリスト化
                var arg_keys = new List<String>();
                foreach (var l in arg_list)
                {
                    arg_keys.Add(l.Key);
                }

                // それを「,」で繋げる
                string args_string = String.Join(", ", arg_keys);
                // それを指定の「文」で実行する形
                string expression = $"{funcname}({args_string})";

                //----------------------------------------------------------------
                TFunctionResult result = new TFunctionResult(null, "", null);

                Object ret = Macro.Var[expression];
 
                if (ret.GetType().Name != "String")
                {
                    result.Result = (int)ret + 0; // 確実に複製を
                    result.Message = "";
                    result.Error = null;
                }
                else
                {
                    result.Result = (String)ret + ""; // 確実に複製を
                    result.Message = "";
                    result.Error = null;
                }

                // 使ったので削除
                foreach (var l in arg_list)
                {
                    if (l.Value is int)
                    {
                        Macro.Var[l.Key] = 0;
                    }
                    else if (l.Value is string)
                    {
                        Macro.Var[l.Key] = "";
                    }
                }

                return result;
            }

            /// <summary>
            /// 対象の「秀丸マクロ変数名」への読み書き
            /// </summary>
            /// <param name = "var_name">書き込みの場合は、変数の値</param>
            /// <returns>読み取りの場合は、対象の変数の値</returns>
            internal static TMacroVar Var = new TMacroVar();
            internal sealed class TMacroVar
            {
                public Object this[String var_name]
                {
                    get
                    {
                        return GetMethod(var_name);
                    }
                    set
                    {
                        value = SetMethod(var_name, value);
                    }
                }

                private static object SetMethod(string var_name, object value)
                {
                    if (var_name.StartsWith("#"))
                    {
                        Object result = new Object();

                        // Boolean型であれば、True:1 Flase:0にマッピングする
                        if (value is bool)
                        {
                            if ((Boolean)value == true)
                            {
                                value = 1;
                            }
                            else
                            {
                                value = 0;
                            }
                        }

                        // まずは整数でトライ
                        int itmp = 0;
                        bool success = int.TryParse(value.ToString(), out itmp);
                        if (success == true)
                        {
                            result = itmp;
                        }
                        else
                        {
                            // 次に少数でトライ
                            double dtmp = 0;
                            success = double.TryParse(value.ToString(), out dtmp);
                            if (success)
                            {
                                result = (int)(dtmp);
                            }
                            else
                            {
                                result = 0;
                            }
                        }
                        HmMacroCOMVar.SetVar(var_name, value);
                        HmMacroCOMVar.ClearVar();
                    }

                    else // if (var_name.StartsWith("$")
                    {

                        String result = value.ToString();
                        HmMacroCOMVar.SetVar(var_name, value);
                        HmMacroCOMVar.ClearVar();
                    }

                    return value;
                }

                private static object GetMethod(string var_name)
                {
                    HmMacroCOMVar.ClearVar();
                    Object ret = HmMacroCOMVar.GetVar(var_name);
                    if (ret.GetType().Name != "String")
                    {
                        return (int)ret + 0; // 確実に複製を
                    }
                    else
                    {
                        return (String)ret + ""; // 確実に複製を
                    }
                }
            }
        }
    }
}

