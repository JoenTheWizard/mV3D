#pragma once
#include "Camera.h"
#include <iostream>
extern "C" {
#include "Lua542/lua.h"
#include "Lua542/lauxlib.h"
#include "Lua542/lualib.h"
}
#if _WIN64
#pragma comment(lib, "Lua542/liblua54.a")
#endif
class LuaEnv
{
public:
	LuaEnv();
	~LuaEnv();
	lua_State* D = luaL_newstate();
	//Lua Multiplex Environment Functions and Variables
	void SetCameraTable(Camera* cameraEngine, char luaText[512]);
private:
	//char* execBuff[512];
};

