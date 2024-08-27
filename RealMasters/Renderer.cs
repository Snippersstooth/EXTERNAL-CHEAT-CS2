using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using ClickableTransparentOverlay;
using ImGuiNET;

namespace RealMasters
{
    public class Renderer : Overlay
    {
        public Vector2 screenSize = new Vector2(1920, 1080);

        private ConcurrentQueue<Entity> entities = new ConcurrentQueue<Entity>();
        private Entity localPlayer = new Entity();
        private readonly object entityLock = new object();

        private bool enableESP = false;
        public bool enableName = false;
        public bool enableVisibilityCheck = false;
        public bool aimbot = false;
        public bool triggerBot = false;
        public bool aimOnTeam = false;
        public bool radar = false;
        public bool antiFlash = false;
        public bool checkVisibility = false;
        public bool aimOnClosest = false;
        public int smooth = 0;
        public int fov = 90;

        private Vector4 enemyColor = new Vector4(1, 0, 0, 1);
        private Vector4 teamColor = new Vector4(0, 1, 0, 1);
        private Vector4 hiddenColor = new Vector4(0, 0, 0, 1);
        private Vector4 nameColor = new Vector4(1, 1, 1, 1);

        ImDrawListPtr drawList;

        private bool menuVisible = true;

        // Hotkey options and selected hotkey
        private readonly string[] hotkeyOptions = { "None","CTRL", "SHIFT", "ALT", "MOUSE 1", "MOUSE 2", "MOUSE 4", "MOUSE 5" };
        private string selectedHotkey = "None"; // Default selected hotkey

        public string GetSelectedHotkey()
        {
            return selectedHotkey;
        }

        public void SetSelectedHotkey(string hotkey)
        {
            selectedHotkey = hotkey;
        }

