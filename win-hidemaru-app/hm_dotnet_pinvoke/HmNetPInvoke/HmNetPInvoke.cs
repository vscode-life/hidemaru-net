﻿/*
 * HmNetPInvoke ver 1.821
 * Copyright (C) 2021 Akitsugu Komiyama
 * under the MIT License
 **/

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace HmNetPInvoke
{
#if BUILD_DLL
    public partial class Hm
#else
    internal partial class Hm
#endif
    {
        static Hm() {
            SetVersion();
            BindHidemaruExternFunctions();
        }

        private static void SetVersion()
        {
            if (Version == 0)
            {
                string hidemaru_fullpath = GetHidemaruExeFullPath();
                System.Diagnostics.FileVersionInfo vi = System.Diagnostics.FileVersionInfo.GetVersionInfo(hidemaru_fullpath);
                Version = 100 * vi.FileMajorPart + 10 * vi.FileMinorPart + 1 * vi.FileBuildPart + 0.01 * vi.FilePrivatePart;
            }
        }
        /// <summary>
        /// 秀丸バージョンの取得
        /// </summary>
        /// <returns>秀丸バージョン</returns>
        public static double Version { get; private set; } = 0;

        private const int filePathMaxLength = 512;

        private static string GetHidemaruExeFullPath()
        {
            var sb = new StringBuilder(filePathMaxLength);
            GetModuleFileName(IntPtr.Zero, sb, filePathMaxLength);
            string hidemaru_fullpath = sb.ToString();
            return hidemaru_fullpath;
        }

        /// <summary>
        /// 呼ばれたプロセスの現在の秀丸エディタのウィンドウハンドルを返します。
        /// </summary>
        /// <returns>現在の秀丸エディタのウィンドウハンドル</returns>
        public static IntPtr WindowHandle
        {
            get
            {
                return pGetCurrentWindowHandle();
            }
        }

        public static class File
        {
            public interface IHidemaruEncoding
            {
                int HmEncode { get; }
            }
            public interface IMicrosoftEncoding
            {
                int MsCodePage { get; }
            }

            public interface IEncoding : IHidemaruEncoding, IMicrosoftEncoding
            {
            }

            private struct TEncodings : IEncoding
            {
                public int HmEncode { get; set; }
                public int MsCodePage { get; set; }
            }

            public static IEncoding GetEncoding(String filepath)
            {
                int hm_encode = GetHmEncode(filepath);
                int ms_codepage = GetMsCodePage(hm_encode);
                TEncodings encoding = new TEncodings();
                encoding.HmEncode = hm_encode;
                encoding.MsCodePage = ms_codepage;
                return encoding;
            }

            private static int GetHmEncode(string filepath)
            {
                if (pAnalyzeEncoding == null)
                {
                    new NullReferenceException("Hidemaru_AnalyzeEncoding");
                    return -1;
                }

                return pAnalyzeEncoding(filepath, IntPtr.Zero, IntPtr.Zero);
            }

            private static int[] key_encode_value_codepage_array = {
                0,      // Unknown
                932,    // encode = 1 ANSI/OEM Japanese; Japanese (Shift-JIS)
                1200,   // encode = 2 Unicode UTF-16, little-endian
                51932,  // encode = 3 EUC
                50221,  // encode = 4 JIS
                65000,  // encode = 5 UTF-7
                65001,  // encode = 6 UTF-8
                1201,   // encode = 7 Unicode UTF-16, big-endian
                1252,   // encode = 8 欧文 ANSI Latin 1; Western European (Windows)
                936,    // encode = 9 簡体字中国語 ANSI/OEM Simplified Chinese (PRC, Singapore); Chinese Simplified (GB2312)
                950,    // encode =10 繁体字中国語 ANSI/OEM Traditional Chinese (Taiwan; Hong Kong SAR, PRC); Chinese Traditional (Big5)
                949,    // encode =11 韓国語 ANSI/OEM Korean (Unified Hangul Code)
                1361,   // encode =12 韓国語 Korean (Johab)
                1250,   // encode =13 中央ヨーロッパ言語 ANSI Central European; Central European (Windows)
                1257,   // encode =14 バルト語 ANSI Baltic; Baltic (Windows)
                1253,   // encode =15 ギリシャ語 ANSI Greek; Greek (Windows)
                1251,   // encode =16 キリル言語 ANSI Cyrillic; Cyrillic (Windows)
                42,     // encode =17 シンボル
                1254,   // encode =18 トルコ語 ANSI Turkish; Turkish (Windows)
                1255,   // encode =19 ヘブライ語 ANSI Hebrew; Hebrew (Windows)
                1256,   // encode =20 アラビア語 ANSI Arabic; Arabic (Windows)
                874,    // encode =21 タイ語 ANSI/OEM Thai (same as 28605, ISO 8859-15); Thai (Windows)
                1258,   // encode =22 ベトナム語 ANSI/OEM Vietnamese; Vietnamese (Windows)
                10001,  // encode =23 x-mac-japanese Japanese (Mac)
                850,    // encode =24 OEM/DOS
                0,      // encode =25 その他
                12000,  // encode =26 Unicode (UTF-32) little-endian
                12001,  // encode =27 Unicode (UTF-32) big-endian

            };

            private static int GetMsCodePage(int hidemaru_encode)
            {
                int result_codepage = 0;

                if (pAnalyzeEncoding == null)
                {
                    new NullReferenceException("Hidemaru_AnalyzeEncoding");
                    return result_codepage;
                }

                /*
                 *
                    Shift-JIS encode=1 codepage=932
                    Unicode encode=2 codepage=1200
                    EUC encode=3 codepage=51932
                    JIS encode=4 codepage=50221
                    UTF-7 encode=5 codepage=65000
                    UTF-8 encode=6 codepage=65001
                    Unicode (Big-Endian) encode=7 codepage=1201
                    欧文 encode=8 codepage=1252
                    簡体字中国語 encode=9 codepage=936
                    繁体字中国語 encode=10 codepage=950
                    韓国語 encode=11 codepage=949
                    韓国語(Johab) encode=12 codepage=1361
                    中央ヨーロッパ言語 encode=13 codepage=1250
                    バルト語 encode=14 codepage=1257
                    ギリシャ語 encode=15 codepage=1253
                    キリル言語 encode=16 codepage=1251
                    シンボル encode=17 codepage=42
                    トルコ語 encode=18 codepage=1254
                    ヘブライ語 encode=19 codepage=1255
                    アラビア語 encode=20 codepage=1256
                    タイ語 encode=21 codepage=874
                    ベトナム語 encode=22 codepage=1258
                    Macintosh encode=23 codepage=0
                    OEM/DOS encode=24 codepage=0
                    その他 encode=25 codepage=0
                    UTF-32 encode=27 codepage=12000
                    UTF-32 (Big-Endian) encode=28 codepage=12001
                */
                if (hidemaru_encode <= 0)
                {
                    return result_codepage;
                }

                if (hidemaru_encode < key_encode_value_codepage_array.Length)
                {
                    // 把握しているコードページなので入れておく
                    result_codepage = key_encode_value_codepage_array[hidemaru_encode];
                    return result_codepage;
                }
                else // 長さ以上なら、予期せぬ未来のencode番号対応
                {
                    return result_codepage;
                }
            }
        }

        public static class Edit
        {
            /// <summary>
            /// 現在アクティブな編集領域のテキスト全体を返す。
            /// </summary>
            /// <returns>編集領域のテキスト全体</returns>

            public static string TotalText
            {
                get
                {
                    string totalText = "";
                    try
                    {
                        IntPtr hGlobal = pGetTotalTextUnicode();
                        if (hGlobal == IntPtr.Zero)
                        {
                            new InvalidOperationException("Hidemaru_GetTotalTextUnicode_Exception");
                        }

                        var pwsz = GlobalLock(hGlobal);
                        if (pwsz != IntPtr.Zero)
                        {
                            totalText = Marshal.PtrToStringUni(pwsz);
                            GlobalUnlock(hGlobal);
                        }
                        GlobalFree(hGlobal);
                    }
                    catch (Exception)
                    {
                        throw;
                    }

                    return totalText;
                }
            }

            /// <summary>
            /// 現在、単純選択している場合、その選択中のテキスト内容を返す。
            /// </summary>
            /// <returns>選択中のテキスト内容</returns>
            public static string SelectedText
            {
                get
                {
                    {
                        string selectedText = "";
                        try
                        {
                            IntPtr hGlobal = pGetSelectedTextUnicode();
                            if (hGlobal == IntPtr.Zero)
                            {
                                new InvalidOperationException("Hidemaru_GetSelectedTextUnicode_Exception");
                            }

                            var pwsz = GlobalLock(hGlobal);
                            if (pwsz != IntPtr.Zero)
                            {
                                selectedText = Marshal.PtrToStringUni(pwsz);
                                GlobalUnlock(hGlobal);
                            }
                            GlobalFree(hGlobal);
                        }
                        catch (Exception)
                        {
                            throw;
                        }

                        return selectedText;
                    }
                }
            }

            /// <summary>
            /// 現在、カーソルがある行(エディタ的)のテキスト内容を返す。
            /// </summary>
            /// <returns>選択中のテキスト内容</returns>
            public static string LineText
            {
                get
                {
                    {
                        string lineText = "";

                        ICursorPos pos = CursorPos;
                        if (pos.LineNo < 0 || pos.Column < 0)
                        {
                            return lineText;
                        }

                        try
                        {
                            IntPtr hGlobal = pGetLineTextUnicode(pos.LineNo);
                            if (hGlobal == IntPtr.Zero)
                            {
                                new InvalidOperationException("Hidemaru_GetLineTextUnicode_Exception");
                            }

                            var pwsz = GlobalLock(hGlobal);
                            if (pwsz != IntPtr.Zero)
                            {
                                lineText = Marshal.PtrToStringUni(pwsz);
                                GlobalUnlock(hGlobal);
                            }
                            GlobalFree(hGlobal);
                        }
                        catch (Exception)
                        {
                            throw;
                        }

                        return lineText;
                    }
                }
            }

            /// <summary>
            /// CursorPos の返り値のインターフェイス
            /// </summary>
            /// <returns>(LineNo, Column)</returns>
            public interface ICursorPos
            {
                int LineNo { get; }
                int Column { get; }
            }

            private struct TCursurPos : ICursorPos
            {
                public int Column { get; set; }
                public int LineNo { get; set; }
            }

            /// <summary>
            /// MousePos の返り値のインターフェイス
            /// </summary>
            /// <returns>(LineNo, Column, X, Y)</returns>
            public interface IMousePos
            {
                int LineNo { get; }
                int Column { get; }
                int X { get; }
                int Y { get; }
            }

            private struct TMousePos : IMousePos
            {
                public int LineNo { get; set; }
                public int Column { get; set; }
                public int X { get; set; }
                public int Y { get; set; }
            }

            /// <summary>
            /// ユニコードのエディタ的な換算でのカーソルの位置を返す
            /// </summary>
            /// <returns>(LineNo, Column)</returns>
            public static ICursorPos CursorPos
            {
                get
                {
                    int lineno = -1;
                    int column = -1;
                    int success = pGetCursorPosUnicode(out lineno, out column);
                    if (success != 0)
                    {
                        TCursurPos pos = new TCursurPos();
                        pos.LineNo = lineno;
                        pos.Column = column;
                        return pos;
                    }
                    else
                    {
                        TCursurPos pos = new TCursurPos();
                        pos.LineNo = -1;
                        pos.Column = -1;
                        return pos;
                    }

                }
            }

            /// <summary>
            /// ユニコードのエディタ的な換算でのマウスの位置に対応するカーソルの位置を返す
            /// </summary>
            /// <returns>(LineNo, Column, X, Y)</returns>
            public static IMousePos MousePos
            {
                get
                {
                    POINT lpPoint;
                    bool success_1 = GetCursorPos(out lpPoint);

                    TMousePos pos = new TMousePos
                    {
                        LineNo = -1,
                        Column = -1,
                        X = -1,
                        Y = -1,
                    };

                    if (!success_1)
                    {
                        return pos;
                    }

                    int column = -1;
                    int lineno = -1;
                    int success_2 = pGetCursorPosUnicodeFromMousePos(IntPtr.Zero, out lineno, out column);
                    if (success_2 == 0)
                    {
                        return pos;
                    }

                    pos.LineNo = lineno;
                    pos.Column = column;
                    pos.X = lpPoint.X;
                    pos.Y = lpPoint.Y;
                    return pos;
                }
            }

            /// <summary>
            /// 現在開いているファイル名のフルパスを返す、無題テキストであれば、nullを返す。
            /// </summary>
            /// <returns>ファイル名のフルパス、もしくは null</returns>

            public static string FilePath
            {
                get
                {
                    IntPtr hWndHidemaru = WindowHandle;
                    if (hWndHidemaru != IntPtr.Zero)
                    {
                        const int WM_USER = 0x400;
                        const int WM_HIDEMARUINFO = WM_USER + 181;
                        const int HIDEMARUINFO_GETFILEFULLPATH = 4;

                        StringBuilder sb = new StringBuilder(filePathMaxLength); // まぁこんくらいでさすがに十分なんじゃないの...
                        bool cwch = SendMessage(hWndHidemaru, WM_HIDEMARUINFO, new IntPtr(HIDEMARUINFO_GETFILEFULLPATH), sb);
                        String filename = sb.ToString();
                        if (String.IsNullOrEmpty(filename))
                        {
                            return null;
                        }
                        else
                        {
                            return filename;
                        }
                    }
                    return null;
                }
            }
        }

        public static class Macro
        {
            /// <summary>
            /// マクロを実行中か否かを判定する
            /// </summary>
            /// <returns>実行中ならtrue, そうでなければfalse</returns>

            public static bool IsExecuting
            {
                get
                {
                    const int WM_USER = 0x400;
                    const int WM_ISMACROEXECUTING = WM_USER + 167;

                    IntPtr hWndHidemaru = WindowHandle;
                    if (hWndHidemaru != IntPtr.Zero)
                    {
                        bool cwch = SendMessage(hWndHidemaru, WM_ISMACROEXECUTING, IntPtr.Zero, IntPtr.Zero);
                        return cwch;
                    }

                    return false;
                }
            }

            /// <summary>
            /// マクロをプログラム内から実行した際の返り値のインターフェイス
            /// </summary>
            /// <returns>(Result, Message, Error)</returns>
            public interface IResult
            {
                int Result { get; }
                String Message { get; }
                Exception Error { get; }
            }

            private class TResult : IResult
            {
                public int Result { get; set; }
                public string Message { get; set; }
                public Exception Error { get; set; }

                public TResult(int Result, String Message, Exception Error)
                {
                    this.Result = Result;
                    this.Message = Message;
                    this.Error = Error;
                }
            }

            /// <summary>
            /// 現在のマクロ実行中に、プログラム中で、マクロを文字列で実行。
            /// マクロ実行中のみ実行可能なメソッド。
            /// </summary>
            /// <returns>(Result, Message, Error)</returns>

            public static IResult Eval(String expression)
            {
                TResult result;
                if (!IsExecuting)
                {
                    Exception e = new InvalidOperationException("Hidemaru_Macro_IsNotExecuting_Exception");
                    result = new TResult(-1, "", e);
                    return result;
                }
                int success = 0;
                try
                {
                    success = pEvalMacro(expression);
                }
                catch (Exception)
                {
                    throw;
                }
                if (success == 0)
                {
                    Exception e = new InvalidOperationException("Hidemaru_Macro_Eval_Exception");
                    result = new TResult(0, "", e);
                    return result;
                }
                else
                {
                    result = new TResult(success, "", null);
                    return result;
                }

            }

            public static class Exec
            {
                /// <summary>
                /// マクロを実行していない時に、プログラム中で、マクロファイルを与えて新たなマクロを実行。
                /// マクロを実行していない時のみ実行可能なメソッド。
                /// </summary>
                /// <returns>(Result, Message, Error)</returns>

                public static IResult File(string filepath)
                {
                    TResult result;
                    if (IsExecuting)
                    {
                        Exception e = new InvalidOperationException("Hidemaru_Macro_IsExecuting_Exception");
                        result = new TResult(-1, "", e);
                        return result;
                    }
                    if (!System.IO.File.Exists(filepath))
                    {
                        Exception e = new FileNotFoundException(filepath);
                        result = new TResult(-1, "", e);
                        return result;
                    }

                    const int WM_USER = 0x400;
                    const int WM_REMOTE_EXECMACRO_FILE = WM_USER + 271;
                    IntPtr hWndHidemaru = WindowHandle;

                    StringBuilder sbFileName = new StringBuilder(filepath);
                    StringBuilder sbRet = new StringBuilder("\x0f0f", 0x0f0f + 1); // 最初の値は帰り値のバッファー
                    bool cwch = SendMessage(hWndHidemaru, WM_REMOTE_EXECMACRO_FILE, sbRet, sbFileName);
                    if (cwch)
                    {
                        result = new TResult(1, sbRet.ToString(), null);
                    }
                    else
                    {
                        Exception e = new InvalidOperationException("Hidemaru_Macro_Eval_Exception");
                        result = new TResult(0, sbRet.ToString(), e);
                    }
                    return result;
                }

                /// <summary>
                /// マクロを実行していない時に、プログラム中で、文字列で新たなマクロを実行。
                /// マクロを実行していない時のみ実行可能なメソッド。
                /// </summary>
                /// <returns>(Result, Message, Error)</returns>
                public static IResult Eval(string expression)
                {
                    TResult result;
                    if (IsExecuting)
                    {
                        Exception e = new InvalidOperationException("Hidemaru_Macro_IsExecuting_Exception");
                        result = new TResult(-1, "", e);
                        return result;
                    }

                    const int WM_USER = 0x400;
                    const int WM_REMOTE_EXECMACRO_MEMORY = WM_USER + 272;
                    IntPtr hWndHidemaru = WindowHandle;

                    StringBuilder sbExpression = new StringBuilder(expression);
                    StringBuilder sbRet = new StringBuilder("\x0f0f", 0x0f0f + 1); // 最初の値は帰り値のバッファー
                    bool cwch = SendMessage(hWndHidemaru, WM_REMOTE_EXECMACRO_MEMORY, sbRet, sbExpression);
                    if (cwch)
                    {
                        result = new TResult(1, sbRet.ToString(), null);
                    }
                    else
                    {
                        Exception e = new InvalidOperationException("Hidemaru_Macro_Eval_Exception");
                        result = new TResult(0, sbRet.ToString(), e);
                    }
                    return result;
                }
            }
        }
    }
}

