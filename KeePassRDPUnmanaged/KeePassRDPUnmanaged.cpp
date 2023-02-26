/*
 *  Copyright (C) 2018 - 2023 iSnackyCracky, NETertainer
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

EXTERN_C DECLSPEC_EXPORT STDMETHODIMP_(PVOID) KprSecureZeroMemory(_In_ PVOID ptr, _In_ SIZE_T cnt)
{
    return SecureZeroMemory(ptr, cnt);
}

EXTERN_C DECLSPEC_EXPORT STDMETHODIMP KprDoDefaultAction(_In_ PVOID ptr)
{
    IUIAutomation* uiAutomation = NULL;
    if (FAILED(CoCreateInstance(__uuidof(CUIAutomation8), NULL, CLSCTX_INPROC_SERVER, IID_PPV_ARGS(&uiAutomation))) || uiAutomation == NULL)
        return E_FAIL;

    IUIAutomationElement* parent = NULL;
    if (FAILED(uiAutomation->ElementFromHandle(ptr, &parent)) || parent == NULL)
        return E_FAIL;

    IUIAutomationLegacyIAccessiblePattern* pattern = NULL;
    if (FAILED(parent->GetCurrentPatternAs(UIA_LegacyIAccessiblePatternId, IID_PPV_ARGS(&pattern))) || pattern == NULL)
        return E_FAIL;

    if (FAILED(pattern->DoDefaultAction()))
        return E_FAIL;

    pattern->Release();
    parent->Release();
    uiAutomation->Release();

    return S_OK;
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

EXTERN_C BOOL APIENTRY DllMain(HMODULE hModule,
    DWORD  ul_reason_for_call,
    LPVOID lpReserved
)
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
        DisableThreadLibraryCalls(hModule);
        break;
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}