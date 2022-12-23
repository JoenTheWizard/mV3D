#include "LuaEnv.h"
LuaEnv::LuaEnv(){
	luaL_openlibs(D);
}
LuaEnv::~LuaEnv() { delete D; }

void LuaEnv::SetCameraTable(Camera* cameraEngine, char luaText[512]) {
	//Manipulate Lua Stack to be able to set the Camera position with tables.
	//I know shitty implementation, but will be fixed over time (aka never... maybe... we'll see)
	if (luaL_dostring(D, luaText) == 0) {
		lua_getglobal(D, "cameraEngine");
		if (lua_istable(D, -1)) {
			lua_pushstring(D, "X");
			lua_gettable(D, -2);
			cameraEngine->Position.x = lua_tonumber(D, -1);
			lua_pop(D, 1);

			lua_pushstring(D, "Y");
			lua_gettable(D, -2);
			cameraEngine->Position.y = lua_tonumber(D, -1);
			lua_pop(D, 1);

			lua_pushstring(D, "Z");
			lua_gettable(D, -2);
			cameraEngine->Position.z = lua_tonumber(D, -1);
			lua_pop(D, 1);
		}
		lua_getglobal(D, "cameraFront");
		if (lua_istable(D, -1)) {
			lua_pushstring(D, "X");
			lua_gettable(D, -2);
			cameraEngine->Front.x = lua_tonumber(D, -1);
			lua_pop(D, 1);

			lua_pushstring(D, "Y");
			lua_gettable(D, -2);
			cameraEngine->Front.y = lua_tonumber(D, -1);
			lua_pop(D, 1);

			lua_pushstring(D, "Z");
			lua_gettable(D, -2);
			cameraEngine->Front.z = lua_tonumber(D, -1);
			lua_pop(D, 1);
		}
	}
}