namespace HmNetPInvoke
{

#if BUILD_DLL
    public partial class Hm
#else
    internal partial class Hm
#endif
    {
        // 秀丸本体から出ている関数群
        private delegate IntPtr TGetCurrentWindowHandle();
        private delegate IntPtr TGetTotalTextUnicode();
        private delegate IntPtr TGetLineTextUnicode(int nLineNo);
        private delegate IntPtr TGetSelectedTextUnicode();
        private delegate int TGetCursorPosUnicode(out int pnLineNo, out int pnColumn);
        private delegate int TGetCursorPosUnicodeFromMousePos(IntPtr lpPoint, out int pnLineNo, out int pnColumn);
        private delegate int TEvalMacro([MarshalAs(UnmanagedType.LPWStr)] String pwsz);
        private delegate int TCheckQueueStatus();
        private delegate int TAnalyzeEncoding([MarshalAs(UnmanagedType.LPWStr)] String pwszFileName, IntPtr lParam1, IntPtr lParam2);
        private delegate IntPtr TLoadFileUnicode([MarshalAs(UnmanagedType.LPWStr)] String pwszFileName, int nEncode, ref int pcwchOut, IntPtr lParam1, IntPtr lParam2);

        // 秀丸本体から出ている関数群
        private static TGetCurrentWindowHandle pGetCurrentWindowHandle;
        private static TGetTotalTextUnicode pGetTotalTextUnicode;
        private static TGetLineTextUnicode pGetLineTextUnicode;
        private static TGetSelectedTextUnicode pGetSelectedTextUnicode;
        private static TGetCursorPosUnicode pGetCursorPosUnicode;
        private static TGetCursorPosUnicodeFromMousePos pGetCursorPosUnicodeFromMousePos;
        private static TEvalMacro pEvalMacro;
        private static TCheckQueueStatus pCheckQueueStatus;
        private static TAnalyzeEncoding pAnalyzeEncoding;
        private static TLoadFileUnicode pLoadFileUnicode;

        // 秀丸本体のexeを指すモジュールハンドル
        private static UnManagedDll hmExeHandle;

        private static void BindHidemaruExternFunctions()
        {
            // 初めての代入のみ
            if (hmExeHandle == null)
            {
                try
                {
                    hmExeHandle = new UnManagedDll(GetHidemaruExeFullPath());

                    pGetTotalTextUnicode = hmExeHandle.GetProcDelegate<TGetTotalTextUnicode>("Hidemaru_GetTotalTextUnicode");
                    pGetLineTextUnicode = hmExeHandle.GetProcDelegate<TGetLineTextUnicode>("Hidemaru_GetLineTextUnicode");
                    pGetSelectedTextUnicode = hmExeHandle.GetProcDelegate<TGetSelectedTextUnicode>("Hidemaru_GetSelectedTextUnicode");
                    pGetCursorPosUnicode = hmExeHandle.GetProcDelegate<TGetCursorPosUnicode>("Hidemaru_GetCursorPosUnicode");
                    pEvalMacro = hmExeHandle.GetProcDelegate<TEvalMacro>("Hidemaru_EvalMacro");
                    pCheckQueueStatus = hmExeHandle.GetProcDelegate<TCheckQueueStatus>("Hidemaru_CheckQueueStatus");

                    pGetCursorPosUnicodeFromMousePos = hmExeHandle.GetProcDelegate<TGetCursorPosUnicodeFromMousePos>("Hidemaru_GetCursorPosUnicodeFromMousePos");
                    pGetCurrentWindowHandle = hmExeHandle.GetProcDelegate<TGetCurrentWindowHandle>("Hidemaru_GetCurrentWindowHandle");

                    if (Version >= 890)
                    {
                        pAnalyzeEncoding = hmExeHandle.GetProcDelegate<TAnalyzeEncoding>("Hidemaru_AnalyzeEncoding");
                        pLoadFileUnicode = hmExeHandle.GetProcDelegate<TLoadFileUnicode>("Hidemaru_LoadFileUnicode");
                    }
                } catch(Exception e)
                {
                    System.Diagnostics.Trace.WriteLine(e.Message);
                }

            }
        }
    }
}