        protected override void Render()
        {
            if (ImGui.IsKeyPressed(ImGui.GetKeyIndex(ImGuiKey.Insert)))
                menuVisible = !menuVisible;

            if (menuVisible)
            {
                ImGui.SetNextWindowSize(new Vector2(800, 600), ImGuiCond.FirstUseEver);
                ImGui.Begin("MasterSos Cheat Menu", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoResize);

                ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(20, 20));
                ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(10, 10));
                ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(15, 10));
                ImGui.PushStyleVar(ImGuiStyleVar.ItemInnerSpacing, new Vector2(5, 5));

                ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0.1f, 0.1f, 0.1f, 0.9f));
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 1, 1, 1));
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.4f, 0.4f, 0.4f, 1.0f));
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.5f, 0.5f, 0.5f, 1.0f));
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.6f, 0.6f, 0.6f, 1.0f));
                ImGui.PushStyleColor(ImGuiCol.CheckMark, new Vector4(0.2f, 0.9f, 0.2f, 1.0f));
                ImGui.PushStyleColor(ImGuiCol.SliderGrab, new Vector4(0.2f, 0.9f, 0.2f, 1.0f));

                if (ImGui.BeginTabBar("Tabs", ImGuiTabBarFlags.NoCloseWithMiddleMouseButton | ImGuiTabBarFlags.FittingPolicyScroll))
                {

                        if (ImGui.BeginTabItem("Visuals"))
                        {
                            ImGui.Text("Esp Settings");
                            ImGui.Separator();
                            ImGui.Checkbox("Enable ESP", ref enableESP);
                            ImGui.Checkbox("ESP Visibility Check", ref enableVisibilityCheck);
                            ImGui.Checkbox("Show Name", ref enableName);
                            ImGui.Checkbox("Radar Hack (not public)", ref radar);
                            ImGui.Checkbox("Anti Flash", ref antiFlash);
                            ImGui.Separator();
                            ImGui.Text("ESP Colors");
                            ImGui.Separator();

                            // Enemy color section
                            ImGui.Text("Enemy:");
                            ImGui.SameLine();
                            ImGui.ColorEdit4("##enemycolor", ref enemyColor, ImGuiColorEditFlags.NoInputs);
                            ImGui.SameLine(150); // Adjust the spacing between sections
                            ImGui.Text("Team:");
                            ImGui.SameLine();
                            ImGui.ColorEdit4("##teamcolor", ref teamColor, ImGuiColorEditFlags.NoInputs);
                            ImGui.SameLine(300); // Adjust the spacing between sections
                            ImGui.Text("Visibility Check:");
                            ImGui.SameLine();
                            ImGui.ColorEdit4("##visibilitycolor", ref hiddenColor, ImGuiColorEditFlags.NoInputs);

                            ImGui.Separator();

                            ImGui.Text("FOV Changer");
                            ImGui.Separator();
                            ImGui.SliderInt("FOV", ref fov, 58, 140);
                            ImGui.EndTabItem();
                        }
                        if (ImGui.BeginTabItem("DeathSide"))
                    {
                        ImGui.Text("Aimbot Settings");
                        ImGui.Separator();
                        ImGui.Checkbox("Enable Aimbot", ref aimbot);
                        ImGui.Checkbox("Aimbot On Team", ref aimOnTeam);
                        ImGui.Separator();
                        ImGui.Separator();

                        // Combo box for selecting a hotkey
                        ImGui.Text("Select Aimbot Hotkey:");
                        if (ImGui.BeginCombo("##hotkeycombo", selectedHotkey))
                        {
                            foreach (var hotkey in hotkeyOptions)
                            {
                                bool isSelected = (selectedHotkey == hotkey);
                                if (ImGui.Selectable(hotkey, isSelected))
                                {
                                    selectedHotkey = hotkey;
                                }

                                // Set the initial focus when opening the combo (scrolling to the current selection)
                                if (isSelected)
                                {
                                    ImGui.SetItemDefaultFocus();
                                }
                            }
                            ImGui.EndCombo();
                        }

                        ImGui.EndTabItem();
                    }

                    ImGui.EndTabBar();
                }

                ImGui.PopStyleVar(4);
                ImGui.PopStyleColor(7);

                ImGui.End();
            }

            DrawOverlay(screenSize);
            drawList = ImGui.GetWindowDrawList();

            if (enableESP)
            {
                foreach (var entity in entities)
                {
                    if (EntityOnScreen(entity))
                    {
                        DrawBox(entity);
                        DrawHealthBar(entity);
                        DrawLine(entity);
                        DrawName(entity, 20);
                    }
                }
            }
        }

        bool EntityOnScreen(Entity entity)
        {
            return entity.position2D.X > 0 && entity.position2D.X < screenSize.X &&
                   entity.position2D.Y > 0 && entity.position2D.Y < screenSize.Y;
        }

        private void DrawHealthBar(Entity entity)
        {
            float entityHeight = entity.position2D.Y - entity.viewPosition2D.Y;
            float boxLeft = entity.viewPosition2D.X - entityHeight / 3;
            float boxRight = entity.position2D.X + entityHeight / 3;
            float barPercentWidth = 0.05f;
            float barPixelWidth = barPercentWidth * (boxRight - boxLeft);
            float barHeight = entityHeight * (entity.health / 100f);
            Vector2 barTop = new Vector2(boxLeft - barPixelWidth, entity.position2D.Y - barHeight);
            Vector2 barBottom = new Vector2(boxLeft, entity.position2D.Y);
            Vector4 barColor = new Vector4(0, 1, 0, 1);

            drawList.AddRectFilled(barTop, barBottom, ImGui.ColorConvertFloat4ToU32(barColor));
        }

        private void DrawName(Entity entity, int yOffset)
        {
            if (enableName)
            {
                Vector2 textLocation = new Vector2(entity.viewPosition2D.X, entity.viewPosition2D.Y - yOffset);
                drawList.AddText(textLocation, ImGui.ColorConvertFloat4ToU32(nameColor), $"{entity.name}");
            }
        }

        private void DrawBox(Entity entity)
        {
            float entityHeight = entity.position2D.Y - entity.viewPosition2D.Y;
            Vector2 rectTop = new Vector2(entity.viewPosition2D.X - entityHeight / 3, entity.viewPosition2D.Y);
            Vector2 rectBottom = new Vector2(entity.position2D.X + entityHeight / 3, entity.position2D.Y);
            Vector4 boxColor = localPlayer.team == entity.team ? teamColor : enemyColor;

            if (enableVisibilityCheck)
                boxColor = entity.spotted ? boxColor : hiddenColor;

            drawList.AddRect(rectTop, rectBottom, ImGui.ColorConvertFloat4ToU32(boxColor));
        }

        private void DrawLine(Entity entity)
        {
            Vector4 lineColor = localPlayer.team == entity.team ? teamColor : enemyColor;
            drawList.AddLine(new Vector2(screenSize.X / 2, screenSize.Y), entity.position2D, ImGui.ColorConvertFloat4ToU32(lineColor));
        }

        public void UpdateEntities(IEnumerable<Entity> newEntities)
        {
            entities = new ConcurrentQueue<Entity>(newEntities);
        }

        public void UpdateLocalPlayer(Entity newEntity)
        {
            lock (entityLock)
            {
                localPlayer = newEntity;
            }
        }

        void DrawOverlay(Vector2 screenSize)
        {
            ImGui.SetNextWindowSize(screenSize);
            ImGui.SetNextWindowPos(new Vector2(0, 0));
            ImGui.Begin("overlay", ImGuiWindowFlags.NoDecoration
                | ImGuiWindowFlags.NoBackground
                | ImGuiWindowFlags.NoBringToFrontOnFocus
                | ImGuiWindowFlags.NoMove
                | ImGuiWindowFlags.NoInputs
                | ImGuiWindowFlags.NoCollapse
                | ImGuiWindowFlags.NoScrollbar
                | ImGuiWindowFlags.NoScrollWithMouse
                );
        }
    }
}
