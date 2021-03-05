#-------------------- coding: utf-8 ---------------------------
# hmPython3 1.8.1.1用 ライブラリ
# Copyright (c) 2017-2021 Akitsugu Komiyama
# under the Apache License Version 2.0
#--------------------------------------------------------------

import hidemaru
import os

class _TText:
    class _TEncoding:

        def __init__(self, name, codepage, hm_encode):
            self.name = name  # Pythonでファイルを開く際にエンコードとして指定できる文字列( "cp932" や "utf8" など )
            self.codepage = codepage  # マイクロソフトコードページの番号が入っている (932 や 65001 など)
            self.hm_encode = hm_encode      # 秀丸の encode としての値が入っている ( 1 や 6 など )

class _TFile:
    """
    秀丸でファイル関連のクラス
    """

    class _TStreamReader:

        def __init__(self, filepath, hm_encode=-1):
            try:
                if not os.path.exists(filepath):
                    raise FileNotFoundError

                if hm_encode == -1:
                    encoding_name, codepage, hm_encode = hidemaru.file.get_encodingfromfile(filepath)
                else:
                    encoding_name, codepage, hm_encode = hidemaru.file.get_encodingfromhmencode(hm_encode)
                self.__encoding = _TText._TEncoding(encoding_name, codepage, hm_encode)
                self.__filepath = filepath
            except:
                raise

        def __enter__(self):
            return self

        def __GetEncoding(self):
            return self.__encoding

        # 開いたファイルのエンコーディング情報の取得
        Encoding = property(__GetEncoding)


        def __GetFilePath(self):
            return self.__filepath

        # ファイルパス
        FilePath = property(__GetFilePath)

        # 開いたファイルのテキストの取得
        def Read(self):
            try:
                if self.__filepath:
                    success, text = hidemaru.file.get_readalltext(self.__filepath, self.__encoding.hm_encode)
                    if success:
                        return text
                    else:
                        raise IOError
            except:
                raise

        def Close(self):
            self.__encoding = None
            self.__filepath = None

        def __exit__(self, exception_type, exception_value, traceback):
            self.Close()

    # ファイルを開く。
    def Open(self, filepath, hm_encode=-1):
        return _TFile._TStreamReader(filepath, hm_encode)

    # 対象のファイルの encode (秀丸マクロの encode 変数の表を参照) を得る。
    def GetEncoding(self, filepath):
        try:
            if not os.path.exists(filepath):
                raise FileNotFoundError

            encoding_name, codepage, hm_encode = hidemaru.file.get_encodingfromfile(filepath)
            return _TText._TEncoding(encoding_name, codepage, hm_encode)
        except:
            raise


class _TEdit:
    """
    秀丸で編集中のドキュメント関連のクラス
    """

    #--------------------------------------------------
    def __GetTotalText(self):
        return hidemaru.edit.get_totaltext()

    def __SetTotalText(self, text):
        return hidemaru.edit.set_totaltext(text)

    # 編集中のテキスト全体
    TotalText = property(__GetTotalText, __SetTotalText)
    #--------------------------------------------------

    #--------------------------------------------------
    def __GetSelectedText(self):
        return hidemaru.edit.get_selectedtext()

    def __SetSelectedText(self, text):
        return hidemaru.edit.set_selectedtext(text)

    # 選択中のテキスト。通常選択もしくは行選択のみ（矩形選択は対象としない）
    SelectedText = property(__GetSelectedText, __SetSelectedText)
    #--------------------------------------------------

    #--------------------------------------------------
    def __GetLineText(self):
        return hidemaru.edit.get_linetext()

    def __SetLineText(self, text):
        return hidemaru.edit.set_linetext(text)

    # カーソルがある行のテキスト
    LineText = property(__GetLineText, __SetLineText)
    #--------------------------------------------------

    #--------------------------------------------------
    class _TCursorPos:

        def __init__(self, lineno, column):
            self.lineno = lineno
            self.column = column

    def __GetCursorPos(self):
        lineno, column = hidemaru.edit.get_cursorpos()
        return _TEdit._TCursorPos(lineno, column)

    # カーソルの位置情報。linenoとcolumn（マクロのlinenoとcolumnと同じ値）
    CursorPos = property(__GetCursorPos)
    #--------------------------------------------------

    #--------------------------------------------------
    class _TMousePos:

        def __init__(self, lineno, column, x, y):
            self.lineno = lineno
            self.column = column
            self.x = x
            self.y = y

    def __GetMousePos(self):
        lineno, column, x, y = hidemaru.edit.get_mousepos()
        return _TEdit._TMousePos(lineno, column, x, y)

    # マウスの位置に対応するカーソルの情報。
    # linenoとcolumn（マクロのlinenoとcolumnと同じ値） xとyは、win32 apiのマウス位置情報と同じ
    MousePos = property(__GetMousePos)
    #--------------------------------------------------


