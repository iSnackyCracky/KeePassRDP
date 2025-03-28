/*
 *  Copyright (C) 2018 - 2025 iSnackyCracky, NETertainer
 *
 *  This file is part of KeePassRDP.
 *
 *  KeePassRDP is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  KeePassRDP is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with KeePassRDP.  If not, see <http://www.gnu.org/licenses/>.
 *
 */

#include "pch.h"

#ifndef DECLSPEC_EXPORT
#define DECLSPEC_EXPORT __declspec(dllexport)
#endif // !DECLSPEC_DLLEXPORT

EXTERN_C DECLSPEC_EXPORT DECLSPEC_NOINLINE STDMETHODIMP_(PVOID) KprSecureZeroMemory(_In_ PVOID __restrict ptr, _In_ SIZE_T cnt)
{
    return SecureZeroMemory(ptr, cnt);
}

EXTERN_C DECLSPEC_EXPORT STDMETHODIMP KprDoDefaultAction(_In_ PVOID __restrict ptr)
{
    IUIAutomation* uiAutomation = NULL;
    IUIAutomationElement* parent = NULL;
    IUIAutomationLegacyIAccessiblePattern* pattern = NULL;
    HRESULT result = S_OK;

    if (FAILED(CoCreateInstance(__uuidof(CUIAutomation8), NULL, CLSCTX_INPROC_SERVER, IID_PPV_ARGS(&uiAutomation))) || uiAutomation == NULL)
    {
        if (uiAutomation != NULL)
            uiAutomation->Release();
        return E_FAIL;
    }

    if (FAILED(uiAutomation->ElementFromHandle(ptr, &parent)) || parent == NULL)
    {
        if (parent != NULL)
            parent->Release();
        uiAutomation->Release();
        return E_FAIL;
    }

    if (FAILED(parent->GetCurrentPatternAs(UIA_LegacyIAccessiblePatternId, IID_PPV_ARGS(&pattern))) || pattern == NULL)
    {
        if (pattern != NULL)
            pattern->Release();
        parent->Release();
        uiAutomation->Release();
        return E_FAIL;
    }

    if (FAILED(pattern->DoDefaultAction()))
        result = E_FAIL;

    pattern->Release();
    parent->Release();
    uiAutomation->Release();

    return result;
}

/*EXTERN_C DECLSPEC_EXPORT STDMETHODIMP KprDoDefaultAction(_In_ PVOID ptr, _In_ PWSTR automationId)
{
    IUIAutomation* uiAutomation = NULL;
    if (FAILED(CoCreateInstance(__uuidof(CUIAutomation8), NULL, CLSCTX_INPROC_SERVER, IID_PPV_ARGS(&uiAutomation))) || uiAutomation == NULL)
        return E_FAIL;

    IUIAutomationElement* parent = NULL;
    if (FAILED(uiAutomation->ElementFromHandle(ptr, &parent)) || parent == NULL)
        return E_FAIL;

    IUIAutomationCondition* condition = NULL;
    VARIANT variant{};
    variant.vt = VT_LPWSTR;
    variant.bstrVal = automationId;
    if (FAILED(uiAutomation->CreatePropertyCondition(UIA_AutomationIdPropertyId, variant, &condition)) || condition == NULL)
        return E_FAIL;

    IUIAutomationElement* child = NULL;
    if (FAILED(parent->FindFirst(TreeScope_Children, condition, &child)) || child == NULL)
        return E_FAIL;

    IUIAutomationLegacyIAccessiblePattern* pattern = NULL;
    if (FAILED(child->GetCurrentPatternAs(UIA_LegacyIAccessiblePatternId, IID_PPV_ARGS(&pattern))) || pattern == NULL)
        return E_FAIL;

    if (FAILED(pattern->DoDefaultAction()))
        return E_FAIL;

    pattern->Release();
    child->Release();
    parent->Release();
    uiAutomation->Release();

    return S_OK;
}*/

#pragma comment(linker, "/SECTION:.SHARED,RWS")
#pragma data_seg(".SHARED")
HWND g_hwnd = NULL;
HHOOK g_hook = NULL;
#pragma data_seg()

HMODULE g_hModule = NULL;

LRESULT CALLBACK WhCbtHookProc(int nCode, WPARAM wParam, LPARAM lParam)
{
    if (nCode < 0)
    {
        return CallNextHookEx(g_hook, nCode, wParam, lParam);
    }

    if (g_hwnd != NULL)
    {
        switch (nCode)
        {
        case HCBT_MINMAX:
            SendMessage(g_hwnd, WM_USER + 1, wParam, lParam);
            break;
        }
    }

    return CallNextHookEx(g_hook, nCode, wParam, lParam);
}

EXTERN_C DECLSPEC_EXPORT STDMETHODIMP KprSetCbtHwnd(_In_ HWND __restrict hwnd)
{
    if (g_hModule == NULL)
        return -1;

    if (g_hook != NULL)
    {
        UnhookWindowsHookEx(g_hook);
        g_hook = NULL;
    }

    g_hwnd = hwnd;

    if (g_hwnd != NULL)
    {
        g_hook = SetWindowsHookEx(WH_CBT, WhCbtHookProc, g_hModule, 0);
    }

    return S_OK;
}

EXTERN_C BOOL APIENTRY DllMain(HMODULE hModule,
    DWORD  ul_reason_for_call,
    LPVOID lpReserved
)
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
        g_hModule = hModule;
        DisableThreadLibraryCalls(hModule);
        break;
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}