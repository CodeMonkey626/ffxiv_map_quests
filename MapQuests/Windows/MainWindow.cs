using System;
using System.Drawing;
using System.Numerics;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Map = FFXIVClientStructs.FFXIV.Client.Game.UI.Map;
using Quest = Lumina.Excel.GeneratedSheets.Quest;
using ImGuiNET;
using Dalamud.Interface;
using Dalamud.Game.Text.SeStringHandling.Payloads;

namespace MapQuests.Windows;

public unsafe class MainWindow : Window, IDisposable
{
    private Plugin Plugin;
    private const float ElementHeight = 48.0f;

    // We give this window a hidden ID using ##
    // So that the user will see "My Amazing Window" as window title,
    // but for ImGui the ID is "My Amazing Window##With a hidden ID"
    public MainWindow(Plugin plugin)
        : base("Unaccepted Quests", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(300.0f, 500.0f),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        Plugin = plugin;
    }

    public void Dispose() { }

    public override void Draw()
    {
        using var child = ImRaii.Child($"quest_list_scrollable_child", new Vector2(0.0f, 0.0f), false, ImGuiWindowFlags.None);

        if (Map.Instance()->UnacceptedQuestMarkers.Count > 0) {
            foreach (var quest in Map.Instance()->UnacceptedQuestMarkers) {
                var questData = Plugin.DataManager.GetExcelSheet<Quest>()!.GetRow(quest.ObjectiveId + 65536u);
                
                foreach (var marker in quest.MarkerData) {
                    var cursorStart = ImGui.GetCursorScreenPos();
                    if (ImGui.Selectable(
                        $"##{quest.ObjectiveId}_Selectable_{marker.LevelId}",
                        false,
                        ImGuiSelectableFlags.None,
                        new Vector2(ImGui.GetContentRegionAvail().X, ElementHeight * ImGuiHelpers.GlobalScale)
                    )) {
                        var mapLink = new MapLinkPayload(
                            marker.TerritoryTypeId,
                            marker.MapId,
                            (int) (marker.X * 1_000f),
                            (int) (marker.Z * 1_000f)
                        );

                        Plugin.GameGui.OpenMapWithMapLink(mapLink);
                    }

                    ImGui.SetCursorScreenPos(cursorStart);
                    ImGui.Image(Plugin.TextureProvider.GetFromGameIcon(marker.IconId).GetWrapOrEmpty().ImGuiHandle, ImGuiHelpers.ScaledVector2(ElementHeight, ElementHeight));
                    
                    ImGui.SameLine();
                    var text = $"Lv. {questData?.ClassJobLevel0} {quest.Label}";
                    
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + ElementHeight * ImGuiHelpers.GlobalScale / 2.0f - ImGui.CalcTextSize(text).Y / 2.0f);
                    ImGui.Text(text);
                }
            }
        }
        else {
            const string text = "No quests available";
            var textSize = ImGui.CalcTextSize(text);
            ImGui.SetCursorPosX(ImGui.GetContentRegionAvail().X / 2.0f - textSize.X / 2.0f);
            ImGui.SetCursorPosY(ImGui.GetContentRegionAvail().Y / 2.0f - textSize.Y / 2.0f);
            ImGui.TextColored(KnownColor.Orange.Vector(), text);
        }
    }
}