class _TMacro:
    """
    秀丸マクロ関連のクラス
    """
    #--------------------------------------------------
    class _TVar:
        """
        秀丸マクロ関連のうち、マクロシンボル（マクロ変数）を扱うクラス
        """

        def __getitem__(self, varname):
            return hidemaru.macro.get_var(varname)

        def __setitem__(self, varname, value):
            if not varname.startswith("#") and not varname.startswith("$"):
                hidemaru.debug_info("指定のマクロ変数への値の代入は出来ません: " + varname)
                hidemaru.macro.set_var(varname, value)
                raise NameError(varname)
            else:
                return hidemaru.macro.set_var(varname, value)
    #--------------------------------------------------

    #--------------------------------------------------
    class _TResult:
        """
        秀丸マクロ関連のうち、マクロ実行結果情報を扱うクラス
        """

        def __init__(self, Result, Message, ErrorMsg):
            self.Result = Result
            self.Message = Message
            if Result >= 1:
                self.Error = None
            else:
                self.Error = RuntimeError(ErrorMsg)
    #--------------------------------------------------

    #--------------------------------------------------
    def __init__(self):
        self.Var = _TMacro._TVar()
    #--------------------------------------------------

    #--------------------------------------------------
    # マクロの実行
    def Eval(self, expression_text):
        res, msg, errmsg = hidemaru.macro.do_eval(expression_text)
        ret = _TMacro._TResult(res, msg, errmsg)
        return ret
    #--------------------------------------------------


class _TOutputPane:
    """
    秀丸アウトプットパネル関連のクラス
    """
    # アウトプットパネルへの出力
    def Output(self, message):
        return hidemaru.outputpane.output(message)
            
    # アウトプット枠情報の一時退避
    def Push(self):
        return hidemaru.outputpane.push()

    # アウトプット枠情報の一時退避したものを復元
    def Push(self):
        return hidemaru.outputpane.pop()

    # アウトプット枠にメッセージを送る
    def SendMessage(self, command_id):
        return hidemaru.outputpane.sendmessage(command_id)

    # アウトプット枠の基底ディレクトリを設定する
    def SetBaseDir(self, dirpath):
        return hidemaru.outputpane.setbasedir(dirpath)


class _THidemaru:
    """
    特定のカテゴリに所属しないようなもの
    """
    #--------------------------------------------------
    def __init__(self):
        self.File = _TFile()
        self.Edit = _TEdit()
        self.Macro = _TMacro()
        self.OutputPane = _TOutputPane()
    #--------------------------------------------------

    #--------------------------------------------------
    # デバッグモニターに出力
    def debuginfo(self, obj):
        return hidemaru.debug_info(obj)
    #--------------------------------------------------

    #--------------------------------------------------
    def __GetVersion(self):
        return hidemaru.get_version()

    # 秀丸のバージョンを 866.05 のような形で取得。
    # 「β5」なら少数部分が「05」のような形。
    # 正式版だと、866.99 のように「99」となる。
    version = property(__GetVersion)
    #--------------------------------------------------


hm = _THidemaru()