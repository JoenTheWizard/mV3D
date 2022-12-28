cameraEngine = {X=0,Y=0,Z=0}

local x = 1
while x < 10 do
  cameraEngine["X"] = math.sin(GetTime())
  x = x + 1
end