namespace HmNetPInvoke
{
#if BUILD_DLL
    public partial class Hm
#else
    internal partial class Hm
#endif
    {
        [DllImport("kernel32.dll")]
        private extern static uint GetModuleFileName(IntPtr hModule, StringBuilder lpFilename, int nSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        private extern static IntPtr GlobalLock(IntPtr hMem);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]

        private extern static bool GlobalUnlock(IntPtr hMem);
        [DllImport("kernel32.dll", SetLastError = true)]
        private extern static IntPtr GlobalFree(IntPtr hMem);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private extern static bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
        private static extern bool SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
        private static extern bool SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, StringBuilder lParam);

        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
        private static extern bool SendMessage(IntPtr hWnd, uint Msg, StringBuilder wParam, StringBuilder lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, int command, IntPtr lparam);
    }
}

namespace HmNetPInvoke
{

#if BUILD_DLL
    public static class HmExtentensions
#else
    internal static class HmExtentensions
#endif
    {
        public static void Deconstruct(this Hm.Edit.ICursorPos pos, out int LineNo, out int Column)
        {
            LineNo = pos.LineNo;
            Column = pos.Column;
        }

        public static void Deconstruct(this Hm.Edit.IMousePos pos, out int LineNo, out int Column, out int X, out int Y)
        {
            LineNo = pos.LineNo;
            Column = pos.Column;
            X = pos.X;
            Y = pos.Y;
        }
    }
}

