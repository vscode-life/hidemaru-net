#include <windows.h>
#include <tchar.h>

#include "hidemaruexe_export.h"

#pragma comment(lib, "version.lib")

HMODULE CHidemaruExeExport::hHideExeHandle = NULL;
TCHAR CHidemaruExeExport::szHidemaruFullPath[MAX_PATH] = L"";

CHidemaruExeExport::PFNGetDllFuncCalledType CHidemaruExeExport::Hidemaru_GetDllFuncCalledType = NULL;
CHidemaruExeExport::PFNGetTotalTextUnicode CHidemaruExeExport::Hidemaru_GetTotalTextUnicode = NULL;
CHidemaruExeExport::PFNGetSelectedTextUnicode CHidemaruExeExport::Hidemaru_GetSelectedTextUnicode = NULL;
CHidemaruExeExport::PFNGetLineTextUnicode CHidemaruExeExport::Hidemaru_GetLineTextUnicode = NULL;
CHidemaruExeExport::PFNGetCursorPosUnicode CHidemaruExeExport::Hidemaru_GetCursorPosUnicode = NULL;
CHidemaruExeExport::PFNEvalMacro CHidemaruExeExport::Hidemaru_EvalMacro = NULL;

double CHidemaruExeExport::hm_version = 0;
double CHidemaruExeExport::QueryFileVersion(TCHAR* path){
	VS_FIXEDFILEINFO* v;
	DWORD dwZero = 0;
	UINT len;
	DWORD sz = GetFileVersionInfoSize(path, &dwZero);
	if (sz){
		void* buf = new char[sz];
		GetFileVersionInfo(path, dwZero, sz, buf);

		if (VerQueryValue(buf, L"\\", (LPVOID*)&v, &len)){
			double ret = 0;
			ret = double(HIWORD(v->dwFileVersionMS)) * 100 +
				double(LOWORD(v->dwFileVersionMS)) * 10 +
				double(HIWORD(v->dwFileVersionLS)) +
				double(LOWORD(v->dwFileVersionLS)) * 0.01;
			delete buf;
			return ret;
		}
		else{
			delete buf;
		}
	}

	return 0;
}


BOOL CHidemaruExeExport::init() {

	if (hHideExeHandle) {
		return TRUE;
	}

	GetModuleFileName(NULL, szHidemaruFullPath, _countof(szHidemaruFullPath));
	hm_version = QueryFileVersion(szHidemaruFullPath);

	if (hm_version < 866) {
		MessageBox(NULL, L"秀丸のバージョンが不足しています。\n秀丸エディタ v8.66以上が必要です。", L"情報", MB_ICONERROR);
		return FALSE;
	}

	// 秀丸本体に関数があるのは 8.66以上
	hHideExeHandle = LoadLibrary(szHidemaruFullPath);

	if (hHideExeHandle) {
		Hidemaru_GetDllFuncCalledType = (PFNGetDllFuncCalledType)GetProcAddress(hHideExeHandle, "Hidemaru_GetDllFuncCalledType");
		Hidemaru_GetTotalTextUnicode = (PFNGetTotalTextUnicode)GetProcAddress(hHideExeHandle, "Hidemaru_GetTotalTextUnicode");
		Hidemaru_GetSelectedTextUnicode = (PFNGetSelectedTextUnicode)GetProcAddress(hHideExeHandle, "Hidemaru_GetSelectedTextUnicode");
		Hidemaru_GetLineTextUnicode = (PFNGetLineTextUnicode)GetProcAddress(hHideExeHandle, "Hidemaru_GetLineTextUnicode");
		Hidemaru_GetCursorPosUnicode = (PFNGetCursorPosUnicode)GetProcAddress(hHideExeHandle, "Hidemaru_GetCursorPosUnicode");
		Hidemaru_EvalMacro = (PFNEvalMacro)GetProcAddress(hHideExeHandle, "Hidemaru_EvalMacro");
		return TRUE;
	}

	return FALSE;
}


wstring CHidemaruExeExport::GetTotalText() {
	HGLOBAL hGlobal = CHidemaruExeExport::Hidemaru_GetTotalTextUnicode();
	if (hGlobal) {
		wchar_t* pwsz = (wchar_t*)GlobalLock(hGlobal);
		wstring text(pwsz); // コピー
		GlobalUnlock(hGlobal);
		GlobalFree(hGlobal); // 元のは解放
		return text;
	}
	return L"";
}


wstring CHidemaruExeExport::GetSelectedText() {
	HGLOBAL hGlobal = CHidemaruExeExport::Hidemaru_GetSelectedTextUnicode();
	if (hGlobal) {
		wchar_t* pwsz = (wchar_t*)GlobalLock(hGlobal);
		wstring text(pwsz); // コピー
		GlobalUnlock(hGlobal);
		GlobalFree(hGlobal); // 元のは解放
		return text;
	}
	return L"";
}

wstring CHidemaruExeExport::GetLineText(int lineno) {
	// ラインナンバーの指定がなければ、現在のカーソルのlineno
	if (lineno == 0) {
		auto pos = CHidemaruExeExport::GetCursorPos();
		lineno = pos.lineno;
	}
	HGLOBAL hGlobal = CHidemaruExeExport::Hidemaru_GetLineTextUnicode(lineno);
	if (hGlobal) {
		wchar_t* pwsz = (wchar_t*)GlobalLock(hGlobal);
		wstring text(pwsz); // コピー
		GlobalUnlock(hGlobal);
		GlobalFree(hGlobal); // 元のは解放
		return text;
	}
	return L"";
}


CHidemaruExeExport::HmCursurPos CHidemaruExeExport::GetCursorPos() {
	int nLineNo = 0;
	int nColumn = 0;
	BOOL f = Hidemaru_GetCursorPosUnicode(&nLineNo, &nColumn);
	HmCursurPos pos(nLineNo, nColumn);
	return pos;
}

BOOL CHidemaruExeExport::EvalMacro(wstring cmd) {
	return Hidemaru_EvalMacro(cmd.data());
}
