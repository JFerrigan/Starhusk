# Agent Notes

- Project: Unity game `Starhusk`.
- Core loop: procedural star system with a player ship, planets, asteroids, map markers, radar pings, and Dyson satellites.
- Relevant scripts are in `Assets/Scripts/`.
- `StarSystemGenerator` builds the world and spawns the main content.
- `DysonSatellite` handles satellite orbit behavior.
- Stationary Dyson satellites are now treated as movable man-made objects.
- `ManMadeMovableObject` provides click-to-edit placement with a translucent preview under the cursor.
- Stationary satellites should update their orbit data after being moved.
- Existing project state may include unrelated local edits; do not revert them unless asked.
- `dotnet build Assembly-CSharp.csproj` currently passes.
- Unity EditMode tests could not run while another Unity instance had the project open.