namespace HmNetPInvoke
{

#if BUILD_DLL
    public partial class Hm
#else
    internal partial class Hm
#endif
    {
        // アンマネージドライブラリの遅延での読み込み。C++のLoadLibraryと同じことをするため
        // これをする理由は、このhmPyとHideamru.exeが異なるディレクトリに存在する可能性があるため、
        // C#風のDllImportは成立しないからだ。
        internal sealed class UnManagedDll : IDisposable
        {
            [DllImport("kernel32")]
            private static extern IntPtr LoadLibrary(string lpFileName);
            [DllImport("kernel32")]
            private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);
            [DllImport("kernel32")]
            private static extern bool FreeLibrary(IntPtr hModule);

            IntPtr moduleHandle;

            public UnManagedDll(string lpFileName)
            {
                moduleHandle = LoadLibrary(lpFileName);
            }

            public IntPtr ModuleHandle
            {
                get
                {
                    return moduleHandle;
                }
            }

            public T GetProcDelegate<T>(string method) where T : class
            {
                IntPtr methodHandle = GetProcAddress(moduleHandle, method);
                T r = Marshal.GetDelegateForFunctionPointer(methodHandle, typeof(T)) as T;
                return r;
            }

            public void Dispose()
            {
                FreeLibrary(moduleHandle);
            }
        }

    }
}