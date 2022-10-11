// CharonComTest.cpp : Implementation of CCharonComTest

#include "stdafx.h"
#include "CharonComTest.h"


void Call1(char *buff)
{
	strcpy(buff, "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
	for(int i = 0; i<100; i++)
		strcat(buff, "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
}

// CCharonComTest


STDMETHODIMP CCharonComTest::Method1(BSTR str, BSTR* ret)
{
	// TODO: Add your implementation code here

	wprintf(L"CCharonComTest::Method1(%s)\n", str);

	*ret = SysAllocString(L"Method1Return");

	return S_OK;
}

STDMETHODIMP CCharonComTest::Method2(BSTR* ret)
{
	// TODO: Add your implementation code here
	printf("CCharonComTest::Method2()\n");
	*ret = SysAllocString(L"Method2Return");

	return S_OK;
}

STDMETHODIMP CCharonComTest::Method3(BSTR str)
{
	// TODO: Add your implementation code here
	wprintf(L"CCharonComTest::Method3(%s)\n", str);

	if (wcslen(str) > 50)
	{
		char buff[10];
		wprintf(L"In Call1");
		Call1(buff);
	}

	return S_OK;
}

STDMETHODIMP CCharonComTest::Method4(void)
{
	// TODO: Add your implementation code here
	printf("CCharonComTest::Method4()\n");

	return S_OK;
}

BSTR property1;

STDMETHODIMP CCharonComTest::get_Property1(BSTR* pVal)
{
	// TODO: Add your implementation code here
	pVal;

	//pVal = &property1;

	printf("CCharonComTest::get_Property1()\n");

	return S_OK;
}

STDMETHODIMP CCharonComTest::put_Property1(BSTR newVal)
{
	// TODO: Add your implementation code here

	wprintf(L"CCharonComTest::put_Property1(%s)\n", newVal);

	return S_OK;
}

STDMETHODIMP CCharonComTest::Method5(LONG int1, SHORT short1, LONG* retval)
{
	// TODO: Add your implementation code here
	*retval = int1 + short1;
	wprintf(L"CCharonComTest::Method5(%d, %d, %d)\n", int1, short1, *retval);

	return S_OK;
}

STDMETHODIMP CCharonComTest::Method6(SHORT shortParam, INT intParam)
{
	// TODO: Add your implementation code here
	wprintf(L"CCharonComTest::Method6(%d, %d)\n", shortParam, intParam);

	return S_OK;
}
